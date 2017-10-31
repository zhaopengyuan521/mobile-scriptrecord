using AppiumUtility.Logger;
using AppiumUtility.Models.Inspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AppiumUtility.Utility
{
    /// <summary>
    /// 通用类
    /// </summary>
    public static class CommonUtility
    {
        /// <summary>
        /// converts the xml page source into a UIAutomatorNode (with it's children)
        /// </summary>
        /// <param name="pageSource"></param>
        /// <returns></returns>
        public static INode ConvertToUIAutomatorNode(string pageSource)
        {
            var nodeStack = new Stack<AbstractNode>();
            INode root = null;
            bool isApple = false;

            using (var reader = XmlReader.Create(new StringReader(pageSource)))
            {
                while (reader.Read())
                {
                    if ("hierarchy" == reader.Name)
                    {
                        continue;
                    }
                    else if ("AppiumAUT" == reader.Name)
                    {
                        isApple = true;
                        continue;
                    }

                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            AbstractNode node = null;
                            if (isApple)
                            {
                                node = new UIAutomatorAppleNode(reader);
                            }
                            else
                            {
                                node = new UIAutomatorAndroidNode(reader);
                            }

                            if (reader.IsEmptyElement)
                            {
                                if (0 == nodeStack.Count)
                                {
                                    root = node;
                                }
                                else
                                {
                                    nodeStack.Peek().Children.Add(node);
                                }
                            }
                            else
                            {
                                nodeStack.Push(node);
                            }
                            break;
                        case XmlNodeType.EndElement:
                            var child = nodeStack.Pop();
                            if (nodeStack.Count == 0)
                            {
                                root = child;
                            }
                            else
                            {
                                var parent = nodeStack.Peek();
                                parent.Children.Add(child);
                            }
                            break;
                    }
                }
            }
            return root;
        }

        /// <summary>
        /// 对象序列化Json字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ObjectToJson<T>(T data)
        {
            string jsonData = "";
            if (data == null) return jsonData;      //参数验证
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    DataContractJsonSerializer jsonSerialize = new DataContractJsonSerializer(data.GetType());
                    jsonSerialize.WriteObject(stream, data);
                    jsonData = Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception ex)
            {
                AppLog.CreateAppLog().OutputLogData(string.Format("对象[{0}]序列化Json失败，{1}", data.GetType(), ex.Message), ex);
            }
            return jsonData;
        }

        /// <summary>
        /// Json字符串反序列化对象
        /// </summary>
        /// <typeparam name="T">反序列化类型</typeparam>
        /// <param name="jsonData">Json数据</param>
        /// <returns></returns>
        public static T JsonToObject<T>(string jsonData)
        {
            T result = default(T);          //反序列化对象类型
            if (string.IsNullOrEmpty(jsonData)) return result;          //参数验证
            try
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
                {
                    DataContractJsonSerializer jsonSerialize = new DataContractJsonSerializer(typeof(T));
                    result = (T)jsonSerialize.ReadObject(stream);
                }
            }
            catch (Exception ex)
            {
                AppLog.CreateAppLog().OutputLogData(string.Format("Json数据[{0}]反序列化类型[{1}]失败，{2}", jsonData, typeof(T), ex.Message), ex);
            }
            return result;
        }
    }
}
