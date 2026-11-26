--!strict
-- LocalScript: StarterPlayer/StarterPlayerScripts/NemezidaMovement
-- Sprint is presented locally; server-side movement validation should be added
-- before competitive play. Default Roblox WASD movement remains enabled.
local Players=game:GetService("Players")
local UserInputService=game:GetService("UserInputService")
local player=Players.LocalPlayer
local WALK_SPEED=16
local RUN_SPEED=23
local sprinting=false
local function apply()
 local character=player.Character
 local humanoid=character and character:FindFirstChildOfClass("Humanoid")
 if humanoid then humanoid.WalkSpeed=if sprinting then RUN_SPEED else WALK_SPEED end
end
UserInputService.InputBegan:Connect(function(input,processed)
 if not processed and input.KeyCode==Enum.KeyCode.LeftShift then sprinting=true; apply() end
end)
UserInputService.InputEnded:Connect(function(input)
 if input.KeyCode==Enum.KeyCode.LeftShift then sprinting=false; apply() end
end)
player.CharacterAdded:Connect(function()
 task.wait(0.2); apply()
end)
