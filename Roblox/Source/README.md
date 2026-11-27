# Исходники Roblox — v0.4-dev.4

Это Luau-исходники dev-среза. Проектная структура и XML Place-файл проверяются статически; запуск в Roblox Studio должен быть подтверждён отдельным Play-прогоном.

## Иерархия в Studio

ReplicatedStorage:
- Nemezida/Shared/NemezidaConfig (ModuleScript)

ServerScriptService:
- Bootstrap (Script)
- Services/CombatService (ModuleScript)
- Services/EnemyAIService (ModuleScript)
- Services/HubGenerator (ModuleScript)
- Services/NemesisService (ModuleScript)
- Services/MovementService (ModuleScript)
- Services/PopulationService (ModuleScript)

StarterPlayer/StarterPlayerScripts:
- NemezidaCamera (LocalScript)
- NemezidaCombat (LocalScript)
- NemezidaMovement (LocalScript)
- NemezidaHUD (LocalScript)

## Управление
- WASD — движение стандартным контроллером Roblox.
- ЛКМ или F — базовая атака.
- Z — Удар Печати.
- X — Теневой рывок.
- C — Разлом земли.
- V — Высвобождение Бездны.
- B — Барьер.
- Shift — бег с расходом выносливости.

## Старт мира и безопасное появление
- Bootstrap отключает автоматическую загрузку персонажей до завершения генерации.
- HubGenerator первым создаёт твёрдую опорную поверхность и отдельную широкую площадку появления.
- NemezidaSpawn находится напрямую в Workspace, а не внутри декоративной папки.
- Bootstrap назначает RespawnLocation и перемещает персонажа на известный безопасный CFrame после появления.
- Если генератор не создаёт пол, Bootstrap создаёт резервную collidable-площадку.
- После подготовки мира автоматическая загрузка и стандартный респавн включаются обратно.

## Генерация и население
HubGenerator детерминированно создаёт хаб 420×420 studs, площадь, улицы, руины, монумент, безопасную площадку и шесть маркеров появления противников. PopulationService создаёт до шести простых R6-врагов из типов в NemezidaConfig, устанавливает боевые атрибуты и тег NemezidaEnemy. EnemyAIService регистрирует их автоматически.

Это greybox-реализация. Финальные модели, анимации, VFX, NPC хаба, подземелья, лут и сохранения пока не входят в эту сборку.

## HUD
NemezidaHUD показывает здоровье, Essence, Stamina и активный Barrier, а внизу напоминает клавиши способностей. HUD читает актуальные атрибуты игрока и состояние Humanoid.

## Проверки
- Tests/STUDIO_WORLD_SPAWN_TEST.lua — проверяет генерацию, пол, SpawnLocation, поверхность под spawn, наличие врагов и положение игроков. Запускается в серверном Command Bar во время Play.
- Tests/STUDIO_TEST_NPC_SETUP.lua — вспомогательный ручной тестовый NPC, больше не обязателен: PopulationService уже создаёт противников автоматически.
- Tests/ACCEPTANCE_TESTS_v0_4.md — полный чек-лист приёмки.

Отсутствие ошибок в статическом анализе не заменяет компиляцию Luau и запуск Roblox Studio. Перед публикацией обязательны Play-тест, тест смерти/респавна и проверка AI.