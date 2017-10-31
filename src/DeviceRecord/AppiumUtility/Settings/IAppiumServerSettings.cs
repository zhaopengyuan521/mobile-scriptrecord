using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Settings
{
    public interface IAppiumServerSettings
    {
        #region Core Server Settings Properties

        /// <summary>path to the application</summary>
        string ApplicationPath { get; set; }

        /// <summary>android activity</summary>
        string AndroidActivity { get; set; }

        /// <summary>android activity</summary>
        uint AndroidDeviceReadyTimeout { get; set; }

        /// <summary>android package</summary>
        string AndroidPackage { get; set; }

        /// <summary>android activity to wait for</summary>
        string AndroidWaitForActivity { get; set; }

        /// <summary>name of the android browser</summary>
        bool UseAndroidBrowser { get; set; }

        /// <summary>true if the given android browser should be used</summary>
        string AndroidBrowser { get; set; }

        /// <summary>true if a full android reset will be performed</summary>
        bool PerformFullAndroidReset { get; set; }

        /// <summary>false to not reset app state between sessions</summary>
        bool NoReset { get; set; }

        /// <summary>true if an android activity is supplied</summary>
        bool UseAndroidActivity { get; set; }

        /// <summary>true if the android device ready timeout will be used</summary>
        bool UseAndroidDeviceReadyTimeout { get; set; }

        /// <summary>true if an android package is supplied</summary>
        bool UseAndroidPackage { get; set; }

        /// <summary>true if an android wait activity is supplied</summary>
        bool UseAndroidWaitForActivity { get; set; }

        /// <summary>Android Only - package name for the Android activity you want to wait for</summary>
        string AndroidWaitForPackage { get; set; }

        /// <summarytrue to use the android wait for package </summary>
        bool UseAndroidWaitForPackage { get; set; }

        /// <summary>true if an application path will be used</summary>
        bool UseApplicationPath { get; set; }

        #region Capabilities Section
        string PlatformName { get; set; }

        string AutomationName { get; set; }

        string PlatformVersion { get; set; }

        bool UseDeviceName { get; set; }

        string DeviceName { get; set; }

        bool UseLanguage { get; set; }

        string Language { get; set; }

        bool UseLocale { get; set; }

        string Locale { get; set; }
        #endregion Capabilities Section

        #endregion

        /// <summary>
        /// Saves settings to underlying data store
        /// </summary>
        void Save();

        /// <summary>
        /// Loads settings from underlying data store
        /// </summary>
        void Load();
    }
}
