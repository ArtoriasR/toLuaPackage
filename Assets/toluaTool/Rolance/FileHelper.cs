using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Rolance
{
    public class FileHelper
    {
        private static FileHelper _instance;


        private string pathPrefix = "/../output/";
        private Dictionary<string, string> dict = new Dictionary<string, string>();

        public static FileHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FileHelper();
                }
                return _instance;
            }
        }
        /// <summary>
        /// 写log
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="value"></param>
        /// <param name="insertLineBreak"></param>
        public void Write(string fileName, string value, bool insertLineBreak = true)
        {
            string content;
            if (dict.ContainsKey(fileName))
            {
                content = dict[fileName];
            }
            else
            {
                content = "";
                dict.Add(fileName, content);
            }
            if (insertLineBreak)
                content += (value + "\n");
            else
                content += value;
            
            dict[fileName] = content;
        }
        /// <summary>
        /// 保存log文件
        /// 每次都有gc，性能不怎样，请勿频繁调用
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveFile(string fileName,string fileType = "txt")
        {
            string sPath = Application.dataPath + pathPrefix;
            if (!Directory.Exists(sPath))
            {

                Directory.CreateDirectory(sPath);

            }
            if (dict.ContainsKey(fileName))
            {
                string stringContent = dict[fileName];
                using (FileStream fs = new FileStream(sPath + fileName + "." + fileType, FileMode.Create))
                {
                    //将字符串转成byte数组
                    byte[] byteFile = Encoding.UTF8.GetBytes(stringContent);
                    //参数：要写入到文件的数据数组，从数组的第几个开始写，一共写多少个字节
                    fs.Write(byteFile, 0, byteFile.Length);
                    Debug.Log("保存成功！");
                }
            }
        }

        public void DelFile(string directory = "")
        {
            string sPath = string.Empty;
            if (string.IsNullOrEmpty(directory))
            {
                sPath = Application.dataPath + pathPrefix;
            }
            else
            {
                sPath = directory;
            }
            Directory.Delete(sPath,true);
        }
    }

}
