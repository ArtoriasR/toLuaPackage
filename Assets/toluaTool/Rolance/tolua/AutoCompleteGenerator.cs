#if ANDROID
using System;
using UnityEngine;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace Rolance.tolua
{
    /// <summary>
    /// 生成自动提示类
    /// </summary>
    public class AutoCompleteGenerator
    {

        private static Type type;
        static FieldInfo[] fields = null;
        private static string exportName;
        public static void Generate(Type temptype, string tempexportName)
        {
            type = temptype;
            exportName = tempexportName;

            if (type.IsInterface && type != typeof(System.Collections.IEnumerator))
            {
                return;
            }

            AutoCompleteExport.AddExportClass(exportName, type);

            if (type.IsEnum)
            {
                GenEnum();
            }
            else
            {
                InitMethods();
                InitProperty();
            }

            AutoCompleteExport.ExportClass(exportName);
            AutoCompleteExport.ExportFile(exportName);
        
        }

        static void GenEnum()
        {
            fields = type.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static);
            List<FieldInfo> list = new List<FieldInfo>(fields);

            for (int i = list.Count - 1; i > 0; i--)
            {
                if (IsObsolete(list[i]))
                {
                    list.RemoveAt(i);
                }
            }

            fields = list.ToArray();
            for (int i = 0; i < fields.Length; i++)
            {
                AutoCompleteExport.AddExportEnum(exportName, fields[i]);
            }
        }

        static void InitMethods()
        {
            bool flag = false;

            if (baseType != null || isStaticClass)
            {
                binding |= BindingFlags.DeclaredOnly;
                flag = true;
            }

            List<MethodInfo> list = new List<MethodInfo>();
            list.AddRange(type.GetMethods(BindingFlags.Instance | binding));

            Type tempType = type.BaseType;
            while (tempType.BaseType != null)
            {
                list.AddRange(tempType.GetMethods(BindingFlags.Instance | binding));
                tempType = tempType.BaseType;
            }

            for (int i = list.Count - 1; i >= 0; --i)
            {
                //去掉操作符函数
                if (list[i].Name.StartsWith("op_") || list[i].Name.StartsWith("add_") || list[i].Name.StartsWith("remove_"))
                {
                    if (!IsNeedOp(list[i].Name))
                    {
                        list.RemoveAt(i);
                    }

                    continue;
                }

                //扔掉 unity3d 废弃的函数                
                if (ToLuaExport.IsObsolete(list[i]))
                {
                    list.RemoveAt(i);
                }
                else
                {
                    AutoCompleteExport.AddExportMethod(exportName, list[i]);
                }
            }
        }

        static void InitProperty()
        {
            fields = type.GetFields(BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance | binding);

            List<FieldInfo> fieldList = new List<FieldInfo>();
            fieldList.AddRange(fields);

            for (int i = 0; i < fieldList.Count; i++)
            {
                AutoCompleteExport.AddExportProperty(exportName, fieldList[i]);
            }
        }



        public static bool IsObsolete(MemberInfo mb)
        {
            object[] attrs = mb.GetCustomAttributes(true);

            for (int j = 0; j < attrs.Length; j++)
            {
                Type t = attrs[j].GetType();

                if (t == typeof(System.ObsoleteAttribute) || t == typeof(NoToLuaAttribute) || t == typeof(MonoPInvokeCallbackAttribute) ||
                    t.Name == "MonoNotSupportedAttribute" || t.Name == "MonoTODOAttribute") // || t.ToString() == "UnityEngine.WrapperlessIcall")
                {
                    return true;
                }
            }

            if (IsMemberFilter(mb))
            {
                return true;
            }

            return false;
        }    
    }
}
#endif