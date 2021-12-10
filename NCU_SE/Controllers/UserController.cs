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
        private readonly ApplicationDbContext _db; //使用資料庫實體
        /*
        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }
        */
        //injector
        
        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }
        

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Register(Member obj)
        {
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
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
