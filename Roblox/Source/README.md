# Исходники Roblox — v0.4-dev.1

Это Luau-исходники текущего этапа разработки, а не готовый бинарный релиз Roblox Studio.

## Иерархия в Studio

- ReplicatedStorage/Nemezida/Shared/NemezidaConfig (ModuleScript)
- ServerScriptService/Services/CombatService (ModuleScript)
- ServerScriptService/Services/EnemyAIService (ModuleScript)
- ServerScriptService/Services/HubGenerator (ModuleScript)
- ServerScriptService/Services/NemesisService (ModuleScript)
- ServerScriptService/Bootstrap (Script)

## Генератор хаба

HubGenerator создаёт детерминированный greybox «Пепельного Рубежа» размером 420×420 studs: границы, центральную площадь, улицы, здания, руины и точку появления. Seed задаётся атрибутом Workspace NemezidaWorldSeed. Одинаковый seed и версия генератора воспроизводят одинаковую планировку.

Это пока геометрический прототип. Уникальные художественные модели, полноценные районы, NPC, интерактивные сундуки, навигационные тесты и декор высокого качества ещё впереди.

## Настройка врагов

NPC должен быть Model с Humanoid и HumanoidRootPart (или PrimaryPart), тегом CollectionService NemezidaEnemy и числовыми атрибутами MaxHealth, Attack, Defense, WalkSpeed, DetectionRange, AttackRange, AttackCooldown. AI нейтрален: бой начинается после серверно подтверждённого попадания.

## RemoteEvents

Bootstrap создаёт ReplicatedStorage/NemezidaRemotes/BasicAttack и UseAbility. Клиент передаёт только цель и имя способности. Урон, стоимость Essence и cooldown проверяются сервером.

## Ещё не реализовано

Клиентское управление и анимации, модели героя и врагов, HUD, NPC и сундуки, генератор подземелий, лут, сохранения, полноценная адаптация Немезиды, тесты Studio. Не считать этот код готовой игрой до сборки и проверки в Roblox Studio.
