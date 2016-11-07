TestCJsonLocalStorage = {}

--[[
  @athor Artorias
  @desc table转cjson本地存储读写测试
  @date 2016-11-08 00:24:50
]]--

function TestCJsonLocalStorage.create()
	local this = Object.create()
	this.Class = TestCJsonLocalStorage
	this.name = "TestCJsonLocalStorage"

	function this.doTest()
	    local tab = {}
	    tab.name = "hellojson"
	    tab.content = "oh my godness u are beauty"
	    trace("test save")

	    SaveFile("testjson",tab)

	    local testjson = LoadFile("testjson")

    	for i,v in pairs(testjson) do
    		print(i,v)
    	end
	end

	return this
end


