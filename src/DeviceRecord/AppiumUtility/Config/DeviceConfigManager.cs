using AppiumUtility.Logger;
using AppiumUtility.Models;
using AppiumUtility.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AppiumUtility.Config
{
    /// <summary>
    /// 设备配置管理
    /// </summary>
    public class DeviceConfigManager
    {
        #region 单例
        private static DeviceConfigManager _configuration;
        /// <summary>
        /// 单例Appium配置信息对象
        /// </summary>
        public static DeviceConfigManager Instance
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = new DeviceConfigManager();
                }
                return _configuration;
            }
        }
        #endregion
        #region 成员列表
        /// <summary> 设备列表（配置文件中的）</summary>
        private List<DeviceInfo> _DeviceList = null;
        /// <summary>
        /// 设备配置文件路径
        /// </summary>
        public readonly string DeviceConfigFilePath = Path.Combine(AppConfigManager.Instance.AppRootFolder, "device_config.xml");

        /// <summary>
        /// 设备列表（配置文件）
        /// </summary>
        public List<DeviceInfo> DeviceList
        {
            get
            {
                if (_DeviceList == null)
                    _DeviceList = LoadDeviceInfo();     //从配置文件加载设备信息
                return _DeviceList;
            }
        }
        #endregion
        #region 配置文件相关操作

        /// <summary>
        /// 从配置文件加载设备信息 
        /// </summary>
        /// <returns></returns>
        public List<DeviceInfo> LoadDeviceInfo()
        {
            List<DeviceInfo> deviceList = new List<DeviceInfo>();
            GenerateDeviceConfigFile();
            XElement rootElement = XElement.Load(DeviceConfigFilePath, LoadOptions.None);
            deviceList = (from deviceElement in rootElement.Elements("device")
                          select ParseDeviceElement(deviceElement)).ToList();
            return deviceList;
        }

        /// <summary>
        /// 从配置文件加载设备信息
        /// </summary>
        /// <param name="serialNo">设备编号</param>
        /// <returns></returns>
        public DeviceInfo LoadDeviceInfo(string serialNo)
        {
            DeviceInfo result = null;
            if (string.IsNullOrEmpty(serialNo)) return result;
            GenerateDeviceConfigFile();
            XElement rootElement = XElement.Load(DeviceConfigFilePath, LoadOptions.None);
            result = (from deviceElement in rootElement.Elements("device")
                      where deviceElement.Attribute("serialno") != null && deviceElement.Attribute("serialno").Value == serialNo
                      select ParseDeviceElement(deviceElement)).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// 保存设备信息到配置文件
        /// </summary>
        /// <param name="deviceInfo"></param>
        public ExecuteResult SaveDeviceInfo(DeviceInfo deviceInfo)
        {
            ExecuteResult result = new ExecuteResult();
            if (deviceInfo == null || string.IsNullOrEmpty(deviceInfo.Serialno))
            {
                result.Message = "设备信息无效！";
                return result;
            }
            try
            {
                GenerateDeviceConfigFile();
                XDocument xDoc = XDocument.Load(DeviceConfigFilePath, LoadOptions.None);
                XElement rootElement = xDoc.Root;          //加载配置文件信息
                //尝试从配置文件中获取设备信息
                XElement deviceElement = (from ele in rootElement.Elements("device")
                                          where ele.Attribute("serialno") != null && ele.Attribute("serialno").Value == deviceInfo.Serialno
                                          select ele).FirstOrDefault();
                //判断保存的设备信息在配置文件中是否存在
                if (deviceElement == null)
                {
                    //创建设备信息
                    deviceElement = GenerateDeviceElement(deviceInfo);
                    rootElement.Add(deviceElement);         //添加到设备列表
                }
                else
                {
                    //修改设备信息
                    ModifyDeviceElement(deviceElement, deviceInfo);
                }
                xDoc.Save(DeviceConfigFilePath, SaveOptions.None);
                AddDeviceToList(deviceInfo);        //添加到列表
                result.Result = true;
            }
            catch (Exception ex)
            {
                string deviceJsonData = CommonUtility.ObjectToJson<DeviceInfo>(deviceInfo);
                AppLog.CreateAppLog().OutputLogData(string.Format("保存设备信息[{0}]到配置文件失败，{1}", deviceJsonData, ex.Message), ex);
                result.Result = false;
                result.Message = "保存设备信息到配置文件失败！";
            }
            return result;
        }

        /// <summary>
        /// 从配置文件移除设备信息
        /// </summary>
        /// <param name="serialNo"></param>
        public ExecuteResult RemoveDeviceInfo(string serialNo)
        {
            ExecuteResult result = new ExecuteResult();
            if (string.IsNullOrEmpty(serialNo))
            {
                result.Message = "设备信息无效！";
                return result;
            }
            try
            {
                GenerateDeviceConfigFile();
                XDocument xDoc = XDocument.Load(DeviceConfigFilePath, LoadOptions.None);
                XElement rootElement = xDoc.Root;          //加载配置文件信息
                //尝试从配置文件中获取设备信息
                XElement deviceElement = (from ele in rootElement.Elements("device")
                                          where ele.Attribute("serialno") != null && ele.Attribute("serialno").Value == serialNo
                                          select ele).FirstOrDefault();
                //判断保存的设备信息在配置文件中是否存在
                if (deviceElement != null)
                {
                    //移除设备信息
                    deviceElement.Remove();
                }
                //保存到文件
                xDoc.Save(DeviceConfigFilePath, SaveOptions.None);
                RemoteDeviceToList(serialNo);       //从列表移除设备
                result.Result = true;
            }
            catch (Exception ex)
            {
                AppLog.CreateAppLog().OutputLogData(string.Format("从配置文件移除设备信息[{0}]失败，{1}", serialNo, ex.Message), ex);
                result.Result = false;
                result.Message = "从配置文件移除设备信息失败！";
            }
            return result;
        }

        /// <summary>
        /// 获取配置文件用户设置值
        /// </summary>
        /// <param name="name">设置名称</param>
        /// <returns>设置值</returns>
        private string GetSettingValue(string name)
        {
            string settingValue = "";

            return settingValue;
        }

        /// <summary>
        /// 获取配置文件用户设置值
        /// </summary>
        /// <typeparam name="T">设置值类型</typeparam>
        /// <param name="name">设置名称</param>
        /// <returns>设置值</returns>
        public T GetSettingValue<T>(string name)
        {
            T settingValue = default(T);

            return settingValue;
        }

        /// <summary>
        /// 添加设备到列表（列表已存在的情况下进行替换）
        /// </summary>
        /// <param name="deviceInfo"></param>
        private void AddDeviceToList(DeviceInfo deviceInfo)
        {
            //从列表尝试获取相同设备编号所在的索引
            int index = DeviceList.FindIndex(device => device.Serialno.Equals(deviceInfo.Serialno));
            //判断是否在列表中已存在
            if (index > -1)
                DeviceList[index] = deviceInfo;
            else
                DeviceList.Add(deviceInfo);
        }

        /// <summary>
        /// 从列表移除设备
        /// </summary>
        /// <param name="deviceSerialno"></param>
        private void RemoteDeviceToList(string deviceSerialno)
        {
            //添加是否在列表中存在
            if (DeviceList.Exists(device => device.Serialno.Equals(deviceSerialno)))
            {
                DeviceList.RemoveAll(device => device.Serialno.Equals(deviceSerialno));     //执行移除操作
            }
        }

        /// <summary>
        /// 解析设备元素
        /// </summary>
        /// <param name="deviceElement"></param>
        /// <returns></returns>
        private DeviceInfo ParseDeviceElement(XElement deviceElement)
        {
            DeviceInfo result = new DeviceInfo();
            result.Serialno = deviceElement.Attribute("serialno").Value;
            result.IsRemote = Convert.ToBoolean(deviceElement.Attribute("isRemote").Value);
            result.SDKLevel = deviceElement.Element("sdkLevel").Value;
            result.SystemVersion = deviceElement.Element("systemVersion").Value;
            result.Type = deviceElement.Element("type").Value;
            result.Brand = deviceElement.Element("brand").Value;
            result.Name = deviceElement.Element("name").Value;
            result.Board = deviceElement.Element("board").Value;
            result.DPI = Convert.ToInt32(deviceElement.Element("dpi").Value);
            result.CPU = deviceElement.Element("cpu").Value;
            result.Memory = deviceElement.Element("memory").Value;
            result.Resolution = deviceElement.Element("resolution").Value;
            result.MAC = deviceElement.Element("mac").Value;
            //解析Appium Setting配置信息
            XElement appiumSettingElement = deviceElement.Element("appiumSetting");
            if (appiumSettingElement != null)
            {
                result.AppiumSetting = new AppiumProcess.AppiumProcessData(
                    appiumSettingElement.Element("address").Value,
                    Convert.ToUInt32(appiumSettingElement.Element("port").Value),
                    result.Serialno);
            }
            return result;
        }

        /// <summary>
        /// 生成设备信息元素
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        private XElement GenerateDeviceElement(DeviceInfo deviceInfo)
        {
            XElement xDevice = new XElement("device",
                new XAttribute("serialno", deviceInfo.Serialno),
                new XAttribute("isRemote", deviceInfo.IsRemote));
            xDevice.Add(new XElement("sdkLevel", (string.IsNullOrEmpty(deviceInfo.SDKLevel) ? "" : deviceInfo.SDKLevel)));
            xDevice.Add(new XElement("systemVersion", (string.IsNullOrEmpty(deviceInfo.SystemVersion) ? "" : deviceInfo.SystemVersion)));
            xDevice.Add(new XElement("type", (string.IsNullOrEmpty(deviceInfo.Type) ? "" : deviceInfo.Type)));
            xDevice.Add(new XElement("brand", (string.IsNullOrEmpty(deviceInfo.Brand) ? "" : deviceInfo.Brand)));
            xDevice.Add(new XElement("name", (string.IsNullOrEmpty(deviceInfo.Name) ? "" : deviceInfo.Name)));
            xDevice.Add(new XElement("board", (string.IsNullOrEmpty(deviceInfo.Board) ? "" : deviceInfo.Board)));
            xDevice.Add(new XElement("dpi", deviceInfo.DPI));
            xDevice.Add(new XElement("cpu", (string.IsNullOrEmpty(deviceInfo.CPU) ? "" : deviceInfo.CPU)));
            xDevice.Add(new XElement("memory", (string.IsNullOrEmpty(deviceInfo.Memory) ? "" : deviceInfo.Memory)));
            xDevice.Add(new XElement("resolution", (string.IsNullOrEmpty(deviceInfo.Resolution) ? "" : deviceInfo.Resolution)));
            xDevice.Add(new XElement("mac", (string.IsNullOrEmpty(deviceInfo.MAC) ? "" : deviceInfo.MAC)));
            //判断是否需要进行Appium配置信息保存
            if (deviceInfo.AppiumSetting == null) return xDevice;
            XElement xAppium = new XElement("appiumSetting");
            xAppium.Add(new XElement("address", deviceInfo.AppiumSetting.ServerAddress));
            xAppium.Add(new XElement("port", deviceInfo.AppiumSetting.AppiumPort));
            xDevice.Add(xAppium);
            return xDevice;
        }

        /// <summary>
        /// 修改设备信息到设备元素
        /// </summary>
        /// <param name="deviceElement">设备元素</param>
        /// <param name="deviceInfo">设备信息</param>
        private void ModifyDeviceElement(XElement deviceElement, DeviceInfo deviceInfo)
        {
            deviceElement.Attribute("isRemote").SetValue(deviceInfo.IsRemote);
            deviceElement.SetElementValue("sdkLevel", (string.IsNullOrEmpty(deviceInfo.SDKLevel) ? "" : deviceInfo.SDKLevel));
            deviceElement.SetElementValue("systemVersion", (string.IsNullOrEmpty(deviceInfo.SystemVersion) ? "" : deviceInfo.SystemVersion));
            deviceElement.SetElementValue("type", (string.IsNullOrEmpty(deviceInfo.Type) ? "" : deviceInfo.Type));
            deviceElement.SetElementValue("brand", (string.IsNullOrEmpty(deviceInfo.Brand) ? "" : deviceInfo.Brand));
            deviceElement.SetElementValue("name", (string.IsNullOrEmpty(deviceInfo.Name) ? "" : deviceInfo.Name));
            deviceElement.SetElementValue("board", (string.IsNullOrEmpty(deviceInfo.Board) ? "" : deviceInfo.Board));
            deviceElement.SetElementValue("dpi", deviceInfo.DPI);
            deviceElement.SetElementValue("cpu", (string.IsNullOrEmpty(deviceInfo.CPU) ? "" : deviceInfo.CPU));
            deviceElement.SetElementValue("memory", (string.IsNullOrEmpty(deviceInfo.Memory) ? "" : deviceInfo.Memory));
            deviceElement.SetElementValue("resolution", (string.IsNullOrEmpty(deviceInfo.Resolution) ? "" : deviceInfo.Resolution));
            deviceElement.SetElementValue("mac", (string.IsNullOrEmpty(deviceInfo.MAC) ? "" : deviceInfo.MAC));
            XElement appiumSettingElement = null;
            if (deviceInfo.AppiumSetting == null)
            {
                //从配置文件中移除AppiumSetting信息
                appiumSettingElement = deviceElement.Element("appiumSetting");
                if (appiumSettingElement != null) appiumSettingElement.Remove();
            }
            else
            {
                //添加AppiumSetting信息到配置文件
                appiumSettingElement = deviceElement.Element("appiumSetting");
                if (appiumSettingElement == null)
                {
                    //创建新的AppiumSetting节点
                    appiumSettingElement = new XElement("appiumSetting");
                    appiumSettingElement.Add(new XElement("address", deviceInfo.AppiumSetting.ServerAddress));
                    appiumSettingElement.Add(new XElement("port", deviceInfo.AppiumSetting.AppiumPort));
                    deviceElement.Add(appiumSettingElement);
                }
                else
                {
                    //修改AppiumSetting节点
                    appiumSettingElement.SetElementValue("address", deviceInfo.AppiumSetting.ServerAddress);
                    appiumSettingElement.SetElementValue("port", deviceInfo.AppiumSetting.AppiumPort);
                }
            }
        }

        /// <summary>
        /// 生成设备配置文件(存在的情况下无任何操作)
        /// </summary>
        private void GenerateDeviceConfigFile()
        {
            //判断设备配置文件是否存在
            if (File.Exists(DeviceConfigFilePath)) return;
            //不存在的情况下创建新的设备配置文件
            XDocument xDoc = new XDocument(new XDeclaration("1.0", "utf-8", null));         //实例化XDocument对象
            xDoc.Add(new XElement("devices"));      //添加根节点
            xDoc.Save(DeviceConfigFilePath, SaveOptions.None);
        }
        #endregion
    }
}
