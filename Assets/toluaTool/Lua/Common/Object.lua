Object = {}

local function Pototype(obj,fnName)
	local key = "pototype_"..fnName
	local function __innerCall(t,...)
		local v = t[key]
		if(v)then
			for i = 1, #v do
				local fn = v[i]
				fn(t,unpack(arg))
			end  
		end
	end
	if(not obj[key])then
		obj[key] = {}
	end
	if(obj[fnName])then
		table.insert(obj[key], obj[fnName])
	end
	return __innerCall
end

--[[

	模板
	function 类名.create(...)
		local this = 基类.create()
		this.Class = 类名
		this.name = "类名"
		-------------------------------------
		--body
		-------------------------------------
		return this
	end

	作为基类被继承
	创建对象
	
	create作为工厂的new方法，派生类请按照这个规范命名，类似于cocos2d的create和createWith的格式
	
	派生类的示例
	Vector2 = {}
	Vector2.create = function()
		local this = Object.create()--相当于调度基类的构造方法
		this.Class = Vector2
		--todo 派生类的定义
		this.x = 10
		this.y = 20
		function this:update( t)
			--todo
			this.x = this.x + 1
		end
		return this
	end
	
	override的示例
	Vector3 = {}
	Vector3.create = function()
		local this = Vector2.create()--调用基类的构造方法
		this.Class = Vector3
		this.z = 0
		
		--这里相当于于override了基类的update方法
		this.super_update = this.update
		function this:update(t)
			this:super_update(t)
			--调用基类的方法，如果必要的话
			this:super_update(t)
			--todo
			this.z = this.z + 1
		end
		
		return this
	end	

	如果需要使用getInstance方法，可以这样写

	Object.singletonMake(Vector3)

	local v = Vector3.getInstance()
	
--]]
Object.create = function()

	local this  = {}
	this.Class = Object
	this.name = "Object"

	--[[
	this.__index = this
	this.Pototype = Pototype

	function this.instanceOf(Class)
		return this.Class == Class
	end

	function this._trace(msg)
		require("Utils")
		_trace(this.name .. "::" .. msg)
	end
	]]



	return this
end

function Object.interfaceImpl(obj , interface )
	for k,v in pairs(interface) do
		obj[k] = v
	end
end

--[[
	用这个来快速创造singleton方法
	@param Class 类名，接受规范中的OOP框架中的class，需要有create方法，支持参数
	@param fnInit 如果需要额外的初始化处理，可以在这里自定义，否则可以不传递

	用法:
	A = {}
	function A.create(x,y)
		.....
	end
	Object.singletonMake(A)

	A.getInstance(10,12)--这里的参数会传递给构造函数

	------------------

	B = {}
	function B.create()
		.....
	end
	Object.singletonMake(B , function(b)
								 b.name = "TTCat" 
							end)

	B.getInstance().name--"TTCat" got
]]
function Object.singletonMake(Class , fnInit)
	function Class.getInstance(...)
		if(not Class.instance)then
			if(arg)then
				Class.instance = Class.create(unpack(arg))
			else
				Class.instance = Class.create()
			end
			if(fnInit)then
				fnInit(Class.instance)
			end
		end
		return Class.instance
	end
end