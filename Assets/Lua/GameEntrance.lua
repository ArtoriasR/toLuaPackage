require("TestCJsonLocalStorage")

GameEntrance = {}

--[[
  @athor Artorias
  @desc 
  @date 2016-11-08 00:31:57
]]--

function GameEntrance.create()
	local this = Object.create()
	this.Class = GameEntrance
	this.name = "GameEntrance"

	function this.init()
	    --[[table转cjson本地存储读写测试]]
	    TestCJsonLocalStorage.create().doTest()
	end

	return this
end