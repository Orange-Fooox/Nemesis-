# Технический статус

## Подтверждённая база: v0.2
Известные функции: арена около 150 × 150 studs, периметр стен, руины, тестовые враги, камера, управление, HUD, серверные бой и AI.

## v0.4-dev.1 — v0.4-dev.3
Начальная Luau-основа: общая конфигурация, серверный бой, нейтральный AI, greybox-генератор хаба, NemesisService, управление, камера и клиентские способности. В dev.2 добавлены Raycast-проверки, ShadowDash, EarthRend, Barrier, VulnerabilityBonus и серверный Stamina. В dev.3 исправлены перепривязка камеры, подтверждения состояния спринта и ручной helper NPC.

## v0.4-dev.6 — последняя сборка, опубликованная в GitHub

- Исправлена синтаксическая ошибка NemesisService, которая останавливала загрузку CombatService и Bootstrap.
- Подтверждено наличие NemezidaRemotes в серверном пути и ожидающих их клиентских скриптов.
- Place содержит 7 ModuleScript, 1 Script и 4 LocalScript.
- Это статическая сверка XML/исходников; запуск в Roblox Studio и Play/multiplayer не выполнялся.

## v0.4-dev.4 — безопасный старт и первая автоматическая популяция
1. Bootstrap отключает Players.CharacterAutoLoads перед инициализацией.
2. HubGenerator сначала создаёт anchored/collidable AshenGround, затем широкую SpawnPlaza.
3. NemezidaSpawn размещается непосредственно в Workspace.
4. Bootstrap ждёт генерацию и задаёт Player.RespawnLocation до первой загрузки персонажа.
5. CharacterAdded переносит персонажа на HubGenerator.GetSpawnCFrame() и сбрасывает скорость.
6. Если генератор не отдал корректный пол или SpawnLocation, Bootstrap добавляет резервную площадку.
7. PopulationService создаёт до шести детерминированных R6-врагов на маркерах хаба.
8. NemezidaHUD показывает здоровье, Essence, Stamina и активный Barrier.
9. default.project.json подключает PopulationService и HUD.

## v0.4-dev.5 — запуск одной кнопкой Play
1. Серверный Bootstrap остаётся единственной точкой входа.
2. Обрабатывается уже существующий player.Character, закрывая гонку появления.
3. HubGenerator помечает мир версией 0.4.0-dev.5.
4. После генерации Bootstrap устанавливает NemezidaWorldReady, включает CharacterAutoLoads и загружает ожидающих персонажей.
5. В журнале сервера выводится сообщение об окончании автозапуска.

## Polygon Pack audit (2026-11-27)
В Place-файле NEMEZIDA_Forest_Grove_Placed_v2(1).rbxlx статически найдены 542 PolygonDungeon MeshPart и один Polygon-модуль внутри Workspace/Meshes. Это подтверждает наличие набора моделей в проекте, но не сборку и не runtime-готовность подземелья. Следующий проход: собрать тестовую комнату, выставить коллизии по назначению, проверить проходы/лестницы/мосты, затем прогнать Play и multiplayer.

## Что ещё не реализовано или не подтверждено
- DataStore и долговременное сохранение NemesisService.
- Полноценная адаптация и физический спавн охотника Немезиды.
- Финальные персонажи, модели врагов, анимации и VFX.
- NPC хаба, взаимодействия, сундуки, инвентарь, награды и лут.
- Сборка и runtime-проверка подземелий на Polygon Pack.
- Генерация подземелий, комнат, ловушек и событий.
- Target lock и dodge.
- Полная серверная сетевая и физическая приёмка.

## Проверка и статус
Структура XML Place, состав исходников и согласованность путей Rojo проверяются статически. Roblox Studio, Luau compiler, Rojo CLI и Play/multiplayer sessions в этой среде не запускались. Runtime-работоспособность должна быть подтверждена в Studio.

Отчёт: Tests/STATIC_REVIEW_v0_4-dev4.txt.
Приёмочные проверки: Tests/ACCEPTANCE_TESTS_v0_4.md.
Проверка безопасного появления: Tests/STUDIO_WORLD_SPAWN_TEST.lua.

Не объявлять версию runtime-проверенной до фактического прогона.