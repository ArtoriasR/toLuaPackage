using System.Reflection;
using System.Collections.Generic;

namespace Rolance.tolua
{
    public class ExportMethod
    {
        public string methodName = "";

        public bool isSetter = false;
        public bool isGetter = false;

        public bool isStaticFunc = false;
        public bool isMemberFunc = false;

        public MethodInfo info;
        public ExportClass ec;
        public void Export()
        {
            //开始处理setter getter
            string argsName = "";
            if (isSetter && isGetter)
            {
                argsName = "_" + info.ReturnParameter.ToString();
                if (info.IsStatic == false)
                {
                    Rolance.FileHelper.Instance.Write(ec.exportClassName, "\t\t{\"trigger\":\"" + ec.className + "." + methodName + "\\t(R&W" + argsName + ")\",\"contents\":\"" + Rolance.Utils.addArgs(1, ec.className) + "." + methodName + "\"},");
                }
                else
                {
                    Rolance.FileHelper.Instance.Write(ec.exportClassName, "\t\t{\"trigger\":\"" + ec.className + "." + methodName + "\\t(R&W" + argsName + ")\",\"contents\":\"" + ec.className + "." + methodName + "\"},");
                }
                return;
            }
            else if (isSetter)
            {
                argsName = "_" + info.GetParameters()[0].ParameterType.ToString();
                if (info.IsStatic == false)
                {
                    Rolance.FileHelper.Instance.Write(ec.exportClassName, "\t\t{\"trigger\":\"" + ec.className + "." + methodName + "\t(W" + argsName + ")\",\"contents\":\"" + Rolance.Utils.addArgs(1, ec.className) + "." + methodName + "\"},");
                }
                else
                {
                    Rolance.FileHelper.Instance.Write(ec.exportClassName, "\t\t{\"trigger\":\"" + ec.className + "." + methodName + "\t(W" + argsName + ")\",\"contents\":\"" + ec.className + "." + methodName + "\"},");
                }
                return;
            }
            else if (isGetter)
            {
                argsName = "_" + info.ReturnParameter.ToString();
                if (info.IsStatic == false)
                {
                    Rolance.FileHelper.Instance.Write(ec.exportClassName, "\t\t{\"trigger\":\"" + ec.className + "." + methodName + "\t(R" + argsName + ")\",\"contents\":\"" + Rolance.Utils.addArgs(1, ec.className) + "." + methodName + "\"},");
                }
                else
                {
                    Rolance.FileHelper.Instance.Write(ec.exportClassName, "\t\t{\"trigger\":\"" + ec.className + "." + methodName + "\t(R" + argsName + ")\",\"contents\":\"" + ec.className + "." + methodName + "\"},");
                }
                    
                return;
            }

            //开始处理函数
            ParameterInfo[] infolist = info.GetParameters();
            
            //+ list[i].GetParameters
            string triggerArgs = "";
            string contentArgs = "";

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
                //获取不包含命名空间的类名
                string fullArgs = infolist[z].ParameterType.ToString();
                string[] sArray = fullArgs.Split('.');
                string nameArgs = sArray[sArray.Length - 1];
                int idx = z + fixIdx;
                //args += (infolist[z].ParameterType.ToString() + ",");
                //
                if (z != (infolist.Length - 1))
                {
                    //处理\t后面的参数提示
                    triggerArgs += (nameArgs + ":" + infolist[z].Name + ",");
                    //处理选中后展示在view里的代码提示
                    contentArgs += (Rolance.Utils.addArgs(idx, nameArgs + ":" + infolist[z].Name) + ",");
                }
                //最后一个参数的处理
                else
                {
                    triggerArgs += nameArgs + ":" + infolist[z].Name;
                    contentArgs += (Rolance.Utils.addArgs(idx, nameArgs + ":" + infolist[z].Name));
                }
            }
            if (triggerArgs.Length > 0) 
            { 
                triggerArgs = triggerArgs.Insert(0, "\\t(");
                triggerArgs = triggerArgs.Insert(triggerArgs.Length, ")");
            }

            string returnvalue = info.ReturnParameter.ToString();
            //如果有返回值，才会添加到表达式里
            string returnArgs = info.ReturnParameter.ToString() == "Void" ? "" : Rolance.Utils.addArgs(1, returnvalue) + " ";

            //如果是成员函数
            if (info.IsStatic == false)
            {
                //要从修正位中减一才是类名的idx
                int classNameIdx = fixIdx - 1;
                Rolance.FileHelper.Instance.Write(ec.exportClassName, "\t\t{\"trigger\":\"" + ec.className + operationSymbol + info.Name + triggerArgs + "\",\"contents\":\"" + returnArgs + Rolance.Utils.addArgs(classNameIdx, info.DeclaringType.ToString()) + operationSymbol + info.Name + "(" + contentArgs + ")\"},");
            }
            else
            {
                Rolance.FileHelper.Instance.Write(ec.exportClassName, "\t\t{\"trigger\":\"" + ec.className + operationSymbol + info.Name + triggerArgs + "\",\"contents\":\"" + returnArgs + info.DeclaringType.ToString() + operationSymbol + info.Name + "(" + contentArgs + ")\"},");
            }
        }
    }
}
