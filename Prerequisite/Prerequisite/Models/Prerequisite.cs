using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Prerequisite.Models
{
    public class Config
    {
        [Display(Name = "IIS Version")]
        public string IISVersion { get; set; }
        [Display(Name = "DotNet Framework")]
        public string DotNetFramework { get; set; }
        [Display(Name = "DotNet Framework Version")]
        public string DotNetFrameworkVersion { get; set; }
        [Display(Name = "SQL Server Name")]
        public string SQLServerName { get; set; }
        [Display(Name = "SQL Server Instance Name")]
        public string SQLServerInstanceName { get; set; }
        [Display(Name = "SQL Server Compact available on system")]
        public bool SQLServerCompact { get; set; }
        [Display(Name = "Required Software available on system")]
        public bool IsRequiredSoftwareInstalled { get; set; }
        [Display(Name = "IIS Feature")]
        public string IISFeature { get; set; }
    }
}