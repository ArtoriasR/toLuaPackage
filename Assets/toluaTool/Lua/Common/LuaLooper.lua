LuaLooper = {}

Object.singletonMake(LuaLooper)

function LuaLooper.create()
	local this = Object.create()
	this.Class = LuaLooper
	this.name = "LuaLooper"

	this.registerClass ={} 

	function this:register(class)
		--print("LuaLooper.register")
		if(this.registerClass[class])then
			--print("注册了相同类 ？")
		else
			this.registerClass[class] = class
		end
	end

	function this:unregister(class)
		this.registerClass[class] = nil
	end

	return this
end

local runTime = LuaLooper.getInstance()

Update = function()
	--print("Lua Update")
	for k, v in pairs(runTime.registerClass) do
		if(v.Update)then
			v.Update()
		end
	end

	for k, v in pairs(runTime.registerClass) do
		if(v._isInvoking and v.CheckInvoke)then
			v.CheckInvoke()
		end
	end
end

FixedUpdate = function()
	--print("Lua FixedUpdate")
	for k, v in pairs(runTime.registerClass) do
		if(v.FixedUpdate)then
			v.FixedUpdate()
		end
	end
end

LateUpdate = function()
	--print("Lua LateUpdate")
	for k, v in pairs(runTime.registerClass) do
		if(v.LateUpdate)then
			v.LateUpdate()
		end
	end
end