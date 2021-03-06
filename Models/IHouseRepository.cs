﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroundHouse.Models
{
    public interface IHouseRepository
    {
        House GetHouse(int id);
        IEnumerable<House> GetAllHouses();
        House Add(House house);
        House Update(House houseChanges);
        House Delete(int id);
    }
}
