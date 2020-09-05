﻿using GroundHouse.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroundHouse.ViewModels
{
    public class HomeDetailsViewModel
    {
        public House Hosue { get; set; }
        public string PageTitle { get; set; }
        public House House { get; internal set; }
    }
}
