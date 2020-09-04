using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroundHouse.Models
{
    public class House
    {
        public int Id { get; set; }
        public string OwnerEmail { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }
        public int Price { get; set; }
    }
}
