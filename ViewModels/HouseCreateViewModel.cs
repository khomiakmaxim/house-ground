using GroundHouse.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GroundHouse.ViewModels
{
    public class HouseCreateViewModel
    {       
        [Required]
        [EmailAddress]
        [MaxLength(50, ErrorMessage = "Address should contain <= 50 characters")]
        [Display(Name = "Owner's email")]
        public string OwnerEmail { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public Tpe? Type { get; set; }

        [Required, Range(100, 100000000)]
        public int? Price { get; set; }

        public IFormFile Photo { get; set; }//for file uploading
    }
}
