using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
/// <summary>
/// 會員相關資料模型
/// </summary>
namespace NCU_SE.Models
{
    public class Member
    {
        [Key]
        public int ID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public string phone { get; set; }
        public int ValidState { get; set; }
        public int profile { get; set; }
        public string ValidationLink { get; set; }
        public string PasswordResetLink { get; set; }
        public DateTime RegisterDate { get; set; }
    }
}
