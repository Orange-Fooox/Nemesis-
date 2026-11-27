--!strict
-- ModuleScript: ServerScriptService/Services/HubGenerator
-- Deterministic greybox generator for the Ashen Frontier.
local Workspace = game:GetService("Workspace")
local HubGenerator = {}
local SIZE = 420
local HALF = SIZE / 2
local SPAWN_POSITION = Vector3.new(0, 0, 165)
local ENEMY_SPAWN_POSITIONS = {
 Vector3.new(-58, 0, 72),
 Vector3.new(58, 0, 72),
 Vector3.new(-58, 0, -68),
 Vector3.new(58, 0, -68),
 Vector3.new(0, 0, -98),
 Vector3.new(0, 0, 112),
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
 local body = makePart(parent, name, Vector3.new(width, height, depth), CFrame.new(x, height/2, z), Color3.fromRGB(119, 105, 88), Enum.Material.Cobblestone)
 body:SetAttribute("GeneratedBuilding", true)
 local roof = makePart(parent, name .. "_Roof", Vector3.new(width+2, 2, depth+2), CFrame.new(x, height+1, z), Color3.fromRGB(76, 58, 51), Enum.Material.WoodPlanks)
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
 root:SetAttribute("GeneratorVersion", "0.4.0-dev.5")
 root.Parent = Workspace

 -- A solid, anchored ground plane is created before any decorative geometry.
 local ground = makePart(root, "AshenGround", Vector3.new(SIZE, 2, SIZE), CFrame.new(0, -1, 0), Color3.fromRGB(94, 111, 79), Enum.Material.Grass)
 ground:SetAttribute("IsWorldGround", true)

 -- Raised, wide spawn plaza prevents the character from appearing on seams or debris.
 local plaza = makePart(root, "SpawnPlaza", Vector3.new(42, 1, 42), CFrame.new(0, 0.5, SPAWN_POSITION.Z), Color3.fromRGB(130, 118, 100), Enum.Material.Cobblestone)
 plaza:SetAttribute("ProtectedSpawnArea", true)
 makePart(root, "SpawnInset", Vector3.new(14, 0.12, 14), CFrame.new(0, 1.06, SPAWN_POSITION.Z), Color3.fromRGB(101, 83, 72), Enum.Material.Slate)

 -- Perimeter walls.
 local walls = {
  {0, -HALF, SIZE, 8}, {0, HALF, SIZE, 8},
  {-HALF, 0, 8, SIZE}, {HALF, 0, 8, SIZE},
 }
 for _, entry in walls do
  makePart(root, "PerimeterWall", Vector3.new(entry[3], 18, entry[4]), CFrame.new(entry[1], 9, entry[2]), Color3.fromRGB(76, 72, 69), Enum.Material.Rock)
 end

 -- Main streets and protected central square.
 makePart(root, "NorthSouthRoad", Vector3.new(18, 0.25, SIZE-20), CFrame.new(0, 0.15, 0), Color3.fromRGB(137, 125, 105), Enum.Material.Cobblestone)
 makePart(root, "EastWestRoad", Vector3.new(SIZE-20, 0.25, 18), CFrame.new(0, 0.16, 0), Color3.fromRGB(75, 70, 66), Enum.Material.Cobblestone)
 makePart(root, "CentralSquare", Vector3.new(60, 0.5, 60), CFrame.new(0, 0.25, 0), Color3.fromRGB(154, 137, 111), Enum.Material.Cobblestone)

 -- A small focal monument makes the square readable from the isometric camera.
 local altarBase = makePart(root, "AbyssAltarBase", Vector3.new(12, 2, 12), CFrame.new(0, 1, 0), Color3.fromRGB(78, 66, 63), Enum.Material.Slate)
 altarBase:SetAttribute("InteractableLandmark", true)
 makePart(root, "AbyssAltarCore", Vector3.new(3, 9, 3), CFrame.new(0, 6.5, 0), Color3.fromRGB(120, 76, 171), Enum.Material.Neon)

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

 -- Stylized woodland edge built from native Roblox materials; Creator Store packs are documented separately.
 local treePositions = {
  Vector3.new(-178,0,-165), Vector3.new(-145,0,-180), Vector3.new(155,0,-170), Vector3.new(182,0,-132),
  Vector3.new(-180,0,12), Vector3.new(182,0,28), Vector3.new(-170,0,185), Vector3.new(170,0,184),
  Vector3.new(-118,0,175), Vector3.new(112,0,180), Vector3.new(-190,0,112), Vector3.new(190,0,-55),
 }
 for _, pos in treePositions do
  local trunk = makePart(root, "AshenOakTrunk", Vector3.new(3,10,3), CFrame.new(pos.X,5,pos.Z), Color3.fromRGB(91,65,48), Enum.Material.Wood)
  trunk.Shape = Enum.PartType.Cylinder
  local crown = makePart(root, "AshenOakCanopy", Vector3.new(15,13,15), CFrame.new(pos.X,13,pos.Z), Color3.fromRGB(72,111,69), Enum.Material.LeafyGrass)
  crown.Shape = Enum.PartType.Ball; crown.CanCollide = false; crown.CanTouch = false
  local crown2 = makePart(root, "AshenOakCanopy", Vector3.new(10,9,10), CFrame.new(pos.X+4,16,pos.Z-2), Color3.fromRGB(91,132,78), Enum.Material.Grass)
  crown2.Shape = Enum.PartType.Ball; crown2.CanCollide = false; crown2.CanTouch = false
 end

 -- Warm cursed lanterns break up the grey stone palette and make routes readable.
 local torchPositions = {Vector3.new(-34,0,34), Vector3.new(34,0,34), Vector3.new(-34,0,-34), Vector3.new(34,0,-34), Vector3.new(-12,0,125), Vector3.new(12,0,125)}
 for index, pos in torchPositions do
  local base = makePart(root, "AshenLanternBase", Vector3.new(3,1,3), CFrame.new(pos.X,0.5,pos.Z), Color3.fromRGB(37,32,39), Enum.Material.Basalt)
  local stem = makePart(root, "AshenLanternStem", Vector3.new(1,7,1), CFrame.new(pos.X,4,pos.Z), Color3.fromRGB(52,43,39), Enum.Material.Metal)
  local flame = makePart(root, "AshenLanternFlame", Vector3.new(1.4,1.8,1.4), CFrame.new(pos.X,7.8,pos.Z), Color3.fromRGB(153,83,49), Enum.Material.Neon)
  flame.Shape = Enum.PartType.Ball; flame.CanCollide = false
  local fire = Instance.new("Fire"); fire.Size = 3.5; fire.Heat = 4; fire.Color = Color3.fromRGB(207,112,58); fire.SecondaryColor = Color3.fromRGB(92,43,123); fire.Parent = flame
  local light = Instance.new("PointLight"); light.Color = Color3.fromRGB(224,137,78); light.Range = 19; light.Brightness = 1.35; light.Shadows = true; light.Parent = flame
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
    Color3.fromRGB(101, 91, 78), Enum.Material.Rock)
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
