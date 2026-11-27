# Roblox Studio releases

## Текущая dev-сборка: v0.4-dev.5

[NEMEZIDA_Studio_v0_4_dev5.rbxlx](NEMEZIDA_Studio_v0_4_dev5.rbxlx)

**Запуск без команд:** скачай Place-файл, открой в Roblox Studio и нажми Play (F5). Серверный Bootstrap автоматически создаёт хаб, безопасную точку появления и противников, после чего загружает персонажа. Command Bar не нужен.

В v0.4-dev.5 дополнительно обработан случай, когда Player/Character уже появился до того, как Bootstrap закончил инициализацию: существующий персонаж также переносится на безопасную точку после готовности мира.

Содержит 12 объектов скриптов: 7 ModuleScript, 1 Script и 4 LocalScript. Проверка ограничена статической сверкой XML и исходников. Roblox Studio, Luau compiler, Rojo CLI и Play session не запускались. Это dev-прототип, runtime-поведение нужно подтвердить в Studio.

Автотест (не требуется для обычного запуска): Tests/STUDIO_WORLD_SPAWN_TEST.lua. Инструкция: Roblox/BUILD_AND_TEST.md.

## Предыдущий dev-срез: v0.4-dev.4

[NEMEZIDA_Studio_v0_4_dev4.rbxlx](NEMEZIDA_Studio_v0_4_dev4.rbxlx)
