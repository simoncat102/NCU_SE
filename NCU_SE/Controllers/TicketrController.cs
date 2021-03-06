using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NCU_SE.Data;
using NCU_SE.Models;
using NCU_SE.SharedModule;

namespace NCU_SE.Controllers
{
    //班機回傳格式
    public class QueryFlight
    {
        public List<FixFlight> Depart { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
    }

    //即時航班格式
    public class FixFlight
    {
        public string ScheduleStartDate { get; set; }
        public string ScheduleEndDate { get; set; }
        public string FlightNumber { get; set; }
        public string AirlineID { get; set; }
        public string AirlineName { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalTime { get; set; }
        public DateTime FlightDate { get; set; }
        public string FlightTime { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }
    }
    public class TicketController : Controller
    {
        sharedModule module = new();
        public IActionResult FixedFlight(FixedFlight obj)
        {
            QueryFlight QF = new();
                //取得固定航班資料
                if (obj != null) QF = getFixedFlight(obj.Origin, obj.Destination, obj.DepartureDate, obj.ReturnDate, obj.FlightNumber);
                ViewBag.Depart = QF.Depart;//將去程資料放入viewbag中
            //如果已登入，自動載入儲存的航班-->避免重複儲存航班用
            ViewData["uid"] = getSession("acc");
            if (LoginStat())
            {
                var flight_saved = _db.Flight.Where(u => u.MemberID == int.Parse(getSession("acc")) && u.DepTime >= DateTime.Today).Select(u => new { u.FlightCode, u.DepTime }).ToList();
                List<string> SavedFlight = new();
                foreach (var f in flight_saved)
                {
                    string flight = (f.FlightCode).ToString()+(f.DepTime).ToString("yyyy/MM/dd");
                    SavedFlight.Add(flight);
                    Debug.Print(flight);
                }
                
                ViewBag.SavedFlight = SavedFlight;
            }
            //Debug.Print("目前已儲存"+ViewBag.SavedFlight.Where(x=> x.FlightCode =="TG-6244" && x.DepTime.ToString("yyyy/MM/dd")=="2021/12/27").Count()) ;

            ViewData["origin"] = QF.Origin;
            ViewData["destination"] = QF.Destination;
            ViewData["login"] = getSession("login_status");
            ViewData["log_action"] = getSession("login_action");
            ViewData["log_uid"] =getSession("acc");

            setSession("LastQuery" , Request.QueryString.ToString());
            
            return View("RealtimeFlight");
        }
        public IActionResult SaveFlight(FixedFlight ff)
        {
            if (!LoginStat()) return RedirectToAction("Login", "Home");//若未登入轉跳到登入畫面
            //因為一個view難以套用兩個model，因此使用FixedFlight先取回資料，再換容器
            Flight obj = new();
            //將資料置入Ticket內
            obj.AirportFrom = ff.Origin;//出發地
            obj.AirportTo = ff.Destination;//目的地
            obj.DepTime = ff.DepartureDate;//預計起飛日期時間
            obj.ArriTime = ff.ReturnDate;//預計降落日期時間
            obj.FlightCode = ff.FlightNumber;//航班編號
            obj.FlightNote = ff.Note;//航班備註            
            obj.MemberID = int.Parse(getSession("acc"));//會員ID
            obj.Airline = ff.FlightNumber.Substring(0,2);
            
            _db.Flight.Add(obj);//新增個人航班
            _db.SaveChanges();//更新至資料庫
            return RedirectToAction("UserTicket", "User");       
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region 登入驗證/session處理
        //檢測登入狀態
        public bool LoginStat()
        {
            try//檢測session 'acc'是否存在，若存在且不為空則表示已經登入
            {
                Debug.Print("session id = " + HttpContext.Session.Id + "  acc = " + HttpContext.Session.GetString("acc"));
                if (getSession("acc") != null && getSession("acc") != "0")//若已登入
                {
                    return true;//跳到首頁
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        //取得Session用的模組==>getSession([Session名稱])==>一律回傳字串，若不存在會傳回null且非字串型態無法取得
        public string getSession(string name)
        {
            string result = null;
            try
            {
                result = HttpContext.Session.GetString(name);
            }
            catch { }
            return result;
        }

        //設定Session用的模組==>setSession([名稱],[文字內容])
        public void setSession(string name, string content)
        {
            try
            {
                HttpContext.Session.SetString(name, content);
            }
            catch { }
        }
        #endregion
        //通用模組
        private readonly ApplicationDbContext _db; //使用資料庫實體
        public TicketController(ApplicationDbContext db)
        {
            _db = db;
        }
        
        //取得航班
        public QueryFlight getFixedFlight(string origin, string destnation, DateTime departureDate, DateTime returnDate, string FlightNumber)
        {
            string json = null;
            
            string[] place = { origin, destnation };
            DateTime[] time = { departureDate, returnDate };
            QueryFlight QF = new();
            QF.Depart = new List<FixFlight>();
            QF.Origin = place[0];
            QF.Destination = place[1];
            #region 取得機場代號           
            for (int i =0; i<2; i++)
            {
                string AirportQuery = string.Format("https://ptx.transportdata.tw/MOTC/v2/Air/Airport?$select=AirportID&$filter=AirportID ne '' and (AirportID eq '{0}' or AirportName/Zh_tw eq '{0}' or AirportName/En eq '{0}' or AirportCityName/Zh_tw eq '{0}' or AirportCityName/En eq '{0}')&$top=2&$format=JSON", place[i]);
                json = module.getAPIdata(AirportQuery).Replace("[", "").Replace("]", "");
               
                try
                {
                    AirportInfo info = JsonSerializer.Deserialize<AirportInfo>(json);
                    place[i] = info.AirportID;
                }
                catch(Exception ex)
                {
                    Debug.Print(ex.Message);
                    place[i] = "";
                }
            }


            #endregion
            #region 取得班機
            //取得班機資訊              
            string url = string.Format("https://ptx.transportdata.tw/MOTC/v2/Air/GeneralSchedule/International? $select=AirlineID,FlightNumber,DepartureTime,ArrivalTime&$filter= ScheduleStartDate ge {0} and ScheduleStartDate le {3} and DepartureAirportID eq '{1}' and ArrivalAirportID eq '{2}' {4}&$format=JSON"
                    , time[0].ToString("yyyy-MM-dd"),place[0], place[1], /*(time[0].ToString("dddd",new CultureInfo("en-US")) + " eq true")*/time[1].ToString("yyyy-MM-dd"), (FlightNumber == null ? "" : ("and FlightNumber eq '" + FlightNumber.Replace("-","").Replace("_","").Trim() + "'")));
            json = module.getAPIdata(url).Replace("[", "").Replace("]", "");

            try //分解json
            {
                    string[] flights = json.Replace(",{", "`{").Split("`");
                    List<FixFlight> Flight = new();
                    for(int j =0; j<flights.Length; j++)
                    {
                        FixFlight FT = JsonSerializer.Deserialize<FixFlight>(flights[j]);//將json轉換成物件
                        DateTime SchEnd = new DateTime(Math.Min(Convert.ToDateTime(FT.ScheduleStartDate).Ticks,time[1].Ticks));//防止超過篩選區間
                        int dayCount = (Convert.ToDateTime(FT.ScheduleEndDate) - SchEnd).Days+1;//每個班機排程區間天數
                        //每個排程中的班機
                        for (int x=0;x<dayCount;x++)
                        {
                            //將所有找到的相關班機資訊放入List
                            FixFlight FF = JsonSerializer.Deserialize<FixFlight>(flights[j]);
                            FF.FlightNumber = FF.FlightNumber.Insert(2, "-");//班機編號
                            FF.AirlineName = getAirlineName(FF.AirlineID);//航空公司中文名稱

                            //飛行時間計算
                            if (TimeSpan.Parse(FF.ArrivalTime.Replace("+1", "")) < TimeSpan.Parse(FF.DepartureTime))
                            {
                                FF.FlightTime = "" + (TimeSpan.Parse("23:59")+TimeSpan.Parse("00:01") - TimeSpan.Parse(FF.DepartureTime) + TimeSpan.Parse(FF.ArrivalTime.Replace("+1", "")));
                            }
                            else
                            {
                                FF.FlightTime = "" + (TimeSpan.Parse(FF.ArrivalTime.Replace("+1", "")) - TimeSpan.Parse(FF.DepartureTime));
                            }
                            FF.FlightTime = FF.FlightTime.Substring(0, 5).Replace(":", "小時") + "分鐘";//飛行時間

                            //將一筆航班資料分為多張機票資料
                            string wday = Convert.ToDateTime(FF.ScheduleStartDate).AddDays(x).ToString("dddd", new CultureInfo("en-US"));

                            if (wday == "Monday" && !FF.Monday) continue;
                            if (wday == "Tuesday" && !FF.Tuesday) continue;
                            if (wday == "Wednesday" && !FF.Wednesday) continue;
                            if (wday == "Thursday" && !FF.Thursday) continue;
                            if (wday == "Friday" && !FF.Friday) continue;
                            if (wday == "Saturday" && !FF.Saturday) continue;
                            if (wday == "Sunday" && !FF.Sunday) continue;
                            FF.FlightDate = Convert.ToDateTime(FF.ScheduleStartDate).AddDays(x);//將出發日期加入[回程日期會在cshtml端的程式加入]
                            //Debug.Print("Flight Date = "+FF.FlightDate);
                            QF.Depart.Add(FF);//將資料存放到class中==>去程
                        }                  
                    }                   
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);                   
                }
            
            #endregion


            #region 取得航空公司名稱
            string getAirlineName(string AirlineID)
            {                
                string AirlineQuery = string.Format("https://ptx.transportdata.tw/MOTC/v2/Air/Airline?$select=AirlineName&$filter=AirlineID eq '{0}'&$format=JSON", AirlineID);
                string AirlineJson = module.getAPIdata(AirlineQuery).Replace("[{\"AirlineName\":", "").Replace("},", ",").Replace("]", "");
                AirlineInfo AI = JsonSerializer.Deserialize<AirlineInfo>(AirlineJson);
                //Debug.Print("機場名稱：" + AI.Zh_tw);
                return AI.Zh_tw;
            }
            #endregion
            //Debug.Print(place[0] + " --> " + place[1]);
            return QF;
        }





        //機場資訊
        public class AirportInfo
        {
            public string AirportID { get; set; }           
        }

        //航空公司資訊
        public class AirlineInfo
        {
            public string Zh_tw { get; set; }
        }

    }
}
