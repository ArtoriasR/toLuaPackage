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
            
            if (fi.IsStatic)
            {
                Rolance.FileHelper.Instance.Write(ec.exportClassName, "\t\t{\"trigger\":\"" + ec.className + "." + propertyName + "\\t(type:" + args + ")\",\"contents\":\"" + ec.className + "." + propertyName + "\"},");
            }
            else
            {
                Rolance.FileHelper.Instance.Write(ec.exportClassName, "\t\t{\"trigger\":\"" + ec.className + "." + propertyName + "\\t(type:" + args + ")\",\"contents\":\"" + Rolance.Utils.addArgs(1, ec.className) + "." + propertyName + "\"},");
            }
        }
    }
}
