--!strict
-- ModuleScript: ServerScriptService/Services/CombatService
local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local CollectionService = game:GetService("CollectionService")
local Config = require(ReplicatedStorage:WaitForChild("Nemezida"):WaitForChild("Shared"):WaitForChild("NemezidaConfig"))
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

local function applyDamage(player: Player, enemy: Model, multiplier: number): boolean
 local _,_,root = characterRoot(player)
 local humanoid = enemy:FindFirstChildOfClass("Humanoid")
 if not root or not humanoid or humanoid.Health <= 0 then return false end
 if (enemy:GetPivot().Position-root.Position).Magnitude > Config.Combat.AttackRange+5 then return false end
 local defense = enemy:GetAttribute("Defense") or 0
 local attack = Config.Player.Attack + (player:GetAttribute("BonusAttack") or 0)
 local damage = math.max(1,attack*multiplier-defense*Config.Combat.DefenseFactor)
 enemy:SetAttribute("LastDamagedByUserId",player.UserId)
 enemy:SetAttribute("ProvokedUntil",workspace:GetServerTimeNow()+Config.Combat.EnemyAlertDuration)
 humanoid:TakeDamage(damage)
 return true
end

function CombatService.BasicAttack(player: Player,target: Instance)
 local now = os.clock()
 if now-(lastAttack[player] or 0) < Config.Combat.AttackCooldown then return end
 local _,_,root = characterRoot(player)
 local enemy = validEnemy(target)
 if not root or not enemy or (enemy:GetPivot().Position-root.Position).Magnitude > Config.Combat.AttackRange then return end
 lastAttack[player]=now
 applyDamage(player,enemy,Config.Combat.BasicAttackMultiplier)
end

function CombatService.UseAbility(player: Player,name: string,target: Instance)
 local ability = Config.Abilities[name]
 local _,_,root = characterRoot(player)
 local enemy = validEnemy(target)
 if not ability or not root or not enemy then return end
 if (enemy:GetPivot().Position-root.Position).Magnitude > ability.Range then return end
 lastAbility[player]=lastAbility[player] or {}
 local now=os.clock()
 if now-(lastAbility[player][name] or 0) < ability.Cooldown then return end
 local essence=player:GetAttribute("Essence")
 if essence==nil then essence=Config.Player.MaxEssence end
 if essence < ability.EssenceCost then return end
 lastAbility[player][name]=now
 player:SetAttribute("Essence",essence-ability.EssenceCost)
 if applyDamage(player,enemy,ability.Multiplier) and ability.Vulnerability then
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
 attack.OnServerEvent:Connect(function(player,target)
  if typeof(target)=="Instance" then CombatService.BasicAttack(player,target) end
 end)
 ability.OnServerEvent:Connect(function(player,name,target)
  if typeof(name)=="string" and #name<=32 and typeof(target)=="Instance" then CombatService.UseAbility(player,name,target) end
 end)
 Players.PlayerRemoving:Connect(function(player) lastAttack[player]=nil; lastAbility[player]=nil end)
end
return CombatService
