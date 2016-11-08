using System.Reflection;

namespace Rolance.tolua
{
    public class ExportProperty
    {
        public string propertyName = "";

        public ExportClass ec;
        public FieldInfo fi;

        public void Export()
        {
            string args = fi.FieldType.Name;
            string eName = ec.className;
            if (ExportSetting.exportBaseTypeMethod)
            {
                eName = ec.type.Name;
            }
            
            if (fi.IsStatic)
            {
                Rolance.FileHelper.Instance.Write(ec.exportClassName, "\t\t{\"trigger\":\"" + eName + "." + propertyName + "\\t(type:" + args + ")\",\"contents\":\"" + eName + "." + propertyName + "\"},");
            }
            else
            {
                Rolance.FileHelper.Instance.Write(ec.exportClassName, "\t\t{\"trigger\":\"" + eName + "." + propertyName + "\\t(type:" + args + ")\",\"contents\":\"" + Rolance.Utils.addArgs(1, eName) + "." + propertyName + "\"},");
            }
        }
    }
}
