using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rolance.tolua
{
    public class ExportSetting
    {
        /// <summary>
        /// 是否输出完整类名代码提示
        /// </summary>
        public static bool ExportNameSpace = true;
        /// <summary>
        /// 是否直接输出成为SublimeText插件
        /// </summary>
        public static bool ZipFile = true;
        /// <summary>
        /// 是否包含父类成员
        /// </summary>
        public static bool exportBaseTypeMethod = true;
    }
}
