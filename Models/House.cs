﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GroundHouse.Models
{
    public class House
    {
        public int Id { get; set; }
        [NotMapped]//it won`t be mapped as database attribute
        public string EncryptedId { get; set; }
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

        public string PhotoPath { get; set; }
    }
}
