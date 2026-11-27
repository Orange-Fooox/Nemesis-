--!strict
-- OPTIONAL DIAGNOSTIC ONLY. This script does NOT start the game.
-- Normal startup: open the Place in Studio and press Play (F5).
local Workspace = game:GetService("Workspace")
local Players = game:GetService("Players")
local CollectionService = game:GetService("CollectionService")

local hub = Workspace:FindFirstChild("NemezidaGeneratedHub")
assert(hub and hub:IsA("Folder"), "NemezidaGeneratedHub was not generated")
local ground = hub:FindFirstChild("AshenGround")
assert(ground and ground:IsA("BasePart"), "AshenGround is missing")
assert(ground.Anchored and ground.CanCollide, "AshenGround must be anchored and collidable")

local spawn = Workspace:FindFirstChild("NemezidaSpawn")
assert(spawn and spawn:IsA("SpawnLocation"), "NemezidaSpawn is missing")
assert(spawn.Parent == Workspace, "NemezidaSpawn must be a direct child of Workspace")
assert(spawn.Anchored and spawn.CanCollide, "SpawnLocation must be a solid anchored part")

local rayParams = RaycastParams.new()
rayParams.FilterType = Enum.RaycastFilterType.Exclude
local excluded = {}
for _, player in Players:GetPlayers() do
 if player.Character then table.insert(excluded, player.Character) end
end
rayParams.FilterDescendantsInstances = excluded
local ray = Workspace:Raycast(spawn.Position + Vector3.new(0, 12, 0), Vector3.new(0, -24, 0), rayParams)
assert(ray ~= nil, "No collidable surface exists beneath the spawn point")
assert(ray.Instance == spawn or ray.Instance.Name == "SpawnPlaza" or ray.Instance.Name == "AshenGround",
 "The spawn point is not above its intended safe surface")

local enemyCount = #CollectionService:GetTagged("NemezidaEnemy")
assert(enemyCount >= 1, "No procedural enemies were tagged")
for _, player in Players:GetPlayers() do
 local character = player.Character
 local root = character and character:FindFirstChild("HumanoidRootPart")
 assert(root and root:IsA("BasePart"), "A connected player has no HumanoidRootPart")
 assert(root.Position.Y > 0, "A connected player is below the generated world")
end

print(string.format("[Nemezida Spawn Test] PASS | ground=%s | spawn=%s | enemies=%d | players=%d",
 ground:GetFullName(), spawn:GetFullName(), enemyCount, #Players:GetPlayers()))
