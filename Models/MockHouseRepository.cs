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
                new House() {Id = 1, OwnerEmail = "owner1@gmail.com", Address = "Address1", Price = 1000, Type = Tpe.Cottage },
                new House() {Id = 2, OwnerEmail = "owner2@gmail.com", Address = "Address2", Price = 2000, Type = Tpe.Crib},
                new House() {Id = 3, OwnerEmail = "owner3@gmail.com", Address = "Address3", Price = 3000, Type = Tpe.Villa },
            };
        }

        public House Add(House house)
        {
            house.Id = _houseList.Max(h => h.Id) + 1;
            _houseList.Add(house);
            return house;
        }

        public House GetHouse(int id)
        {
            return _houseList.FirstOrDefault(h => h.Id == id);
        }

        public IEnumerable<House> GetAllHouses()
        {
            return _houseList;
        }

        public House Update(House houseChanges)
        {
            House house = _houseList.FirstOrDefault(h => houseChanges.Id == h.Id);
            if (house != null)
            {
                house.Address = houseChanges.Address;
                house.OwnerEmail = houseChanges.OwnerEmail;
                house.Type = houseChanges.Type;
                house.Price = houseChanges.Price;
            }
            return house;
        }

        public House Delete(int id)
        {
            House house = _houseList.FirstOrDefault(h => h.Id == id);
            if (house != null)
            {
                _houseList.Remove(house);
            }
            return house;
        }
    }
}
