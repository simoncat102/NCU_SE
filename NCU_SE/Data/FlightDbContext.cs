using Microsoft.EntityFrameworkCore;
using NCU_SE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace NCU_SE.Data

{
    public class FlightDbContext : DbContext
    {
        public FlightDbContext(DbContextOptions<FlightDbContext> options) :base(options)
        {

        }

        public DbSet<Flight> Flight { get; set; }
    }
}
