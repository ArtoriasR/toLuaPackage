require("Utils")
require("Define")

ShowMsg = nil

local pbdata = nil

local function Test()
	--print("ttttt")
	debug.traceback("Stack trace")
	--print(debug.getinfo(1))
end


function myfunction ()
	print(debug.traceback("Stack trace"))
	printClass(debug.getinfo(1))
	print("Stack trace end")
	return 10
end

LuaEntrance = {}

--[[
	Lua入口类
]]
function LuaEntrance.create()
	local this = Object.create()
	this.Class = LuaEntrance
	this.name = "LuaEntrance"

	function this:init()
		--[[官方的cjson读取
	    function Test(str)
		    local data = cjson.decode(str)
	        print(data.glossary.title)
		    s = cjson.encode(data)
		    print(s)
	    end 
 
	    local res = UnityEngine.Resources.Load("jsonexample",typeof(UnityEngine.TextAsset))
	    Test(res:ToString())
		]]

	    --[[table转cjson本地存储读写测试]]
	    local tab = {}
	    tab.name = "hellojson"
	    tab.content = "oh my godness u are beauty"
	    trace("test save")

	    SaveFile("testjson",tab)

	    local testjson = LoadFile("testjson")

    	for i,v in pairs(testjson) do
    		print(i,v)
    	end

    	

		--[[
		for k,v in pairs(_G) do
			if(k == "Global")then
				print("begin print Global:",v)
				for m,n in pairs(v) do
					print(m,n)
				end
			end
			if(k == "MyClass")then
				print("begin print MyClass:",v.TestFunc)
				for m,n in pairs(v) do
					print(m,n)
				end
			end
		end
		]]
		--local t = debug.getinfo(Main)
		--local t = debug.getinfo(Test)
		--printClass(t)
		--table_print(_G)

		--Test()
		--myfunction()
		--printClass(debug.getinfo(1))
		--debug.getlocal (Test,2)
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


function SaveFile(fileName,tab)
	file_save(fileName,cjson.encode(tab))
end

function LoadFile(fileName)
	local loadedFile = file_load(fileName)
	local jsoncode = cjson.decode(loadedFile)

	return jsoncode
end