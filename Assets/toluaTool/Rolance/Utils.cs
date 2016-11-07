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
            return "${" + idx + ":" + idx.ToString() + "_" + value + "}";//idx.ToString() + "_"为了避免sb自动选中了相同的类型
        }

        public static string cutNamespace(string str)
        {
            string[] sArray = str.Split('.');
            string nameArgs = sArray[sArray.Length - 1];
            return nameArgs;
        }
    }
}
