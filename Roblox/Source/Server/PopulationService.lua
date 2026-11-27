--!strict
-- ModuleScript: ServerScriptService/Services/PopulationService
-- Creates a deterministic set of simple R6 enemy rigs for the current hub.
local Workspace = game:GetService("Workspace")
local CollectionService = game:GetService("CollectionService")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local Config = require(ReplicatedStorage:WaitForChild("Nemezida"):WaitForChild("Shared"):WaitForChild("NemezidaConfig"))
local PopulationService = {}
local started = false

local BODY_COLORS = {
 Moss = Color3.fromRGB(83, 112, 69),
 Bone = Color3.fromRGB(193, 183, 152),
 Shade = Color3.fromRGB(76, 55, 103),
 Ash = Color3.fromRGB(99, 91, 84),
}

local function makeBodyPart(model: Model, name: string, size: Vector3, cf: CFrame, color: Color3, transparency: number?): Part
 local p = Instance.new("Part")
 p.Name = name
 p.Size = size
 p.CFrame = cf
 p.Color = color
 p.Material = Enum.Material.Slate
 p.Anchored = false
 p.CanCollide = name == "Torso" or name == "Left Leg" or name == "Right Leg"
 p.CanTouch = false
 p.Massless = name ~= "HumanoidRootPart"
 p.Transparency = transparency or 0
 p.TopSurface = Enum.SurfaceType.Smooth
 p.BottomSurface = Enum.SurfaceType.Smooth
 p.Parent = model
 return p
end

local function makeMotor(parent: Instance, name: string, part0: BasePart, part1: BasePart, c0: CFrame, c1: CFrame)
 local joint = Instance.new("Motor6D")
 joint.Name = name
 joint.Part0 = part0
 joint.Part1 = part1
 joint.C0 = c0
 joint.C1 = c1
 joint.Parent = parent
end

local function createRig(enemyName: string, stats: {[string]: any}, position: Vector3, index: number): Model
 local color = BODY_COLORS[stats.Color] or BODY_COLORS.Ash
 local model = Instance.new("Model")
 model.Name = enemyName .. "_" .. index
 local pivot = CFrame.new(position + Vector3.new(0, 3, 0))

 local root = makeBodyPart(model, "HumanoidRootPart", Vector3.new(2, 2, 1), pivot, color, 1)
 root.CanCollide = false
 local torso = makeBodyPart(model, "Torso", Vector3.new(2, 2, 1), pivot, color)
 local head = makeBodyPart(model, "Head", Vector3.new(2, 1, 1), pivot * CFrame.new(0, 1.5, 0), Color3.fromRGB(163, 148, 126))
 local rightArm = makeBodyPart(model, "Right Arm", Vector3.new(1, 2, 1), pivot * CFrame.new(1.5, 0, 0), color)
 local leftArm = makeBodyPart(model, "Left Arm", Vector3.new(1, 2, 1), pivot * CFrame.new(-1.5, 0, 0), color)
 local rightLeg = makeBodyPart(model, "Right Leg", Vector3.new(1, 2, 1), pivot * CFrame.new(0.5, -2, 0), Color3.fromRGB(45, 42, 43))
 local leftLeg = makeBodyPart(model, "Left Leg", Vector3.new(1, 2, 1), pivot * CFrame.new(-0.5, -2, 0), Color3.fromRGB(45, 42, 43))

 makeMotor(root, "RootJoint", root, torso, CFrame.new(), CFrame.new())
 makeMotor(torso, "Neck", torso, head, CFrame.new(0, 1, 0), CFrame.new(0, -0.5, 0))
 makeMotor(torso, "Right Shoulder", torso, rightArm, CFrame.new(1, 0.5, 0), CFrame.new(-0.5, 0.5, 0))
 makeMotor(torso, "Left Shoulder", torso, leftArm, CFrame.new(-1, 0.5, 0), CFrame.new(0.5, 0.5, 0))
 makeMotor(torso, "Right Hip", torso, rightLeg, CFrame.new(0.5, -1, 0), CFrame.new(0, 1, 0))
 makeMotor(torso, "Left Hip", torso, leftLeg, CFrame.new(-0.5, -1, 0), CFrame.new(0, 1, 0))

 local humanoid = Instance.new("Humanoid")
 humanoid.Name = "Humanoid"
 humanoid.DisplayName = stats.DisplayName
 humanoid.MaxHealth = stats.MaxHealth
 humanoid.Health = stats.MaxHealth
 humanoid.WalkSpeed = stats.WalkSpeed
 humanoid.AutoRotate = true
 humanoid.BreakJointsOnDeath = false
 humanoid.DisplayDistanceType = Enum.HumanoidDisplayDistanceType.Viewer
 humanoid.Parent = model

 model.PrimaryPart = root
 model:SetAttribute("EnemyType", enemyName)
 model:SetAttribute("MaxHealth", stats.MaxHealth)
 model:SetAttribute("Attack", stats.Attack)
 model:SetAttribute("Defense", stats.Defense)
 model:SetAttribute("WalkSpeed", stats.WalkSpeed)
 model:SetAttribute("DetectionRange", stats.DetectionRange)
 model:SetAttribute("AttackRange", stats.AttackRange)
 model:SetAttribute("AttackCooldown", stats.AttackCooldown)
 model:SetAttribute("XPReward", stats.XP)
 model:SetAttribute("AIState", "Idle")
 model:PivotTo(pivot)
 model.Parent = Workspace
 pcall(function() root:SetNetworkOwner(nil) end)
 CollectionService:AddTag(model, "NemezidaEnemy")
 return model
end

function PopulationService.Init(seed: number?)
 if started then return end
 started = true
 local hub = Workspace:FindFirstChild("NemezidaGeneratedHub")
 local spawnFolder = hub and hub:FindFirstChild("EnemySpawnPoints")
 if not spawnFolder then
  warn("[Немезида Population] EnemySpawnPoints не найдены; автоматическое население пропущено.")
  return
 end

 local names = {}
 for name in Config.Enemies do table.insert(names, name) end
 table.sort(names)
 local points = spawnFolder:GetChildren()
 table.sort(points, function(a, b) return a.Name < b.Name end)
 local rng = Random.new(seed or Workspace:GetAttribute("NemezidaWorldSeed") or 847291)
 local maximum = math.min(#points, Config.World.EnemyPopulation)
 for index = 1, maximum do
  local point = points[index]
  if point:IsA("BasePart") then
   local enemyType = names[rng:NextInteger(1, #names)]
   local stats = Config.Enemies[enemyType]
   createRig(enemyType, stats, point.Position, index)
  end
 end
 print("[Немезида Population] Создано противников:", maximum)
end

return PopulationService
