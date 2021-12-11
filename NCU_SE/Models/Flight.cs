using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
namespace NCU_SE.Models
{
    public class Flight
    {
        [Key]
        //設定這個資料表中有什麼資料
        public int FlightID { get; set; }
        //航班主鍵
        public int FlightCode { get; set; }
        //航班編號
        public int MemberID { get; set; }
        //使用者編號
        public string Airline { get; set; }
        //航空公司
        public string CityTo { get; set; }
        //出發城市
        public string CityFrom { get; set; }
        //抵達城市
        public string AirportTo { get; set; }
        //出發機場
        public string AirportFrom { get; set; }
        //抵達機場
        public DateTime DepDate { get; set; }
        //出發日期
        public DateTime DepTime { get; set; }
        //出發時間
        public DateTime ArriDate { get; set; }
        //抵達日期
        public DateTime ArriTime { get; set; }
        //抵達時間
        public string FlightNote { get; set; }
        //航班備註
        public string Status { get; set; }
        //航班狀態

    }
}
