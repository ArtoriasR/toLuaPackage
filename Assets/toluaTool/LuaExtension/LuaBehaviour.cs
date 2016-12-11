using UnityEngine;
using LuaInterface;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using System.Reflection;
using UnityEngine.EventSystems;
using UnityEditor;

/*
***********************************************
Author:Rolance ◢◤◢◤
　　　　　◢████◤
　　　⊙███████◤
　●████████◤
　　▼　　～◥███◤
　　▲▃▃◢███　●　　●　　●　　●　　●　　●　
　　　　　　███／█　／█　／█　／█　／█　／█　　　◢◤
　　　　　　███████████████████████◤

***********************************************
*/
/// <summary>
/// 
/// </summary>
[Serializable]
public class LuaBehaviour : MonoBehaviour
{

    public string luaFileName = "";

    [SerializeField]
    [HideInInspector]
    public List<string> keyList = new List<string>();
    [SerializeField]
    [HideInInspector]
    public List<UnityEngine.Object> valueList = new List<UnityEngine.Object>();
    [SerializeField]
    [HideInInspector]
    public List<string> typeList = new List<string>();


    public LuaFunction luaAwake = null;
    public LuaFunction luaOnEnable = null;
    public LuaFunction luaStart = null;
    public LuaFunction luaOnDisable = null;
    public LuaFunction luaOnDestroy = null;

    public LuaFunction luaOnTriggerEnter = null;
    public LuaFunction luaOnTriggerExit = null;

    protected LuaTable _luaTable;
    private bool _isInit = false;
    public void SetLuaFileName(string value)
    {
        luaFileName = value;
        DoLuaFile();
    }

    private void Init()
    {
        if (!_isInit)
        {
            if (luaFileName != "" && luaFileName != null)
                SetLuaFileName(luaFileName);
        }
    }

    public void DoLuaFile()
    {
        Debugger.Log("LuaBehaviour:" + luaFileName + " Init");
        if (_luaTable != null)
        {
            if (luaOnDestroy != null)
            {
                luaOnDestroy.Call();
            }
            _luaTable.Dispose();
        }

        try
        {
            LuaSupport.lua.DoFile(luaFileName);
        }
        catch (Exception e)
        {
            Debugger.Log("DoFile error:" + luaFileName);
            throw;
        }
        LuaFunction createFunc = LuaSupport.lua.GetFunction(luaFileName + ".create");
        object[] r = createFunc.Call(gameObject);
        _luaTable = (LuaTable)r[0];
        //_luaTable["luaBehaviour"] = this;
        
        luaAwake = _luaTable.GetLuaFunction("Awake");
        luaOnEnable = _luaTable.GetLuaFunction("OnEnable");
        luaStart = _luaTable.GetLuaFunction("Start");

        luaOnDisable = _luaTable.GetLuaFunction("OnDisable");
        luaOnDestroy = _luaTable.GetLuaFunction("OnDestroy");

        luaOnTriggerEnter = _luaTable.GetLuaFunction("OnTriggerEnter");
        luaOnTriggerExit = _luaTable.GetLuaFunction("OnTriggerExit");

        SetArgsToLua();

        _isInit = true;
    }

    private void SetArgsToLua()
    {
        for (int i = 0; i < keyList.Count; i++)
        {
            _luaTable[keyList[i]] = valueList[i];
        }
    }

    protected void Awake()
    {
        Init();
        if (luaAwake != null)
            luaAwake.Call();
    }

    void OnEnable()
    {
        Init();
        if (luaOnEnable != null)
            luaOnEnable.Call();
    }

    void Start()
    {
        Init();                                                                                                                                                                                                                                                                                          
        if (luaStart != null)
        {
            luaStart.Call();
        }
    }

    void OnDisable()
    {
        if (luaOnDisable != null)
            luaOnDisable.Call();
    }

    void OnDestroy()
    {
        if (luaOnDestroy != null)
            luaOnDestroy.Call();
    }
    void OnTriggerEnter(Collider other)
    {
        if (luaOnTriggerEnter != null)
            luaOnTriggerEnter.Call(other);
    }
    void OnTriggerExit(Collider other)
    {
        if (luaOnTriggerExit != null)
            luaOnTriggerExit.Call(other);
    }

    public LuaTable luaTable
    {
        get
        {
            return _luaTable;
        }
    }

    public bool isInit
    {
        get
        {
            return _isInit;
        }
    }

    public object Call(string funcName, params object[] args)
    {
        object[] objs = Calls(funcName, args);
        if (objs != null&& objs.Length>0)
            return objs[0];
        return null;
    }
    public object[] Calls(string funcName, params object[] args)
    {
        if (_luaTable != null)
        {
            LuaFunction func = _luaTable.GetLuaFunction(funcName);
            if (func != null)
            {
                //TO DO
                return func.Call(args);
            }
        }
        return null;
    }
    /// <summary>
    /// 获取或设置lua里的属性
    /// </summary>
    public object Get(string key)
    {
        return this[key];
    }
    public void Set(string key,object value)
    {
        this[key]=value;
    }
    public object this[string key]
    {
        get
        {
            if (_luaTable != null)
            {
                LuaFunction func = _luaTable.GetLuaFunction("get_"+key);
                if (func != null)
                    return func.Call(null)[0];
                else
                    return luaTable[key];
            }
            return null;
        }
        set
        {
            if (_luaTable != null)
            {
                LuaFunction func = _luaTable.GetLuaFunction("set_" + key);
                if (func != null)
                    func.Call(value);
                else
                    luaTable[key] = value;
            } 
        }
    }
}

