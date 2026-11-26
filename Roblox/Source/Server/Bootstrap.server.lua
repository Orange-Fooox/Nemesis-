--!strict
-- Script: ServerScriptService/Bootstrap
local ReplicatedStorage=game:GetService("ReplicatedStorage")
local ServerScriptService=game:GetService("ServerScriptService")
local root=ReplicatedStorage:FindFirstChild("Nemezida") or Instance.new("Folder")
root.Name="Nemezida"; root.Parent=ReplicatedStorage
local services=ServerScriptService:WaitForChild("Services")
require(services:WaitForChild("CombatService")).Init()
require(services:WaitForChild("EnemyAIService")).Init()
local HubGenerator=require(services:WaitForChild("HubGenerator"))
require(services:WaitForChild("NemesisService"))
if workspace:GetAttribute("NemezidaWorldSeed")==nil then workspace:SetAttribute("NemezidaWorldSeed",847291) end
HubGenerator.Generate(workspace:GetAttribute("NemezidaWorldSeed"))
print("[Немезида] Серверные службы v0.4-dev.1 запущены.")
