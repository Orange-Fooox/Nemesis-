--!strict
-- LocalScript: StarterPlayer/StarterPlayerScripts/NemezidaCombat
local Players=game:GetService("Players")
local ReplicatedStorage=game:GetService("ReplicatedStorage")
local UserInputService=game:GetService("UserInputService")
local CollectionService=game:GetService("CollectionService")
local player=Players.LocalPlayer
local mouse=player:GetMouse()
local remotes=ReplicatedStorage:WaitForChild("NemezidaRemotes")
local attackRemote=remotes:WaitForChild("BasicAttack") :: RemoteEvent
local abilityRemote=remotes:WaitForChild("UseAbility") :: RemoteEvent

local function enemyFromTarget(target: Instance?): Model?
 if not target then return nil end
 local model=if target:IsA("Model") then target else target:FindFirstAncestorOfClass("Model")
 if model and CollectionService:HasTag(model,"NemezidaEnemy") then return model end
 return nil
end

local function nearestEnemy(maxDistance: number): Model?
 local character=player.Character
 local root=character and character:FindFirstChild("HumanoidRootPart")
 if not root then return nil end
 local best: Model?=nil
 local bestDistance=maxDistance
 for _,item in CollectionService:GetTagged("NemezidaEnemy") do
  if item:IsA("Model") and item.Parent then
   local h=item:FindFirstChildOfClass("Humanoid")
   local part=item:FindFirstChild("HumanoidRootPart") or item.PrimaryPart
   if h and h.Health>0 and part then
    local distance=(part.Position-root.Position).Magnitude
    if distance<bestDistance then best=item; bestDistance=distance end
   end
  end
 end
 return best
end

local function attack()
 local enemy=enemyFromTarget(mouse.Target) or nearestEnemy(9)
 if enemy then attackRemote:FireServer(enemy) end
end
local keyAbilities={
 [Enum.KeyCode.Z]="SealStrike",
 [Enum.KeyCode.X]="ShadowDash",
 [Enum.KeyCode.C]="EarthRend",
 [Enum.KeyCode.V]="AbyssRelease",
 [Enum.KeyCode.B]="Barrier",
}
UserInputService.InputBegan:Connect(function(input,processed)
 if processed then return end
 if input.UserInputType==Enum.UserInputType.MouseButton1 or input.KeyCode==Enum.KeyCode.F then
  attack()
  return
 end
 local ability=keyAbilities[input.KeyCode]
 if ability then
  local enemy=enemyFromTarget(mouse.Target)
  if ability=="Barrier" or ability=="ShadowDash" then
   abilityRemote:FireServer(ability,enemy)
  elseif enemy then
   abilityRemote:FireServer(ability,enemy)
  end
 end
end)

-- Clickable ability bar for players who do not want to memorize every key.
local gui=Instance.new("ScreenGui")
gui.Name="NemezidaSkillBar"
gui.ResetOnSpawn=false
gui.DisplayOrder=25
gui.Parent=player:WaitForChild("PlayerGui")
local bar=Instance.new("Frame")
bar.Name="Skills"
bar.AnchorPoint=Vector2.new(0.5,1)
bar.Position=UDim2.new(0.5,0,1,-54)
bar.Size=UDim2.fromOffset(430,56)
bar.BackgroundColor3=Color3.fromRGB(18,16,22)
bar.BackgroundTransparency=0.12
bar.BorderSizePixel=0
bar.Parent=gui
local round=Instance.new("UICorner")
round.CornerRadius=UDim.new(0,8)
round.Parent=bar
local outline=Instance.new("UIStroke")
outline.Color=Color3.fromRGB(205,165,94)
outline.Transparency=0.4
outline.Parent=bar
local layout=Instance.new("UIListLayout")
layout.FillDirection=Enum.FillDirection.Horizontal
layout.HorizontalAlignment=Enum.HorizontalAlignment.Center
layout.VerticalAlignment=Enum.VerticalAlignment.Center
layout.Padding=UDim.new(0,4)
layout.Parent=bar
local function addSkill(label: string,key: string,color: Color3,callback: () -> ())
 local b=Instance.new("TextButton")
 b.Size=UDim2.fromOffset(66,44)
 b.BackgroundColor3=color
 b.BorderSizePixel=0
 b.Font=Enum.Font.GothamBold
 b.Text=label.."\\n["..key.."]"
 b.TextColor3=Color3.fromRGB(245,233,211)
 b.TextSize=10
 b.TextWrapped=true
 b.Parent=bar
 local c=Instance.new("UICorner")
 c.CornerRadius=UDim.new(0,5)
 c.Parent=b
 b.Activated:Connect(callback)
end
addSkill("АТАКА","ЛКМ",Color3.fromRGB(110,54,61),attack)
addSkill("ПЕЧАТЬ","Z",Color3.fromRGB(126,63,83),function() local e=enemyFromTarget(mouse.Target) or nearestEnemy(11); if e then abilityRemote:FireServer("SealStrike",e) end end)
addSkill("РЫВОК","X",Color3.fromRGB(78,67,126),function() abilityRemote:FireServer("ShadowDash",enemyFromTarget(mouse.Target) or nearestEnemy(13)) end)
addSkill("РАЗЛОМ","C",Color3.fromRGB(123,83,49),function() local e=enemyFromTarget(mouse.Target) or nearestEnemy(12); if e then abilityRemote:FireServer("EarthRend",e) end end)
addSkill("БЕЗДНА","V",Color3.fromRGB(95,48,126),function() local e=enemyFromTarget(mouse.Target) or nearestEnemy(14); if e then abilityRemote:FireServer("AbyssRelease",e) end end)
addSkill("БАРЬЕР","B",Color3.fromRGB(49,91,116),function() abilityRemote:FireServer("Barrier",nil) end)
