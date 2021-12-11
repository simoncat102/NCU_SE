using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        private readonly ApplicationDbContext _db; //使用資料庫實體
        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }
        

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Register(Member obj)
        {
            //新增資料語法
            _db.Member.Add(obj);
            _db.SaveChanges();
            return View();
        }

        public IActionResult PersonalInfo()
        {
            return View();
        }

        public IActionResult UserTicket()
        {
            //讀取資料語法
            IEnumerable<Flight> objList = _db.Flight;
            return View(objList);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
