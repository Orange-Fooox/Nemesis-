--!strict
-- Studio-only helper. Paste this whole script into Roblox Studio's Command Bar
-- while the v0.4-dev.3 place is running in Play mode.
-- It creates one standard R15 NPC for AI/combat acceptance testing.

local Players = game:GetService("Players")
local CollectionService = game:GetService("CollectionService")

task.spawn(function()
 local description = Instance.new("HumanoidDescription")
 local ok, result = pcall(function()
  return Players:CreateHumanoidModelFromDescriptionAsync(description, Enum.HumanoidRigType.R15)
 end)
 description:Destroy()

 if not ok or not result then
  warn("[Nemezida Test] Could not create the test NPC:", result)
  return
 end

 local npc = result
 npc.Name = "TEST_GoblinScavenger"
 local spawnPart = workspace:FindFirstChild("NemezidaGeneratedHub")
  and workspace.NemezidaGeneratedHub:FindFirstChild("NemezidaSpawn")

 local spawnCFrame = if spawnPart and spawnPart:IsA("BasePart")
  then spawnPart.CFrame * CFrame.new(0, 3, -20)
  else CFrame.new(0, 4, 140)

 npc:PivotTo(spawnCFrame)
 npc:SetAttribute("MaxHealth", 55)
 npc:SetAttribute("Attack", 9)
 npc:SetAttribute("Defense", 2)
 npc:SetAttribute("WalkSpeed", 14)
 npc:SetAttribute("DetectionRange", 18)
 npc:SetAttribute("AttackRange", 5)
 npc:SetAttribute("AttackCooldown", 1.8)
 npc.Parent = workspace
 CollectionService:AddTag(npc, "NemezidaEnemy")

 print("[Nemezida Test] Test NPC created. It should remain neutral until damaged.")
end)
