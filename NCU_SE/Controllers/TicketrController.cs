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
        /*
        private readonly ILogger<TicketController> _logger;

        public TicketController(ILogger<TicketController> logger)
        {
            _logger = logger;
        }
        */
        
        public IActionResult FixedFlight(FixedFlight obj)
        {
            QueryFlight QF = new();
                //取得固定航班資料
                if (obj != null) QF = getFixedFlight(obj.Origin, obj.Destination, obj.DepartureDate, obj.ReturnDate, obj.FlightNumber);
                ViewBag.Depart = QF.Depart;//將去程資料放入viewbag中
            //如果已登入，自動載入儲存的航班-->避免重複儲存航班用
            Debug.Print(Login_Var.login_uid+"");
            ViewData["uid"] = Login_Var.login_uid;
            if (LoginStat())
            {
                var flight_saved = _db.Flight.Where(u => u.MemberID == Login_Var.login_uid && u.DepTime >= DateTime.Today).Select(u => new { u.FlightCode, u.DepTime }).ToList();
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
            ViewData["login"] = Login_Var.login_status;
            ViewData["log_action"] = Login_Var.login_action;
            ViewData["log_uid"] = Login_Var.login_uid;

            Login_Var.LastQuery = Request.QueryString;
            
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
            obj.MemberID = Login_Var.login_uid;//會員ID
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

        #region 登入狀態/取得session模組/共用模組
        //檢測登入狀態
        public bool LoginStat()
        {
            try//檢測session 'acc'是否存在，若存在且不為空則表示已經登入
            {
                if (session.HttpContext.Session.GetString("acc") != null)//若已登入
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
                result = session.HttpContext.Session.GetString(name);
            }
            catch { }
            return result;
        }

        //通用模組
        private readonly ApplicationDbContext _db; //使用資料庫實體
        private readonly IHttpContextAccessor session;

        public TicketController(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            session = httpContextAccessor;
        }
        #endregion
        //取得航班
        public QueryFlight getFixedFlight(string origin, string destnation, DateTime departureDate, DateTime returnDate, string FlightNumber)
        {
            string json = null;
            #region API KEY
            //申請的APPID
            //（FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF 為 Guest 帳號，以IP作為API呼叫限制，請替換為註冊的APPID & APPKey）
            const string APPID = "8a8ffddac5af4e42a3fb1ed014472ece";
            //申請的APPKey
            const string APPKey = "cRraTKB4MRKUsfMseiq0UxislXA";

            //取得當下UTC時間
            string xdate = DateTime.Now.ToUniversalTime().ToString("r");
            string SignDate = "x-date: " + xdate;

            //加密簽章產生
            Encoding _encode = Encoding.GetEncoding("utf-8");
            byte[] _byteData = Encoding.GetEncoding("utf-8").GetBytes(SignDate);
            HMACSHA1 _hmac = new(_encode.GetBytes(APPKey));
            using (CryptoStream _cs = new(Stream.Null, _hmac, CryptoStreamMode.Write))
            {
                _cs.Write(_byteData, 0, _byteData.Length);
            }
            //取得加密簽章
            string Signature = Convert.ToBase64String(_hmac.Hash);
            string sAuth = "hmac username=\"" + APPID + "\", algorithm=\"hmac-sha1\", headers=\"x-date\", signature=\"" + Signature + "\"";
            #endregion
            
            string[] place = { origin, destnation };
            DateTime[] time = { departureDate, returnDate };
            QueryFlight QF = new();
            QF.Depart = new List<FixFlight>();          
            QF.Origin = origin;
            QF.Destination = destnation;
            #region 取得機場代號           
            for (int i =0; i<2; i++)
            {
                string AirportQuery = string.Format("https://ptx.transportdata.tw/MOTC/v2/Air/Airport?$select=AirportID&$filter=AirportID ne '' and (AirportID eq '{0}' or AirportName/Zh_tw eq '{0}' or AirportName/En eq '{0}' or AirportCityName/Zh_tw eq '{0}' or AirportCityName/En eq '{0}')&$top=2&$format=JSON", place[i]);
                json = null;
                //取得API資料(官方提供方法)
                using (HttpClient Client = new(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip }))
                {
                    Client.DefaultRequestHeaders.Add("Authorization", sAuth);
                    Client.DefaultRequestHeaders.Add("x-date", xdate);
                    json = Client.GetStringAsync(AirportQuery).Result.Replace("[", "").Replace("]", "");                
                }
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
                string url = string.Format("https://ptx.transportdata.tw/MOTC/v2/Air/GeneralSchedule/International? $select=AirlineID,FlightNumber,DepartureTime,ArrivalTime&$filter= ScheduleStartDate ge {0} and ScheduleEndDate le {3} and DepartureAirportID eq '{1}' and ArrivalAirportID eq '{2}' {4}&$format=JSON"
                    , time[0].ToString("yyyy-MM-dd"),place[0], place[1], /*(time[0].ToString("dddd",new CultureInfo("en-US")) + " eq true")*/time[1].ToString("yyyy-MM-dd"), (FlightNumber == null ? "" : ("and FlightNumber eq'" + FlightNumber + "'")));
                //取得API資料(官方提供方法)
                using (HttpClient Client = new(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip }))
                {
                    Client.DefaultRequestHeaders.Add("Authorization", sAuth);
                    Client.DefaultRequestHeaders.Add("x-date", xdate);
                    json = Client.GetStringAsync(url).Result.Replace("[", "").Replace("]", "");
                }
                try //分解json
                {
                    string[] flights = json.Replace(",{", "`{").Split("`");
                    List<FixFlight> Flight = new();
                    for(int j =0; j<flights.Length; j++)
                    {
                        FixFlight FT = JsonSerializer.Deserialize<FixFlight>(flights[j]);
                        int dayCount = (Convert.ToDateTime(FT.ScheduleEndDate) - Convert.ToDateTime(FT.ScheduleStartDate)).Days+1;
                        
                        for (int x=0;x<dayCount;x++)
                        {
                            //將所有找到的相關班機資訊放入List
                            FixFlight FF = JsonSerializer.Deserialize<FixFlight>(flights[j]);
                            FF.FlightNumber = FF.FlightNumber.Insert(2, "-");//班機編號
                            FF.AirlineName = getAirlineName(FF.AirlineID);//航空公司中文名稱

                            //飛行時間計算
                            if (TimeSpan.Parse(FF.ArrivalTime.Replace("+1", "")) < TimeSpan.Parse(FF.DepartureTime))
                            {
                                FF.FlightTime = "" + (TimeSpan.Parse("23:59") - TimeSpan.Parse(FF.DepartureTime) + TimeSpan.Parse(FF.ArrivalTime.Replace("+1", "")));
                            }
                            else
                            {
                                FF.FlightTime = "" + (TimeSpan.Parse(FF.ArrivalTime.Replace("+1", "")) - TimeSpan.Parse(FF.DepartureTime));
                            }
                            FF.FlightTime = FF.FlightTime.Substring(0, 5).Replace(":", "小時") + "分鐘";//飛行時間
                            //將一筆航班資料分為多張航班資料
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
                string AirlineJson = null;
                using (HttpClient Client = new(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip }))
                {
                    Client.DefaultRequestHeaders.Add("Authorization", sAuth);
                    Client.DefaultRequestHeaders.Add("x-date", xdate);
                    AirlineJson = Client.GetStringAsync(AirlineQuery).Result.Replace("[{\"AirlineName\":", "").Replace("},",",").Replace("]", "");
                }
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
