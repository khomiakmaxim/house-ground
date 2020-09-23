using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroundHouse.Models
{
    //below <ApplicationUser> is for custom user model with additional fields
    public class AppDbContext : IdentityDbContext<ApplicationUser>//change DbCotext to IdentityDbContext for using IdentityCore
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        public DbSet<House> Houses { get; set; }//this prop can be used to query and save House instances

        //by overriding below method we can seed some initial data
        protected override void OnModelCreating(ModelBuilder modelBuilder)//initializes only once
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Seed();//this is an extension method for clean code

            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
                    .SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
