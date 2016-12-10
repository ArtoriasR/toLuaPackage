require("Utils")
require("Define")
require("GameEntrance")

LuaEntrance = {}

--[[
	Lua入口类
]]
function LuaEntrance.create()
	local this = Object.create()
	this.Class = LuaEntrance
	this.name = "LuaEntrance"

	function this:init()
		GameEntrance.create().init()
	end
	
	return this
end
44295
43750
43300

--主入口函数
function Main()					
	print("LuaEntrance.Main")
	local entrance = LuaEntrance.create()
	entrance:init()
end

--场景切换通知
function OnLevelWasLoaded(level)
	Time.timeSinceLevelLoad = 0
	print("LuaEntrance.OnLevelWasLoaded")
end

this.Awake()

function this:reqLuaGameObject()
	if(not this.isTest)then
		local req = LuaGameObject()
		trace("SendReq "..tostring(LuaGameObject)..":"..tostring(req))
		local bytes = req:SerializeToString()
		NetManager.Send(bytes, LuaGameObject.pid, this.rspLoginGameRsp, LoginGameRsp)
	else
		this.rspLoginGameRsp()
	end
end

function this.rspLoginGameRsp(pkgData)
	if(not this.isTest)then
		local rsp = LoginGameRsp()
		rsp:ParseFromString(pkgData)

		trace("onReceiveRsp "..tostring(LoginGameRsp)..":"..tostring(rsp))
	else
		trace("onReceiveRsp")
	end
end
LuaGameObject
luagame