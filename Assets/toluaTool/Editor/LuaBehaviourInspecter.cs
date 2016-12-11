using UnityEngine;
using System.Collections;
using UnityEditor;
using LuaInterface;
using System.IO;
using System;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(LuaBehaviour))]
public class LuaBehaviourInspecter : Editor {
    /// <summary>
    /// 引用字典
    /// string名字
    /// Object引用
    /// </summary>
    private Dictionary<string, UnityEngine.Object> reflectDict = new Dictionary<string, UnityEngine.Object>();
    /// <summary>
    /// 类型字典
    /// string名字
    /// string类型
    /// </summary>
    private Dictionary<string, string> keyDic = new Dictionary<string, string>();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        LuaBehaviour behaviour = (target as LuaBehaviour);

        if (GUILayout.Button("GetArgs"))
        {
            reflectDict.Clear();
            keyDic.Clear();

            LuaState luaState = new LuaState();
            luaState.Start();

            luaState.OpenLibs(LuaDLL.luaopen_pb);
            luaState.OpenLibs(LuaDLL.luaopen_struct);
            luaState.OpenLibs(LuaDLL.luaopen_lpeg);

            luaState.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
            luaState.OpenLibs(LuaDLL.luaopen_cjson);
            luaState.LuaSetField(-2, "cjson");

            luaState.OpenLibs(LuaDLL.luaopen_cjson_safe);
            luaState.LuaSetField(-2, "cjson.safe");

            LuaBinder.Bind(luaState);
            LuaSupport.AddSearchPath(luaState);
            luaState.Require("Define");

            luaState.DoFile(behaviour.luaFileName);

            string ctorName = behaviour.luaFileName + ".create";

            LuaFunction luaClassCreate = luaState.GetFunction(ctorName);
            LuaTable luaClass = luaClassCreate.Call(behaviour.gameObject)[0] as LuaTable;
            LuaTable luaClassArgs = luaClass["initParamNames"] as LuaTable;

            for (int i = 1; i <= luaClassArgs.Length; i++)
            {
                Debug.Log("show args:" + i + ",args:" + luaClassArgs[i]);
                string fullArgs = Convert.ToString(luaClassArgs[i]);
                string[] typeArr = fullArgs.Split(new char[] { '=' });
                string typeName = typeArr[0];
                string t = typeArr[1];
                UnityEngine.Object obj = default(UnityEngine.Object);
                
                if (!reflectDict.ContainsKey(typeName))
                {
                    reflectDict.Add(typeName, obj);
                    keyDic.Add(typeName, t);
                }
            }
            luaState.Dispose();
        }
        Save();
        Load();
        Draw();
    }
    /// <summary>
    /// GUI上绘制参数自定义框
    /// </summary>
    void Draw()
    {
        foreach (var item in keyDic)
        {
            string key = item.Key;
            string t = item.Value;
            UnityEngine.Object value = reflectDict[key];
            reflectDict[key] = EditorGUILayout.ObjectField(new GUIContent(key), value, GetType(t), true, null);
            //value = EditorGUI.ObjectField(new Rect(0, 0, 250, 40), new GUIContent(item.Key.ToString()), value, (Type)item.Value, true);
        }
    }
    /// <summary>
    /// 反序列化获取数据
    /// </summary>
    void Load()
    {
        keyDic.Clear();
        reflectDict.Clear();
        SerializedProperty targetKeyList = serializedObject.FindProperty("keyList");
        SerializedProperty targetValueList = serializedObject.FindProperty("valueList");
        SerializedProperty targetTypeList = serializedObject.FindProperty("typeList");

        for (int i = 0; i < targetKeyList.arraySize; i++)
        {
            //名字
            string key = targetKeyList.GetArrayElementAtIndex(i).stringValue;
            //引用、值
            UnityEngine.Object value = targetValueList.GetArrayElementAtIndex(i).objectReferenceValue;
            //类型
            string t = targetTypeList.GetArrayElementAtIndex(i).stringValue;

            keyDic.Add(key, t);
            reflectDict.Add(key, value);
        }
    }
    /// <summary>
    /// 序列化存储数据
    /// </summary>
    void Save()
    {
        if (keyDic.Count > 0)
        {
            SerializedProperty targetKeyList = serializedObject.FindProperty("keyList");
            SerializedProperty targetValueList = serializedObject.FindProperty("valueList");
            SerializedProperty targetTypeList = serializedObject.FindProperty("typeList");

            targetKeyList.ClearArray();
            targetValueList.ClearArray();
            targetTypeList.ClearArray();

            foreach (var item in keyDic)
            {
                //名字
                targetKeyList.InsertArrayElementAtIndex(targetKeyList.arraySize);
                SerializedProperty key = targetKeyList.GetArrayElementAtIndex(targetKeyList.arraySize - 1);
                key.stringValue = item.Key;
                //引用
                targetValueList.InsertArrayElementAtIndex(targetValueList.arraySize);
                SerializedProperty value = targetValueList.GetArrayElementAtIndex(targetValueList.arraySize - 1);
                value.objectReferenceValue = reflectDict[item.Key];
                //类型
                targetTypeList.InsertArrayElementAtIndex(targetTypeList.arraySize);
                SerializedProperty t = targetTypeList.GetArrayElementAtIndex(targetTypeList.arraySize - 1);
                t.stringValue = item.Value;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    /// <summary>
    /// From:http://answers.unity3d.com/questions/206665/typegettypestring-does-not-work-in-unity.html
    /// Author:jahroy
    /// </summary>
    /// <param name="TypeName"></param>
    /// <returns></returns>
    public static Type GetType(string TypeName)
    {
        // Try Type.GetType() first. This will work with types defined
        // by the Mono runtime, in the same assembly as the caller, etc.
        var type = Type.GetType(TypeName);

        // If it worked, then we're done here
        if (type != null)
            return type;

        // If the TypeName is a full name, then we can try loading the defining assembly directly
        if (TypeName.Contains("."))
        {

            // Get the name of the assembly (Assumption is that we are using 
            // fully-qualified type names)
            var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));

            // Attempt to load the indicated Assembly
            var assembly = Assembly.Load(assemblyName);
            if (assembly == null)
                return null;

            // Ask that assembly to return the proper Type
            type = assembly.GetType(TypeName);
            if (type != null)
                return type;

        }

        // If we still haven't found the proper type, we can enumerate all of the 
        // loaded assemblies and see if any of them define the type
        var currentAssembly = Assembly.GetExecutingAssembly();
        var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
        foreach (var assemblyName in referencedAssemblies)
        {

            // Load the referenced assembly
            var assembly = Assembly.Load(assemblyName);
            if (assembly != null)
            {
                // See if that assembly defines the named type
                type = assembly.GetType(TypeName);
                if (type != null)
                    return type;
            }
        }

        // The type just couldn't be found...
        return null;

    }
}
