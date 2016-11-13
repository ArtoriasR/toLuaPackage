using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections.Generic;
using Rolance;
using Ionic.Zip;

namespace Rolance.tolua
{
    /// <summary>
    /// 生成自动提示类的接口类
    /// </summary>
    public class AutoCompleteExport
    {
        public static Dictionary<string, ExportClass> cacheDict = new Dictionary<string, ExportClass>();

        public static bool isShowClassName = false;
        public static void ExportHead(string fileName)
        {
            Rolance.FileHelper.Instance.Write(fileName, "{\n\t\"scope\": \"source.lua -variable.other.lua\",\n\n\t\"completions\":\n\t[");
        }
        public static void ExportEnd(string fileName)
        {
            Rolance.FileHelper.Instance.Write(fileName, "\t]\n}");
        }

        public static void ExportFile(string fileName)
        {
            Rolance.FileHelper.Instance.SaveFile(fileName, "sublime-completions");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        private static ExportClass GetExportClass(string className)
        {
            ExportClass ec;
            if (cacheDict.ContainsKey(className))
            {
                ec = cacheDict[className];
            }
            else
            {
                ec = new ExportClass();

                cacheDict.Add(className,ec);
            }

            return ec;
        }

        public static void ExportClass(string className)
        {
            ExportClass ec = cacheDict[className];
            ec.Export();
        }

        public static void AddExportClass(string exportClassName,Type t)
        {
            ExportClass ec = GetExportClass(exportClassName);
            ec.type = t;
        }

        public static void AddExportMethod(string exportClassName, MethodInfo info)
        {
            ExportClass ec = GetExportClass(exportClassName);
            ec.fullClassName = info.DeclaringType.ToString();
            ec.className = Rolance.Utils.cutNamespace(info.DeclaringType.ToString());
            ec.exportClassName = exportClassName;
            bool isGetter = false;
            bool isSetter = false;
            //因为有函数重载，所以这里不能直接使用名字，避免名字重复
            string methodName = info.Name + info.GetHashCode();
            isGetter = info.Name.StartsWith("get_");
            isSetter = info.Name.StartsWith("set_");
            if (isGetter || isSetter)
            {
                methodName = info.Name.Substring(4);
            }
            ExportMethod em = ec.GetMethod(methodName);
            em.isGetter = em.isGetter || isGetter;
            em.isSetter = em.isSetter || isSetter;
            em.methodName = methodName;

            //方法的处理
            em.isStaticFunc = info.IsStatic;
            em.info = info;
        }

        public static void AddExportProperty(string exportClassName, FieldInfo info)
        {
            ExportClass ec = GetExportClass(exportClassName);
            ec.fullClassName = info.DeclaringType.ToString();
            ec.className = Rolance.Utils.cutNamespace(info.DeclaringType.ToString());
            ec.exportClassName = exportClassName;
            
            string propertyName = info.Name;
            ExportProperty ep = ec.GetProperty(propertyName);
            ep.propertyName = propertyName;
            
            ep.fi = info;
        }

        public static void AddExportEnum(string exportClassName,FieldInfo info)
        {
            ExportClass ec = GetExportClass(exportClassName);
            ec.fullClassName = info.DeclaringType.ToString();
            ec.className = Rolance.Utils.cutNamespace(info.DeclaringType.ToString());
            ec.exportClassName = exportClassName;

            string enumName = info.Name;
            ExportEnum ep = ec.GetEnum(enumName);
            ep.enumName = enumName;

            ep.fi = info;
        }

        /// <summary>
        /// 打包所有自动完成文件
        /// </summary>
        /// <param name="zipName"></param>
        public static void ZipAutoCompletionToFile(string zipName = "tolua_autocomplete")
        {

            using (ZipFile zip = new ZipFile())
            {
                string fullpath = Application.dataPath + "/../output";
                zip.UpdateDirectory(fullpath);
                zip.Save(zipName + ".sublime-package");
            }
        }

        public static void Clear()
        {
            cacheDict.Clear();
        }
    }
}
