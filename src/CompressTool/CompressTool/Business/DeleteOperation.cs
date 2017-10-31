using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressTool.Business
{
    /// <summary>
    /// 删除操作
    /// </summary>
    public class DeleteOperation : Operation
    {
        /// <summary>
        /// 删除命令进程
        /// </summary>
        private Process _DeleteCommandProcess = null;

        /// <summary>
        /// 进度最大值
        /// </summary>
        private double _MaxProcessValue = 100;

        /// <summary>
        /// 进度的值
        /// </summary>
        private double _ProcessValue = 0;

        /// <summary>
        /// 进度累计
        /// </summary>
        private int _StepIndex = 0;

        /// <summary>
        /// 进度增加步长
        /// </summary>
        private int _StepLength = 0;

        private string _PMHeader = "删除文件：";

        /// <summary>
        /// 删除操作
        /// </summary>
        public override void StartOperate()
        {
            try
            {
                //验证删除的路径是否是目录
                FileAttributes attribute = File.GetAttributes(CompressData.DeletePath);
                CompressData.IsDirectory = ((attribute & FileAttributes.Directory) == FileAttributes.Directory);
                DeleteFile();                           //删除文件或目录
                OperationCompete();         //删除完成
            }
            catch (Exception ex)
            {
                if (ExcaptionHandle != null)
                    ExcaptionHandle(ex.Message);
            }
        }

        /// <summary>
        /// 删除文件操作
        /// </summary>
        public void DeleteFile()
        {
            if (CompressData.IsDirectory)
            {
                SetProcessMessage("正在检测文件数量，请稍候...");
                int totalCount = GetFileCount(new DirectoryInfo(CompressData.DeletePath));
                this._StepLength = (totalCount / 100);
                if ((totalCount % 100) > 0) this._StepLength++;
            }
            else
            {
                this._StepLength = 1;
            }
            SetProcessMessage("正在执行删除操作...");
            if (CompressData.IsDirectory)
            {
                DeleteFile(CompressData.DeletePath);
            }
            else
            {
                File.Delete(CompressData.DeletePath);
            }
            double currentProcessValue = GetInstallProcess();
            //检验当前进度值
            if (currentProcessValue < _MaxProcessValue)
                SetInstallProcess(_MaxProcessValue);
            SetProcessMessage("文件删除完成...");
        }

        /// <summary>
        /// 获取目录中文件数量
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <returns></returns>
        public int GetFileCount(DirectoryInfo dirInfo)
        {
            int totalCount = 0;
            totalCount += dirInfo.GetFiles().Length;
            foreach (var subDir in dirInfo.GetDirectories())
            {
                totalCount += GetFileCount(subDir);
            }
            return totalCount;
        }

        /// <summary>
        /// 删除目录及子文件
        /// </summary>
        /// <param name="dirInfo"></param>
        public void DeleteFile(string directory)
        {
            //删除目录中的文件
            string[] files = Directory.GetFiles(directory);//dirInfo.GetFiles();
            if (files != null)
            {
                string filePath = "";
                for (int i = 0; i < files.Length; i++)
                {
                    filePath = files[i];
                    SetProcessMessage(_PMHeader + files[i]);
                    //判断文件路径长度是否超出250个字符
                    if (filePath.Length > 250) throw new Exception("检测到长路径的文件，请使用当前程序目录中的[LongPathTool_v2.20]工具进行删除！");

                    File.Delete(files[i]);
                    this._StepIndex++;
                    //设置当前执行进度
                    if ((_StepIndex % _StepLength == 0) && (_ProcessValue < _MaxProcessValue))
                    {
                        _ProcessValue += 1;
                        SetInstallProcess(_ProcessValue);
                    }
                }
            }
            string[] dirs = Directory.GetDirectories(directory);//.GetDirectories();
            if (dirs != null)
            {
                for (int i = 0; i < dirs.Length; i++)
                {
                    DeleteFile(dirs[i]);
                }
            }
            Directory.Delete(directory, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void DeleteFileByProcess(string path)
        {
            string workDirectory = Directory.GetParent(path).FullName;
            if (_DeleteCommandProcess == null)
            {
                //初始化删除命令进程
                _DeleteCommandProcess = new Process();
                //设置进程启动参数
                _DeleteCommandProcess.StartInfo = new ProcessStartInfo();
                _DeleteCommandProcess.StartInfo.FileName = "del";
                _DeleteCommandProcess.StartInfo.CreateNoWindow = true;
                _DeleteCommandProcess.StartInfo.UseShellExecute = false;
                _DeleteCommandProcess.StartInfo.WorkingDirectory = workDirectory;
                _DeleteCommandProcess.StartInfo.RedirectStandardInput = true;
                _DeleteCommandProcess.StartInfo.RedirectStandardOutput = true;
                _DeleteCommandProcess.StartInfo.RedirectStandardError = true;
                //设置删除命令
                if (CompressData.IsDirectory)
                    _DeleteCommandProcess.StartInfo.Arguments = string.Format(" /F /S /Q {0}", Path.GetDirectoryName(path));
                else
                    _DeleteCommandProcess.StartInfo.Arguments = string.Format(" /F /S /Q {0}", Path.GetFileName(path));
            }
            else
            {
                FileInfo fi = new FileInfo(path);
                fi.Delete();
                //重新设置进程启动参数
                _DeleteCommandProcess.StartInfo.WorkingDirectory = workDirectory;
                //设置删除命令
                if (CompressData.IsDirectory)
                    _DeleteCommandProcess.StartInfo.Arguments = string.Format(" /F /S /Q {0}", Path.GetDirectoryName(path));
                else
                    _DeleteCommandProcess.StartInfo.Arguments = string.Format(" /F /S /Q {0}", Path.GetFileName(path));
            }
            //启动程序
            _DeleteCommandProcess.Start();
            //等待删除完成
            _DeleteCommandProcess.WaitForExit(5000);        //等待5秒的时间来删除
            _DeleteCommandProcess.Close();
        }
    }
}
