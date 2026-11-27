# Roblox Studio releases

## Последняя сборка, опубликованная в GitHub: v0.4-dev.6

[Скачать NEMEZIDA_Studio_v0_4_dev6.rbxlx](NEMEZIDA_Studio_v0_4_dev6.rbxlx)

Это единственный Place-файл, оставленный в каталоге Releases. Предыдущие бинарные сборки удалены из текущей ветки, чтобы не путать их с актуальной.

**Запуск:** скачай Place-файл, открой в Roblox Studio через **File → Open from File** и нажми **Play (F5)**. Серверный Bootstrap автоматически создаёт хаб, безопасную точку появления и противников. Command Bar не нужен.

В v0.4-dev.6 исправлена синтаксическая ошибка NemesisService, из-за которой падала загрузка CombatService и Bootstrap. После этого серверная инициализация создаёт NemezidaRemotes, которые ожидают клиентские скрипты.

Содержит 12 объектов скриптов: 7 ModuleScript, 1 Script и 4 LocalScript. Проверка ограничена статической сверкой XML и исходников. Roblox Studio, Luau compiler, Rojo CLI и Play session не запускались. Это dev-прототип, runtime-поведение нужно подтвердить в Studio.

## Более поздние материалы разработки

- [DEVLOG v0.4.1 — First playable combat & dungeon pass](../../docs/DEVLOG_v0_4_1.md): описывает отдельную сборку с первым проходом боя, подземельем и NPC. Сам Place-файл в GitHub не опубликован.
- [DEVLOG v0.4.2 — Forest Grove integration](../../docs/DEVLOG_v0_4_2.md): описывает интеграцию 57 моделей лесного окружения в отдельный Place-файл, переданный в чате. Файл также не опубликован в GitHub.

Пока эти отдельные Place-файлы не загружены в репозиторий, единственной доступной для скачивания сборкой остаётся v0.4-dev.6.

Инструкция: [Roblox/BUILD_AND_TEST.md](../BUILD_AND_TEST.md).
