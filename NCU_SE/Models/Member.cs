using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NCU_SE.Models
{
    public class Member
    {
        [Key]
        public int ID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Birthday { get; set; }
        public string phone { get; set; }
        public int profile { get; set; }
        public string ValidationLink { get; set; }
        public string RegisterDate { get; set; }
    }
}
