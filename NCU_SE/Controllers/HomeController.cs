//連線資料庫
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Mvc;

using NCU_SE.Data;
using NCU_SE.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using NCU_SE.SharedModule;

namespace NCU_SE.Controllers
{
    //快取航空公司名稱增加速度[所有使用者會共用，所以會越查閱快]
    public static class Airline
    {
        public static List<string> id { get; set; } = new();
        public static List<string> name { get; set; } = new();
    }

    public class HomeController : Controller
    {
        

        sharedModule module = new();
        private readonly ApplicationDbContext _db; //使用資料庫實體
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        //這邊控制了navbar點什麼會顯示什麼頁面
        //return View 要看的是上面的名稱 他回去找Home資料夾有沒有相對應的頁面(VIEW)
        public IActionResult Index(FixedFlight obj)
        {
            if (getSession("login_status") == null) setSession("login_status", "登入/註冊");
            if (getSession("login_action") == null) setSession("login_action", "Login");
            ViewData["login"] = getSession("login_status");
            ViewData["log_action"] = getSession("login_action");
            ViewData["log_uid"] = getSession("acc");
            ViewData["log_name"] = getSession("login_name");
            ViewData["log_email"] = getSession("login_email");
            ViewData["log_birthday"] = getSession("login_birthday");
            ViewData["log_profile"] = getSession("login_profile");
            ViewData["log_age"] = getSession("login_age");
            Debug.Print("Session ID = " + HttpContext.Session.Id);
            Debug.Print("initial uid = "+getSession("acc"));
            return View("Index");
        }

        public IActionResult Login(Member obj)
        {
            setSession("login_action", "Logout");
            ViewData["log_action"] = getSession("login_action");//預設還沒登入的按鈕動作
            if (LoginStat())
            {
                return View("Index");
            }
            //登入判定:1=成功 0=失敗
            int AccExist = (obj.Email == null && obj.Password == null) ? -1 : _db.Member.Where(u => u.Email == (obj.Email.ToString()) && u.Password == obj.Password.ToString()).Count();
            if (AccExist == 1)
            {
                //擷取必要的會員資料
                IEnumerable<Member> MemberInfo = _db.Member.Where(u => u.Email == (obj.Email.ToString()) && u.Password == obj.Password.ToString()).Select(u => new Member { ID = u.ID, Name = u.Name, profile = u.profile, Email = u.Email, Birthday = u.Birthday}); ;
                //取得會員ID
                ViewData["logid"] = MemberInfo.First().ID;
                setSession(("acc"), MemberInfo.First().ID.ToString());//將會員ID寫入session
                //取得會員姓名
                ViewData["login"] = MemberInfo.First().Name + "，您好 按此登出";//將會員姓名(歡迎訊息)放入全域變數+右上角顯示的歡迎訊息(兼登出按鈕)                
                ViewData["log_name"] = MemberInfo.First().Name;
                setSession("login_status", MemberInfo.First().Name + "，您好 按此登出");
                setSession("login_name",MemberInfo.First().Name);
                //取得會員Email
                ViewData["log_email"] = MemberInfo.First().Email;
                setSession("login_email", MemberInfo.First().Email);
                //取得會員生日
                ViewData["log_birthday"] = MemberInfo.First().Birthday.ToString("yyyy-MM-dd");
                setSession("login_birthday", MemberInfo.First().Birthday.ToString("yyyy-MM-dd"));
                //取得會員頭像
                ViewData["log_profile"] = "/img/img" + MemberInfo.First().profile + ".png";//~/img/img1.png                
                setSession("login_profile", "/img/img" + MemberInfo.First().profile + ".png");
                //計算年齡==>這是啥算法?
                int birth = int.Parse(MemberInfo.First().Birthday.ToString("yyyyMMdd"));
                int now = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
                int age = (now - birth) / 10000;
                ViewData["log_age"] = age;
                setSession("login_age", age.ToString());
                if (getSession("LastQuery") != null)
                {
                    return Redirect("../Ticket/FixedFlight" + getSession("LastQuery"));
                }
                else
                {
                    return View("Index");//登入成功時跳轉到首頁
                }
            }
            else if (AccExist == 0)
            {
                ModelState.AddModelError(nameof(Member.Email), "帳號或密碼錯誤，請重新輸入");//將錯誤訊息附加到欄位上           
            }
            ViewData["login"] = "登入/註冊";//右上角顯示的"登入/註冊"按鈕
            ViewData["logid"] = 0;//在??顯示會員ID-->有需要顯示嗎?
            ViewData["log_name"] = "無";
            ViewData["log_email"] = "無";
            ViewData["log_birthday"] = "無";
            ViewData["log_profile"] = "無";
            ViewData["log_age"] = 0;
            setSession("login_status","登入/註冊");
            setSession("acc","0");
            setSession("login_name","無");
            setSession("login_email","無");
            setSession("login_birthday","無");
            setSession("login_profile","無");
            setSession("login_age","0");
            return View();
        }

        public IActionResult Realtime(Flight obj)
        {
            ViewData["log_action"] = getSession("login_action");
            ViewData["login"] = getSession("login_status");
            ViewData["logid"] = getSession("acc");
            ViewData["log_name"] = getSession("login_name");
            ViewData["log_email"] = getSession("login_email");
            ViewData["log_birthday"] = getSession("login_birthday");

            ViewBag.AllFlight = getRealtimeFlight(); //Viewbag存資料
            return View();
        }

        public ActionResult Logout()

        {
            ViewData["login"] = "登入/註冊";
            ViewData["log_action"] = "Login";
            setSession("login_status", "登入/註冊");
            setSession("login_action", "Login");
            setSession("acc", "0");
            setSession("login_name", "無");
            setSession("login_email", "無");
            setSession("login_birthday", "無");
            setSession("login_profile", "無");
            setSession("login_age", "0");
            setSession("LastQuery", null);
            try
            {
                HttpContext.Session.Remove("acc");
            }
            catch { }
            return View("Login");
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
                if (getSession("acc") != null && getSession("acc")!="0")//若已登入
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
        //取得即時航班
        public List<Flight_data> getRealtimeFlight()
        {
            //取得即時航班API網址(僅限當日)
            const string url = "https://ptx.transportdata.tw/MOTC/v2/Air/FIDS/Flight/?$format=JSON&$select=AirlineID,FlightNumber,DepartureAirportID,ArrivalAirportID,DepartureRemark,ArrivalRemark&$filter=IsCargo eq false and%20ArrivalRemark%20ne%20%27?%27%20and%20DepartureRemark%20ne%20%27?%27";
            string json = module.getAPIdata(url);
            json = json.Replace("[", "").Replace("]", "").Replace(",{", "`{");//將json外面的陣列括號去除，並將分割多個json的逗號改為`方便切分
            string[] FlightList = json.Split('`');//將json集合分開

            //儲存即時航班資料的List
            List<Flight_data> flightlist = new();
            //解析每個json-->將解析結果放入List中
            for (int i = 0; i < FlightList.Length; i++)
            //for (int i = 0; i < 30; i++)
            {
                Flight_data flight = JsonSerializer.Deserialize<Flight_data>(FlightList[i]);
                flightlist.Add(flight); //原本的code
                try
                {
                    flightlist[i].AirlineID_zh = flightlist[i].AirlineID;
                    //flightlist[i].ActualArrivalTime = flightlist[i].ActualArrivalTime == null ? "時間未定" : flightlist[i].ActualArrivalTime.Substring(flightlist[i].ActualArrivalTime.Length - 11,5)+" "+flightlist[i].ActualArrivalTime.Substring(flightlist[i].ActualArrivalTime.Length - 5);
                    flightlist[i].ScheduleArrivalTime = flightlist[i].ScheduleArrivalTime == null ? "未確定" : flightlist[i].ScheduleArrivalTime.Length>=11? flightlist[i].ScheduleArrivalTime.Substring(flightlist[i].ScheduleArrivalTime.Length - 11, 5) + " " + flightlist[i].ScheduleArrivalTime.Substring(flightlist[i].ScheduleArrivalTime.Length - 5): flightlist[i].ScheduleArrivalTime;
                    flightlist[i].ArrivalRemark = flightlist[i].ArrivalRemark == null ? "未知" : flightlist[i].ArrivalRemark;
                    flightlist[i].AirlineID = flightlist[i].AirlineID == null ? "未知" : getAirlineName(flightlist[i].AirlineID);
                    flightlist[i].AirlineID = flightlist[i].AirlineID.Length > 4 ? flightlist[i].AirlineID.Substring(0, 4) : flightlist[i].AirlineID;
                }
                catch(Exception ex) 
                {
                    Debug.Print("data-error" + ex.Message);
                }

                //Debug.Print(flightlist[i].FlightNumber + "\n");
            }
            string getAirlineName(string AirlineID)
            {
                if (Airline.id.IndexOf(AirlineID) > -1) return Airline.name[Airline.id.IndexOf(AirlineID)];
                string AirlineQuery = string.Format("https://ptx.transportdata.tw/MOTC/v2/Air/Airline?$top=1&$select=AirlineName&$filter=AirlineID eq '{0}'&$format=JSON", AirlineID);
                string AirlineJson = module.getAPIdata(AirlineQuery);
                AirlineJson = AirlineJson.Replace("[{\"AirlineName\":", "").Replace("},", ",").Replace("]", "");

                AirlineInfo AI = JsonSerializer.Deserialize<AirlineInfo>(AirlineJson);
                //Debug.Print("機場名稱：" + AI.Zh_tw);
                Airline.name.Add(AI.Zh_tw);
                Airline.id.Add(AirlineID);
                return AI.Zh_tw;
            }
            return flightlist;
        }

        //即時航班格式
        public class Flight_data
        {
            public string FlightNumber { get; set; }
            public string AirlineID { get; set; }
            public string AirlineID_zh { get; set; }
            public string DepartureAirportID { get; set; }
            public string ArrivalAirportID { get; set; }
            public string ScheduleDepartureTime { get; set; }
            public string ActualDepartureTime { get; set; }
            public string ScheduleArrivalTime { get; set; }
            public string ActualArrivalTime { get; set; }
            public string DepartureRemark { get; set; } //出發狀態
            public string ArrivalRemark { get; set; } //抵達狀態

        }
        public class AirlineInfo
        {
            public string Zh_tw { get; set; }
        }
    }
}
