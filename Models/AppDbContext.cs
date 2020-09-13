using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroundHouse.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        public DbSet<House> Houses { get; set; }//this prop can be used to query and save House instances

        //by overriding below method we can seed some initial data
        protected override void OnModelCreating(ModelBuilder modelBuilder)//initializes only once
        {
            modelBuilder.Seed();//this is an extension method for clean code
        }
    }
}
