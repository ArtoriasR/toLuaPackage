Utils = {}

function Utils.Reload(fileName)
	if (package.loaded[fileName]) then
		trace("reload "..tostring(fileName))
		package.loaded[fileName] = nil
	end

	require(fileName)
end

function Utils.tips(value,time)
	GUIManager.Instance:ShowToast(value, 0.5, 1)
end

function Utils.messagebox(title,msg,callbackfunc,acceptText,cancelText)
	acceptText = acceptText or "确认"
	cancelText = cancelText or "取消"
	--[[
	GUIManager.Instance:ShowAlert(msg, title, function (go)
		trace("onClick Comfirm")
        local alert = go:GetComponent(typeof(Alert));
        if (alert) then
            if (alert.detail ~= 0) then
            	callbackfunc()
            end
        end
        return true
    end, LuaTool.toStringArray({ cancelText, acceptText }), true, "", AlertType.None, TouchCloseMode.None)
	]]
	GUIManager.Instance:ShowConfirm(msg, function ()
		callbackfunc()
	end, title, true, "", nil, 
	LuaTool.toStringArray({ cancelText, acceptText }), 
	TouchCloseMode.None)
end

--[[
	简单加载资源(省略参数)
	callback(obj, opr) 中opr 可省略
	resType 可省略
]]
function Utils.LoadRes(path, callback, resType)
	local function onLoadCompleted(loadOpr)
		if (callback) then
			trace(tostring(loadOpr))
			callback(loadOpr.content.obj, loadOpr)
		end
	end

	local opr = AssetManager.Instance:LoadResourceAsync(
			path, 
			onLoadCompleted, 
			LoadAssetMode.Default,
			100)
	if (resType) then
		opr.content.type = resType
	end
end

function Utils.LoadResWithLoading(path, callback)
	local loadedGo = nil
	local function onLoadInited(obj)
		loadedGo = obj
	end

	local function onLoadCompleted()
		if (callback) then
			callback(loadedGo)
		end
	end

	local opr = AssetManager.Instance:GetAssetInstance(
		nil,
		path, 
		100,
		onLoadInited,
		onLoadCompleted, 
		true)
end

--[[
	获得对象的LuaGameObject
]]
function Utils.GetLuaGameObject(gameObject)
	local luaBehaviour = gameObject.GetComponent(typeof(LuaBehaviour))
	return luaBehaviour.luaTable
end

--[[
	根据分隔符分割字符串，返回分割后的table
--]]
function Utils.split(s, delim)
	assert(type (delim) == "string" and string.len (delim) > 0, "bad delimiter")
	local start = 1
	local t = {}  -- results table

	-- find each instance of a string followed by the delimiter
	while true do
		local pos = string.find (s, delim, start, true) -- plain find
		if not pos then
		  	break
		end
		table.insert (t, string.sub (s, start, pos - 1))
		start = pos + string.len (delim)
	end -- while

	-- insert final one (after last delimiter)
	table.insert (t, string.sub (s, start))
	return t
end

Utils.CHINESE_MAP = {[0]="零", [1]="一", [2]="二", [3]="三", [4]="四", [5]="五", [6]="六", [7]="七", [8]="八", [9]="九"}
--[[数字转中文
100以内
]]
Utils.toChinese = function(num, isIncludeTen)
	if(num > 999) then num = 999 end
	if(num < 10) then
		return Utils.CHINESE_MAP[num]
	else
		local c = math.floor(num/100)
		local a = math.floor(num%100/10)
		local b = num % 10
		local hundredStr = ""
		if (c and c ~= 0) then
			hundredStr = Utils.CHINESE_MAP[c].."百"
		end

		local tenStr = ""
		if (a == 1 and c == 0) then
			tenStr = "十"
		else
			--百位有数字
			if (a == 0 and c ~= 0) then
				tenStr = "零"
			else
				tenStr = Utils.CHINESE_MAP[a].."十"
			end
		end

		local numStr = ""
		if (b == 0) then
			return hundredStr..tenStr
		else
			return hundredStr..tenStr..Utils.CHINESE_MAP[b]
		end
	end
	return ""
end

--[[根据秒数得到时间，最多显示两位数
    满天只显示天数（最多7天）
    不满天，满时则显示小时数
    不满时，满分则显示分钟数
    不满分，满秒则显示秒数 ]]
function Utils.getTimeBySeconds(seconds)
	local ret = ""
	if (seconds < 0) then
		seconds = -seconds
	end
	if (seconds < 60) then
		ret = ret..seconds.."秒钟"
	elseif (seconds >= 60 and seconds < 3600) then
		local munitesValue = math.floor(seconds / 60)
		ret = ret..munitesValue.."分钟" 
	elseif (seconds >= 3600 and seconds < 86400) then
		local hoursValue = math.floor(seconds / 3600)
		ret = ret..hoursValue.."小时"
	elseif (seconds >= 86400) then
		local daysValue = math.floor(seconds / 86400)
		ret = ret..daysValue.."天"
	end

	return ret
end

trace = function(value)
	print(value)
end

traceTable = function (table, tableName)
	local str = tableName or ""
	for k, v in pairs(table) do
		str = str.."["..tostring(k).."] = "..tostring(v).."\n"
	end
	print(str)
end

--[[返回表中元素数目]]
tableLen = function(table)
	local length = 0
	for k,v in pairs(table) do
		length = length + 1
	end
	return length
end


string.IsNullOrEmpty = function (str)
  return str == nil or str == ""
end

tableAddRange = function(tableDest, tableSource)
  local length = tableLen(tableDest)
  for k,v in pairs(tableSource) do
    tableDest[length] = tableSource
    length = length + 1
  end
end


tableCopy = function(tableSource)
  local copy = {}
  for orig_key, orig_value in pairs(orig) do
      copy[orig_key] = orig_value
  end
  return copy
end


tableReverse = function(tableSource)
  local length = table.getn(tableSource)
  length = length - length % 2
  for i = 1, length / 2, 1 do
    tableSource[i], tableSource[length + 1 - i] = tableSource[length + 1 - i], tableSource[i]
  end
end

table.ForEach = function(tableSource, callback)
  if callback ~= nil then
    for k,v in pairs(tableSource) do
      callback(k,v)
    end
  end
end


string.lenght = function(str)
	local _, count = string.gsub(str, "[^\128-\193]", "")
	return count
end
