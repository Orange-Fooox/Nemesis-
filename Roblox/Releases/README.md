# Roblox Studio export — v0.4-dev.3

Файл: [NEMEZIDA_Studio_v0_4_dev3.rbxlx](NEMEZIDA_Studio_v0_4_dev3.rbxlx)

Это XML Place-файл Roblox Studio, собранный из исходников репозитория на момент v0.4-dev.3. Внутри размещены:

- ReplicatedStorage/Nemezida/Shared/NemezidaConfig
- ServerScriptService/Bootstrap и Services (CombatService, EnemyAIService, HubGenerator, NemesisService, MovementService)
- StarterPlayer/StarterPlayerScripts (изометрическая камера, бой, движение)
- Базовые настройки тёмного освещения

## Открытие

1. Скачай .rbxlx из GitHub.
2. В Roblox Studio открой File → Open from File.
3. Нажми Play (F5).

Хаб «Пепельный Рубеж» создаётся серверным HubGenerator при запуске. Для ручной проверки AI/боя создай тестового R15 NPC скриптом Tests/STUDIO_TEST_NPC_SETUP.lua через Command Bar во время Play.

## Статус проверки

- XML: статическая проверка структуры тегов пройдена.
- Состав: 10 исходных Luau-файлов встроены в 10 объектов скриптов/модулей.
- Roblox Studio / Play: не проверено в этой среде.
- Это dev-прототип, не публичный релиз и не гарантия отсутствия runtime-ошибок.
