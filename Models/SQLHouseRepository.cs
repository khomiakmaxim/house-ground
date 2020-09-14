using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroundHouse.Models
{
    //this implementation can be used to store and retrieve data from sql database
    public class SQLHouseRepository : IHouseRepository
    {
        private readonly AppDbContext context;
        private readonly ILogger<SQLHouseRepository> logger;

        public SQLHouseRepository(AppDbContext context,
                                  ILogger<SQLHouseRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public House Add(House house)
        {
            context.Houses.Add(house);
            context.SaveChanges();//when this method is called, EFCore makes a request to update db
            return house;
        }

        public House Delete(int id)
        {
            House house = context.Houses.Find(id);
            if (house != null)
            {
                context.Houses.Remove(house);
                context.SaveChanges();
            }
            return house;            
        }

        public IEnumerable<House> GetAllHouses()
        {
            return context.Houses;
        }

        public House GetHouse(int id)
        {
            logger.LogTrace("Trace Log");
            logger.LogDebug("Debug Log");
            logger.LogInformation("Information Log");
            logger.LogWarning("Warning Log");
            logger.LogError("Error Log");
            logger.LogCritical("Critical Log");
            return context.Houses.Find(id);
        }

        public House Update(House houseChanges)
        {
            var house = context.Houses.Attach(houseChanges);
            house.State = Microsoft.EntityFrameworkCore.EntityState.Modified;//telling ef that that the enitity that we have attached is modified
            context.SaveChanges();
            return houseChanges;
        }
    }
}
