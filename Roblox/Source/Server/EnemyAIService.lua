--!strict
-- ModuleScript: ServerScriptService/Services/EnemyAIService
-- Tagged NPCs remain neutral until a player damages/provokes them.
local Players=game:GetService("Players")
local CollectionService=game:GetService("CollectionService")
local PathfindingService=game:GetService("PathfindingService")
local Config=require(game:GetService("ReplicatedStorage"):WaitForChild("Nemezida"):WaitForChild("Shared"):WaitForChild("NemezidaConfig"))
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
 states[model]={home=model:GetPivot(),target=nil,nextAttack=0,nextPath=0}
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

local function update(model: Model,s)
 local h=model:FindFirstChildOfClass("Humanoid")
 local root=model:FindFirstChild("HumanoidRootPart") or model.PrimaryPart
 if not h or h.Health<=0 or not root then return end
 if not s.target or not playerParts(s.target) then s.target=getProvoker(model,root.Position) end
 if not s.target then
  model:SetAttribute("AIState","Idle")
  if (root.Position-s.home.Position).Magnitude > Config.Combat.EnemyReturnDistance then
   model:SetAttribute("AIState","Return")
   h:MoveTo(s.home.Position)
  end
  return
 end
 local _,targetHumanoid,targetRoot=playerParts(s.target)
 if not targetHumanoid or not targetRoot then s.target=nil; return end
 local distance=(targetRoot.Position-root.Position).Magnitude
 local range=model:GetAttribute("AttackRange") or 5
 model:SetAttribute("AIState","Combat")
 if distance>range then
  if os.clock()>=s.nextPath then s.nextPath=os.clock()+1; moveTo(model,h,targetRoot.Position) end
 elseif os.clock()>=s.nextAttack then
  s.nextAttack=os.clock()+(model:GetAttribute("AttackCooldown") or 1.8)
  local attack=model:GetAttribute("Attack") or 9
  local defense=s.target:GetAttribute("Defense") or Config.Player.Defense
  targetHumanoid:TakeDamage(math.max(1,attack-defense*Config.Combat.DefenseFactor))
 end
 if workspace:GetServerTimeNow()>(model:GetAttribute("ProvokedUntil") or 0) and distance>range*1.5 then
  s.target=nil; model:SetAttribute("AIState","Return")
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
