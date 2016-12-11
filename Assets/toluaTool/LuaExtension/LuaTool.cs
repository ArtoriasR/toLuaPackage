using LuaInterface;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Reflection;


public static class LuaTool
{
    static public LuaTable CreateLuaTable()
    {
        return  (LuaTable)LuaSupport.lua.DoString("return {}")[0];
    }

    static public LuaTable CreateLuaTable(IEnumerable objs)
    {
        var table = CreateLuaTable();
        int index = 1;
        foreach (var obj in objs)
        {
            table[index] = obj;
            index++;
        }
        return table;
    }

    static public LuaTable CreateLuaTable(IList objs)
    {
        var table = CreateLuaTable();
        int index = 1;
        foreach (var obj in objs)
        {
            table[index] = obj;
            index++;
        }
        return table;
    }

    static public LuaTable CreateLuaTable(IDictionary objs)
    {
        var table = CreateLuaTable();

        foreach (var key in objs.Keys)
        {
            table[key.ToString()] = objs[key];
        }
        return table;
    }

    public static LuaTable toLuaTable(this IEnumerable objs)
    {
        return CreateLuaTable(objs);
    }

    public static LuaTable toLuaTable(this IList objs)
    {
        return CreateLuaTable(objs);
    }

    public static LuaTable toLuaTable(this IDictionary objs)
    {
        return CreateLuaTable(objs);
    }

    public static List<object> toList(LuaTable table)
    {
        List<object> list = new List<object>();
        object[] tableArr = table.ToArray();
        for (int i = 0; i < tableArr.Length; i++)
        {
            list.Add(tableArr[i]);
        }
        return list;
    }

    public static object[] toArray(LuaTable table)
    {
        return table.ToArray();
    }

    public static string[] toStringArray(LuaTable table)
    {
        object[] arr = table.ToArray();
        int length = arr.GetLength(0);
        string[] strArr = new string[length];
        for (int i = 0; i < length; i++)
        {
            strArr[i] = arr[i].ToString();
        }
        return strArr;
    }

    public static Dictionary<object, object> toDict(LuaTable table)
    {
        Dictionary<object, object> dict = new Dictionary<object, object>();
        LuaDictTable dictTable = table.ToDictTable();
        
        foreach (var item in dictTable)
        {
            dict.Add(item.Key, item.Value);
        }
        return dict;
    }

    public static void Debug(LuaTable tab)
    {
        object[] objArr = tab.ToArray();

    }
}