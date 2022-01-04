using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using NCU_SE.Data;
using NCU_SE.Models;
// <summary>
// 使用者相關控制器
// </summary>
namespace NCU_SE.Controllers
{
    public class UserController : Controller
    {
        //private readonly ILogger<UserController> _logger;
        /*
        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }
        */
        //injector

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Register(Member obj)
        {
            //驗證Email是否已被註冊-->SQL語法轉換 = SELECT count(Email) from Member where Email ='[使用者填寫的Email]'
            int accountExist = obj.Email == null ? 0: _db.Member.Where(u => u.Email == obj.Email.ToString()).Count();
            Debug.Print("帳戶存在數量"+accountExist);
            if(accountExist == 0 && obj.Email!=null)
            {
                obj.profile = 1;
                obj.RegisterDate = System.DateTime.Today;//將註冊日期加入
                _db.Member.Add(obj);//相當於SQL的insert語法
                _db.SaveChanges();//儲存資料庫變更
                return RedirectToAction("Login", "Home");//註冊成功時將導回登入頁面
            }
            else if(obj.Email!=null)//需檢驗Email欄位是否為空-->第一次進入是空的
            {
                ModelState.AddModelError(nameof(Member.Email), "此帳號已被註冊");//將錯誤訊息附加到欄位上               
            }
            return View();           

        }

        public IActionResult PersonalInfo(Member obj)
        {
            Debug.Print(obj.Birthday.ToString());
            //Debug.Print(Login_Var.login_uid.ToString() + "  " + getSession(Login_Var.login_uid.ToString()));

            if (!LoginStat())
            {
                return RedirectToAction("Login", "Home");
            }//若未登入轉跳到登入畫面
            else
            {
                ViewData["log_action"] = getSession("login_action");
                //反正能進來就是以登入 我要登出才會按！
                ViewData["login"] = getSession("login_status");
                ViewData["logid"] = getSession("acc");
                ViewData["log_name"] = getSession("login_name");
                ViewData["log_email"] = getSession("login_email");
                ViewData["log_birthday"] = getSession("login_birthday");
                ViewData["log_profile"] = getSession("login_profile");
                ViewData["log_age"] = getSession("login_age");
            }
            return View();
        }

        public IActionResult EditPersonalInfo(Member obj)
        {
            if (ModelState.IsValid)
            {
                _db.Member.Attach(obj);
                //_db.Entry(obj).Property(u => u.profile).IsModified = true;
                _db.Entry(obj).Property(u => u.Name).IsModified = (obj.Name != null);
                _db.Entry(obj).Property(u => u.Email).IsModified = (obj.Email != null);
                _db.Entry(obj).Property(u => u.Birthday).IsModified = (obj.Birthday.ToString("yyyy/MM/dd") != "0001/1/1");
                _db.SaveChanges();

                //重設全域變數
                ViewData["log_action"] = getSession("login_action");
                ViewData["login"] = obj.Name + "，您好 按此登出";
                ViewData["logid"] = getSession("acc");
                ViewData["log_name"] = obj.Name;
                ViewData["log_email"] = obj.Email;
                setSession("login_status", obj.Name + "，您好 按此登出");
                setSession("login_name",obj.Name);
                setSession("login_email", obj.Name + "，您好 按此登出");
                if (obj.Birthday.ToString("yyyy/MM/dd") != "0001/1/1")  //篩選是否有值存在，若不存在時將以"0001/1/1"表示
                {
                    ViewData["log_birthday"] = obj.Birthday.ToString("yyyy-MM-dd");
                    setSession("login_birthday", obj.Birthday.ToString("yyyy-MM-dd"));
                    //計算年齡
                    int birth = int.Parse(obj.Birthday.ToString("yyyyMMdd"));
                    int now = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
                    int age = (now - birth) / 10000;
                    ViewData["log_age"] = age;
                    setSession("login_age",age.ToString());
                }
                ViewData["log_profile"] = getSession("login_profile");
                return View("PersonalInfo");
            }
            return View("PersonalInfo");
        }

        public IActionResult EditPersonalPicture(Member obj)
        {
            if (ModelState.IsValid)
            {
                //使用者選的頭像代號傳到資料庫
                _db.Member.Attach(obj);
                _db.Entry(obj).Property(u => u.profile).IsModified = true;
                _db.SaveChanges();

                //重設全域變數
                ViewData["log_action"] = getSession("login_action");
                ViewData["login"] = getSession("login_status");
                ViewData["logid"] = getSession("acc");
                ViewData["log_name"] = getSession("login_name");
                ViewData["log_email"] = getSession("login_email");
                ViewData["log_profile"] = "/img/img" + obj.profile + ".png";
                setSession("login_profile", "/img/img" + obj.profile + ".png");
                ViewData["log_birthday"] = getSession("login_birthday");
                DateTime birthday = Convert.ToDateTime(getSession("login_birthday"));
                //計算年齡
                int birth = int.Parse(birthday.ToString("yyyyMMdd"));
                int now = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
                int age = (now - birth) / 10000;
                ViewData["log_age"] = age;
                setSession("login_age", age.ToString());
                return View("PersonalInfo");
            }
            return View("PersonalInfo");
        }

        public IActionResult UserTicket()
        {
            if (!LoginStat()) return RedirectToAction("Login", "Home");//若未登入轉跳到登入畫面
            ViewData["log_action"] = getSession("login_action");
            ViewData["login"] = getSession("login_status");
            ViewData["logid"] = getSession("acc");
            ViewData["log_name"] = getSession("login_name");
            ViewData["log_email"] = getSession("login_email");
            ViewData["log_profile"] = getSession("login_profile");
            //讀取機票資料語法
            //IEnumerable<Flight> objList = _db.Flight;
            //下面註解的部分是篩選機票必須是還沒起飛的
            IEnumerable<Flight> ticket = _db.Flight.Where(u=>u.MemberID == int.Parse(getSession("acc")) /*&& u.DepTime>=DateTime.Today*/).Select(u => new Flight {FlightID = u.FlightID, FlightCode = u.FlightCode, Airline = u.Airline, AirportFrom = u.AirportFrom, AirportTo = u.AirportTo, DepTime = u.DepTime, ArriTime = u.ArriTime, FlightNote = u.FlightNote }).ToList();
            ViewBag.ticket = ticket;
            return View();
        }

        public IActionResult AlterTicket(Flight ticket)
        {
            _db.Flight.Attach(ticket);
            if(ticket.FlightNote == "!del!")
            {
                _db.Flight.Remove(ticket);
            }
            else
            {
                _db.Entry(ticket).Property(u => u.FlightNote).IsModified = (ticket.FlightNote != null);
            }
            _db.SaveChanges();
            return RedirectToAction("UserTicket");
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

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }
        

    }
}
