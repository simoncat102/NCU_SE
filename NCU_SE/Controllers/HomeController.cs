using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NCU_SE.Models;
using NCU_SE.Data;
using System.Linq;
using Microsoft.AspNetCore.Session;
using System.Text.Json;



//連線資料庫
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;

namespace NCU_SE.Controllers
{
    public static class Login_Var
    {
        public static string login_status { get; set; } = "登入/註冊";
        public static string login_action { get; set; } = "Login";
        public static int login_uid { get; set; } = 0;
        public static string login_name { get; set; } = "無";
        public static string login_email { get; set; } = "無";
        public static string login_birthday { get; set; } = "無";
        public static string login_profile { get; set; } = "無";
        public static int login_age { get; set; } = 0;
    }

    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db; //使用資料庫實體
        private IHttpContextAccessor session;

        public HomeController(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            session = httpContextAccessor;           
        }

        #region 註解
        //private readonly IConfiguration configuration;

        //public HomeController(IConfiguration config)
        //{
        //    this.configuration = config;
        //}

        //這邊控制了navbar點什麼會顯示什麼頁面
        //return View 要看的是上面的名稱 他回去找Home資料夾有沒有相對應的頁面(VIEW)
        //少邦的影片
        #endregion
        public IActionResult Index()
        {
            #region 原先連接DB的方式
            //跟_db衝突了 暫時用不到我先註解掉 但留著參考
            ////測試有沒有連到
            //string connectionstring = configuration.GetConnectionString("DefaultConnection");

            ////找到SQLCONNECTION，新增了NuGet套件
            //SqlConnection connection = new SqlConnection(connectionstring);
            //connection.Open();
            //SqlCommand com = new SqlCommand("Select count(*) from Member", connection);
            //var count = (int)com.ExecuteScalar();

            //ViewData["TotalData"] = count; //Member資料表的資料數量

            //connection.Close();
            #endregion

            ViewData["login"] = Login_Var.login_status;
            ViewData["log_action"] = Login_Var.login_action;
            ViewData["log_uid"] = Login_Var.login_uid;
            ViewData["log_name"] = Login_Var.login_name;
            ViewData["log_email"] = Login_Var.login_email;
            ViewData["log_birthday"] = Login_Var.login_birthday;
            ViewData["log_profile"] = Login_Var.login_profile;
            ViewData["log_age"] = Login_Var.login_age;
            return View();
        }


        public IActionResult Login(Member obj)

        {
            ViewData["log_action"] = Login_Var.login_action = "Login";//預設還沒登入的按鈕動作
            if (LoginStat())
            {
                return View("Index");
            }
            //以將Login.cshtml的form action改為Login
            //登入判定:1=成功 0=失敗
            int AccExist = (obj.Email == null && obj.Password == null) ? -1 : _db.Member.Where(u => u.Email == (obj.Email.ToString()) && u.Password == obj.Password.ToString()).Count();
            if (AccExist == 1)
            {
                session.HttpContext.Session.SetString("acc", obj.Email.ToString());//登入成功時加入session
                //取得會員ID
                int uid = _db.Member.Where(u => u.Email == obj.Email.ToString()).Select(u => u.ID).First();
                session.HttpContext.Session.SetString("uid", uid.ToString());//將會員ID寫入session
                //取得會員姓名
                string name = _db.Member.Where(u => u.Email == obj.Email.ToString()).Select(u => u.Name).First();
                session.HttpContext.Session.SetString("uname", name.ToString());//將會員姓名寫入session
                string email = _db.Member.Where(u => u.Email == obj.Email.ToString()).Select(u => u.Email).First();
                DateTime birthday = _db.Member.Where(u => u.Email == obj.Email.ToString()).Select(u => u.Birthday).First();
                int profile = _db.Member.Where(u => u.Email == obj.Email.ToString()).Select(u => u.profile).First();
                ViewData["logid"] = Login_Var.login_uid = uid;//將會員ID放入全域變數+??顯示-->顯示在哪?
                ViewData["login"] = Login_Var.login_status = getSession("uname") + "，您好 按此登出";//將會員姓名(歡迎訊息)放入全域變數+右上角顯示的歡迎訊息(兼登出按鈕)                
                ViewData["log_action"] = Login_Var.login_action = "Logout";//設定"登入/登出"按鈕動作
                ViewData["log_name"] = Login_Var.login_name = getSession("uname");
                ViewData["log_email"] = Login_Var.login_email = email;
                ViewData["log_birthday"] = Login_Var.login_birthday = birthday.ToString("yyyy-MM-dd");//原本是("MM/dd/yyyy")
                ViewData["log_profile"] = Login_Var.login_profile = "/img/img" + profile.ToString() + ".png";//~/img/img1.png
                //計算年齡
                int birth = int.Parse(birthday.ToString("yyyyMMdd"));
                int now = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
                int age = (now - birth) / 10000;
                ViewData["log_age"] = Login_Var.login_age = age;
                return View("Index");//登入成功時跳轉到首頁
            }
            else if (AccExist == 0)
            {
                ModelState.AddModelError(nameof(Member.Email), "帳號或密碼錯誤，請重新輸入");//將錯誤訊息附加到欄位上           
            }
            ViewData["login"] = Login_Var.login_status = "登入/註冊";//右上角顯示的"登入/註冊"按鈕
            ViewData["logid"] = Login_Var.login_uid = -1;//在??顯示會員ID-->有需要顯示嗎?
            ViewData["log_name"] = Login_Var.login_name = "無";
            ViewData["log_email"] = Login_Var.login_email = "無";
            ViewData["log_birthday"] = Login_Var.login_birthday = "無";
            ViewData["log_profile"] = Login_Var.login_profile = "無";
            ViewData["log_age"] = Login_Var.login_age = 0;
            return View();
        }
        public IActionResult Verify(Member obj)
        {
            #region 
            /*少邦
            var SearchEmail = _db.Member.Where(x => x.Email.Equals(obj.Email.ToString()));
            var SearchPW = _db.Member.Where(x => x.Password.Equals(obj.Password.ToString()));
            
            if (SearchEmail != null) //email存在
            {
                if (SearchPW != null)
                {
                    @ViewData["login"] = "歡迎登入 " + obj.Email;
                    return View("Index");
                    
                }


            }
            return View("Login");
            */

            //冠廷
            /*
            try//檢測session 'acc'是否存在，若存在且不為空則表示已經登入
            {
                if(session.HttpContext.Session.GetString("acc") != null)//若已登入
                {
                    return View("Index");//跳到首頁
                    Login_Var.login_status = obj.Email;
                }
            }
            catch
            {
                Debug.Print("Session不存在!");
            }
            */
            #endregion
            return View("Login");
        }

        public IActionResult Realtime() 
        {
            ViewData["log_action"] = Login_Var.login_action;
            ViewData["login"] = Login_Var.login_status;
            ViewData["logid"] = Login_Var.login_uid;
            ViewData["log_name"] = Login_Var.login_name;
            ViewData["log_email"] = Login_Var.login_email;
            ViewData["log_birthday"] = Login_Var.login_birthday;
           
            ViewBag.AllFlight = getRealtimeFlight(); //Viewbag存資料
            return View();
        }

        public ActionResult Logout() 

        {
            #region 原本的內容
            /*原本的內容
            ViewData["login"] = "登入/註冊";
            return Redirect("Register");
            */
            #endregion
            ViewData["login"] = Login_Var.login_status = "登入/註冊";
            ViewData["log_action"] = Login_Var.login_action="Login";
            Login_Var.login_action = "Login";
            try
            {
                session.HttpContext.Session.Remove("acc");
            }
            catch{}          
            return View("Login");           
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

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
            catch{}
            return result;
        }

        //取得即時航班
        public List<Flight> getRealtimeFlight()
        {
            //取得即時航班API網址(僅限當日)
            const string url = "https://ptx.transportdata.tw/MOTC/v2/Air/FIDS/Flight/?$format=JSON&$select=AirlineID,FlightNumber,DepartureAirportID,ArrivalAirportID,DepartureRemark,ArrivalRemark&$filter=%20ArrivalRemark%20ne%20%27?%27%20and%20DepartureRemark%20ne%20%27?%27";
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
            HMACSHA1 _hmac = new HMACSHA1(_encode.GetBytes(APPKey));
            using (CryptoStream _cs = new CryptoStream(Stream.Null, _hmac, CryptoStreamMode.Write))
            {
                _cs.Write(_byteData, 0, _byteData.Length);
            }
            //取得加密簽章
            string Signature = Convert.ToBase64String(_hmac.Hash);
            string sAuth = "hmac username=\"" + APPID + "\", algorithm=\"hmac-sha1\", headers=\"x-date\", signature=\"" + Signature + "\"";
            #endregion

            //取得API資料(官方提供方法)
            string json = null;
            using (HttpClient Client = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip }))
            {
                Client.DefaultRequestHeaders.Add("Authorization", sAuth);
                Client.DefaultRequestHeaders.Add("x-date", xdate);
                json = Client.GetStringAsync(url).Result;
            }

            json = json.Replace("[", "").Replace("]", "").Replace(",{", "`{");//將json外面的陣列括號去除，並將分割多個json的逗號改為`方便切分
            string[] FlightList = json.Split('`');//將json集合分開

            //儲存即時航班資料的List
            List<Flight> flightlist = new List<Flight>();
            //解析每個json-->將解析結果放入List中
            //for (int i = 0; i < FlightList.Length; i++)
            for (int i = 0; i < 30; i++)
            {
                Flight flight = JsonSerializer.Deserialize<Flight>(FlightList[i]);

                    flightlist.Add(flight); //原本的code
                try
                {
                    flightlist[i].ActualArrivalTime = flightlist[i].ActualArrivalTime == null ? null : flightlist[i].ActualArrivalTime.Substring(flightlist[i].ActualArrivalTime.Length - 11,5)+" "+flightlist[i].ActualArrivalTime.Substring(flightlist[i].ActualArrivalTime.Length - 5);
                                                                                                                    //path.Substring(path.Length - 14, 10)    
                }
                catch { }

                Debug.Print(flightlist[i].FlightNumber + "\n");


            }

            //固定的ViewBag 測試用 用不到了 
            //string ArrivalTime = flightlist[1].ActualArrivalTime.Substring(flightlist[1].ActualArrivalTime.Length - 5);
            //ViewBag.Flight = new Flight()
            //{
            //    ActualArrivalTime = ArrivalTime,
            //    AirlineID = flightlist[1].AirlineID,
            //    FlightNumber = flightlist[1].AirlineID + flightlist[1].FlightNumber,
            //    DepartureAirportID = flightlist[1].DepartureAirportID,
            //    ArrivalAirportID = flightlist[1].ArrivalAirportID,
            //    ArrivalRemark = flightlist[1].ArrivalRemark
            //};

            return flightlist;
        }

        //即時航班格式
        public class Flight
        {
            public string FlightNumber { get; set; }
            public string AirlineID { get; set; }
            public string DepartureAirportID { get; set; }
            public string ArrivalAirportID { get; set; }
            public string ScheduleDepartureTime { get; set; }
            public string ActualDepartureTime { get; set; }
            public string ScheduleArrivalTime { get; set; }
            public string ActualArrivalTime { get; set; }
            public string DepartureRemark { get; set; } //出發狀態
            public string ArrivalRemark { get; set; } //抵達狀態

        }
    }
}
