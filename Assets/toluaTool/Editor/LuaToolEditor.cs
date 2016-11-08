using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using UnityEditor.ProjectWindowCallback;
using System.Text.RegularExpressions;
using System.Reflection;
using LuaInterface;
using System.Collections.Generic;
using Rolance.tolua;

public class LuaToolEditor
{
    static List<ToLuaMenu.BindType> allTypes = new List<ToLuaMenu.BindType>();
    public static string className = string.Empty;
    public static Type type = null;
    public static Type baseType = null;
    public static bool isStaticClass = true;
    static HashSet<string> usingList = new HashSet<string>();
    public static string wrapClassName = "";
    public static string exportName = "";
    static StringBuilder sb;

    static BindingFlags binding = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;
    static FieldInfo[] fields = null;
    static MetaOp op = MetaOp.None;

    //static List<MethodInfo> getItems = new List<MethodInfo>();   //特殊属性
    //static List<MethodInfo> setItems = new List<MethodInfo>();


    static PropertyInfo[] props = null;
    static List<PropertyInfo> propsPrivate = new List<PropertyInfo>();
    static List<PropertyInfo> propList = new List<PropertyInfo>();  //非静态属性
    static List<PropertyInfo> allProps = new List<PropertyInfo>();
    static EventInfo[] events = null;
    static List<EventInfo> eventList = new List<EventInfo>();
    public static HashSet<Type> eventSet = new HashSet<Type>();

    [MenuItem("Assets/Create/Lua Script", false, 80)]
    public static void CreatNewLua()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
        ScriptableObject.CreateInstance<MyDoCreateScriptAsset>(),
        GetSelectedPathOrFallback() + "/New Lua.lua",
        null,
       "Assets/toluaTool/Editor/template.lua");
    }

    public static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }


    [MenuItem("Lua/CreateAutoComplete")]
    public static void CreateAutoComplete()
    {
        Debug.Log("CreateAutoComplete");
        AutoCompleteExport.Clear();

        ExportSetting.ExportNameSpace = true;
        ExportSetting.ZipFile = false;
        
        if (EditorApplication.isCompiling)
        {
            EditorUtility.DisplayDialog("警告", "请等待编辑器完成编译再执行此功能", "确定");
            return;
        }

        if (!File.Exists(CustomSettings.saveDir))
        {
            Directory.CreateDirectory(CustomSettings.saveDir);
        }

        List<ToLuaMenu.BindType> allTypes = new List<ToLuaMenu.BindType>();
        allTypes.Clear();
        ToLuaMenu.BindType[] typeList = CustomSettings.customTypeList;

        ToLuaMenu.BindType[] list = GenBindTypes(typeList);
        ToLuaExport.allTypes.AddRange(ToLuaMenu.baseType);

        for (int i = 0; i < list.Length; i++)
        {
            ToLuaExport.allTypes.Add(list[i].type);
        }

        for (int i = 0; i < list.Length; i++)
        {
            ToLuaExport.type = list[i].type;
            Clear();
            className = list[i].name;
            type = list[i].type;
            isStaticClass = list[i].IsStatic;
            baseType = list[i].baseType;
            wrapClassName = list[i].wrapName;
            Generate(CustomSettings.saveDir);
        }

        if (ExportSetting.ZipFile)
        {
            AutoCompleteExport.ZipAutoCompletionToFile();
            Rolance.FileHelper.Instance.DelFile();
        }

        Debug.Log("Generate lua binding files over");
        ToLuaExport.allTypes.Clear();
        allTypes.Clear();
        AssetDatabase.Refresh();
    }

    public static void Generate(string dir)
    {
        if (type.IsInterface && type != typeof(System.Collections.IEnumerator))
        {
            return;
        }

        Debugger.Log("Begin Generate lua Wrap for class {0}", className);
        //Rolance.AutoCompleteExport.Clear();

        sb = new StringBuilder();
        usingList.Add("System");

        if (wrapClassName == "")
        {
            wrapClassName = className;
        }
        exportName = wrapClassName + "Wrap";

        AutoCompleteExport.AddExportClass(exportName,type);

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
            if (ToLuaExport.IsObsolete(list[i]))
            {
                list.RemoveAt(i);
            }
        }

        fields = list.ToArray();
        for (int i = 0; i < fields.Length; i++)
        {
            AutoCompleteExport.AddExportEnum(exportName, fields[i]);
        }
        /*
        sb.AppendLineEx("\tpublic static void Register(LuaState L)");
        sb.AppendLineEx("\t{");
        sb.AppendFormat("\t\tL.BeginEnum(typeof({0}));\r\n", className);

        for (int i = 0; i < fields.Length; i++)
        {
            sb.AppendFormat("\t\tL.RegVar(\"{0}\", get_{0}, null);\r\n", fields[i].Name);
        }

        sb.AppendFormat("\t\tL.RegFunction(\"IntToEnum\", IntToEnum);\r\n");
        sb.AppendFormat("\t\tL.EndEnum();\r\n");
        sb.AppendLineEx("\t}");

        for (int i = 0; i < fields.Length; i++)
        {
            sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
            sb.AppendFormat("\tstatic int get_{0}(IntPtr L)\r\n", fields[i].Name);
            sb.AppendLineEx("\t{");
            sb.AppendFormat("\t\tToLua.Push(L, {0}.{1});\r\n", className, fields[i].Name);
            sb.AppendLineEx("\t\treturn 1;");
            sb.AppendLineEx("\t}");
        }

        sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
        sb.AppendLineEx("\tstatic int IntToEnum(IntPtr L)");
        sb.AppendLineEx("\t{");
        sb.AppendLineEx("\t\tint arg0 = (int)LuaDLL.lua_tonumber(L, 1);");
        sb.AppendFormat("\t\t{0} o = ({0})arg0;\r\n", className);
        sb.AppendLineEx("\t\tToLua.Push(L, o);");
        sb.AppendLineEx("\t\treturn 1;");
        sb.AppendLineEx("\t}");
         * */
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
    


    static ToLuaMenu.BindType[] GenBindTypes(ToLuaMenu.BindType[] list, bool beDropBaseType = true)
    {
        List<ToLuaMenu.BindType> allTypes = new List<ToLuaMenu.BindType>(list);

        for (int i = 0; i < list.Length; i++)
        {
            for (int j = i + 1; j < list.Length; j++)
            {
                if (list[i].type == list[j].type)
                    throw new NotSupportedException("Repeat BindType:" + list[i].type);
            }

            if (ToLuaMenu.dropType.IndexOf(list[i].type) >= 0)
            {
                Debug.LogWarning(list[i].type.FullName + " in dropType table, not need to export");
                allTypes.Remove(list[i]);
                continue;
            }
            else if (beDropBaseType && ToLuaMenu.baseType.IndexOf(list[i].type) >= 0)
            {
                Debug.LogWarning(list[i].type.FullName + " is Base Type, not need to export");
                allTypes.Remove(list[i]);
                continue;
            }
            else if (list[i].type.IsEnum)
            {
                continue;
            }

            AutoAddBaseType(list[i], beDropBaseType);
        }

        return allTypes.ToArray();
    }

    static void AutoAddBaseType(ToLuaMenu.BindType bt, bool beDropBaseType)
    {
        Type t = bt.baseType;

        if (t == null)
        {
            return;
        }

        if (t.IsInterface)
        {
            Debugger.LogWarning("{0} has a base type {1} is Interface, use SetBaseType to jump it", bt.name, t.FullName);
            bt.baseType = t.BaseType;
        }
        else if (ToLuaMenu.dropType.IndexOf(t) >= 0)
        {
            Debugger.LogWarning("{0} has a base type {1} is a drop type", bt.name, t.FullName);
            bt.baseType = t.BaseType;
        }
        else if (!beDropBaseType || ToLuaMenu.baseType.IndexOf(t) < 0)
        {
            int index = allTypes.FindIndex((iter) => { return iter.type == t; });

            if (index < 0)
            {
#if JUMP_NODEFINED_ABSTRACT
                if (t.IsAbstract && !t.IsSealed)
                {
                    Debugger.LogWarning("not defined bindtype for {0}, it is abstract class, jump it, child class is {1}", t.FullName, bt.name);
                    bt.baseType = t.BaseType;
                }
                else
                {
                    Debugger.LogWarning("not defined bindtype for {0}, autogen it, child class is {1}", t.FullName, bt.name);
                    bt = new BindType(t);
                    allTypes.Add(bt);
                }
#else
                Debugger.LogWarning("not defined bindtype for {0}, autogen it, child class is {1}", t.FullName, bt.name);
                bt = new ToLuaMenu.BindType(t);
                allTypes.Add(bt);
#endif
            }
            else
            {
                return;
            }
        }
        else
        {
            return;
        }

        AutoAddBaseType(bt, beDropBaseType);
    }

    static bool IsNeedOp(string name)
    {
        if (name == "op_Addition")
        {
            op |= MetaOp.Add;
        }
        else if (name == "op_Subtraction")
        {
            op |= MetaOp.Sub;
        }
        else if (name == "op_Equality")
        {
            op |= MetaOp.Eq;
        }
        else if (name == "op_Multiply")
        {
            op |= MetaOp.Mul;
        }
        else if (name == "op_Division")
        {
            op |= MetaOp.Div;
        }
        else if (name == "op_UnaryNegation")
        {
            op |= MetaOp.Neg;
        }
        else if (name == "ToString" && !isStaticClass)
        {
            op |= MetaOp.ToStr;
        }
        else
        {
            return false;
        }


        return true;
    }
    static bool IsItemThis(PropertyInfo info)
    {
        MethodInfo md = info.GetGetMethod();

        if (md != null)
        {
            return md.GetParameters().Length != 0;
        }

        md = info.GetSetMethod();

        if (md != null)
        {
            return md.GetParameters().Length != 1;
        }

        return true;
    }

    //记录所有的导出类型
    public static List<Type> allTypes2 = new List<Type>();
    static bool BeDropMethodType(MethodInfo md)
    {
        Type t = md.DeclaringType;

        if (t == type)
        {
            return true;
        }

        return allTypes2.IndexOf(t) < 0;
    }

    public static void Clear()
    {
        className = null;
        type = null;
        baseType = null;
        isStaticClass = false;
        usingList.Clear();
        op = MetaOp.None;
        sb = new StringBuilder();
        fields = null;
        wrapClassName = "";

        
        //getItems.Clear();
        //setItems.Clear();
    }
}




class MyDoCreateScriptAsset : EndNameEditAction
{


    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        UnityEngine.Object o = CreateScriptAssetFromTemplate(pathName, resourceFile);
        ProjectWindowUtil.ShowCreatedAsset(o);
    }

    internal static UnityEngine.Object CreateScriptAssetFromTemplate(string pathName, string resourceFile)
    {
        string fullPath = Path.GetFullPath(pathName);
        StreamReader streamReader = new StreamReader(resourceFile);
        string text = streamReader.ReadToEnd();
        streamReader.Close();
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);
        text = Regex.Replace(text, "#NAME#", fileNameWithoutExtension);

        /*
        string text2 = Regex.Replace(fileNameWithoutExtension, " ", string.Empty);
        text = Regex.Replace(text, "#SCRIPTNAME#", text2);
        if (char.IsUpper(text2, 0))
        {
            text2 = char.ToLower(text2[0]) + text2.Substring(1);
            text = Regex.Replace(text, "#SCRIPTNAME_LOWER#", text2);
        }
        else
        {
            text2 = "my" + char.ToUpper(text2[0]) + text2.Substring(1);
            text = Regex.Replace(text, "#SCRIPTNAME_LOWER#", text2);
        }
        */

        bool encoderShouldEmitUTF8Identifier = true;
        bool throwOnInvalidBytes = false;
        UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
        bool append = false;
        StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
        streamWriter.Write(text);
        streamWriter.Close();
        AssetDatabase.ImportAsset(pathName);
        return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
    }

}