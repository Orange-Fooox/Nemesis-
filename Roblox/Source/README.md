# Исходники Roblox — v0.4-dev.1

Это Luau-исходники текущего этапа разработки, а не готовый бинарный релиз Roblox Studio.

## Иерархия в Studio

- ReplicatedStorage/Nemezida/Shared/NemezidaConfig (ModuleScript)
- ServerScriptService/Services/CombatService (ModuleScript)
- ServerScriptService/Services/EnemyAIService (ModuleScript)
- ServerScriptService/Bootstrap (Script)

Файлы в репозитории:
- Shared/NemezidaConfig.lua
- Server/CombatService.lua
- Server/EnemyAIService.lua
- Server/Bootstrap.server.lua

## Настройка врагов

NPC должен быть Model с Humanoid и HumanoidRootPart (или PrimaryPart), тегом CollectionService NemezidaEnemy и атрибутами EnemyType (string), MaxHealth, Attack, Defense, WalkSpeed, DetectionRange, AttackRange, AttackCooldown (number).

AI нейтрален: не ищет игрока сам по себе. Бой начинается после подтверждённого сервером попадания. По истечении ProvokedUntil враг прекращает преследование и возвращается к исходной точке.

## RemoteEvents

Bootstrap создаёт ReplicatedStorage/NemezidaRemotes/BasicAttack и UseAbility. Клиент передаёт только Instance цели и имя способности. Урон, стоимость Essence и cooldown проверяются сервером.

## Ещё не реализовано в этом dev-срезе

Клиентское управление и анимации, модели героя и врагов, HUD, генератор хаба и подземелий, сундуки/NPC, лут, сохранения, полноценная система Немезиды и тесты Studio. Не считать этот код готовой игрой до сборки и проверки в Roblox Studio.
