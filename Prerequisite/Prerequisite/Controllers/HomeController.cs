using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Web;
using System.Web.Mvc;
using Prerequisite.Models;
namespace Prerequisite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Config config = new Config();
            config.IISVersion = GetIISVersion();
            config.SQLServerName = GetServerName("SQL");
            config.SQLServerInstanceName = GetSQLServerInstance();
            config.IsRequiredSoftwareInstalled = checkInstalled("SQL");
            config.DotNetFramework = Get45PlusFromRegistry();
            config.IISFeature = GetIISComponents();
            return View(config);
        }
        private static string GetSQLServerInstance()
        {
            string instances = string.Empty;
            RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                RegistryKey instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);
                if (instanceKey != null)
                {
                    foreach (var instanceName in instanceKey.GetValueNames())
                    {
                        instances= instances+instanceName+",";
                    }
                }
            }
            if(!string.IsNullOrEmpty(instances))
            instances = instances.Remove(instances.Length - 1);
            return instances;
        }
        private static bool checkInstalled(string c_name)
        {
            string displayName;

            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains(c_name))
                    {
                        return true;
                    }
                }
                key.Close();
            }

            registryKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            key = Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains(c_name))
                    {
                        return true;
                    }
                }
                key.Close();
            }
           return false;
        }
        private static string GetServerName(string c_name)
        {
            string displayName;

            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains(c_name))
                    {
                        return displayName;
                    }
                }
                key.Close();
            }

            registryKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            key = Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains(c_name))
                    {
                        return displayName;
                    }
                }
                key.Close();
            }
            return string.Empty;
        }
        private static string Get45PlusFromRegistry()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                   return(CheckFor45PlusVersion((int)ndpKey.GetValue("Release")));
                }
                else
                {
                    return ".NET Framework Version 4.5 or later is not detected.";
                }
            }
        }
        private static string CheckFor45PlusVersion(int releaseKey)
        {
            if (releaseKey >= 461808)
                return "4.7.2 or later";
            if (releaseKey >= 461308)
                return "4.7.1";
            if (releaseKey >= 460798)
                return "4.7";
            if (releaseKey >= 394802)
                return "4.6.2";
            if (releaseKey >= 394254)
                return "4.6.1";
            if (releaseKey >= 393295)
                return "4.6";
            if (releaseKey >= 379893)
                return "4.5.2";
            if (releaseKey >= 378675)
                return "4.5.1";
            if (releaseKey >= 378389)
                return "4.5";
            // This code should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }
        private static bool IsIISRunning()
        {

            bool isIISrunning = false;
            ServiceController controller = new ServiceController("W3SVC");
            if (controller.Status == ServiceControllerStatus.Running)
            {
                isIISrunning = true;
            }
            return isIISrunning;
        }
        private static string GetIISVersion()
        {
            if (IsIISRunning())
            {

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp", false))
                {
                    if (key != null)
                    {
                        int majorVersion = (int)key.GetValue("MajorVersion", -1);
                        int minorVersion = (int)key.GetValue("MinorVersion", -1);

                        if (majorVersion != -1 && minorVersion != -1)
                        {
                            return majorVersion.ToString();
                        }
                    }
                    else
                        return "IIS Not enabled yet";
                }
            }

            return "IIS Not enabled yet";
        }
        private static string GetIISComponents()
        {
            string instances = string.Empty;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp\Components\"))
            {
                if (key != null)
                {
                    foreach (var item in key.GetValueNames())
                    {
                        instances = instances + item + ",";
                    }
                }
            }

            if (!string.IsNullOrEmpty(instances))
                instances = instances.Remove(instances.Length - 1);
            return instances;
        }

    }
}