﻿using System;
using System.Collections.Generic;
using System.Text;

namespace KinaUnaXamarin.Models
{
    public class DeviceRegistration
    {
        public string Platform { get; set; }
        public string Handle { get; set; }
        public string[] Tags { get; set; }
    }
}
