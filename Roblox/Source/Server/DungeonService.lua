--!strict
-- ServerScriptService/Services/DungeonService
-- Builds the first playable dungeon and provides server-authoritative gate travel.
local Players = game:GetService("Players")
local Workspace = game:GetService("Workspace")
local Lighting = game:GetService("Lighting")
local CollectionService = game:GetService("CollectionService")
local DungeonService = {}
local DUNGEON_ORIGIN = Vector3.new(0, 0, -720)
local HUB_RETURN = CFrame.new(0, 5, 165)
local started = false

local function part(parent: Instance, name: string, size: Vector3, cf: CFrame, color: Color3, material: Enum.Material, transparency: number?): Part
 local p = Instance.new("Part")
 p.Name = name; p.Size = size; p.CFrame = cf; p.Anchored = true
 p.Color = color; p.Material = material; p.Transparency = transparency or 0
 p.TopSurface = Enum.SurfaceType.Smooth; p.BottomSurface = Enum.SurfaceType.Smooth
 p.Parent = parent
 return p
end

local function prompt(host: BasePart, action: string, object: string, distance: number, callback: (Player) -> ())
 local p = Instance.new("ProximityPrompt")
 p.ActionText = action; p.ObjectText = object; p.KeyboardKeyCode = Enum.KeyCode.E
 p.HoldDuration = 0.35; p.MaxActivationDistance = distance; p.RequiresLineOfSight = false
 p.Parent = host
 p.Triggered:Connect(callback)
end

local function teleport(player: Player, target: CFrame)
 local character = player.Character
 if not character then return end
 local root = character:FindFirstChild("HumanoidRootPart")
 local humanoid = character:FindFirstChildOfClass("Humanoid")
 if not root or not humanoid or humanoid.Health <= 0 then return end
 character:PivotTo(target)
 root.AssemblyLinearVelocity = Vector3.zero
 root.AssemblyAngularVelocity = Vector3.zero
end

local function buildHubGate()
 local hub = Workspace:FindFirstChild("NemezidaGeneratedHub")
 if not hub or hub:FindFirstChild("DungeonGate") then return end
 local gate = Instance.new("Model"); gate.Name = "DungeonGate"; gate.Parent = hub
 local z = -125
 part(gate,"LeftObelisk",Vector3.new(5,22,6),CFrame.new(-10,11,z),Color3.fromRGB(30,27,36),Enum.Material.Basalt)
 part(gate,"RightObelisk",Vector3.new(5,22,6),CFrame.new(10,11,z),Color3.fromRGB(30,27,36),Enum.Material.Basalt)
 part(gate,"Lintel",Vector3.new(25,5,6),CFrame.new(0,22,z),Color3.fromRGB(26,23,32),Enum.Material.Slate)
 local portal = part(gate,"AbyssPortal",Vector3.new(13,17,1),CFrame.new(0,9,z),Color3.fromRGB(101,45,143),Enum.Material.Neon,0.28)
 portal.CanCollide = false
 local light = Instance.new("PointLight"); light.Color = Color3.fromRGB(137,69,190); light.Range = 24; light.Brightness = 2.5; light.Parent = portal
 prompt(portal,"Войти","ПЕЧАТЬ БЕЗДНЫ",12,function(player)
  teleport(player,CFrame.new(DUNGEON_ORIGIN + Vector3.new(0,5,72)))
 end)
end

local function buildDungeon()
 local old = Workspace:FindFirstChild("NemezidaDungeon")
 if old then old:Destroy() end
 local map = Instance.new("Folder"); map.Name = "NemezidaDungeon"; map.Parent = Workspace
 map:SetAttribute("DungeonName","Затонувший склеп")
 local ox, oz = DUNGEON_ORIGIN.X, DUNGEON_ORIGIN.Z
 part(map,"Floor",Vector3.new(150,3,190),CFrame.new(ox,-1.5,oz),Color3.fromRGB(35,32,39),Enum.Material.Slate)
 local wallColor = Color3.fromRGB(24,22,29)
 part(map,"NorthWall",Vector3.new(150,24,5),CFrame.new(ox,12,oz-95),wallColor,Enum.Material.Basalt)
 part(map,"SouthWall",Vector3.new(150,24,5),CFrame.new(ox,12,oz+95),wallColor,Enum.Material.Basalt)
 part(map,"WestWall",Vector3.new(5,24,190),CFrame.new(ox-75,12,oz),wallColor,Enum.Material.Basalt)
 part(map,"EastWall",Vector3.new(5,24,190),CFrame.new(ox+75,12,oz),wallColor,Enum.Material.Basalt)
 for i=-2,2 do
  part(map,"CryptPillar",Vector3.new(7,22,7),CFrame.new(ox+i*27,11,oz-20),Color3.fromRGB(47,42,49),Enum.Material.Rock)
  part(map,"CryptPillar",Vector3.new(7,22,7),CFrame.new(ox+i*27,11,oz+38),Color3.fromRGB(47,42,49),Enum.Material.Rock)
 end
 part(map,"BloodRunes",Vector3.new(28,0.18,28),CFrame.new(ox,0.12,oz-52),Color3.fromRGB(110,37,55),Enum.Material.Neon,0.22).CanCollide=false
 local altar = part(map,"BossAltar",Vector3.new(20,4,16),CFrame.new(ox,2,oz-62),Color3.fromRGB(34,27,39),Enum.Material.Marble)
 altar:SetAttribute("DungeonObjective","Defeat the crypt guardian")
 local exit = part(map,"ReturnPortal",Vector3.new(12,16,1),CFrame.new(ox,8,oz+82),Color3.fromRGB(68,116,133),Enum.Material.Neon,0.25)
 exit.CanCollide=false
 local exitLight=Instance.new("PointLight"); exitLight.Color=Color3.fromRGB(76,170,190); exitLight.Range=20; exitLight.Brightness=2; exitLight.Parent=exit
 prompt(exit,"Вернуться","ВЫХОД В ПЕПЕЛЬНЫЙ РУБЕЖ",14,function(player) teleport(player,HUB_RETURN) end)

 -- Clone the existing authored enemy rigs so dungeon AI uses the same validated combat attributes.
 local templates = CollectionService:GetTagged("NemezidaEnemy")
 local template: Model? = nil
 for _, candidate in templates do if candidate:IsA("Model") and candidate.Parent and candidate:GetAttribute("EnemyType") then template=candidate; break end end
 if template then
  local positions={Vector3.new(-36,0,oz-35),Vector3.new(36,0,oz-35),Vector3.new(0,0,oz-5),Vector3.new(-25,0,oz-65),Vector3.new(25,0,oz-65)}
  for i,pos in positions do
   local enemy=template:Clone(); enemy.Name="CryptWarden_"..i
   local h=enemy:FindFirstChildOfClass("Humanoid")
   if h then h.Health=h.MaxHealth end
   enemy:PivotTo(CFrame.new(ox+pos.X,3,pos.Z))
   enemy.Parent=map
   CollectionService:AddTag(enemy,"NemezidaEnemy")
  end
 end
end

local function buildHubNPCs()
 local hub=Workspace:FindFirstChild("NemezidaGeneratedHub")
 if not hub or hub:FindFirstChild("HubNPCs") then return end
 local template: Model?=nil
 for _,candidate in CollectionService:GetTagged("NemezidaEnemy") do
  if candidate:IsA("Model") and candidate.Parent and candidate:GetAttribute("EnemyType") then template=candidate; break end
 end
 if not template then return end
 local folder=Instance.new("Folder"); folder.Name="HubNPCs"; folder.Parent=hub
 local specs={
  {Name="Gatekeeper",Display="Смотритель печати",Position=Vector3.new(15,0,145),Color=Color3.fromRGB(78,68,91),Text="Печать ведёт в Затонувший склеп. Возвращайся живым."},
  {Name="Quartermaster",Display="Снабженец Эдрик",Position=Vector3.new(-16,0,145),Color=Color3.fromRGB(103,78,54),Text="Пока у меня только припасы и дурные предчувствия. Торговля появится позже."},
 }
 for _,spec in specs do
  local npc=template:Clone(); npc.Name=spec.Name
  for _,d in npc:GetDescendants() do if d:IsA("BasePart") then d.Color=spec.Color; d.Material=Enum.Material.Fabric end end
  CollectionService:RemoveTag(npc,"NemezidaEnemy")
  npc:SetAttribute("EnemyType",nil); npc:SetAttribute("ProvokedUntil",nil)
  local h=npc:FindFirstChildOfClass("Humanoid")
  if h then h.DisplayName=spec.Display; h.MaxHealth=100000; h.Health=h.MaxHealth; h.WalkSpeed=0; h.JumpPower=0; h.DisplayDistanceType=Enum.HumanoidDisplayDistanceType.Viewer end
  npc:PivotTo(CFrame.new(spec.Position+Vector3.new(0,3,0)))
  npc.Parent=folder
  local head=npc:FindFirstChild("Head")
  if head and head:IsA("BasePart") then
   prompt(head,"Поговорить",spec.Display,10,function(player)
    local remotes=game:GetService("ReplicatedStorage"):FindFirstChild("NemezidaRemotes")
    local remote=remotes and remotes:FindFirstChild("ShowNotice")
    if remote and remote:IsA("RemoteEvent") then remote:FireClient(player,spec.Display..": "..spec.Text) end
   end)
  end
 end
end

function DungeonService.Init()
 if started then return end
 started=true
 Lighting.ClockTime=14.35
 Lighting.Brightness=2.7
 Lighting.Ambient=Color3.fromRGB(118,112,122)
 Lighting.OutdoorAmbient=Color3.fromRGB(145,137,127)
 Lighting.EnvironmentDiffuseScale=0.75
 Lighting.EnvironmentSpecularScale=0.45
 local atmosphere=Lighting:FindFirstChild("NemezidaAtmosphere") or Instance.new("Atmosphere")
 atmosphere.Name="NemezidaAtmosphere"; atmosphere.Density=0.16; atmosphere.Offset=0.05
 atmosphere.Color=Color3.fromRGB(190,181,194); atmosphere.Decay=Color3.fromRGB(104,94,91)
 atmosphere.Glare=0.05; atmosphere.Haze=0.55; atmosphere.Parent=Lighting
 local grade=Lighting:FindFirstChild("NemezidaColorGrade") or Instance.new("ColorCorrectionEffect")
 grade.Name="NemezidaColorGrade"; grade.Brightness=0.015; grade.Contrast=0.06
 grade.Saturation=-0.04; grade.TintColor=Color3.fromRGB(255,245,232); grade.Parent=Lighting
 local bloom=Lighting:FindFirstChild("NemezidaBloom") or Instance.new("BloomEffect")
 bloom.Name="NemezidaBloom"; bloom.Intensity=0.18; bloom.Size=22; bloom.Threshold=1.6; bloom.Parent=Lighting
 buildHubGate()
 buildDungeon()
 buildHubNPCs()
end
return DungeonService
