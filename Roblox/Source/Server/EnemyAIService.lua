--!strict
-- ModuleScript: ServerScriptService/Services/EnemyAIService
-- Tagged NPCs remain neutral until a player damages/provokes them.
local Players=game:GetService("Players")
local CollectionService=game:GetService("CollectionService")
local PathfindingService=game:GetService("PathfindingService")
local ServerScriptService=game:GetService("ServerScriptService")
local Config=require(game:GetService("ReplicatedStorage"):WaitForChild("Nemezida"):WaitForChild("Shared"):WaitForChild("NemezidaConfig"))
local NemesisService=require(ServerScriptService:WaitForChild("Services"):WaitForChild("NemesisService"))
local Service={}
local states: {[Model]: any}={}
local started=false

local function playerParts(player: Player)
 local c=player.Character
 if not c then return nil end
 local h=c:FindFirstChildOfClass("Humanoid")
 local r=c:FindFirstChild("HumanoidRootPart")
 if not h or h.Health<=0 or not r then return nil end
 return c,h,r
end

local function register(model: Model)
 if states[model] then return end
 local h=model:FindFirstChildOfClass("Humanoid")
 local root=model:FindFirstChild("HumanoidRootPart") or model.PrimaryPart
 if not h or not root then warn("Enemy missing Humanoid/root:",model:GetFullName()); return end
 h.MaxHealth=model:GetAttribute("MaxHealth") or 55
 h.Health=h.MaxHealth
 h.WalkSpeed=model:GetAttribute("WalkSpeed") or 14
 states[model]={home=model:GetPivot(),target=nil,nextAttack=0,nextPath=0,encounterPlayerId=nil}
 h.Died:Connect(function() model:SetAttribute("AIState","Dead"); states[model]=nil end)
end

local function getProvoker(model: Model,position: Vector3): Player?
 if workspace:GetServerTimeNow() > (model:GetAttribute("ProvokedUntil") or 0) then return nil end
 local id=model:GetAttribute("LastDamagedByUserId")
 if typeof(id)~="number" then return nil end
 local p=Players:GetPlayerByUserId(id)
 if not p then return nil end
 local _,_,r=playerParts(p)
 if not r then return nil end
 if (r.Position-position).Magnitude > (model:GetAttribute("DetectionRange") or 18)*2 then return nil end
 return p
end

local function moveTo(model: Model,h: Humanoid,goal: Vector3)
 local root=model:FindFirstChild("HumanoidRootPart") or model.PrimaryPart
 if not root then return end
 local path=PathfindingService:CreatePath({AgentRadius=2,AgentHeight=5,AgentCanJump=true})
 local ok=pcall(function() path:ComputeAsync(root.Position,goal) end)
 if ok and path.Status==Enum.PathStatus.Success then
  local points=path:GetWaypoints()
  if points[2] then h:MoveTo(points[2].Position); return end
 end
 h:MoveTo(goal)
end

local function hasLineOfSight(enemy: Model,targetCharacter: Model): boolean
 local enemyRoot=enemy:FindFirstChild("HumanoidRootPart") or enemy.PrimaryPart
 local targetRoot=targetCharacter:FindFirstChild("HumanoidRootPart")
 if not enemyRoot or not targetRoot then return false end
 local origin=enemyRoot.Position+Vector3.new(0,1.5,0)
 local direction=targetRoot.Position-origin
 local params=RaycastParams.new()
 params.FilterType=Enum.RaycastFilterType.Exclude
 params.FilterDescendantsInstances={enemy}
 params.IgnoreWater=true
 local result=workspace:Raycast(origin,direction,params)
 return result==nil or result.Instance:IsDescendantOf(targetCharacter)
end

local function damagePlayer(player: Player,humanoid: Humanoid,amount: number)
 local now=workspace:GetServerTimeNow()
 local shield=player:GetAttribute("BarrierHealth") or 0
 local barrierUntil=player:GetAttribute("BarrierUntil") or 0
 if now < barrierUntil and shield > 0 then
  local absorbed=math.min(shield,amount)
  shield-=absorbed
  amount-=absorbed
  player:SetAttribute("BarrierHealth",shield)
  if shield<=0 then player:SetAttribute("BarrierUntil",0) end
 end
 if amount>0 then humanoid:TakeDamage(amount) end
end

local function returnHome(model: Model,h: Humanoid,root: BasePart,s)
 s.target=nil
 s.encounterPlayerId=nil
 model:SetAttribute("AIState","Return")
 if (root.Position-s.home.Position).Magnitude > Config.Combat.EnemyReturnDistance then
  if os.clock()>=s.nextPath then
   s.nextPath=os.clock()+1
   moveTo(model,h,s.home.Position)
  end
 else
  model:SetAttribute("AIState","Idle")
 end
end

local function update(model: Model,s)
 local h=model:FindFirstChildOfClass("Humanoid")
 local root=model:FindFirstChild("HumanoidRootPart") or model.PrimaryPart
 if not h or h.Health<=0 or not root then return end
 local now=workspace:GetServerTimeNow()

 -- Expiry always ends the encounter, even if the player is standing beside the NPC.
 if s.target and now>(model:GetAttribute("ProvokedUntil") or 0) then
  returnHome(model,h,root,s)
  return
 end
 if not s.target or not playerParts(s.target) then
  s.target=getProvoker(model,root.Position)
 end
 if not s.target then
  if (root.Position-s.home.Position).Magnitude > Config.Combat.EnemyReturnDistance then
   returnHome(model,h,root,s)
  else
   model:SetAttribute("AIState","Idle")
  end
  return
 end

 local targetPlayer=s.target :: Player
 if s.encounterPlayerId~=targetPlayer.UserId then
  s.encounterPlayerId=targetPlayer.UserId
  NemesisService.RecordEncounter(targetPlayer)
 end
 local targetCharacter,targetHumanoid,targetRoot=playerParts(targetPlayer)
 if not targetCharacter or not targetHumanoid or not targetRoot then
  s.target=nil; s.encounterPlayerId=nil; return
 end
 local distance=(targetRoot.Position-root.Position).Magnitude
 local range=model:GetAttribute("AttackRange") or 5
 model:SetAttribute("AIState","Combat")
 if distance>range or not hasLineOfSight(model,targetCharacter) then
  if os.clock()>=s.nextPath then s.nextPath=os.clock()+1; moveTo(model,h,targetRoot.Position) end
 elseif os.clock()>=s.nextAttack then
  s.nextAttack=os.clock()+(model:GetAttribute("AttackCooldown") or 1.8)
  local attack=model:GetAttribute("Attack") or 9
  local defense=targetPlayer:GetAttribute("Defense") or Config.Player.Defense
  damagePlayer(targetPlayer,targetHumanoid,math.max(1,attack-defense*Config.Combat.DefenseFactor))
 end
end

function Service.Init()
 if started then return end
 started=true
 for _,item in CollectionService:GetTagged("NemezidaEnemy") do if item:IsA("Model") then register(item) end end
 CollectionService:GetInstanceAddedSignal("NemezidaEnemy"):Connect(function(item) if item:IsA("Model") then register(item) end end)
 task.spawn(function()
  while started do
   for model,state in states do
    if model.Parent then
     local ok,err=pcall(update,model,state)
     if not ok then warn("[Nemezida AI]",model:GetFullName(),err) end
    else states[model]=nil end
   end
   task.wait(0.25)
  end
 end)
end
return Service
