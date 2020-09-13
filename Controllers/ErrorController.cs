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
        public ErrorController(ILogger<ErrorController> logger)//injection logger using dependency injection
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

                case 404://this thing can actually handle all of the status codes
                    ViewBag.ErrorMessage = "Sorry, the resource you requested was not found";

                    logger.LogWarning($"404 Error Ocurred. Path = {statusCodeResult.OriginalPath} + " +
                        $"and QueryString = {statusCodeResult.OriginalQueryString}");
                    //ViewBag.Path = statusCodeResult.OriginalPath;//filling up the error view with all helpful info
                    //ViewBag.QS = statusCodeResult.OriginalQueryString;
                    break;  
            }

            return View("NotFound");
        }

        [Route("/Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            //below object will have all the info about exception that happened
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            logger.LogError($"The path {exceptionDetails.Path}" +
                $" threw an exception {exceptionDetails.Error}");            

            ViewBag.ExceptionPath = exceptionDetails.Path;
            ViewBag.ExceptionMessage = exceptionDetails.Error.Message;
            ViewBag.StackTrace = exceptionDetails.Error.StackTrace;

            return View("Error");
        }
    }
}
