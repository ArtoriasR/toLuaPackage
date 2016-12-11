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
	this.AddMember("UnityEngine.Vector3", "vect3")
	this.AddMember("UnityEngine.Transform", "go4")
	this.AddMember("UnityEngine.UI.Image","img")

	function this.Start()
		print(this.test.name)
	end

	return this
end