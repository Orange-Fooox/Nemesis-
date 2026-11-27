--!strict
-- ModuleScript: ServerScriptService/Services/NemesisService
-- Server-owned, intentionally bounded adaptation data.
local Players=game:GetService("Players")
local NemesisService={}
local memory: {[number]: {melee: number,ranged: number,dodges: number,blocks: number,encounters: number}}={}
local regionalThreat=0
local function getMemory(player: Player)
 local data=memory[player.UserId]
 if not data then
  data={melee=0,ranged=0,dodges=0,blocks=0,encounters=0}
  memory[player.UserId]=data
 end
 return data
end
function NemesisService.RecordAction(player: Player,action: string)
 local data=getMemory(player)
 if action=="Melee" then data.melee+=1
 elseif action=="Ranged" then data.ranged+=1
 elseif action=="Dodge" then data.dodges+=1
 elseif action=="Block" then data.blocks+=1
 end
end
function NemesisService.RecordEncounter(player: Player)
 local data=getMemory(player); data.encounters+=1
end
function NemesisService.GetProfile(player: Player)
 local data=getMemory(player)
 local total=math.max(1,data.melee+data.ranged)
 return {
  MeleeRatio=data.melee/total,
  RangedRatio=data.ranged/total,
  DodgeCount=data.dodges,
  BlockCount=data.blocks,
  Encounters=data.encounters,
 }
end
function NemesisService.AddRegionalThreat(amount: number)
 regionalThreat=math.clamp(regionalThreat+math.floor(amount),0,100)
 return regionalThreat
end
function NemesisService.ReduceRegionalThreat(amount: number)
 regionalThreat=math.clamp(regionalThreat-math.floor(amount),0,100)
 return regionalThreat
end
function NemesisService.GetRegionalThreat()
 return regionalThreat
end
function NemesisService.GetThreatState()
 if regionalThreat>=100 then return "RegionalEvent"
 elseif regionalThreat>=80 then return "Invasion"
 elseif regionalThreat>=60 then return "EliteThreats"
 elseif regionalThreat>=40 then return "ReinforcedPatrols"
 elseif regionalThreat>=20 then return "Uneasy"
 else return "Calm"
 end
Players.PlayerRemoving:Connect(function(player) memory[player.UserId]=nil end)
return NemesisService
