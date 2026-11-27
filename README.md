# Немезида: Печать Бездны

**Nemesis: Seal of the Abyss** — мрачная изометрическая action-RPG для Roblox.

> Текущий срез исходников: v0.4-dev.4. XML Place-файл собран из актуальных Luau-исходников. Статическая структура проверена; реальный Roblox Studio Play-тест ещё требуется.

## Концепция
Игрок — Носитель Печати, исследующий разрушенный мир, сражающийся с существами Бездны и раскрывающий происхождение катастрофы. Визуальное направление: тёмная изометрическая RPG, чёрный металл, пепел, руины, золотые элементы интерфейса и фиолетовая энергия.

## Текущий технический статус
v0.4-dev.4 содержит серверную боевую систему, нейтральный AI, детерминированную генерацию хаба, безопасный порядок появления игрока, автоматическое процедурное население из шести врагов, изометрическую камеру, способности, спринт и HUD.

Подробности: [технический статус](docs/TECHNICAL_STATUS.md), [статическая проверка v0.4-dev.4](Tests/STATIC_REVIEW_v0_4-dev4.txt), [история изменений](CHANGELOG.md).

**Граница проверки:** Roblox Studio, Luau compiler, Rojo build и Play session в этой среде не запускались. Не считать статическую проверку подтверждением runtime-работоспособности.

## Файл Roblox Studio
Открой или скачай [NEMEZIDA_Studio_v0_4_dev4.rbxlx](Roblox/Releases/NEMEZIDA_Studio_v0_4_dev4.rbxlx), затем в Studio выбери File → Open from File. В нём находится актуальная серверная и клиентская логика v0.4-dev.4.

## Быстрый старт
1. Скачать .rbxlx из Roblox/Releases/.
2. Открыть через File → Open from File в Roblox Studio.
3. Нажать Play (F5).
4. Убедиться, что игрок стоит на площади, в Explorer есть Workspace/NemezidaGeneratedHub, а в мире появились нейтральные враги.
5. При необходимости запустить Tests/STUDIO_WORLD_SPAWN_TEST.lua в серверном Command Bar во время Play.

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
- [Вехи разработки](docs/MILESTONES.md)
- [Технический статус](docs/TECHNICAL_STATUS.md)
- [Приёмочные тесты](Tests/ACCEPTANCE_TESTS_v0_4.md)

## Структура
- Roblox/Releases/ — Place-файлы Studio и заметки к ним.
- Roblox/Source/ — Luau-исходники.
- Roblox/Assets/ — ресурсы и концепт-арт.
- docs/ — документация.
- Tests/ — статические и приёмочные проверки.