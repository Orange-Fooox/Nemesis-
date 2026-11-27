# Немезида: Печать Бездны

**Nemesis: Seal of the Abyss** — мрачная изометрическая action-RPG для Roblox.

> Текущий срез исходников: v0.4-dev.6. Статический статус описан в технической документации; Play-тест в Roblox Studio ещё требуется.

## Текущий технический статус
v0.4-dev.6 содержит автоматический сценарий запуска по Play, серверную боевую систему, нейтральный AI, генерацию хаба, шесть тестовых врагов, изометрическую камеру, способности, спринт и HUD.

В файле NEMEZIDA_Forest_Grove_Placed_v2(1).rbxlx подтверждена папка Workspace/Meshes с 542 PolygonDungeon MeshPart и одним Polygon-модулем. Это библиотека ассетов внутри Place, а не подтверждение готового и протестированного подземелья.

Подробности: [технический статус](docs/TECHNICAL_STATUS.md), [аудит Polygon Pack](docs/POLYGON_PACK_AUDIT.md), [план переноса на Unity](docs/UNITY_PORT_PLAN.md), [история изменений](CHANGELOG.md).

**Граница проверки:** Roblox Studio, Luau compiler, Rojo build и Play session в этой среде не запускались.

## Файл Roblox Studio
Аудированный Place-файл NEMEZIDA_Forest_Grove_Placed_v2(1).rbxlx пока не опубликован в Roblox/Releases репозитория. Старые dev-файлы находятся в [Roblox/Releases](Roblox/Releases/).

## Быстрый старт
1. Открыть актуальный .rbxlx через File → Open from File в Roblox Studio.
2. Нажать Play (F5).
3. Проверить появление игрока, HUD, хаб и коллизии.
4. Отдельно проверить Workspace/Meshes: наличие ассетов не означает, что dungeon layout уже собран.

Управление: WASD, ЛКМ/F, Z/X/C/V/B, Shift.

## Исходники
Luau-код находится в [Roblox/Source](Roblox/Source/README.md). Для синхронизации через Rojo используй default.project.json и инструкцию [BUILD_AND_TEST](Roblox/BUILD_AND_TEST.md).

## Документы
- [Game Bible](docs/GAME_BIBLE.md)
- [Бестиарий](docs/BESTIARY.md)
- [Оружие и броня](docs/WEAPONS_AND_ARMOR.md)
- [Боевая система](docs/COMBAT_SYSTEM.md)
- [Система Немезиды](docs/NEMESIS_SYSTEM.md)
- [Процедурная генерация](docs/PROCEDURAL_GENERATION.md)
- [Мир и подземелья](docs/WORLD_AND_DUNGEONS.md)
- [Аудит Polygon Pack](docs/POLYGON_PACK_AUDIT.md)
- [План переноса на Unity](docs/UNITY_PORT_PLAN.md)
- [Вехи разработки](docs/MILESTONES.md)
- [Технический статус](docs/TECHNICAL_STATUS.md)
- [Приёмочные тесты](Tests/ACCEPTANCE_TESTS_v0_4.md)

## Структура
- Roblox/Releases/ — Place-файлы Studio.
- Roblox/Source/ — Luau-исходники.
- Roblox/Assets/ — ресурсы и концепт-арт.
- docs/ — документация.
- Tests/ — проверки.