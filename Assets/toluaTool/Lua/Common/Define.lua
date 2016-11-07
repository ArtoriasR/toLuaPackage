require("LuaGameObject")
require("LuaLooper")
require("Utils")
require("XArray")
require("events")
require("eventlib")

cjson = require 'cjson'
ut = require("util")	
file_save = ut.file_save
file_load = ut.file_load

function SaveFile(fileName,tab)
	file_save(fileName,cjson.encode(tab))
end

function LoadFile(fileName)
	local loadedFile = file_load(fileName)
	local tab = cjson.decode(loadedFile)
	return tab
end