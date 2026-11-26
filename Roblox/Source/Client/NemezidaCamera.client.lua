--!strict
-- LocalScript: StarterPlayer/StarterPlayerScripts/NemezidaCamera
local Players=game:GetService("Players")
local RunService=game:GetService("RunService")
local player=Players.LocalPlayer
local camera=workspace.CurrentCamera
local HEIGHT=42
local BACK=34
local LOOK_OFFSET=Vector3.new(0,2,0)
camera.CameraType=Enum.CameraType.Scriptable
RunService:BindToRenderStep("NemezidaIsometricCamera",Enum.RenderPriority.Camera.Value,function()
 local character=player.Character
 local root=character and character:FindFirstChild("HumanoidRootPart")
 if not root then return end
 local focus=root.Position+LOOK_OFFSET
 local position=focus+Vector3.new(0,HEIGHT,BACK)
 camera.CFrame=CFrame.lookAt(position,focus)
 camera.FieldOfView=42
end)
player.CharacterAdded:Connect(function()
 camera.CameraType=Enum.CameraType.Scriptable
end)
