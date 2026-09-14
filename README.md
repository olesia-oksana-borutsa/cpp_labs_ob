# CrossApp
Проєкт з крос-платформного програмування.

Предметна область: (a) Склад. Сутності: Product (товар), StockBatch (партія), Warehouse (склад), Movement (переміщення). Призначення: облік залишків товарів по партіях.

## Структура рішення (Solution)
* `CrossApp.sln` — головний файл рішення.
* `src/Core/` — бібліотека класів (`Core`), що містить бізнес-логику, моделі та збір інформації про середовище
  * `Core/Dto/` — record-типи формату даних 
  * `Core/Domain/` — доменні сутності з поведінкою та інваріантами 
  * `Core/Storage/` — реалізації сховищ даних 
* `src/Cli/` — консольний клієнт (`Cli`), що відповідає виключно за взаємодію з користувачем і форматування виводу

## Команди для збірки та запуску
* **Збірка всього рішення:**
 
  dotnet build

* **Запуск консольного застосунку:**
  
  dotnet run --project src/Cli

* **Запуск у форматі JSON:**

  dotnet run --project src/Cli --json
 

## Публікація (Publish)
* **Self-contained збірка (.NET Runtime):**

  dotnet publish src/Cli -c Release -r win-x64 --self-contained true
  
* **Framework-dependent збірка ( .NET 10 Runtime):**

  dotnet publish src/Cli -c Release -r win-x64 --self-contained false
 

## Порівняння режимів публікації та RID
| RID | Режим (--self-contained) | Приблизний розмір | Потрібен встановлений runtime |
| :--- | :--- | :--- | :--- |
| `win-x64` | `self-contained` | ~70-80 МБ | ні |
| `win-x64` | `framework-dependent` | ~3-5 МБ | так (.NET 10) |