using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using SharpCompress.Readers;
using System.Diagnostics;

namespace CompressTool.Business
{
    /// <summary>
    /// 文件辅助类
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// 解压数据文件
        /// </summary>
        /// <param name="compressFile">压缩包文件</param>
        /// <param name="targetDirectory">解压到目录</param>
        /// <param name="pmHeader">进度提示标头信息</param>
        /// <param name="maxProcessValue">占用最大进度值</param>
        /// <param name="install">安装程序</param>
        /// <returns></returns>
        public void DecompressionAppData(string compressFile, string targetDirectory, string pmHeader, double maxProcessValue, Operation install)
        {
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }
            #region 进度控制
            double processValue = install.GetInstallProcess();      //获取当前执行进度
            int stepIndex = 0;      //进度累计  
            int stepLength = 160;        //进度增加步长
            #endregion
            //读取数据压缩文件
            if (!File.Exists(compressFile))
                throw new FileNotFoundException("未找到程序数据文件！");
            Stream resourceData = null;
            string tempFilePath = string.Empty;
            string currentFilePath = string.Empty;
            try
            {
                //从资源获取程序数据
                resourceData = new FileStream(compressFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                string sourceTargetDirectory = targetDirectory;
                SharpCompress.Common.ArchiveEncoding.Default = Encoding.Default;
                var reader = ReaderFactory.Open(resourceData);
                ExtractionOptions options = new ExtractionOptions();
                options.Overwrite = true;
                options.ExtractFullPath = true;
                options.PreserveFileTime = true;
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        tempFilePath = reader.Entry.Key.Replace("/", @"\");
                        currentFilePath = Path.Combine(targetDirectory, tempFilePath);
                        System.Diagnostics.Debug.WriteLine(currentFilePath);
                        if (currentFilePath.EndsWith("resources.ap_"))
                        {
                            continue;
                        }
                        reader.WriteEntryToDirectory(targetDirectory, options);
                        install.SetProcessMessage(pmHeader + currentFilePath);
                        //设置当前执行进度
                        if ((stepIndex % stepLength == 0) && (processValue < maxProcessValue))
                        {
                            processValue += 1;
                            install.SetInstallProcess(processValue);
                        }
                        stepIndex++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("解压程序数据文件错误：" + ex.Message);
            }
            finally
            {
                if (resourceData != null)
                {
                    resourceData.Close();
                    resourceData.Dispose();
                    resourceData = null;
                }
            }
        }

        /// <summary>
        /// 删除长路径文件
        /// </summary>
        /// <param name="filePath"></param>
        public static void DeleteLongFile(string filePath)
        {
            string toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LongPathTool_v2.20.exe");
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = toolPath;
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardInput = true;
            processInfo.RedirectStandardError = true;
            Process toolProcess = new Process();
            toolProcess.StartInfo = processInfo;
            var toolInputStream = toolProcess.StandardInput;
            return;
        }

        /// <summary>
        /// 解压数据文件(使用反射)
        /// </summary>
        /// <param name="sourceFilePath">压缩文件</param>
        /// <param name="targetFilePath">解压到目录</param>
        /// <returns></returns>
        private static void DecompressionAppData_Old(string targetDirectory)
        {
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }
            //从程序资源读取数据压缩文件
            Assembly app = Assembly.GetExecutingAssembly();
            string[] manifestList = app.GetManifestResourceNames();
            string appDataResourcePath = (from m in manifestList
                                          where m.ToLower().Contains("appdata")
                                          select m).FirstOrDefault();
            string scPath = (from m in manifestList
                             where m.Contains("SharpCompress.dll")
                             select m).FirstOrDefault();
            if (string.IsNullOrEmpty(appDataResourcePath))
                throw new FileNotFoundException("未找到程序数据文件！");
            string dllPath = TempSaveSharpCompressLibrary(app.GetManifestResourceStream(scPath));      //加载Dll文件到临时目录
            Assembly sharpCompressLibrary = null;
            Stream resourceData = null;
            try
            {
                //加载Dll
                sharpCompressLibrary = Assembly.LoadFrom(dllPath);
                //从资源获取程序数据
                resourceData = app.GetManifestResourceStream(appDataResourcePath);
                //反射执行ReaderFactory.Open(resourceData)方法
                var readerFactoryType = sharpCompressLibrary.GetType("SharpCompress.Reader.ReaderFactory");
                MethodInfo openMethodInfo = readerFactoryType.GetMethod("Open", BindingFlags.Static | BindingFlags.Public);
                object readerObj = openMethodInfo.Invoke(null, new object[] { resourceData, 1 });
                //反射执行reader.MoveToNextEntry()方法并接收返回值
                var readerType = sharpCompressLibrary.GetType("SharpCompress.Reader.IReader");
                MethodInfo moveToNextEntryMethodInfo = readerType.GetMethod("MoveToNextEntry", BindingFlags.Instance | BindingFlags.Public);
                object result = moveToNextEntryMethodInfo.Invoke(readerObj, null);
                bool resultBool = Convert.ToBoolean(result);      //转换返回值类型
                //声明循环中使用到的对象
                object entryAtt;
                Type entryType = sharpCompressLibrary.GetType("SharpCompress.Common.IEntry");
                bool isDirectory;
                //获取静态扩展方法
                var readExtensionType = sharpCompressLibrary.GetType("SharpCompress.Reader.IReaderExtensions");
                MethodInfo writeEntryToDirectoryMethodInfo = readExtensionType.GetMethod("WriteEntryToDirectory", BindingFlags.Public | BindingFlags.Static);
                string currentFilePath = string.Empty;
                string tempFilePath = string.Empty;
                while (resultBool)
                {
                    //获取Entry属性
                    entryAtt = readerType.InvokeMember("Entry", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, readerObj, null);
                    isDirectory = (bool)entryType.InvokeMember("IsDirectory", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, entryAtt, null);
                    if (!isDirectory)
                    {
                        writeEntryToDirectoryMethodInfo.Invoke(null, new object[] { readerObj, targetDirectory, 3 });
                        tempFilePath = (string)entryType.InvokeMember("Key", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, entryAtt, null);
                        currentFilePath = Path.Combine(targetDirectory, tempFilePath);
                    }

                    //移动到下一次
                    result = moveToNextEntryMethodInfo.Invoke(readerObj, null);
                    resultBool = Convert.ToBoolean(result);      //转换返回值类型
                }
                //var reader = ReaderFactory.Open(resourceData);
                //while (reader.MoveToNextEntry())
                //{
                //    if (!reader.Entry.IsDirectory)
                //    {
                //        reader.Entry.Key;
                //        reader.WriteEntryToDirectory(targetDirectory, SharpCompress.Common.ExtractOptions.ExtractFullPath | SharpCompress.Common.ExtractOptions.Overwrite);
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw new Exception("解压程序数据文件错误：" + ex.Message);
            }
            finally
            {
                if (resourceData != null)
                {
                    resourceData.Close();
                    resourceData.Dispose();
                    resourceData = null;
                }
                if (sharpCompressLibrary != null)
                {
                    sharpCompressLibrary = null;
                    GC.Collect();
                }
                if (!string.IsNullOrEmpty(dllPath))
                {
                    try
                    {
                        if (File.Exists(dllPath))
                        {
                            File.Delete(dllPath);
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        /// <summary>
        /// 临时存储SharpCompress文件
        /// </summary>
        /// <returns></returns>
        private static string TempSaveSharpCompressLibrary(Stream resourceStream)
        {
            string dllFileName = "SharpCompress.dll";
            //获取临时文件存储路径
            string tempFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), dllFileName);
            //释放到临时文件
            using (StreamWriter sw = new StreamWriter(tempFilePath, false))
            {
                resourceStream.CopyTo(sw.BaseStream);
                sw.Flush();
                sw.Close();
            }
            return tempFilePath;
        }
    }
}
