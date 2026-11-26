--!strict
-- ModuleScript: ServerScriptService/Services/HubGenerator
-- Generates a deterministic greybox hub. Art pass and bespoke building models come later.
local Workspace=game:GetService("Workspace")
local HubGenerator={}
local SIZE=420
local HALF=SIZE/2
local function part(parent: Instance,name: string,size: Vector3,cf: CFrame,color: Color3,material: Enum.Material)
 local p=Instance.new("Part")
 p.Name=name; p.Size=size; p.CFrame=cf; p.Anchored=true
 p.Color=color; p.Material=material; p.TopSurface=Enum.SurfaceType.Smooth; p.BottomSurface=Enum.SurfaceType.Smooth
 p.Parent=parent
 return p
end
local function block(rng: Random,parent: Instance,name: string,x: number,z: number)
 local w=rng:NextInteger(18,30)
 local d=rng:NextInteger(18,30)
 local h=rng:NextInteger(12,28)
 local base=part(parent,name,Vector3.new(w,h,d),CFrame.new(x,h/2,z),Color3.fromRGB(42,42,48),Enum.Material.Slate)
 base:SetAttribute("GeneratedBuilding",true)
 local roof=part(parent,name.."_Roof",Vector3.new(w+2,2,d+2),CFrame.new(x,h+1,z),Color3.fromRGB(22,22,28),Enum.Material.Metal)
 roof:SetAttribute("GeneratedDecoration",true)
end
function HubGenerator.Generate(seed: number?)
 local actualSeed=seed or 847291
 local rng=Random.new(actualSeed)
 local old=Workspace:FindFirstChild("NemezidaGeneratedHub")
 if old then old:Destroy() end
 local root=Instance.new("Folder"); root.Name="NemezidaGeneratedHub"; root.Parent=Workspace
 root:SetAttribute("WorldSeed",actualSeed); root:SetAttribute("GeneratorVersion","0.4.0-dev.1")
 part(root,"AshenGround",Vector3.new(SIZE,2,SIZE),CFrame.new(0,-1,0),Color3.fromRGB(55,53,52),Enum.Material.Slate)
 -- Outer boundary.
 for _,entry in {{0,-HALF, SIZE,8},{0,HALF,SIZE,8},{-HALF,0,8,SIZE},{HALF,0,8,SIZE}} do
  local x,z,w,d=entry[1],entry[2],entry[3],entry[4]
  part(root,"PerimeterWall",Vector3.new(w,18,d),CFrame.new(x,9,z),Color3.fromRGB(25,25,30),Enum.Material.Basalt)
 end
 -- Crossroads and protected central square.
 part(root,"NorthSouthRoad",Vector3.new(18,0.25,SIZE-20),CFrame.new(0,0.15,0),Color3.fromRGB(75,70,66),Enum.Material.Cobblestone)
 part(root,"EastWestRoad",Vector3.new(SIZE-20,0.25,18),CFrame.new(0,0.16,0),Color3.fromRGB(75,70,66),Enum.Material.Cobblestone)
 part(root,"CentralSquare",Vector3.new(60,0.5,60),CFrame.new(0,0.25,0),Color3.fromRGB(90,82,70),Enum.Material.Cobblestone)
 -- District blocks: market, shrine, ruined homes and corrupted outskirts.
 local slots={
  {95,75},{135,115},{-95,75},{-135,115},
  {95,-75},{135,-115},{-95,-75},{-135,-115},
  {65,155},{-65,155},{65,-155},{-65,-155},
 }
 for i,slot in slots do
  local x=slot[1]+rng:NextInteger(-8,8)
  local z=slot[2]+rng:NextInteger(-8,8)
  block(rng,root,"RuinedBuilding_"..i,x,z)
 end
 -- Scattered ash-covered stone debris, capped for predictable server cost.
 for i=1,90 do
  local x=rng:NextInteger(-195,195)
  local z=rng:NextInteger(-195,195)
  if math.abs(x)>38 or math.abs(z)>38 then
   local s=rng:NextInteger(2,6)
   part(root,"AshenRubble",Vector3.new(s,s,s),CFrame.new(x,s/2,z)*CFrame.Angles(0,rng:NextNumber(0,math.pi),rng:NextNumber(-0.2,0.2)),Color3.fromRGB(48,46,46),Enum.Material.Rock)
  end
 end
 -- Spawn point is kept clear at the southern approach to the plaza.
 local spawn=Instance.new("SpawnLocation")
 spawn.Name="NemezidaSpawn"; spawn.Size=Vector3.new(8,1,8); spawn.CFrame=CFrame.new(0,1,165)
 spawn.Anchored=true; spawn.Neutral=true; spawn.Duration=0; spawn.Transparency=0.25
 spawn.Color=Color3.fromRGB(95,65,125); spawn.Material=Enum.Material.Neon; spawn.Parent=root
 return root
end
return HubGenerator
