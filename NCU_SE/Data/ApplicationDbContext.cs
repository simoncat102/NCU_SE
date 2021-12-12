using Microsoft.EntityFrameworkCore;
using NCU_SE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCU_SE.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Member> Member { get; set; }
        public DbSet<Ticket> Ticket { get; set; }

        public DbSet<test> test { get; set; }
    }
    
}
