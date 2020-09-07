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
                    Address = "DefaultAddress",
                    Type = Tpe.Cottage,
                    OwnerEmail = "default@gmail.com",
                    Price = 10000
                },
                new House
                {
                    Id = 2,
                    Address = "DefaultAddress2",
                    Type = Tpe.Villa,
                    OwnerEmail = "default2@gmail.com",
                    Price = 20000
                },
                new House
                {
                    Id = 3,
                    Address = "DefaultAddress3",
                    Type = Tpe.Crib,
                    OwnerEmail = "default3@gmail.com",
                    Price = 30000
                },
                new House
                {
                    Id = 4,
                    Address = "DefaultAddress4",
                    Type = Tpe.Crib,
                    OwnerEmail = "default4@gmail.com",
                    Price = 40000
                }
            );
        }
    }
}
