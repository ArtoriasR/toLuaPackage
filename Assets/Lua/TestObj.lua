TestObj = {}

--[[
  @athor Artorias
  @desc 
  @date 2016-11-30 21:19:53
]]--

function TestObj.create(gameObject)
	local this = LuaGameObject.create(gameObject)
	this.Class = TestObj
	this.name = "TestObj"

	this.AddMember("UnityEngine.GameObject", "test")
	this.AddMember("UnityEngine.GameObject", "go1")
	this.AddMember("UnityEngine.GameObject", "go2")
	this.AddMember("UnityEngine.GameObject", "go3")
	this.AddMember("UnityEngine.Transform", "go4")

	function this.Start()

	end

	return this
end