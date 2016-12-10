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
    private static Dictionary<string, UnityEngine.Object> dic = new Dictionary<string, UnityEngine.Object>();
    private static Dictionary<string, Type> keyDic = new Dictionary<string, Type>();
    GUILayoutOption[] option = new GUILayoutOption[] { GUILayout.Width(250) };
    //UnityEngine.Object obj = default(UnityEngine.Object);
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("GetArgs"))
        {
            dic.Clear();
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
            LuaBehaviour behaviour = (target as LuaBehaviour);

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
                string type = typeArr[1];
                Type t = GetType(type);
                UnityEngine.Object obj = default(UnityEngine.Object);
                //obj = EditorGUILayout.ObjectField(new GUIContent(typeName), obj, t, true, option);
                obj = EditorGUI.ObjectField(new Rect(0, 0, 250, 40), new GUIContent(typeName), obj, t, true);
                if (!dic.ContainsKey(typeName))
                {
                    dic.Add(typeName, obj);
                    keyDic.Add(typeName, t);
                }
            }
            luaState.Dispose();
        }

        if (keyDic.Count > 0)
        {
            foreach (var item in keyDic)
            {
                UnityEngine.Object obj = default(UnityEngine.Object);
                obj = EditorGUILayout.ObjectField(new GUIContent(item.Key), obj, item.Value, true, option);
                //obj = EditorGUI.ObjectField(new Rect(0, 0, 250, 40), new GUIContent(item.Key), obj, item.Value, true);
            }
        }
    }
    void OnGUI()
    {
        if (GUILayout.Button("Test"))
        {
            Debug.Log("good");
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
