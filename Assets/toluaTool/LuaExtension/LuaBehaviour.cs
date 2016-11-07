using UnityEngine;
using LuaInterface;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using System.Reflection;
using UnityEngine.EventSystems;

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

    public List<UnityEngine.Object> argsList = new List<UnityEngine.Object>();

    public LuaFunction luaAwake = null;
    public LuaFunction luaOnEnable = null;
    public LuaFunction luaStart = null;
    public LuaFunction luaOnDisable = null;
    public LuaFunction luaOnDestroy = null;

    public LuaFunction luaOnTriggerEnter = null;
    public LuaFunction luaOnTriggerExit = null;

    protected LuaTable _luaTable;
    private bool _isInit = false;

    protected void Awake()
    {
        if (luaFileName != "" && luaFileName != null)
            SetLuaFileName ( luaFileName);
    }
    public void SetLuaFileName(string value)
    {
   
            luaFileName = value;
            DoLuaFile();
    }

    public void DoLuaFile()
    {
        /*使用scroll view的时候，里面的cell默认是关闭的，导致LuaBehavior没有初始化，需求比较急，还没想好怎么处理，临时打开
        if (enabled == false)
        {
            return; 
        }
        */
        Debugger.Log("LuaBehaviour:" + luaFileName + " Init");
        //LuaSupport.lua.Require(_luaFileName);
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
        _luaTable["luaBehaviour"] = this;
        luaAwake = _luaTable.GetLuaFunction("Awake");
        luaOnEnable = _luaTable.GetLuaFunction("OnEnable");
        luaStart = _luaTable.GetLuaFunction("Start");

        luaOnDisable = _luaTable.GetLuaFunction("OnDisable");
        luaOnDestroy = _luaTable.GetLuaFunction("OnDestroy");

        luaOnTriggerEnter = _luaTable.GetLuaFunction("OnTriggerEnter");
        luaOnTriggerExit = _luaTable.GetLuaFunction("OnTriggerExit");

        LuaTable argsTable = (LuaTable)_luaTable["argsList"];
        /*
        for (int i = 0; i < argsList.Count; i++)
        {
            if (argsList[i] != null)
            {
                argsTable[(i + 1)] = argsList[i];
            }
        }
        
        for (int i = 0; i < initParamNames.Count; i++)
        {
            object obj = initParamObjectValues.Count > i ? initParamObjectValues[i] : null;
            if (obj == null)
                obj = initParamStringValues.Count > i ? initParamStringValues[i] : null;
            if (obj is LuaEventComponent)
                _luaTable[initParamNames[i]] = ((LuaEventComponent)obj).uEvent;
            else
            {
                if (obj is string && (((string)obj) == "") && initParamTypes[i] != "string" && initParamTypes[i] != "String")
                    _luaTable[initParamNames[i]] = null;
                else
                    _luaTable[initParamNames[i]] = obj;
            }
        }
        */
        if (luaAwake != null)
            luaAwake.Call();

        _isInit = true;
        /*
        for (int i = 0; i < mActionsWhenInit.Count; i++)
        {
            mActionsWhenInit[i].Invoke((Action<object>)mActionParamsWhenInit[i][0], (string)mActionParamsWhenInit[i][1], (object[])mActionParamsWhenInit[i][2]);
        }
        mActionsWhenInit.Clear();
        mActionParamsWhenInit.Clear();
        */

    }

    void OnEnable()
    {
        if (luaOnEnable != null)
            luaOnEnable.Call();
    }

    void Start()
    {
        //Debug.Log("luabehaviour :" + _luaFileName + ",start");                                                                                                                                                                                                                                                                                                                         
        if (luaStart != null)
        {
            //Debug.Log("luabehaviour :" + _luaFileName + ",start call");
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
        //TO DO 释放luafunction
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

