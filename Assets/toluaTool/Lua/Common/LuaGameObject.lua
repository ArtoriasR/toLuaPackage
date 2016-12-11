require("Object")
require("LuaLooper")
LuaGameObject = {}

function LuaGameObject.create(gameObject)
	local this = Object.create()
	this.Class = LuaGameObject
	this.name = "LuaGameObject"
	
	this.gameObject = gameObject
	if(gameObject~=nil)then
        this.transform = gameObject.transform
    end

    this.argsList = {}
	this.initParamNames={}

	function this.AddMember(varType, name)
		table.insert(this.initParamNames, name.."="..varType)
	end

	return this
end