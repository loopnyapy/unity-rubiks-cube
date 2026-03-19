# Rubik's Cube — Unity Project

## Общие сведения

| Параметр | Значение |
|---|---|
| Движок | Unity **6000.2.2f1** |
| Render Pipeline | Universal Render Pipeline (URP 17.2.0) |
| Input System | New Input System (`com.unity.inputsystem` 1.14.2) |
| Язык | C# |
| Сцена | `Assets/Scenes/SampleScene.unity` |

---

## Структура проекта

```
Assets/
├── Prefabs/
│   ├── Materials/          # 7 материалов (Black, Blue, Green, Orange, Red, White, Yellow)
│   ├── Piece.prefab        # Префаб одного маленького кубика (piece)
│   └── RayStart.prefab     # Пустой GO, используется как точка запуска лучей (raycast)
├── Scripts/
│   ├── CubeState.cs        # Хранит состояние сторон куба
│   ├── ReadCube.cs          # Чтение текущего состояния куба с помощью raycast
│   ├── PivotRotation.cs     # Вращение стороны куба мышью + авто-доворот до 90°
│   ├── SelectFace.cs        # Выбор стороны куба кликом (raycast → PickUp)
│   ├── DragDetection.cs     # Обобщённый детектор перетаскивания (LMB / RMB / MMB)
│   ├── SwipeDetection.cs    # Детектор свайпов (для вращения всего куба)
│   ├── RotateBigCube.cs     # Вращение всего куба (свайп / перетаскивание MMB)
│   ├── MouseInput.cs        # (Legacy?) Обёртка над левым кликом мыши
│   └── Extensions/
│       └── Vector2_Ext.cs   # Вспомогательные направления: upRight, upLeft, downLeft, downRight
├── Scenes/
│   └── SampleScene.unity
├── Settings/                # Настройки URP (PC, Mobile профили)
└── InputSystem_Actions.*    # Конфигурация New Input System
```

---

## Архитектура и поток данных

### Диаграмма взаимодействий

```
Ввод игрока
    │
    ├── LMB (левая кнопка) ──► DragDetection(LMB)
    │                               │
    │                               ├── PressPerformed ──► SelectFace.OnLeftMousePressed()
    │                               │                          │
    │                               │                          ├── ReadCube.ReadState() — обновить состояние
    │                               │                          ├── Raycast на Layer 8 — определить лицевую грань
    │                               │                          └── CubeState.PickUp(side)
    │                               │                                  │
    │                               │                                  ├── Перепривязать дочерние кубики к центральному pivot
    │                               │                                  └── PivotRotation.Rotate(side) — начать вращение стороны
    │                               │
    │                               ├── IsDragging (Update) ──► PivotRotation.SpinSide() — мышь вращает сторону
    │                               │
    │                               └── DragCancelled ──► PivotRotation.RotateToRightAngle()
    │                                                          │
    │                                                          └── AutoRotate() → доворот до ближайших 90°
    │                                                              → ReadCube.ReadState() — снова прочитать состояние
    │
    ├── MMB (средняя кнопка) ──► DragDetection(MMB)
    │                               │
    │                               └── DragPerformed ──► RotateBigCube.Drag() — свободное вращение куба
    │
    └── RMB (правая кнопка) ──► SwipeDetection
                                    │
                                    └── SwipePerformed ──► RotateBigCube.Swipe() — поворот на 90° в мировых осях
```

---

## Описание скриптов

### `CubeState.cs`

- **Роль:** Центральное хранилище состояния куба.
- **Данные:** 6 списков `List<GameObject>` — `front`, `back`, `right`, `left`, `up`, `down`. Каждый список содержит 9 GameObject'ов (лицевые грани маленьких кубиков одной стороны).
- **Метод `PickUp(cubeSide)`:**
  1. Перепривязывает `transform.parent` всех кубиков стороны к центральному кубику стороны (`cubeSide[4]`).
  2. Вызывает `PivotRotation.Rotate()` на pivot'е центрального кубика, инициируя вращение.
- **Зависимости:** `PivotRotation` (на родительском объекте центрального кубика).

### `ReadCube.cs`

- **Роль:** Чтение текущего физического расположения граней куба через raycast'ы.
- **Механизм:**
  - 6 Transform'ов (`tUp`, `tDown`, `tLeft`, `tRight`, `tFront`, `tBack`) — точки, с которых стреляют лучи.
  - Для каждой стороны создаётся 9 raycast'ов (3×3 сетка) в `BuildRays()`.
  - `ReadFace()` стреляет лучами и возвращает `List<GameObject>` попавших граней.
  - `ReadState()` обновляет все стороны в `CubeState`.
- **Layer Mask:** `1 << 8` (Layer 8 — предположительно «Face» / «CubeFace»).
- **Важно:** Вызывается при каждом клике (`SelectFace`) и после завершения авто-поворота (`PivotRotation.AutoRotate`).

### `PivotRotation.cs`

- **Роль:** Вращение одной стороны куба.
- **Прикреплён к:** Родительскому объекту каждого центрального кубика (pivot).
- **Логика:**
  1. `Rotate(side)` — запоминает активную сторону и ось вращения (`localForward` — вектор от центра куба к центру стороны).
  2. Во `Update()` при `IsDragging` — `SpinSide()` вращает transform вокруг `localForward` пропорционально движению мыши.
  3. `RotateToRightAngle()` — по отпусканию мыши: округляет угол до ближайших 90° и включает `isAutoRotating`.
  4. `AutoRotate()` — плавно доворачивает до целевого угла (300°/с). По достижении — вызывает `ReadCube.ReadState()`.
- **Параметры:** `sensitivity = 0.25f`, `speed = 300f`.

### `SelectFace.cs`

- **Роль:** Определение, на какую сторону куба кликнул пользователь.
- **Механизм:**
  1. По `PressPerformed` (LMB) обновляет состояние (`ReadCube.ReadState()`).
  2. Делает raycast из камеры по позиции мыши (Layer 8).
  3. Ищет попавшую грань во всех шести сторонах `CubeState`.
  4. Вызывает `CubeState.PickUp(cubeSide)` для найденной стороны.

### `DragDetection.cs`

- **Роль:** Универсальный детектор перетаскивания для любой кнопки мыши.
- **Паттерн:** Синглтон по кнопке (`static Dictionary<Button, DragDetection>`). Доступ: `DragDetection.Get(Button.LMB)`.
- **События:**
  - `PressPerformed` — кнопка нажата.
  - `DragPerformed(Vector2 delta)` — каждый кадр при перетаскивании (дельта позиции).
  - `DragPressed` — каждый кадр при перетаскивании (без аргумента).
  - `DragCancelled` — кнопка отпущена.
- **Свойства:** `IsDragging`, `CurrentPosition`.
- **Используется:** LMB для вращения стороны, MMB для вращения всего куба.

### `SwipeDetection.cs`

- **Роль:** Распознавание жестов свайпа (RMB).
- **Паттерн:** Синглтон (`instance`).
- **Механизм:** Замеряет начальную и конечную позицию мыши; если дельта по оси > `swipeResistance` (100 px) — генерирует направление (`Vector2` с компонентами -1/0/1).
- **Событие:** `SwipePerformed(Vector2 direction)`.

### `RotateBigCube.cs`

- **Роль:** Вращение всего кубика Рубика.
- **Два режима:**
  1. **Свайп (RMB):** Поворот `target` на 90° в мировых осях. Основной куб плавно подъезжает к `target.rotation` в `Update()` со скоростью `swipeSpeed = 400°/с`.
  2. **Перетаскивание (MMB):** Свободное вращение вокруг осей камеры (`cam.transform.right`, `cam.transform.up`), `dragSpeed = 0.1f`.
- **Зависимости:** `SwipeDetection.instance`, `DragDetection.Get(MMB)`.

### `MouseInput.cs`

- **Роль:** Обёртка над левым кликом мыши (позиция + нажатие/отпускание).
- **Статус:** Предположительно **устаревший** (legacy). Функциональность дублируется `DragDetection(LMB)`. Не используется другими скриптами напрямую.

### `Vector2_Ext.cs`

- **Namespace:** `Vector2Extensions`
- **Роль:** Добавляет статические `Vector2` для диагональных направлений: `upRight(1,1)`, `upLeft(-1,1)`, `downLeft(-1,-1)`, `downRight(1,-1)`.
- **Используется:** `RotateBigCube.Swipe()`.

---

## Иерархия объектов в сцене (предполагаемая)

```
Scene
├── Main Camera
├── Rubik's Cube (RotateBigCube)
│   ├── Piece_Center_Front (PivotRotation)
│   │   ├── SmallCube (с Collider на Layer 8 для каждой грани)
│   │   └── ... (при PickUp — временно дочерние кубики стороны)
│   ├── Piece_Center_Back (PivotRotation)
│   ├── ... (26 кубиков, 6 центральных с PivotRotation)
│   └── Target (пустой GO для целевого вращения свайпом)
├── ReadCube (ReadCube)
│   ├── tUp / tDown / tLeft / tRight / tFront / tBack (точки старта лучей)
│   └── RayStart instances (по 9 на сторону, 54 всего)
├── CubeState (CubeState)
├── SelectFace (SelectFace)
├── DragDetection_LMB (DragDetection, button=LMB)
├── DragDetection_MMB (DragDetection, button=MMB)
└── SwipeDetection (SwipeDetection)
```

---

## Механика вращения стороны — пошагово

1. Игрок нажимает **LMB** на грань куба.
2. `SelectFace` → raycast → определяет сторону → `CubeState.PickUp(side)`.
3. `PickUp` перепривязывает все 8 несерединных кубиков стороны к pivot'у центрального.
4. `PivotRotation.Rotate(side)` сохраняет ось вращения и начальную позицию мыши.
5. При перетаскивании: `PivotRotation.SpinSide()` вращает pivot вокруг оси пропорционально перемещению мыши.
6. При отпускании мыши: `RotateToRightAngle()` → авто-доворот до ближайших 90°.
7. После доворота: `ReadCube.ReadState()` перечитывает все 6 сторон через raycast.
8. **Важно:** после ReadState кубики **не** перепривязываются обратно — они остаются в новой иерархии до следующего PickUp.

---

## Управление

| Ввод | Действие |
|---|---|
| **LMB** (клик + перетаскивание) | Выбрать и вращать сторону куба |
| **MMB** (перетаскивание) | Свободное вращение всего куба |
| **RMB** (свайп) | Поворот всего куба на 90° |

---

## Известные проблемы и TODO

| # | Файл | Проблема / TODO |
|---|---|---|
| 1 | `ReadCube.cs:33` | `// TODO is it needed?` — `ReadState()` вызывается в `Start()`. Возможно, избыточно, если сцена уже настроена. |
| 2 | `RotateBigCube.cs:7` | `// TODO get current object instead of Passing Down Dependency` — `target` задаётся через Inspector, а не находится автоматически. |
| 3 | `MouseInput.cs` | Предположительно не используется. Кандидат на удаление. |
| 4 | `ReadCube.cs:3` | `using System.IO.Compression;` — лишний импорт, не используется. |
| 5 | `PivotRotation.cs:45-74` | `SpinSide()` — дублирование логики для каждой стороны. Можно вынести множитель направления (`-1f` / `1f`) в словарь или метод. |
| 6 | `CubeState.cs:27` | После вращения и `ReadState()` кубики не возвращаются к оригинальным родителям (un-parent). Это может приводить к проблемам при каскадных поворотах. Комментарий на строке 113 `PivotRotation.cs` упоминает «unparent», но код не реализован. |
| 7 | Общее | Нет проверки, что `activeSide` содержит ровно 9 элементов — при ошибке raycast может быть < 9, что сломает логику. |
| 8 | Общее | Нет системы перемешивания (scramble), таймера, определения собранного состояния (win condition). |

---

## Ключевые концепции для разработки

- **Layer 8** — все грани кубиков (Collider'ы) находятся на этом слое; raycast'ы в `ReadCube` и `SelectFace` фильтруют только его.
- **Pivot = родитель центрального кубика стороны** — именно этот объект вращается при повороте стороны.
- **ReadState через raycast** — состояние куба определяется не по иерархии объектов, а по физическому положению лучей, что устойчиво к перепривязкам.
- **New Input System** — весь ввод через `InputAction`, привязки конфигурируются в Inspector (`[SerializeField]`).
- **Паттерн DragDetection** — универсальный компонент с доступом по enum-ключу кнопки; повторно используется для LMB, MMB (и потенциально RMB).
