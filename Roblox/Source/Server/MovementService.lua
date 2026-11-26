--!strict
-- ModuleScript: ServerScriptService/Services/MovementService
local Players=game:GetService("Players")
local ReplicatedStorage=game:GetService("ReplicatedStorage")
local Service={}
local WALK_SPEED=16
local RUN_SPEED=23
function Service.Init()
 local folder=ReplicatedStorage:FindFirstChild("NemezidaRemotes") or Instance.new("Folder")
 folder.Name="NemezidaRemotes"; folder.Parent=ReplicatedStorage
 local remote=folder:FindFirstChild("SetSprinting") or Instance.new("RemoteEvent")
 remote.Name="SetSprinting"; remote.Parent=folder
 local function setup(player: Player,character: Model)
  local humanoid=character:WaitForChild("Humanoid",10)
  if humanoid and humanoid:IsA("Humanoid") then
   humanoid.WalkSpeed=WALK_SPEED
   player:SetAttribute("IsSprinting",false)
  end
 end
 Players.PlayerAdded:Connect(function(player)
  player.CharacterAdded:Connect(function(character) setup(player,character) end)
  if player.Character then task.spawn(setup,player,player.Character) end
 end)
 for _,player in Players:GetPlayers() do
  player.CharacterAdded:Connect(function(character) setup(player,character) end)
  if player.Character then task.spawn(setup,player,player.Character) end
 end
 remote.OnServerEvent:Connect(function(player,value)
  if typeof(value)~="boolean" then return end
  local character=player.Character
  local humanoid=character and character:FindFirstChildOfClass("Humanoid")
  if not humanoid or humanoid.Health<=0 then return end
  player:SetAttribute("IsSprinting",value)
  humanoid.WalkSpeed=if value then RUN_SPEED else WALK_SPEED
 end)
end
return Service
