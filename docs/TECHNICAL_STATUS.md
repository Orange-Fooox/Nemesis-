# Технический статус

## Подтверждённая база: v0.2
Известная структура:
```
StarterPlayer/StarterPlayerScripts/
  NemezidaCamera.client.lua
  NemezidaControls.client.lua
StarterGui/
  NemezidaHUD.client.lua
ServerScriptService/
  NemezidaWorld.server.lua
  NemezidaCombat.server.lua
  NemezidaEnemyAI.server.lua
```

Известные функции: арена около 150 × 150 studs, периметр стен, базовые руины, шесть тестовых врагов, генератор Workspace.NEMEZIDA_WORLD, камера, управление, HUD, серверные бой и AI. Боевая система использует CollectionService-теги и проверяет цели примерно в пределах 8 studs перед игроком.

## v0.4-dev.1
Была добавлена начальная Luau-основа: общая конфигурация, серверные боевые сервисы, нейтральный AI, greybox-генератор хаба, NemesisService, управление и изометрическая камера. Первичная статическая проверка выявила дефекты, перечисленные в Tests/STATIC_REVIEW_v0_4-dev1.txt.

## v0.4-dev.2 — исправления исходников
Внесены изменения в:
- `Roblox/Source/Shared/NemezidaConfig.lua`
- `Roblox/Source/Server/CombatService.lua`
- `Roblox/Source/Server/EnemyAIService.lua`
- `Roblox/Source/Server/MovementService.lua`
- `Roblox/Source/Client/NemezidaCombat.client.lua`

Исправлено/добавлено на уровне исходного кода:
1. AI завершает агрессию при истечении ProvokedUntil независимо от расстояния до игрока и возвращается домой.
2. Проверка линии видимости через Raycast добавлена для атак игрока и врагов.
3. ShadowDash выполняет серверный импульс движения; цель для рывка необязательна.
4. EarthRend наносит урон врагам в радиусе вокруг выбранной цели.
5. Добавлена способность Barrier: запас щита равен 30% MaxHealth, длительность 4 секунды; AI сначала поглощает урон щитом.
6. VulnerabilityBonus от AbyssRelease теперь учитывается при последующих попаданиях в пределах таймера.
7. Спринт расходует и восстанавливает Stamina на сервере; включение ограничено по частоте, отпускание спринта принимается сразу.
8. CombatService и EnemyAIService вызывают NemesisService для регистрации действий и встреч.

## Оставшиеся ограничения
- NemesisService всё ещё хранит профиль только в памяти; DataStore не добавлен.
- Реальный спавн охотника Немезиды и адаптация его боевого поведения не реализованы.
- HUD не отображает Essence, Stamina и Barrier.
- Нет финальных моделей, анимаций, VFX, NPC хаба, сундуков, подземелий, лута и наград.
- Нет полноценного target lock; выбор цели использует Mouse.Target.
- Генератор хаба создаёт greybox, а не финальную локацию.

## Проверка и статус
Изменения v0.4-dev.2 внесены в GitHub как текстовые исходники. В рамках этого обновления Roblox Studio, Luau runtime, Rojo build и multiplayer session НЕ запускались. Поэтому синтаксис, запуск, физика, Raycast, сетевое поведение и баланс остаются непроверенными. Приёмочный чек-лист обновлён в `Tests/ACCEPTANCE_TESTS_v0_4.md`.

Проект не следует считать готовой или протестированной игрой до отдельного запуска и проверки в Roblox Studio.
