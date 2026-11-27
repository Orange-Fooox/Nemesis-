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

local TweenService=game:GetService("TweenService")
local Debris=game:GetService("Debris")

-- Procedural R15/R6-compatible combat motion. No third-party animation IDs required.
local swingBusy=false
local function playSwing(kind: string)
 if swingBusy then return end
 local character=player.Character
 local humanoid=character and character:FindFirstChildOfClass("Humanoid")
 if not character or not humanoid then return end
 local animator=humanoid:FindFirstChildOfClass("Animator")
 if not animator then animator=Instance.new("Animator"); animator.Parent=humanoid end
 local shoulder: Motor6D?=nil
 local waist: Motor6D?=nil
 for _,joint in character:GetDescendants() do
  if joint:IsA("Motor6D") then
   if joint.Name=="RightShoulder" or joint.Name=="Right Shoulder" then shoulder=joint end
   if joint.Name=="Waist" then waist=joint end
  end
 end
 if not shoulder then return end
 swingBusy=true
 local originalShoulder=shoulder.Transform
 local originalWaist=if waist then waist.Transform else CFrame.identity
 local windup=if kind=="heavy" then CFrame.Angles(math.rad(-105),math.rad(12),math.rad(28)) else CFrame.Angles(math.rad(-72),math.rad(4),math.rad(18))
 local strike=if kind=="heavy" then CFrame.Angles(math.rad(82),math.rad(-20),math.rad(-48)) else CFrame.Angles(math.rad(58),math.rad(-12),math.rad(-38))
 local t1=TweenService:Create(shoulder,TweenInfo.new(0.13,Enum.EasingStyle.Quad,Enum.EasingDirection.Out),{Transform=windup})
 t1:Play(); t1.Completed:Wait()
 if not shoulder.Parent then swingBusy=false; return end
 local t2=TweenService:Create(shoulder,TweenInfo.new(0.10,Enum.EasingStyle.Back,Enum.EasingDirection.Out),{Transform=strike})
 t2:Play()
 if waist then TweenService:Create(waist,TweenInfo.new(0.12),{Transform=CFrame.Angles(0,math.rad(-18),0)}):Play() end
 -- A short, clean blade-like arc made from native Trail attachments.
 local hand=character:FindFirstChild("RightHand") or character:FindFirstChild("Right Arm")
 if hand and hand:IsA("BasePart") then
  local a0=Instance.new("Attachment"); a0.Position=Vector3.new(0,-0.35,0); a0.Parent=hand
  local a1=Instance.new("Attachment"); a1.Position=Vector3.new(0,0.45,0); a1.Parent=hand
  local trail=Instance.new("Trail"); trail.Attachment0=a0; trail.Attachment1=a1
  trail.Lifetime=0.16; trail.MinLength=0.05; trail.LightEmission=0.8
  trail.Color=ColorSequence.new(if kind=="heavy" then Color3.fromRGB(255,185,105) else Color3.fromRGB(224,226,255),Color3.fromRGB(141,87,207))
  trail.Transparency=NumberSequence.new({NumberSequenceKeypoint.new(0,0.12),NumberSequenceKeypoint.new(1,1)})
  trail.Parent=hand; Debris:AddItem(trail,0.35); Debris:AddItem(a0,0.35); Debris:AddItem(a1,0.35)
 end
 task.wait(0.12)
 if shoulder.Parent then TweenService:Create(shoulder,TweenInfo.new(0.18,Enum.EasingStyle.Quad,Enum.EasingDirection.Out),{Transform=originalShoulder}):Play() end
 if waist and waist.Parent then TweenService:Create(waist,TweenInfo.new(0.18),{Transform=originalWaist}):Play() end
 task.wait(0.08); swingBusy=false
end


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
 if enemy then task.spawn(playSwing,"light"); attackRemote:FireServer(enemy) end
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
  if ability~="Barrier" then task.spawn(playSwing,"heavy") end
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
addSkill("ПЕЧАТЬ","Z",Color3.fromRGB(126,63,83),function() local e=enemyFromTarget(mouse.Target) or nearestEnemy(11); if e then task.spawn(playSwing,"heavy"); abilityRemote:FireServer("SealStrike",e) end end)
addSkill("РЫВОК","X",Color3.fromRGB(78,67,126),function() abilityRemote:FireServer("ShadowDash",enemyFromTarget(mouse.Target) or nearestEnemy(13)) end)
addSkill("РАЗЛОМ","C",Color3.fromRGB(123,83,49),function() local e=enemyFromTarget(mouse.Target) or nearestEnemy(12); if e then task.spawn(playSwing,"heavy"); abilityRemote:FireServer("EarthRend",e) end end)
addSkill("БЕЗДНА","V",Color3.fromRGB(95,48,126),function() local e=enemyFromTarget(mouse.Target) or nearestEnemy(14); if e then task.spawn(playSwing,"heavy"); abilityRemote:FireServer("AbyssRelease",e) end end)
addSkill("БАРЬЕР","B",Color3.fromRGB(49,91,116),function() abilityRemote:FireServer("Barrier",nil) end)
