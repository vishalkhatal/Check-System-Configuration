using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Web;
using System.Web.Mvc;
using Prerequisite.Models;
using System.Data.SqlClient;
using System.Data;

namespace Prerequisite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Config config = new Config();
            config.DotNetFramework = Get45PlusFromRegistry();
            config.DotNetFrameworkVersion = GetVersionFromRegistry();
            config.IISVersion = GetIISVersion();
            config.SQLServerName = GetServerName("SQL");
            config.SQLServerInstanceName = GetSQLServerInstance();
            //config.IsRequiredSoftwareInstalled = checkInstalled("SQL");
            config.IISFeature = GetIISComponents();
            config.SQLServerCompact = IsV40Installed();
            return View(config);
        }

        public JsonResult TestSQLConnection(string servername, string database, string username, string password, bool windowsAuthentication)
        {
            string result = string.Empty;
            string connetionString = null;
            SqlConnection cnn;
            if (!windowsAuthentication)
                connetionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}", servername, database, username, password);
            else
                connetionString = string.Format("Data Source={0};Initial Catalog={1};Integrated Security=True", servername, database);
            cnn = new SqlConnection(connetionString);
            try
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM fn_my_permissions (NULL, 'DATABASE')", cnn);
                cmd.CommandType = CommandType.Text;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();

                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
                var permissions = dataTable.DataTableToList<permissions>();
                return Json(permissions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result = "";
            }
            finally
            {
                cnn.Close();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TestSoftwareAvailability(string softwarename)
        {
            string displayName;
            string result = "";
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.ToLower().Contains(softwarename.ToLower()))
                    {
                        result="Software Available";
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
                    if (displayName != null && displayName.ToLower().Contains(softwarename.ToLower()))
                    {
                        result = "Software Available";
                    }
                }
                key.Close();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region Helper
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
                    return (CheckFor45PlusVersion((int)ndpKey.GetValue("Release")));
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
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\InetStp\Components\StaticContent"))
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
        private static string GetVersionFromRegistry()
        {
            string instances = string.Empty;

            // Opens the registry key for the .NET Framework entry.
            using (RegistryKey ndpKey =
                RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").
                OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                // As an alternative, if you know the computers you will query are running .NET Framework 4.5 
                // or later, you can use:
                // using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, 
                // RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    if (versionKeyName.StartsWith("v"))
                    {

                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        string name = (string)versionKey.GetValue("Version", "");
                        string sp = versionKey.GetValue("SP", "").ToString();
                        string install = versionKey.GetValue("Install", "").ToString();
                        if (install == "")
                        { //no install info, must be later.
                          //Console.WriteLine(versionKeyName + "  " + name);
                        }
                        else
                        {
                            if (sp != "" && install == "1")
                            {
                                instances = instances + versionKeyName + "-" + name + "SP" + sp + ",";

                                //Console.WriteLine(versionKeyName + "  " + name + "  SP" + sp);
                            }

                        }
                        if (name != "")
                        {
                            continue;
                        }
                        foreach (string subKeyName in versionKey.GetSubKeyNames())
                        {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string)subKey.GetValue("Version", "");
                            if (name != "")
                                sp = subKey.GetValue("SP", "").ToString();
                            install = subKey.GetValue("Install", "").ToString();
                            if (install == "")
                            {
                                //no install info, must be later.
                                // Console.WriteLine(versionKeyName + "  " + name);
                            }
                            else
                            {
                                if (sp != "" && install == "1")
                                {
                                    instances = instances + subKeyName + "-" + name + "SP" + sp + ",";

                                    // Console.WriteLine("  " + subKeyName + "  " + name + "  SP" + sp);
                                }
                                else if (install == "1")
                                {
                                    instances = instances + subKeyName + "-" + name + ",";

                                    // Console.WriteLine("  " + subKeyName + "  " + name);
                                }
                            }
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(instances))
                instances = instances.Remove(instances.Length - 1);
            return instances;
        }
        private static bool IsV40Installed()
        {
            try
            {
                System.Reflection.Assembly.Load("System.Data.SqlServerCe, Version=4.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
            }
            catch (System.IO.FileNotFoundException)
            {
                return false;
            }
            try
            {
                var factory = System.Data.Common.DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");
            }
            catch (System.Configuration.ConfigurationException)
            {
                return false;
            }
            catch (System.ArgumentException)
            {
                return false;
            }
            return true;
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
                        instances = instances + instanceName + ",";
                    }
                }
            }
            if (!string.IsNullOrEmpty(instances))
                instances = instances.Remove(instances.Length - 1);
            return instances;
        }
        #endregion


    }
}