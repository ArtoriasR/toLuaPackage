using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rolance
{
    public class Utils
    {
        public static string addArgs(int idx, string value)
        {
            //return "${" + idx + ":" + idx.ToString() + "_" + value + "}";//idx.ToString() + "_"为了避免sb自动选中了相同的类型
            return "${" + idx + " " + value + "}";//不需要加上idx了，因为提示里已经加上参数名字，中间加上空格，前面是类名，后面是参数名
        }

        public static string cutNamespace(string str)
        {
            string[] sArray = str.Split('.');
            string nameArgs = sArray[sArray.Length - 1];
            return nameArgs;
        }
    }
}
