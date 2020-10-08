using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GroundHouse.Models;
using GroundHouse.Security;
using GroundHouse.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GroundHouse.Controllers//it is the controller who handles the http request and gives http response
{//actions are just methods which do the do
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IHouseRepository _houseRepository;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILogger<HomeController> logger;
        private readonly IDataProtector protector;

        public HomeController(IHouseRepository houseRepository,
                              IHostingEnvironment hostingEnvironment,
                              ILogger<HomeController> logger,
                              IDataProtectionProvider dataProtectionProvider,
                              DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {
            _houseRepository = houseRepository;
            this.hostingEnvironment = hostingEnvironment;
            this.logger = logger;
            protector = dataProtectionProvider
                            .CreateProtector(dataProtectionPurposeStrings.HouseIdRouteValue);//there is some logic behind that

        }

        [HttpPost]
        [Authorize(Policy = "DeleteHomePolicy")]
        public IActionResult DeleteHome(string id)
        {
            int decryptedId = int.Parse(protector.Unprotect(id));
            House house = _houseRepository.GetHouse(decryptedId);             
            if (house == null)
            {
                ViewBag.ErrorMessage = $"House with id = {id} could not be found"; 
                return View("NotFound");
            }

            _houseRepository.Delete(decryptedId);
            return RedirectToAction("Index");

        }

        [HttpGet]
        [Authorize(Policy = "EditHomePolicy")]
        public IActionResult Edit(string id)
        {
            int decryptedId = int.Parse(protector.Unprotect(id));
            House house = _houseRepository.GetHouse(decryptedId);

            if (house == null)
            {
                Response.StatusCode = 404;
                return View("HouseNotFound", id);
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
        [Authorize(Policy = "EditHomePolicy")]
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

                    house.PhotoPath = ProcessUploadedFile(model);//adding another photo
                }

                _houseRepository.Update(house);
                return RedirectToAction("index");
            }

            return View();
        }

        //utility method
        private string ProcessUploadedFile(HouseCreateViewModel model)
        {
            string uniqueFileName = null;
            if (model.Photo != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");//set path where to store
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;//adding guid for uniqueness
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);                
                using (var fileStream = new FileStream(filePath, FileMode.Create))//this stream will copy file to it's path
                { 
                    model.Photo.CopyTo(fileStream);             
                }
                //it is properly disposed now via "using" statement
            }

            return uniqueFileName;//if model has Photo unset => client hasn't set photo(default photo will be rendered)
        }

        [HttpGet]
        [Authorize(Policy = "CreateHomePolicy")]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "CreateHomePolicy")]
        public IActionResult Create(HouseCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                //for file uploading
                string uniqueFileName = ProcessUploadedFile(model);//utility method for processing photo upload
                House newHouse = new House()//id is generated by ef core
                { 
                    Address = model.Address,
                    OwnerEmail = model.OwnerEmail,
                    Type = model.Type,
                    Price = model.Price,
                    PhotoPath = uniqueFileName
                };

                _houseRepository.Add(newHouse);//this method generates id value
                return RedirectToAction("details", new { id = protector.Protect(newHouse.Id.ToString()) });//this statement redirects request to another action
            }

            return View();//if ModelState is not valid view will render all errors
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Details(string id)//id is a route value Home/Details/?id=someid
        {            
            #region testlogs
            //logger.LogTrace("Trace Log");
            //logger.LogDebug("Debug Log");
            //logger.LogInformation("Information Log");
            //logger.LogWarning("Warning Log");
            //logger.LogError("Error Log");
            //logger.LogCritical("Critical Log"); 
            #endregion

            int decryptedId = int.Parse(protector.Unprotect(id));
            House house = _houseRepository.GetHouse(decryptedId);

            if (house == null)
            {
                Response.StatusCode = 404;
                return View("HouseNotFound", id);
            }

            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                House = house,
                PageTitle = "Details page"
            };
            return View(homeDetailsViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var model = _houseRepository.GetAllHouses()//sets all encrypted id values
                                    .Select(e =>
                                    {
                                        e.EncryptedId = protector.Protect(e.Id.ToString());
                                        return e;
                                    });
            return View(model);
        }        
    }
}
