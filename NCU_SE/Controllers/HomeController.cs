using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NCU_SE.Models;
using NCU_SE.Data;
using System.Linq;



//連線資料庫
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace NCU_SE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db; //使用資料庫實體

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }


        //private readonly IConfiguration configuration;

        //public HomeController(IConfiguration config)
        //{
        //    this.configuration = config;
        //}

        //這邊控制了navbar點什麼會顯示什麼頁面
        //return View 要看的是上面的名稱 他回去找Home資料夾有沒有相對應的頁面(VIEW)
        //少邦的影片
        public IActionResult Index()
        {
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
            ViewData["login"] = "登入/註冊";
            return View();
        }


        public IActionResult Login(Member obj)

        {
            
            // 測試是否有抓到值
            ViewData["Exist"] = obj.Email;
            ViewData["login"] = "登入/註冊";
            return View();

        }
        public IActionResult Verify(Member obj)
        {
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

        }

        public IActionResult Realtime() 
        {
            ViewData["login"] = "登入/註冊";
            return View();
        }

        public ActionResult Logout() 

        {
            ViewData["login"] = "登入/註冊";
            return Redirect("Register");
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
