--!strict
-- ModuleScript: ServerScriptService/Services/CombatService
local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local ServerScriptService = game:GetService("ServerScriptService")
local CollectionService = game:GetService("CollectionService")
local Config = require(ReplicatedStorage:WaitForChild("Nemezida"):WaitForChild("Shared"):WaitForChild("NemezidaConfig"))
local NemesisService = require(ServerScriptService:WaitForChild("Services"):WaitForChild("NemesisService"))
local CombatService = {}
local lastAttack: {[Player]: number} = {}
local lastAbility: {[Player]: {[string]: number}} = {}

local function characterRoot(player: Player)
 local character = player.Character
 if not character then return nil end
 local humanoid = character:FindFirstChildOfClass("Humanoid")
 local root = character:FindFirstChild("HumanoidRootPart")
 if not humanoid or humanoid.Health <= 0 or not root then return nil end
 return character, humanoid, root
end

local function validEnemy(target: Instance?): Model?
 if not target then return nil end
 local model = if target:IsA("Model") then target else target:FindFirstAncestorOfClass("Model")
 if model and CollectionService:HasTag(model,"NemezidaEnemy") then return model end
 return nil
end

local function hasLineOfSight(originCharacter: Model, target: Model): boolean
 local originRoot = originCharacter:FindFirstChild("HumanoidRootPart")
 local targetRoot = target:FindFirstChild("HumanoidRootPart") or target.PrimaryPart
 if not originRoot or not targetRoot then return false end
 local origin = originRoot.Position + Vector3.new(0,1.5,0)
 local direction = targetRoot.Position-origin
 local params = RaycastParams.new()
 params.FilterType = Enum.RaycastFilterType.Exclude
 params.FilterDescendantsInstances = {originCharacter}
 params.IgnoreWater = true
 local result = workspace:Raycast(origin,direction,params)
 return result == nil or result.Instance:IsDescendantOf(target)
end

local function applyDamage(player: Player, enemy: Model, multiplier: number, maxDistance: number): boolean
 local character,_,root = characterRoot(player)
 local humanoid = enemy:FindFirstChildOfClass("Humanoid")
 if not character or not root or not humanoid or humanoid.Health <= 0 then return false end
 if (enemy:GetPivot().Position-root.Position).Magnitude > maxDistance then return false end
 if not hasLineOfSight(character,enemy) then return false end
 local defense = enemy:GetAttribute("Defense") or 0
 local attack = Config.Player.Attack + (player:GetAttribute("BonusAttack") or 0)
 local damage = math.max(1,attack*multiplier-defense*Config.Combat.DefenseFactor)
 local now = workspace:GetServerTimeNow()
 if now < (enemy:GetAttribute("VulnerableUntil") or 0) then
  damage *= 1 + (enemy:GetAttribute("VulnerabilityBonus") or 0)
 end
 enemy:SetAttribute("LastDamagedByUserId",player.UserId)
 enemy:SetAttribute("ProvokedUntil",now+Config.Combat.EnemyAlertDuration)
 humanoid:TakeDamage(damage)
 return true
end

function CombatService.BasicAttack(player: Player,target: Instance)
 local now = os.clock()
 if now-(lastAttack[player] or 0) < Config.Combat.AttackCooldown then return end
 local character,_,root = characterRoot(player)
 local enemy = validEnemy(target)
 if not character or not root or not enemy then return end
 if (enemy:GetPivot().Position-root.Position).Magnitude > Config.Combat.AttackRange then return end
 if not hasLineOfSight(character,enemy) then return end
 if applyDamage(player,enemy,Config.Combat.BasicAttackMultiplier,Config.Combat.AttackRange) then
  lastAttack[player]=now
  NemesisService.RecordAction(player,"Melee")
 end
end

function CombatService.UseAbility(player: Player,name: string,target: Instance?)
 local ability = Config.Abilities[name]
 local character,_,root = characterRoot(player)
 if not ability or not character or not root then return end
 lastAbility[player]=lastAbility[player] or {}
 local now=os.clock()
 if now-(lastAbility[player][name] or 0) < ability.Cooldown then return end
 local essence=player:GetAttribute("Essence")
 if essence==nil then essence=Config.Player.MaxEssence end
 if essence < ability.EssenceCost then return end

 local enemy = if name == "Barrier" then nil else validEnemy(target)
 -- Shadow Dash remains usable when the cursor happens to be over a distant or occluded enemy.
 if name == "ShadowDash" and enemy then
  if (enemy:GetPivot().Position-root.Position).Magnitude > ability.Range or not hasLineOfSight(character,enemy) then
   enemy=nil
  end
 end
 if name ~= "Barrier" and name ~= "ShadowDash" and not enemy then return end
 if enemy and (enemy:GetPivot().Position-root.Position).Magnitude > ability.Range then return end
 if enemy and not hasLineOfSight(character,enemy) then return end

 lastAbility[player][name]=now
 player:SetAttribute("Essence",essence-ability.EssenceCost)
 if name == "Barrier" then
  NemesisService.RecordAction(player,"Block")
 else
  NemesisService.RecordAction(player,"Melee")
 end

 if name == "Barrier" then
  local humanoid = character:FindFirstChildOfClass("Humanoid")
  if not humanoid then return end
  player:SetAttribute("BarrierHealth",humanoid.MaxHealth*ability.ShieldHealthPercent)
  player:SetAttribute("BarrierUntil",workspace:GetServerTimeNow()+ability.Duration)
  return
 elseif name == "ShadowDash" then
  local flat = Vector3.new(root.CFrame.LookVector.X,0,root.CFrame.LookVector.Z)
  if flat.Magnitude < 0.01 then return end
  root:ApplyImpulse(flat.Unit*root.AssemblyMass*ability.DashImpulse + Vector3.new(0,root.AssemblyMass*8,0))
  if enemy then applyDamage(player,enemy,ability.Multiplier,ability.Range) end
  return
 elseif name == "EarthRend" then
  local center = enemy:GetPivot().Position
  for _,candidate in CollectionService:GetTagged("NemezidaEnemy") do
   if candidate:IsA("Model") and candidate.Parent then
    local distance=(candidate:GetPivot().Position-center).Magnitude
    if distance <= ability.Radius then
     applyDamage(player,candidate,ability.Multiplier,ability.Range+ability.Radius)
    end
   end
  end
  return
 end

 if enemy and applyDamage(player,enemy,ability.Multiplier,ability.Range) and ability.Vulnerability then
  enemy:SetAttribute("VulnerableUntil",workspace:GetServerTimeNow()+ability.VulnerabilityDuration)
  enemy:SetAttribute("VulnerabilityBonus",ability.Vulnerability)
 end
end

function CombatService.Init()
 local folder=ReplicatedStorage:FindFirstChild("NemezidaRemotes") or Instance.new("Folder")
 folder.Name="NemezidaRemotes"; folder.Parent=ReplicatedStorage
 local attack=folder:FindFirstChild("BasicAttack") or Instance.new("RemoteEvent")
 attack.Name="BasicAttack"; attack.Parent=folder
 local ability=folder:FindFirstChild("UseAbility") or Instance.new("RemoteEvent")
 ability.Name="UseAbility"; ability.Parent=folder
 local notice=folder:FindFirstChild("ShowNotice") or Instance.new("RemoteEvent")
 notice.Name="ShowNotice"; notice.Parent=folder
 attack.OnServerEvent:Connect(function(player,target)
  if typeof(target)=="Instance" then CombatService.BasicAttack(player,target) end
 end)
 ability.OnServerEvent:Connect(function(player,name,target)
  if typeof(name)=="string" and #name<=32 and (target==nil or typeof(target)=="Instance") then
   CombatService.UseAbility(player,name,target)
  end
 end)
 Players.PlayerRemoving:Connect(function(player) lastAttack[player]=nil; lastAbility[player]=nil end)
end
return CombatService
