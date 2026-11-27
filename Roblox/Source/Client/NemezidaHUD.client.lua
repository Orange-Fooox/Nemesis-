--!strict
-- LocalScript: StarterPlayer/StarterPlayerScripts/NemezidaHUD
-- Compact dark-and-gold HUD for health, essence, stamina and Barrier.
local Players = game:GetService("Players")
local RunService = game:GetService("RunService")
local player = Players.LocalPlayer
local playerGui = player:WaitForChild("PlayerGui")

local GOLD = Color3.fromRGB(206, 165, 91)
local MUTED = Color3.fromRGB(155, 145, 132)
local PANEL = Color3.fromRGB(19, 17, 22)
local TRACK = Color3.fromRGB(48, 43, 52)

local gui = Instance.new("ScreenGui")
gui.Name = "NemezidaHUD"
gui.ResetOnSpawn = false
gui.IgnoreGuiInset = false
gui.DisplayOrder = 20
gui.Parent = playerGui

local panel = Instance.new("Frame")
panel.Name = "StatusPanel"
panel.Position = UDim2.fromOffset(22, 22)
panel.Size = UDim2.fromOffset(292, 190)
panel.BackgroundColor3 = PANEL
panel.BackgroundTransparency = 0.12
panel.BorderSizePixel = 0
panel.Parent = gui

local panelCorner = Instance.new("UICorner")
panelCorner.CornerRadius = UDim.new(0, 8)
panelCorner.Parent = panel

local border = Instance.new("UIStroke")
border.Color = GOLD
border.Thickness = 1
border.Transparency = 0.3
border.Parent = panel

local title = Instance.new("TextLabel")
title.Name = "Title"
title.Position = UDim2.fromOffset(14, 9)
title.Size = UDim2.new(1, -28, 0, 23)
title.BackgroundTransparency = 1
title.Font = Enum.Font.GothamBold
title.Text = "Н Е М Е З И Д А"
title.TextColor3 = GOLD
title.TextSize = 15
title.TextXAlignment = Enum.TextXAlignment.Left
title.Parent = panel

local subtitle = Instance.new("TextLabel")
subtitle.Position = UDim2.fromOffset(14, 31)
subtitle.Size = UDim2.new(1, -28, 0, 15)
subtitle.BackgroundTransparency = 1
subtitle.Font = Enum.Font.Gotham
subtitle.Text = "НОСИТЕЛЬ ПЕЧАТИ  //  ПЕПЕЛЬНЫЙ РУБЕЖ"
subtitle.TextColor3 = MUTED
subtitle.TextSize = 9
subtitle.TextXAlignment = Enum.TextXAlignment.Left
subtitle.Parent = panel

local barRows: {[string]: {Fill: Frame, Value: TextLabel}} = {}
local barSpecs = {
 {Key="Health", Label="ЗДОРОВЬЕ", Color=Color3.fromRGB(164, 61, 66)},
 {Key="Essence", Label="ЭСЕНЦИЯ", Color=Color3.fromRGB(137, 78, 184)},
 {Key="Stamina", Label="ВЫНОСЛИВОСТЬ", Color=Color3.fromRGB(183, 150, 78)},
 {Key="Barrier", Label="БАРЬЕР", Color=Color3.fromRGB(85, 150, 185)},
}

for index, spec in barSpecs do
 local y = 54 + (index-1)*31
 local label = Instance.new("TextLabel")
 label.Position = UDim2.fromOffset(14, y)
 label.Size = UDim2.fromOffset(104, 13)
 label.BackgroundTransparency = 1
 label.Font = Enum.Font.GothamBold
 label.Text = spec.Label
 label.TextColor3 = MUTED
 label.TextSize = 9
 label.TextXAlignment = Enum.TextXAlignment.Left
 label.Parent = panel

 local value = Instance.new("TextLabel")
 value.Position = UDim2.new(1, -75, 0, y-1)
 value.Size = UDim2.fromOffset(61, 14)
 value.BackgroundTransparency = 1
 value.Font = Enum.Font.GothamMedium
 value.Text = "0 / 0"
 value.TextColor3 = Color3.fromRGB(225, 218, 205)
 value.TextSize = 9
 value.TextXAlignment = Enum.TextXAlignment.Right
 value.Parent = panel

 local track = Instance.new("Frame")
 track.Position = UDim2.fromOffset(14, y+15)
 track.Size = UDim2.new(1, -28, 0, 7)
 track.BackgroundColor3 = TRACK
 track.BorderSizePixel = 0
 track.ClipsDescendants = true
 track.Parent = panel

 local trackCorner = Instance.new("UICorner")
 trackCorner.CornerRadius = UDim.new(1, 0)
 trackCorner.Parent = track

 local fill = Instance.new("Frame")
 fill.Size = UDim2.fromScale(1, 1)
 fill.BackgroundColor3 = spec.Color
 fill.BorderSizePixel = 0
 fill.Parent = track

 local fillCorner = Instance.new("UICorner")
 fillCorner.CornerRadius = UDim.new(1, 0)
 fillCorner.Parent = fill
 barRows[spec.Key] = {Fill=fill, Value=value}
end

local footer = Instance.new("TextLabel")
footer.Name = "Controls"
footer.AnchorPoint = Vector2.new(0.5, 1)
footer.Position = UDim2.new(0.5, 0, 1, -18)
footer.Size = UDim2.fromOffset(540, 28)
footer.BackgroundColor3 = PANEL
footer.BackgroundTransparency = 0.18
footer.BorderSizePixel = 0
footer.Font = Enum.Font.GothamMedium
footer.Text = "ЛКМ / F  АТАКА     Z  УДАР     X  РЫВОК     C  РАЗЛОМ     V  БЕЗДНА     B  БАРЬЕР     SHIFT  БЕГ"
footer.TextColor3 = Color3.fromRGB(211, 196, 170)
footer.TextSize = 10
footer.Parent = gui

local footerCorner = Instance.new("UICorner")
footerCorner.CornerRadius = UDim.new(0, 6)
footerCorner.Parent = footer

local humanoid: Humanoid? = nil
local function bindCharacter(character: Model)
 humanoid = character:WaitForChild("Humanoid", 10) :: Humanoid?
end
player.CharacterAdded:Connect(bindCharacter)
if player.Character then task.spawn(bindCharacter, player.Character) end

local function updateBar(key: string, current: number, maximum: number, visible: boolean?)
 local row = barRows[key]
 if not row then return end
 local ratio = if maximum > 0 then math.clamp(current/maximum, 0, 1) else 0
 row.Fill.Size = UDim2.fromScale(ratio, 1)
 row.Value.Text = string.format("%d / %d", math.floor(math.max(0,current)+0.5), math.floor(math.max(0,maximum)+0.5))
 if visible ~= nil then
  row.Fill.Parent.Parent.Visible = visible
 end
end

RunService.RenderStepped:Connect(function()
 local currentHumanoid = humanoid
 local health = currentHumanoid and currentHumanoid.Health or 0
 local maxHealth = currentHumanoid and currentHumanoid.MaxHealth or 100
 updateBar("Health", health, maxHealth)
 local essence = player:GetAttribute("Essence") or 0
 local maxEssence = player:GetAttribute("MaxEssence") or 100
 updateBar("Essence", essence, maxEssence)
 local stamina = player:GetAttribute("Stamina") or 0
 local maxStamina = player:GetAttribute("MaxStamina") or 100
 updateBar("Stamina", stamina, maxStamina)
 local shield = player:GetAttribute("BarrierHealth") or 0
 local barrierUntil = player:GetAttribute("BarrierUntil") or 0
 local barrierActive = shield > 0 and workspace:GetServerTimeNow() < barrierUntil
 updateBar("Barrier", shield, maxHealth*0.3, barrierActive)
end)
