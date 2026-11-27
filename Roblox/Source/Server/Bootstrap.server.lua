--!strict
-- ServerScriptService/Bootstrap
-- World generation and safe spawn complete before the first character loads.
local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local ServerScriptService = game:GetService("ServerScriptService")
local Workspace = game:GetService("Workspace")

Players.CharacterAutoLoads = false
local rootFolder = ReplicatedStorage:FindFirstChild("Nemezida") or Instance.new("Folder")
rootFolder.Name = "Nemezida"; rootFolder.Parent = ReplicatedStorage
local shared = rootFolder:WaitForChild("Shared")
local Config = require(shared:WaitForChild("NemezidaConfig"))
local services = ServerScriptService:WaitForChild("Services")
local HubGenerator = require(services:WaitForChild("HubGenerator"))
local PopulationService = require(services:WaitForChild("PopulationService"))
local DungeonService = require(services:WaitForChild("DungeonService"))
local worldReady = false
local spawnLocation: SpawnLocation? = nil
local function applyPlayerDefaults(player: Player)
 player:SetAttribute("MaxEssence", Config.Player.MaxEssence)
 if player:GetAttribute("Essence") == nil then player:SetAttribute("Essence", Config.Player.MaxEssence) end
 player:SetAttribute("MaxStamina", Config.Player.MaxStamina)
 if player:GetAttribute("Stamina") == nil then player:SetAttribute("Stamina", Config.Player.MaxStamina) end
 player:SetAttribute("Defense", Config.Player.Defense)
 player:SetAttribute("IsSprinting", false)
 if spawnLocation and spawnLocation.Parent then player.RespawnLocation = spawnLocation end
end
local function placeCharacterSafely(character: Model)
 local humanoid = character:FindFirstChildOfClass("Humanoid") or character:WaitForChild("Humanoid", 10)
 local characterRoot = character:FindFirstChild("HumanoidRootPart") or character:WaitForChild("HumanoidRootPart", 10)
 if not humanoid or not characterRoot or not characterRoot:IsA("BasePart") then warn("[Немезида Spawn] Персонаж не получил HumanoidRootPart; позиционирование пропущено."); return end
 if not worldReady then local deadline=os.clock()+15; while not worldReady and os.clock()<deadline do task.wait(0.1) end end
 if not worldReady then warn("[Немезида Spawn] Мир не готов; игрок не будет отправлен в пустоту."); return end
 character:PivotTo(HubGenerator.GetSpawnCFrame())
 characterRoot.AssemblyLinearVelocity=Vector3.zero; characterRoot.AssemblyAngularVelocity=Vector3.zero
 humanoid.PlatformStand=false; humanoid.Sit=false
end
local function onPlayerAdded(player: Player)
 applyPlayerDefaults(player)
 player.CharacterAdded:Connect(function(character) task.spawn(placeCharacterSafely,character) end)
 local existingCharacter=player.Character
 if existingCharacter then task.spawn(placeCharacterSafely,existingCharacter) end
end
Players.PlayerAdded:Connect(onPlayerAdded)
for _,player in Players:GetPlayers() do onPlayerAdded(player) end
local combatService=require(services:WaitForChild("CombatService"))
local enemyAIService=require(services:WaitForChild("EnemyAIService"))
local movementService=require(services:WaitForChild("MovementService"))
combatService.Init(); enemyAIService.Init(); movementService.Init()
local seed=Workspace:GetAttribute("NemezidaWorldSeed")
if typeof(seed)~="number" then seed=847291 end
local generated=false
local ok,result=pcall(function() return HubGenerator.Generate(seed :: number) end)
if ok and result then generated=true else warn("[Немезида World] Генератор завершился с ошибкой; создаётся резервная площадка:",result) end
local generatedHub=Workspace:FindFirstChild("NemezidaGeneratedHub")
local mainGround=generatedHub and generatedHub:FindFirstChild("AshenGround")
local needsFallback=not generated or not mainGround or not mainGround:IsA("BasePart") or not mainGround.CanCollide
if needsFallback and not Workspace:FindFirstChild("NemezidaFallbackFloor") then
 local floor=Instance.new("Part"); floor.Name="NemezidaFallbackFloor"; floor.Size=Vector3.new(96,2,96)
 floor.CFrame=CFrame.new(0,-1,165); floor.Anchored=true; floor.CanCollide=true; floor.Material=Enum.Material.Slate
 floor.Color=Color3.fromRGB(55,53,52); floor.Parent=Workspace
end
spawnLocation=Workspace:FindFirstChild("NemezidaSpawn") :: SpawnLocation?
if not spawnLocation or not spawnLocation:IsA("SpawnLocation") then
 local spawn=Instance.new("SpawnLocation"); spawn.Name="NemezidaSpawn"; spawn.Size=Vector3.new(12,1,12)
 spawn.CFrame=CFrame.new(0,1.5,165); spawn.Anchored=true; spawn.CanCollide=true; spawn.Neutral=true
 spawn.Duration=2; spawn.Parent=Workspace; spawnLocation=spawn
end
PopulationService.Init(seed :: number)
DungeonService.Init()
for _,player in Players:GetPlayers() do applyPlayerDefaults(player); player.RespawnLocation=spawnLocation end
worldReady=true; Workspace:SetAttribute("NemezidaWorldReady",true); Players.CharacterAutoLoads=true
for _,player in Players:GetPlayers() do
 if not player.Character then task.spawn(function() local loaded,loadError=pcall(function() player:LoadCharacterAsync() end); if not loaded then warn("[Немезида Spawn] Не удалось загрузить персонажа:",loadError) end end) end
end
print("[Немезида] Автозапуск v0.4.1-dev.1 завершён. Мир готов:",worldReady,"Хаб создан:",generated,"Точка спавна:",spawnLocation:GetFullName())
