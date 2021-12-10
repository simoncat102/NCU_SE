using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NCU_SE.Models;

//連線資料庫
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace NCU_SE.Controllers
{
    public class HomeController : Controller
    {
        /*
        private readonly IConfiguration configuration;
        
        public HomeController(IConfiguration config)
        {
            this.configuration = config;
        }
        */
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        //這邊控制了navbar點什麼會顯示什麼頁面
        //return View 要看的是上面的名稱 他回去找Home資料夾有沒有相對應的頁面(VIEW)
        public IActionResult Index()
        {
            //測試有沒有連到
            //string connectionstring = configuration.GetConnectionString("DefaultConnection");

            //這邊我弄到一半 他找不到SQLCONNECTION
            //SqlConnection connection = new SqlConnection(connectionstring);
            


            return View();
        }
        
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Realtime() 
        {
            return View();
        }
        


        //public IActionResult Register() 
        //{
        //    return View();
        //}
        
        public ActionResult Logout() //侑霖打的
        {
            //Session.Abandon();
            return Redirect("Register");
            //return RedirectToAction("Index", "Login");
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
