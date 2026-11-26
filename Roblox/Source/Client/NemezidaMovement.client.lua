--!strict
-- LocalScript: StarterPlayer/StarterPlayerScripts/NemezidaMovement
local Players=game:GetService("Players")
local UserInputService=game:GetService("UserInputService")
local ReplicatedStorage=game:GetService("ReplicatedStorage")
local player=Players.LocalPlayer
local remote=ReplicatedStorage:WaitForChild("NemezidaRemotes"):WaitForChild("SetSprinting") :: RemoteEvent
local requestedSprinting=false

local function setSprint(value: boolean)
 if requestedSprinting==value then return end
 requestedSprinting=value
 remote:FireServer(value)
end

remote.OnClientEvent:Connect(function(serverState)
 if typeof(serverState)~="boolean" then return end
 -- If the server rejects a start request (for example, empty stamina), wait
 -- for the player to release and press Shift again instead of pretending to run.
 if not serverState and requestedSprinting then
  requestedSprinting=false
 end
end)

UserInputService.InputBegan:Connect(function(input,processed)
 if not processed and input.KeyCode==Enum.KeyCode.LeftShift then setSprint(true) end
end)
UserInputService.InputEnded:Connect(function(input)
 if input.KeyCode==Enum.KeyCode.LeftShift then setSprint(false) end
end)

player.CharacterAdded:Connect(function()
 requestedSprinting=false
end)
