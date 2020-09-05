using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GroundHouse.Models;
using GroundHouse.ViewModels;
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
        
        public IActionResult Index()
        {
            var model = _houseRepository.GetAllHouses();
            return View(model);
        }

        public IActionResult Details(int? id)
        {
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                House = _houseRepository.GetHouse(id??1),
                PageTitle = "Details page"
            };   
            return View(homeDetailsViewModel);
        }
    }
}
