using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCU_SE.Models
{
    public class FixedFlight //與Ticket.cs必須對應，因為一個View使用多個model必須所有model都是資料庫的資料表，但FixedFlight並不是
    {
        public string Origin { get; set; } //出發地(起飛機場) = DepartAirport
        public string Destination { get; set; } //目的地(機場) = DestinationAirport
        public DateTime DepartureDate { get; set; }//出發日期時間 = DepartureDateTime ==>在此表代表出發日期
        public DateTime ReturnDate { get; set; }//預計降落時間 = ArriveDateTime ==>在此表代表的是回程日期
        public string FlightNumber { get; set; }//航班編號 = FlightID
        public string Note { get; set; }//機票備註 = Note ==>此表並不使用
    }
}
