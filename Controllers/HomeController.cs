using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GroundHouse.Models;
using Microsoft.AspNetCore.Mvc;

namespace GroundHouse.Controllers//it is the controller who handles the http request and gives http response
{//actions are just methods who do the do
    public class HomeController : Controller
    {
        private readonly IHouseRepository _houseRepository;

        public HomeController(IHouseRepository houseRepository)
        {
            _houseRepository = houseRepository;
        }
        
        public string Index()
        {
            return _houseRepository.GetHouse(1).Address;
        }

        public IActionResult Details()
        {
            House model = _houseRepository.GetHouse(1);
            return View("Test");
        }
    }
}
