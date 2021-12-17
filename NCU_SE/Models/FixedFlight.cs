using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCU_SE.Models
{
    public class FixedFlight
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public string FlightNumber { get; set; }
    }
}
