using System.Reflection;

namespace Rolance
{
    public class ExportEnum
    {
        public string enumName = "";
        public FieldInfo fi;
        public ExportClass ec;
        public void Export()
        {
            Rolance.FileHelper.Instance.Write(ec.exportClassName, "\t\t{\"trigger\":\"" + fi.DeclaringType + "." + fi.Name + "\\t(type:" + ")\",\"contents\":\"" + fi.DeclaringType + "." + fi.Name + "\"},");
        }
    }
}
