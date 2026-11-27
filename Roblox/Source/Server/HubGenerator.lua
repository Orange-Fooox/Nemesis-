--!strict
-- ModuleScript: ServerScriptService/Services/HubGenerator
-- Deterministic greybox generator for the Ashen Frontier.
local Workspace = game:GetService("Workspace")
local HubGenerator = {}
local SIZE = 420
local HALF = SIZE / 2
local SPAWN_POSITION = Vector3.new(0, 0, 165)
local ENEMY_SPAWN_POSITIONS = {
 Vector3.new(-175, 0, -175),
 Vector3.new(175, 0, -175),
 Vector3.new(-175, 0, 175),
 Vector3.new(175, 0, 175),
 Vector3.new(0, 0, -185),
 Vector3.new(185, 0, 0),
}

local function makePart(parent: Instance, name: string, size: Vector3, cf: CFrame, color: Color3, material: Enum.Material): Part
 local p = Instance.new("Part")
 p.Name = name
 p.Size = size
 p.CFrame = cf
 p.Anchored = true
 p.CanCollide = true
 p.CanTouch = false
 p.Color = color
 p.Material = material
 p.TopSurface = Enum.SurfaceType.Smooth
 p.BottomSurface = Enum.SurfaceType.Smooth
 p.Parent = parent
 return p
end

local function makeBuilding(rng: Random, parent: Instance, name: string, x: number, z: number)
 local width = rng:NextInteger(18, 30)
 local depth = rng:NextInteger(18, 30)
 local height = rng:NextInteger(12, 28)
 local body = makePart(parent, name, Vector3.new(width, height, depth), CFrame.new(x, height/2, z), Color3.fromRGB(42, 42, 48), Enum.Material.Slate)
 body:SetAttribute("GeneratedBuilding", true)
 local roof = makePart(parent, name .. "_Roof", Vector3.new(width+2, 2, depth+2), CFrame.new(x, height+1, z), Color3.fromRGB(22, 22, 28), Enum.Material.Metal)
 roof:SetAttribute("GeneratedDecoration", true)
end

local function makeEnemySpawnPoints(parent: Instance, rng: Random)
 local folder = Instance.new("Folder")
 folder.Name = "EnemySpawnPoints"
 folder.Parent = parent
 -- Seed-based small offsets keep encounters reproducible without overlapping buildings.
 for index, point in ENEMY_SPAWN_POSITIONS do
  local marker = Instance.new("Part")
  marker.Name = "EnemySpawn_" .. index
  marker.Size = Vector3.new(2, 0.2, 2)
  marker.CFrame = CFrame.new(point + Vector3.new(rng:NextInteger(-4, 4), 0.1, rng:NextInteger(-4, 4)))
  marker.Anchored = true
  marker.CanCollide = false
  marker.CanTouch = false
  marker.CanQuery = false
  marker.Transparency = 1
  marker:SetAttribute("SpawnIndex", index)
  marker.Parent = folder
 end
 return folder
end

local function createSpawnLocation()
 local old = Workspace:FindFirstChild("NemezidaSpawn")
 if old then old:Destroy() end
 local spawn = Instance.new("SpawnLocation")
 spawn.Name = "NemezidaSpawn"
 spawn.Size = Vector3.new(8, 1, 8)
 spawn.CFrame = CFrame.new(SPAWN_POSITION + Vector3.new(0, 1.5, 0))
 spawn.Anchored = true
 spawn.CanCollide = true
 spawn.Neutral = true
 spawn.AllowTeamChangeOnTouch = false
 spawn.Duration = 2
 spawn.Transparency = 0.15
 spawn.Color = Color3.fromRGB(115, 75, 155)
 spawn.Material = Enum.Material.Neon
 -- Keep the spawn point directly under Workspace so Roblox's spawn resolver sees it.
 spawn.Parent = Workspace
 return spawn
end

function HubGenerator.Generate(seed: number?)
 local actualSeed = seed or 847291
 local rng = Random.new(actualSeed)
 local old = Workspace:FindFirstChild("NemezidaGeneratedHub")
 if old then old:Destroy() end
 local root = Instance.new("Folder")
 root.Name = "NemezidaGeneratedHub"
 root:SetAttribute("WorldSeed", actualSeed)
 root:SetAttribute("GeneratorVersion", "0.4.0-dev.4")
 root.Parent = Workspace

 -- A solid, anchored ground plane is created before any decorative geometry.
 local ground = makePart(root, "AshenGround", Vector3.new(SIZE, 2, SIZE), CFrame.new(0, -1, 0), Color3.fromRGB(55, 53, 52), Enum.Material.Slate)
 ground:SetAttribute("IsWorldGround", true)

 -- Raised, wide spawn plaza prevents the character from appearing on seams or debris.
 local plaza = makePart(root, "SpawnPlaza", Vector3.new(42, 1, 42), CFrame.new(0, 0.5, SPAWN_POSITION.Z), Color3.fromRGB(79, 70, 72), Enum.Material.Cobblestone)
 plaza:SetAttribute("ProtectedSpawnArea", true)
 makePart(root, "SpawnInset", Vector3.new(14, 0.12, 14), CFrame.new(0, 1.06, SPAWN_POSITION.Z), Color3.fromRGB(60, 43, 76), Enum.Material.Slate)

 -- Perimeter walls.
 local walls = {
  {0, -HALF, SIZE, 8}, {0, HALF, SIZE, 8},
  {-HALF, 0, 8, SIZE}, {HALF, 0, 8, SIZE},
 }
 for _, entry in walls do
  makePart(root, "PerimeterWall", Vector3.new(entry[3], 18, entry[4]), CFrame.new(entry[1], 9, entry[2]), Color3.fromRGB(25, 25, 30), Enum.Material.Basalt)
 end

 -- Main streets and protected central square.
 makePart(root, "NorthSouthRoad", Vector3.new(18, 0.25, SIZE-20), CFrame.new(0, 0.15, 0), Color3.fromRGB(75, 70, 66), Enum.Material.Cobblestone)
 makePart(root, "EastWestRoad", Vector3.new(SIZE-20, 0.25, 18), CFrame.new(0, 0.16, 0), Color3.fromRGB(75, 70, 66), Enum.Material.Cobblestone)
 makePart(root, "CentralSquare", Vector3.new(60, 0.5, 60), CFrame.new(0, 0.25, 0), Color3.fromRGB(90, 82, 70), Enum.Material.Cobblestone)

 -- A small focal monument makes the square readable from the isometric camera.
 local altarBase = makePart(root, "AbyssAltarBase", Vector3.new(12, 2, 12), CFrame.new(0, 1, 0), Color3.fromRGB(29, 26, 34), Enum.Material.Basalt)
 altarBase:SetAttribute("InteractableLandmark", true)
 makePart(root, "AbyssAltarCore", Vector3.new(3, 9, 3), CFrame.new(0, 6.5, 0), Color3.fromRGB(92, 45, 130), Enum.Material.Neon)

 -- District blocks: market, shrine, ruined homes and corrupted outskirts.
 local slots = {
  {95, 75}, {135, 115}, {-95, 75}, {-135, 115},
  {95, -75}, {135, -115}, {-95, -75}, {-135, -115},
  {65, 155}, {-65, 155}, {65, -155}, {-65, -155},
 }
 for index, slot in slots do
  local x = slot[1] + rng:NextInteger(-8, 8)
  local z = slot[2] + rng:NextInteger(-8, 8)
  makeBuilding(rng, root, "RuinedBuilding_" .. index, x, z)
 end

 -- Deterministic rubble outside the main roads and away from the player spawn.
 for index = 1, 90 do
  local x = rng:NextInteger(-195, 195)
  local z = rng:NextInteger(-195, 195)
  local candidate = Vector3.new(x, 0, z)
  local awayFromSpawn = (candidate - SPAWN_POSITION).Magnitude > 34
  local clearOfEnemySpawns = true
  for _, enemySpawn in ENEMY_SPAWN_POSITIONS do
   if (candidate - enemySpawn).Magnitude < 20 then
    clearOfEnemySpawns = false
    break
   end
  end
  local offRoad = math.abs(x) > 13 and math.abs(z) > 13
  if awayFromSpawn and clearOfEnemySpawns and offRoad then
   local size = rng:NextInteger(2, 6)
   local rubble = makePart(root, "AshenRubble", Vector3.new(size, size, size),
    CFrame.new(x, size/2, z) * CFrame.Angles(0, rng:NextNumber(0, math.pi), rng:NextNumber(-0.2, 0.2)),
    Color3.fromRGB(48, 46, 46), Enum.Material.Rock)
   rubble.CanCollide = true
  end
 end

 makeEnemySpawnPoints(root, rng)
 local spawn = createSpawnLocation()
 root:SetAttribute("SpawnLocationName", spawn.Name)
 Workspace:SetAttribute("NemezidaWorldSeed", actualSeed)
 Workspace:SetAttribute("NemezidaWorldReady", true)
 return root
end

function HubGenerator.GetSpawnCFrame(): CFrame
 return CFrame.new(0, 5, SPAWN_POSITION.Z)
end

return HubGenerator
