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

