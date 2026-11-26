# Исходники Roblox — v0.4-dev.1

Это Luau-исходники текущего этапа разработки, а не готовый бинарный релиз Roblox Studio.

## Иерархия в Studio

ReplicatedStorage:
- Nemezida/Shared/NemezidaConfig (ModuleScript)

ServerScriptService:
- Bootstrap (Script)
- Services/CombatService (ModuleScript)
- Services/EnemyAIService (ModuleScript)
- Services/HubGenerator (ModuleScript)
- Services/NemesisService (ModuleScript)

StarterPlayer/StarterPlayerScripts:
- NemezidaCamera (LocalScript)
- NemezidaCombat (LocalScript)
- NemezidaMovement (LocalScript)

## Управление

- WASD — движение (стандартный контроллер Roblox).
- ЛКМ или F — базовая атака.
- Z — Удар Печати.
- X — Теневой рывок.
- C — Разлом земли.
- V — Высвобождение Бездны.
- Shift — бег.
- Камера — изометрическая, следует за персонажем.

## Генератор хаба

HubGenerator создаёт детерминированный greybox «Пепельного Рубежа» размером 420×420 studs: границы, центральную площадь, улицы, здания, руины и точку появления. Seed задаётся атрибутом Workspace NemezidaWorldSeed.

Это пока геометрический прототип. Уникальные художественные модели, полноценные районы, NPC, интерактивные сундуки, навигационные тесты и декор высокого качества ещё впереди.

## Настройка врагов

NPC должен быть Model с Humanoid и HumanoidRootPart (или PrimaryPart), тегом CollectionService NemezidaEnemy и числовыми атрибутами MaxHealth, Attack, Defense, WalkSpeed, DetectionRange, AttackRange, AttackCooldown. AI нейтрален: бой начинается после серверно подтверждённого попадания.

## RemoteEvents

Bootstrap создаёт ReplicatedStorage/NemezidaRemotes/BasicAttack и UseAbility. Клиент передаёт только цель и имя способности. Урон, стоимость Essence и cooldown проверяются сервером.

## Ещё не реализовано

Модели героя и врагов, анимации, полноценный HUD, NPC и сундуки, генератор подземелий, лут, сохранения, полноценная адаптация Немезиды, серверная проверка скорости и тесты Studio. Не считать этот код готовой игрой до сборки и проверки в Roblox Studio.
