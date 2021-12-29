using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NCU_SE.Models
{
    public class Ticket
    {
        [Key]
        public string TicketID { get; set; } //航班號碼
        public int MemberID { get; set; } //會員編號
        public string FlightID { get; set; } //航班編號
        public DateTime DepartureDateTime { get; set; } //預計起飛時間
        public DateTime ArriveDateTime { get; set; } //預計降落時間
        public string DepartAirport { get; set; } //起飛機場
        public string DestinationAirport { get; set; } //目的地機場
        public DateTime ActualDepartureDateTime { get; set; } //實際起飛時間
        public DateTime ActualArrivedTime { get; set; } //實際降落時間
        public string Note { get; set; }//航班備註
    }
}
