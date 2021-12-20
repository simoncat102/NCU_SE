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

        public IActionResult PersonalInfo()
        {
            if (!LoginStat())
            {
                return RedirectToAction("Login", "Home");
            }//若未登入轉跳到登入畫面
            else
            {
                ViewData["log_action"] = Login_Var.login_action;
                //反正能進來就是以登入 我要登出才會按！
                ViewData["login"] = Login_Var.login_status;
                ViewData["logid"] = Login_Var.login_uid;
                ViewData["log_name"] = Login_Var.login_name;
                ViewData["log_email"] = Login_Var.login_email;
                ViewData["log_birthday"] = Login_Var.login_birthday;
                ViewData["log_profile"] = Login_Var.login_profile;
                ViewData["log_age"] = Login_Var.login_age;
            }

            return View();
        }

        public IActionResult EditPersonalInfo(int id)
        {
            var memberdetail = _db.Member.Find(id);

            return View("PersonalInfo");
        }

        public IActionResult UserTicket()
        {
            if (!LoginStat()) return RedirectToAction("Login", "Home");//若未登入轉跳到登入畫面
            ViewData["login"] = Login_Var.login_status;
            ViewData["logid"] = Login_Var.login_uid;
            ViewData["log_name"] = Login_Var.login_name;
            ViewData["log_email"] = Login_Var.login_email;
            ViewData["log_profile"] = Login_Var.login_profile;
            //讀取資料語法
            IEnumerable<Flight> objList = _db.Flight;
            return View(objList);
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
        private IHttpContextAccessor session;

        public UserController(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            session = httpContextAccessor;
        }
        #endregion

    }
}
