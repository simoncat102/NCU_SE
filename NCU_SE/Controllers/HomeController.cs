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


        //只能有一個建構子
        //private readonly ILogger<HomeController> _logger;
        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        //這邊控制了navbar點什麼會顯示什麼頁面
        //return View 要看的是上面的名稱 他回去找Home資料夾有沒有相對應的頁面(VIEW)
        public IActionResult Index()
        {
            //測試有沒有連到
            //string connectionstring = configuration.GetConnectionString("DefaultConnection");

            //找到SQLCONNECTION，新增了NuGet套件
            SqlConnection connection = new SqlConnection(connectionstring);
            connection.Open();
            SqlCommand com = new SqlCommand("Select count(*) from Member", connection);
            var count = (int)com.ExecuteScalar();

            ViewData["TotalData"] = count; //Member資料表的資料數量

            connection.Close();

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

        public ActionResult Logout() 

        {
            return Redirect("Register");
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
