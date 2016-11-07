using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections.Generic;
using Rolance;

namespace Rolance
{
    public class AutoCompleteExport
    {
        public static Dictionary<string, Rolance.ExportClass> cacheDict = new Dictionary<string, Rolance.ExportClass>();

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

        /*
        public static void ExportTrigger(string fileName,string trigger,string content)
        {
            Rolance.FileHelper.Instance.Write(fileName, "\t\t{\"trigger\":\"" + trigger + "\",\"contents\":\"" + content + "\"},");
        }

        public static void ExportTrigger(string fileName,MethodInfo info)
        {
            //开始处理setter getter
            bool isGetSeter = false;
            string attribute;
            isGetSeter = info.Name.StartsWith("get_");
            isGetSeter = isGetSeter || info.Name.StartsWith("set_");

            if (isGetSeter)
            {
                attribute = info.Name.Substring(4);
                //Rolance.FileHelper.Instance.Write(fileName, "\t\t{\"trigger\":\"" + info.DeclaringType.ToString() + "." + info.Name + "\"},");

                Rolance.FileHelper.Instance.Write(fileName, "\t\t{\"trigger\":\"" + cutNamespace(info.DeclaringType.ToString()) + "." + attribute + "\",\"contents\":\"" + addArgs(1, info.DeclaringType.ToString()) + "." + attribute + "\"},");
                return;
            }
            
            //开始处理函数
            ParameterInfo[] infolist = info.GetParameters();
            //+ list[i].GetParameters
            string args = "";
            
            int fixIdx = info.ReturnParameter.ToString() == "Void" ? 1 : 2;

            //依据不同的方法类型使用不同的调用符号
            string operationSymbol = ".";
            if (info.IsStatic == false)
            {
                //如果是成员函数
                operationSymbol = ":";
                //还要增加一位，这位把类名当作参数
                fixIdx++;
            }

            for (int z = 0; z < infolist.Length; z++)
            {
                string fullArgs = infolist[z].ParameterType.ToString();
                string[] sArray=fullArgs.Split('.');
                string nameArgs = sArray[sArray.Length-1];
                int idx = z + fixIdx;
                //args += (infolist[z].ParameterType.ToString() + ",");
                if (z!=(infolist.Length-1))
                    args += (addArgs(idx, nameArgs) + ",");
                else
                    args += (addArgs(idx, nameArgs));
            }

            string returnvalue = info.ReturnParameter.ToString();
            //如果有返回值，才会添加到表达式里
            string returnArgs = info.ReturnParameter.ToString() == "Void" ? "" : addArgs(1, returnvalue) + " ";

            //如果是成员函数
            if (info.IsStatic == false)
            {
                //要从修正位中减一才是类名的idx
                int classNameIdx = fixIdx - 1;
                Rolance.FileHelper.Instance.Write(fileName, "\t\t{\"trigger\":\"" + cutNamespace(info.DeclaringType.ToString()) + operationSymbol + info.Name + "\",\"contents\":\"" + returnArgs + addArgs(classNameIdx, info.DeclaringType.ToString()) + operationSymbol + info.Name + "(" + args + ")\"},");
            }
            else
            {
                Rolance.FileHelper.Instance.Write(fileName, "\t\t{\"trigger\":\"" + cutNamespace(info.DeclaringType.ToString()) + operationSymbol + info.Name + "\",\"contents\":\"" + returnArgs + info.DeclaringType.ToString() + operationSymbol + info.Name + "(" + args + ")\"},");
            }
        }


        /// <summary>
        /// 添加可填参数
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string addArgs(int idx,string value)
        {
            return "${" + idx + ":" + idx.ToString() + "_" + value + "}";//idx.ToString() + "_"为了避免sb自动选中了相同的类型
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>

        */
        /////////////////////////////正义的分割线，新的实现方案//////////////////////////////
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

        public static void AddExportMethod(string exportClassName, MethodInfo info)
        {
            ExportClass ec = GetExportClass(exportClassName);
            ec.fullClassName = info.DeclaringType.ToString();
            ec.className = Rolance.Utils.cutNamespace(info.DeclaringType.ToString());
            ec.exportClassName = exportClassName;
            bool isGetter = false;
            bool isSetter = false;
            string methodName = info.Name;
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

        public static void Clear()
        {
            cacheDict.Clear();
        }
    }
}
