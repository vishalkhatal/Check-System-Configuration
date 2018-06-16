﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prerequisite.Models
{
    public class Config
    {
        public string IISVersion { get; set; }
        public string DotNetFramework { get; set; }
        public string SQLServerName { get; set; }
        public List<string> SQLServerInstanceName { get; set; }
        public bool IsSoftwareInstalled { get; set; }
        public List<string> IISFeature { get; set; }
    }
}