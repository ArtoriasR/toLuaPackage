XTimer = {}

function XTimer.create(time,callback)
	local this = Object.create()
	this.Class = XTimer
	this.name = "XTimer"

	this.cTime = 0
	this.tTime = time

	function this:Update()
		this.cTime += Time.deltaTime
		if(this.cTime >= this.tTime)then
			callback()
		end
	end

	return this
end