using AppiumUtility.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Settings
{
    public class DefaultAppiumAppSettings : IAppiumAppSettings
    {
        public string ApplicationPath { get; set; }

        public string AndroidActivity { get; set; }

        public uint AndroidDeviceReadyTimeout { get; set; }

        public string AndroidPackage { get; set; }

        public string AndroidWaitForActivity { get; set; }

        public string AndroidWaitForPackage { get; set; }

        public bool PerformFullAndroidReset { get; set; }

        public bool NoReset { get; set; }

        public bool UseAndroidActivity { get; set; }

        public bool UseAndroidDeviceReadyTimeout { get; set; }

        public bool UseAndroidPackage { get; set; }

        public bool UseAndroidWaitForActivity { get; set; }

        public bool UseAndroidWaitForPackage { get; set; }

        public bool UseApplicationPath { get; set; }

        public bool BreakOnApplicationStart { get; set; }

        public bool ResetApplicationState { get; set; }

        public bool UseAndroidBrowser { get; set; }

        public string AndroidBrowser { get; set; }

        #region Capabilities Section
        public string PlatformName { get; set; }

        public string AutomationName { get; set; }

        public string PlatformVersion { get; set; }

        public bool UseDeviceName { get; set; }

        public string DeviceName { get; set; }

        public bool UseLanguage { get; set; }

        public string Language { get; set; }

        public bool UseLocale { get; set; }

        public string Locale { get; set; }
        #endregion Capabilities Section

        public DeviceEnum InspectorDeviceCapability { get; set; }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            throw new NotImplementedException();
        }
    }
}
