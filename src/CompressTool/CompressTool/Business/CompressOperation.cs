using CompressTool.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CompressTool
{
    /// <summary>
    /// 解压操作
    /// </summary>
    public class CompressOperation : Operation
    {
        /// <summary>
        /// 开始解压操作
        /// </summary>
        public override void StartOperate()
        {
            try
            {
                DecompressionFile();           //解压数据文件
                OperationCompete();            //解压完成
            }
            catch (Exception ex)
            {
                if (ExcaptionHandle != null)
                    ExcaptionHandle(ex.Message);
            }
        }

        /// <summary>
        /// 解压数据文件
        /// </summary>
        /// <param name="maxProcessValue">进度占比</param>
        private void DecompressionFile()
        {
            SetProcessMessage("正在打开压缩文件，请稍候...");
            double maxProcessValue = 100;       //进度最大值
            FileHelper fh = new FileHelper();       //创建文件辅助类
            fh.DecompressionAppData(CompressData.CompressFilePath, CompressData.CompressToDirectory, "释放文件到：", 100, this);
            double currentProcessValue = GetInstallProcess();
            //检验当前进度值
            if (currentProcessValue < maxProcessValue)
                SetInstallProcess(maxProcessValue);
            SetProcessMessage("文件解压完成...");
        }
    }
}
