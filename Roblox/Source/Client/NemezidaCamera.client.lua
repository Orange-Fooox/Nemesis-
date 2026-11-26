--!strict
-- LocalScript: StarterPlayer/StarterPlayerScripts/NemezidaCamera
local Players=game:GetService("Players")
local RunService=game:GetService("RunService")
local player=Players.LocalPlayer
local activeCamera: Camera?=nil
local HEIGHT=42
local BACK=34
local LOOK_OFFSET=Vector3.new(0,2,0)

RunService:BindToRenderStep("NemezidaIsometricCamera",Enum.RenderPriority.Camera.Value,function()
 local camera=workspace.CurrentCamera
 if not camera then return end
 if activeCamera~=camera then
  activeCamera=camera
  camera.CameraType=Enum.CameraType.Scriptable
 end

 local character=player.Character
 local root=character and character:FindFirstChild("HumanoidRootPart")
 if not root then return end
 local focus=root.Position+LOOK_OFFSET
 local position=focus+Vector3.new(0,HEIGHT,BACK)
 camera.CFrame=CFrame.lookAt(position,focus)
 camera.FieldOfView=42
end)
