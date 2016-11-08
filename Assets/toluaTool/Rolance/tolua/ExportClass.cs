using System.Collections.Generic;
using System.Reflection;
using System;

namespace Rolance.tolua
{
    public class ExportClass
    {
        public string exportClassName = "";
        public string fullClassName = "";
        public string className = "";
        /// <summary>
        /// 只有通过AddExportClass初始化的才会有type，但是目前一定会调用AddExportClass，待优化
        /// </summary>
        public Type type = null;
        //public List<string> methodList = new List<string>();
        public Dictionary<string, ExportEnum> enumDict = new Dictionary<string, ExportEnum>();
        public Dictionary<string, ExportMethod> methodDict = new Dictionary<string, ExportMethod>();
        public Dictionary<string, ExportProperty> propertyDict = new Dictionary<string, ExportProperty>();

        
        public ExportMethod AddMethod(string methodName)
        {
            ExportMethod em = new ExportMethod();
            methodDict.Add(methodName, em);
            em.ec = this;
            return em;
        }
        public ExportMethod GetMethod(string methodName)
        {
            ExportMethod em;
            if (methodDict.ContainsKey(methodName))
            {
                em = methodDict[methodName];
            }
            else
            {
                em = new ExportMethod();
                methodDict.Add(methodName, em);
                em.ec = this;
            }

            return em;
        }

        public ExportProperty GetProperty(string methodName)
        {
            ExportProperty em;
            if (propertyDict.ContainsKey(methodName))
            {
                em = propertyDict[methodName];
            }
            else
            {
                em = new ExportProperty();
                propertyDict.Add(methodName, em);
                em.ec = this;
            }

            return em;
        }

        public ExportEnum GetEnum(string enumName)
        {
            ExportEnum em;
            if (enumDict.ContainsKey(enumName))
            {
                em = enumDict[enumName];
            }
            else
            {
                em = new ExportEnum();
                enumDict.Add(enumName, em);
                em.ec = this;
            }

            return em;
        }

        public void Export()
        {
            AutoCompleteExport.ExportHead(exportClassName);
            //输出类名
            if (ExportSetting.ExportNameSpace) {
                string nameWithNameSpace = type.Namespace + "." + type.Name;
                FileHelper.Instance.Write(exportClassName, "\t\t{\"trigger\":\"" + nameWithNameSpace + "\",\"contents\":\"" + nameWithNameSpace + "\"},");
            }

            foreach (var item in enumDict)
            {
                item.Value.Export();
            }

            foreach (var item in propertyDict)
            {
                item.Value.Export();
            }
            foreach (var item in methodDict)
            {
                item.Value.Export();
            }

            AutoCompleteExport.ExportEnd(exportClassName);
        }
    }
}
