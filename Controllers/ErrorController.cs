using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GroundHouse.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> logger;

        //below generic parameter allows to know who exactly threw an exception
        public ErrorController(ILogger<ErrorController/*this parameter*/> logger)//injection logger via dependency injection
        {
            this.logger = logger;
        }
        
        [Route("/Error/{statusCode}")]//attribute routing with placeholding
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            //we can use this only with ReExecute()
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            //retrieving info about that request
            switch (statusCode)
            {
                //this thing can actually handle all of the status codes
                case 404:
                    ViewBag.ErrorMessage = "Sorry, the resource you requested was not found";

                    logger.LogWarning($"404 Error Ocurred. Path = {statusCodeResult.OriginalPath}" +
                        $"and QueryString = {statusCodeResult.OriginalQueryString}");            
                    break;
                default:
                    ViewBag.ErrorMessage = "Client side error ocurred";
                    logger.LogWarning($"Client side error ocurred. Path = {statusCodeResult} " +
                        $"and QueryString = {statusCodeResult.OriginalQueryString}");
                    break;
            }
            return View("NotFound");
        }

        //this handles serves global errors
        [Route("/Error")]        
        public IActionResult Error()
        {
            //below object will have all the info about exception that happened
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            logger.LogError($"The path {exceptionDetails.Path}" +
                $" threw an exception {exceptionDetails.Error}");          
            return View("Error");
        }
    }
}
