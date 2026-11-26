--!strict
-- LocalScript: StarterPlayer/StarterPlayerScripts/NemezidaMovement
local Players=game:GetService("Players")
local UserInputService=game:GetService("UserInputService")
local ReplicatedStorage=game:GetService("ReplicatedStorage")
local player=Players.LocalPlayer
local remote=ReplicatedStorage:WaitForChild("NemezidaRemotes"):WaitForChild("SetSprinting") :: RemoteEvent
local sprinting=false
local function setSprint(value: boolean)
 if sprinting==value then return end
 sprinting=value
 remote:FireServer(value)
end
UserInputService.InputBegan:Connect(function(input,processed)
 if not processed and input.KeyCode==Enum.KeyCode.LeftShift then setSprint(true) end
end)
UserInputService.InputEnded:Connect(function(input)
 if input.KeyCode==Enum.KeyCode.LeftShift then setSprint(false) end
end)
player.CharacterAdded:Connect(function() sprinting=false end)
