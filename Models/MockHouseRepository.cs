using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroundHouse.Models
{
    public class MockHouseRepository : IHouseRepository
    {
        private List<House> _houseList;

        public MockHouseRepository()
        {
            _houseList = new List<House>
            { 
                new House() {Id = 1, OwnerEmail = "owner1@gmail.com", Address = "Address1", Price = 1000, Type = "Type1" },
                new House() {Id = 2, OwnerEmail = "owner2@gmail.com", Address = "Address2", Price = 2000, Type = "Type2" },
                new House() {Id = 3, OwnerEmail = "owner3@gmail.com", Address = "Address3", Price = 3000, Type = "Type3" },
            };
        }
        public House GetHouse(int id)
        {
            return _houseList.FirstOrDefault(e => e.Id == id);
        }
    }
}
