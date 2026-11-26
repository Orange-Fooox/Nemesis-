--!strict
-- ModuleScript: ServerScriptService/Services/MovementService
local Players=game:GetService("Players")
local ReplicatedStorage=game:GetService("ReplicatedStorage")
local Config=require(ReplicatedStorage:WaitForChild("Nemezida"):WaitForChild("Shared"):WaitForChild("NemezidaConfig"))
local Service={}
local lastRequest: {[Player]: number}={}
local WALK_SPEED=Config.Player.WalkSpeed
local RUN_SPEED=Config.Player.RunSpeed

local function setup(player: Player,character: Model)
 local humanoid=character:WaitForChild("Humanoid",10)
 if not humanoid or not humanoid:IsA("Humanoid") then return end
 humanoid.WalkSpeed=WALK_SPEED
 player:SetAttribute("IsSprinting",false)
 if player:GetAttribute("MaxStamina")==nil then player:SetAttribute("MaxStamina",Config.Player.MaxStamina) end
 if player:GetAttribute("Stamina")==nil then player:SetAttribute("Stamina",Config.Player.MaxStamina) end
end

function Service.Init()
 local folder=ReplicatedStorage:FindFirstChild("NemezidaRemotes") or Instance.new("Folder")
 folder.Name="NemezidaRemotes"; folder.Parent=ReplicatedStorage
 local remote=folder:FindFirstChild("SetSprinting") or Instance.new("RemoteEvent")
 remote.Name="SetSprinting"; remote.Parent=folder

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
  local now=os.clock()
  -- A rejected rapid press is acknowledged with the current server state.
  if value and now-(lastRequest[player] or 0)<Config.Movement.SprintRequestInterval then
   remote:FireClient(player,player:GetAttribute("IsSprinting")==true)
   return
  end
  if value then lastRequest[player]=now end
  local character=player.Character
  local humanoid=character and character:FindFirstChildOfClass("Humanoid")
  if not humanoid or humanoid.Health<=0 then
   remote:FireClient(player,false)
   return
  end
  local stamina=player:GetAttribute("Stamina") or 0
  if value and stamina<=0 then
   player:SetAttribute("IsSprinting",false)
   humanoid.WalkSpeed=WALK_SPEED
   remote:FireClient(player,false)
   return
  end
  player:SetAttribute("IsSprinting",value)
  humanoid.WalkSpeed=if value then RUN_SPEED else WALK_SPEED
  remote:FireClient(player,value)
 end)

 task.spawn(function()
  local previous=os.clock()
  while true do
   task.wait(0.1)
   local now=os.clock()
   local dt=math.min(now-previous,0.25)
   previous=now
   for _,player in Players:GetPlayers() do
    local stamina=player:GetAttribute("Stamina") or 0
    local maxStamina=player:GetAttribute("MaxStamina") or Config.Player.MaxStamina
    local character=player.Character
    local humanoid=character and character:FindFirstChildOfClass("Humanoid")
    if humanoid and humanoid.Health>0 then
     if player:GetAttribute("IsSprinting")==true then
      stamina=math.max(0,stamina-Config.Movement.StaminaDrainPerSecond*dt)
      if stamina<=0 then
       player:SetAttribute("IsSprinting",false)
       humanoid.WalkSpeed=WALK_SPEED
       remote:FireClient(player,false)
      end
     else
      stamina=math.min(maxStamina,stamina+Config.Movement.StaminaRegenPerSecond*dt)
     end
     player:SetAttribute("Stamina",stamina)
    end
   end
  end
 end)
 Players.PlayerRemoving:Connect(function(player) lastRequest[player]=nil end)
end
return Service
