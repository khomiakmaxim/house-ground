using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroundHouse.Models
{
    public static class ModelBuilderExtensions
    {
        //extension method for ModelBuilder
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<House>().HasData(
                new House
                {
                    Id = 1,
                    Address = "ModelBuilder",
                    Type = Tpe.Cottage,
                    OwnerEmail = "builder@gmail.com",
                    Price = 10000
                }
            );
        }
    }
}
