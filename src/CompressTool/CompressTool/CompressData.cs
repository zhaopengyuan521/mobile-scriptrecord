using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompressTool
{
    /// <summary>
    /// 解压配置信息
    /// </summary>
    public static class CompressData
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        public static OperationType OperationType { get; set; }

        /// <summary>
        /// 压缩包文件路径
        /// </summary>
        public static string CompressFilePath { get; set; }

        /// <summary>
        /// 解压到目录
        /// </summary>
        public static string CompressToDirectory { get; set; }

        /// <summary>
        /// 删除的文件路径
        /// </summary>
        public static string DeletePath { get; set; }

        /// <summary>
        /// 是否目录
        /// </summary>
        public static bool IsDirectory
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 操作类型
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// 解压文件
        /// </summary>
        Compress,
        /// <summary>
        /// 删除文件
        /// </summary>
        Delete
    }
}
