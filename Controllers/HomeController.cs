﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GroundHouse.Models;
using GroundHouse.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GroundHouse.Controllers//it is the controller who handles the http request and gives http response
{//actions are just methods who do the do
    public class HomeController : Controller
    {
        private readonly IHouseRepository _houseRepository;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILogger<HomeController> logger;

        public HomeController(IHouseRepository houseRepository,
                              IHostingEnvironment hostingEnvironment,
                              ILogger<HomeController> logger)
        {
            _houseRepository = houseRepository;
            this.hostingEnvironment = hostingEnvironment;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {

            House house = _houseRepository.GetHouse(id ?? 4);

            if (house == null)
            {
                Response.StatusCode = 404;
                return View("HouseNotFound", id ?? 4);
            }           

            HouseEditViewModel model = new HouseEditViewModel
            { 
                Id = house.Id,
                Address = house.Address,
                OwnerEmail = house.OwnerEmail,
                Price = house.Price,
                Type = house.Type,
                ExistingPhotoPath = house.PhotoPath                
            };            

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(HouseEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                House house = _houseRepository.GetHouse(model.Id);
                house.Address = model.Address;
                house.OwnerEmail = model.OwnerEmail;
                house.Type = model.Type;
                house.Price = model.Price;

                if (model.Photo != null)
                {
                    if (model.ExistingPhotoPath != null)//if user already has photo we have to delete it before changing
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath,
                            "images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);//deleting existing photo
                    }

                    house.PhotoPath = ProcessUploadedFile(model);
                }

                _houseRepository.Update(house);
                return RedirectToAction("index");
            }

            return View();
        }

        private string ProcessUploadedFile(HouseCreateViewModel model)
        {
            string uniqueFileName = null;
            if (model.Photo != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                { 
                    model.Photo.CopyTo(fileStream);                    
                }
                //it is properly disposed now via using statement
            }

            return uniqueFileName;
        }

        [HttpGet]
        [Authorize]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create(HouseCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                //for file uploading
                string uniqueFileName = ProcessUploadedFile(model);
                House newHouse = new House()//id is generated by ef core(actally i`m not sure if it is)
                { 
                    Address = model.Address,
                    OwnerEmail = model.OwnerEmail,
                    Type = model.Type,
                    Price = model.Price,
                    PhotoPath = uniqueFileName
                };

                _houseRepository.Add(newHouse);
                return RedirectToAction("details", new { id = newHouse.Id });
            }

            return View();
        }

        public IActionResult Details(int? id)
        {
            //firstly, goes checking for not found

            //test log
            //logger.LogTrace("Trace Log");
            //logger.LogDebug("Debug Log");
            //logger.LogInformation("Information Log");
            //logger.LogWarning("Warning Log");
            //logger.LogError("Error Log");
            //logger.LogCritical("Critical Log");


            House house = _houseRepository.GetHouse(id??4);

            if (house == null)
            {
                Response.StatusCode = 404;
                return View("HouseNotFound", id??4);
            }

            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                House = house,
                PageTitle = "Details page"
            };
            return View(homeDetailsViewModel);
        }

        public IActionResult Index()
        {
            var model = _houseRepository.GetAllHouses();
            return View(model);
        }        
    }
}
