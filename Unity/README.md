# NEMESIS — Unity project

Целевая версия: **Unity 6000.0.23f1**. Язык игровой логики: **C#**.

## Открытие проекта

1. Установи Unity Hub и Unity 6000.0.23f1 с Windows Build Support.
2. В Unity Hub выбери **Add → Add project from disk** и укажи эту папку `Unity`.
3. Дождись импорта и компиляции.
4. Выполни **Tools → Nemesis → Generate Asset Library**.
5. Если сцена отсутствует, выполни **Tools → Nemesis → Create Prototype Scene**.
6. Открой `Assets/Scenes/NemesisPrototype.unity` и нажми Play.

## Текущий прототип

- Изометрическая камера и управление WASD относительно камеры.
- Бег, уклонение, Stamina, Essence и HP.
- Базовая ближняя атака и пять способностей.
- Ash Raider, нейтральный до получения урона.
- Ashen Warlord с преследованием и телеграфируемой атакой по области.
- Прототипные персонажи, торговец, экипировка и элементы окружения.
- Editor-инструменты генерации префабов, материалов и тестовой сцены.

Генерируемые модели собраны из Unity primitives и служат greybox-ресурсами. Это не финальные pixel-art модели и не импортированные пакеты Asset Store.

## Сборка Windows x64

В Unity Editor выбери **Tools → Nemesis → Build Windows x64**. Результат появится в `Unity/Build/`.

Для облачной сборки используется GitHub Actions workflow `.github/workflows/unity-windows-build.yml`. Добавь секрет репозитория `UNITY_LICENSE` с действующей лицензией Unity. Workflow запускается при изменениях Unity-проекта в `main` или вручную из Actions.

## Управление

- WASD — движение
- Shift — бег
- Space — уклонение
- ЛКМ / F — обычная атака
- Z — Seal Strike
- X — Shadow Dash
- C — Earth Rend
- V — Abyss Release
- B — Barrier

## Проверка

Unity Editor в среде подготовки исходников не запускался. Компиляция, Play Mode и Windows build требуют фактического прогона в Unity 6000.0.23f1. Не считать прототип runtime-проверенным до прохождения этих шагов.

Ранний этап разработки проходил на Roblox; краткая историческая справка сохранена в корневом README и документации миграции. Текущая разработка ведётся только на Unity.
