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

local function attack()
 local enemy=enemyFromTarget(mouse.Target)
 if enemy then attackRemote:FireServer(enemy) end
end
local keyAbilities={
 [Enum.KeyCode.Z]="SealStrike",
 [Enum.KeyCode.X]="ShadowDash",
 [Enum.KeyCode.C]="EarthRend",
 [Enum.KeyCode.V]="AbyssRelease",
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
  if enemy then abilityRemote:FireServer(ability,enemy) end
 end
end)
