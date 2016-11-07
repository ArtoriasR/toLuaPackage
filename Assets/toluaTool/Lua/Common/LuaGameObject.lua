require("Object")
require("LuaLooper")
LuaGameObject = {}

function LuaGameObject.create(gameObject)
	local this = Object.create()
	this.Class = LuaGameObject
	this.name = "LuaGameObject"
    this.luaBehaviour=nil
	this.gameObject = gameObject
	if(gameObject~=nil)then
        this.transform = gameObject.transform
    end
	this.argsList = {}
	--[[提供给unity属性面板可设置的参数，格式{key=value} 
	key为参数名,value为参数类型
	可设置的类型有：int,float,string,Color,GameObject,及一切继自UnityEngine.Object的类的全类名)
	Awake时，就可以直接通过this.参数名,的方式访问获取,
	可以支持定义UGUI事件，如，onClick=UnityEngine.Events.UnityEvent,用法与C#一致，onClick:Invoke()，onClick:AddListener
	--]]
	this.initParamNames={}

	this._isInvoking=false
	local _invokes={}

	function this.InvalidView()
		this.luaBehaviour:InvalidView();
	end

	function this.Refresh()
		this.luaBehaviour:Refresh();
	end
	function this.IsWaitForUpdate()
		return this.luaBehaviour.isWaitForUpdate;
	end

	function this.Invoke(methodName ,time)
		this._isInvoking=true;
		_invokes[methodName]={time+os.time(),0}
	end

	function this.InvokeRepeating (methodName ,time,repeatRate )
		this._isInvoking=true;
		_invokes[methodName]={time+os.time(),repeatRate}
	end

	function this.IsInvoking  (methodName  )
		return _invokes[methodName]~=nil;
	end

	function this.CancelInvoke  (methodName  )
		if(methodName==nil) then
			_invokes={}
		else
			_invokes[methodName]=nil;
		end
		if(#_invokes==0) then
			this._isInvoking=false;
		end
	end

	function this.CheckInvoke()
		local t=os.time();
		for i,v in ipairs(_invokes) do
			if(t>=v[1]) then
				if(v[2]>0) then
					v[1]=v[1]+v[2]
				else
					_invokes[i]=nil
				end
				this[i]();
			end
		end
		if(#_invokes==0) then
			this._isInvoking=false;
		end
	end

	LuaLooper.getInstance():register(this)

	function this.AddMember(varType, name)
		table.insert(this.initParamNames, name.."="..varType)
	end

	return this
end