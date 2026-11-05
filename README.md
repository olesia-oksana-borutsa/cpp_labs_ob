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
| RID       | Режим (--self-contained) | Приблизний розмір | Потрібен встановлений runtime |
| :---      | :---                     | :---              | :---                          |
| `win-x64` | `self-contained`         | ~70-80 МБ         | ні                            |
| `win-x64` | `framework-dependent`    | ~3-5 МБ           | так (.NET 10)                 |


# 4: Доменна модель (Склад творчих товарів)

## Інваріанти доменної сутності Product:
1. **Обов'язковий ідентифікатор та SKU:** `Id` та `Sku` не можуть бути порожніми або складатися з пробілів.
2. **Невід'ємна кількість при створенні:** Початковий залишок (`Quantity`) не може бути меншим за 0.
3. **Заборона перевитрати (контроль стану):** Операція видачі (`Issue`) неможлива, якщо кількість для видачі перевищує поточний залишок на складі.
4. **Коректність операцій приходу/видачі:** Кількість товару для приходу чи видачі має бути строго більшою за нуль.

# Лабораторна робота 5

## Архітектура та компоненти системи

* **Абстракція (Контракт):** 
  * `ICatalogStore` — інтерфейс, який визначає контракт взаємодії зі сховищем товарів (CRUD операції та пошук).
* **Реалізації сховища:**
  * `InMemoryCatalogStore` — реалізація на базі словника `Dictionary`. Зберігає дані виключно в оперативній пам'яті під час виконання програми.
  * `FileCatalogStore` — персистентна реалізація, яка зберігає стан системи у файл `catalog.json`, використовуючи мапінг доменних сутностей у DTO.
  * `CachingCatalogStore` — кешуючий декоратор, що обгортає базове сховище для оптимізації зчитування (додаткове завдання).
* **Сервісний шар:** 
  * `CatalogService` — містить високорівневу бізнес-логіку складу. Залежить виключно від інтерфейсу `ICatalogStore`, отримуючи його через конструктор (Dependency Injection).

## Режими запуску (Composition Root)
Вибір конкретної реалізації сховища здійснюється у точці входу за допомогою аргументів CLI:
* `dotnet run --project src/Cli` — запуск зі сховищем у пам'яті.
* `dotnet run --project src/Cli -- --file` — запуск із файловим сховищем.
* `dotnet run --project src/Cli -- --file --cache` — запуск із файловим сховищем та декоратором кешування.

# Лабораторна робота 6. Доменні події, делегати Action/Func, лямбди

## 1. Події у домені
У моєму домені (каталог товарів для творчості та писанкарства) подіями вважаються такі зміни стану: додавання нового товару (`Added`), надходження товару на склад (`Received`), видача товару (`Issued`) та видалення з каталогу (`Removed`). Саме ці зміни є подіями, оскільки вони фіксують фактичну зміну кількості запасів, про яку потрібно сповіщати інші компоненти системи (наприклад, для оновлення залишків на вітрині чи запису в журнал аудиту), не порушуючи при цьому ізоляцію доменного шару.

## 2. Оголошення події та підняття (Core)

// Оголошення події
"public event EventHandler<CatalogChangedEventArgs>? Changed;

// Метод для безпечного підняття події
private void OnChanged(Product p, CatalogChangeKind kind, int delta) =>
    Changed?.Invoke(this, new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity));"


    # Лабораторна робота №7: Звіти через LINQ

## Відкладене виконання LINQ (Deferred Execution)
У  демонстрації відкладеного виконання два виклики `query.Count()` дають різні числа (3 і 4). Це відбувається тому, що метод `Where` не виконує пошук одразу в момент оголошення, а лише створює "дерево виразів" (правило). 
Виконання запиту відбувається в момент виклику агрегатної функції `Count()`. Оскільки між першим і другим викликом `Count()`  додали новий товар ("Скоба") у вихідний список `sourceList`, повторний виклик `Count()` заново пройшовся по зміненій колекції і врахував новий товар. Натомість метод `.ToList()` матеріалізує запит у пам'яті: він створює знімок `snapshot`, тому подальше очищення `sourceList.Clear()` на нього вже не впливає.

## Додаткове завдання 3: Порівняння синтаксисів
(`ByUnitQuerySyntax`) був реалізований за допомогою синтаксису запитів (`from ... group ... into ...`).
* **Методний синтаксис** (`source.GroupBy(...).Select(...)`) читається зліва направо і є більш звичним та компактним для ланцюжків простих фільтрацій (`Where`).
* **Синтаксис запитів**, навпаки, значно краще підходить для операцій групування. Конструкція `group ... into g` робить код набагато читабельнішим, бо вона візуально нагадує SQL. Змінна групи `g` явно виділена, що робить подальшу проєкцію `.Select(new GroupSummary(g.Key, g.Count()...))` інтуїтивно зрозумілою порівняно з "лямбдою в лямбді" у методному синтаксисі.*/

# Лабораторна робота №8: async/await, скасування, доменні винятки, unit-тести Core

## Запуск юніт-тестів

# Запуск усіх 8 тестів проєкту Core.Tests
dotnet test

# Запуск конкретного тесту
dotnet test --filter "Name~ImportAsync_WhenAlreadyCancelled_Throws"

## Лабораторна робота 9: Конфігурація та Секрети

### Профілі
- **Development** (`appsettings.Development.json`): Детальне логування (`MinimumLevel: Debug`).
- **Production** (`appsettings.json`): Базове логування (`MinimumLevel: Information`).

### Таблиця ключів конфігурації
| Ключ конфігурації | Змінна середовища (Bash) | Змінна середовища (PowerShell) |
| :--- | :--- | :--- |
| `Storage:DataPath` | `CROSSAPP_Storage__DataPath` | `$env:CROSSAPP_Storage__DataPath` |
| `Storage:FileName` | `CROSSAPP_Storage__FileName` | `$env:CROSSAPP_Storage__FileName` |
| `Logging:MinimumLevel`| `CROSSAPP_Logging__MinimumLevel` | `$env:CROSSAPP_Logging__MinimumLevel`|
| `Integration:ApiKey` | `CROSSAPP_Integration__ApiKey` | `$env:CROSSAPP_Integration__ApiKey` |
| *(спадкова змінна)* | `CROSSAPP_DATA_PATH` | `$env:CROSSAPP_DATA_PATH` |


**PowerShell (Windows):**
`$env:CROSSAPP_DATA_PATH="C:\Users\st\data"
dotnet run --project src/Cli -- export`

**cmd **
set CROSSAPP_DATA_PATH=C:\custom_data
dotnet run --project src/Cli -- export

## Лабораторна робота 10: Minimal API

Запуск API:
`dotnet run --project src/Api`
API працює на порту 5080. Документація OpenAPI (Scalar UI) доступна за адресою http://localhost:5080/scalar/v1 (тільки в середовищі Development).

### Таблиця маршрутів
| Метод  | Шлях                  | Тіло запиту (JSON) | Коди відповідей         |
| ------ | --------------------- | ------------------ | ----------------------- |
| GET    | `/api/products`       | -                  | 200 OK                  |
| GET    | `/api/products/{id}`  | -                  | 200 OK, 404 Not Found   |
| POST   | `/api/products`       | SaveProductRequest | 201 Created, 400 Bad Req|
| PUT    | `/api/products/{id}`  | SaveProductRequest | 200 OK, 404, 400        |
| DELETE | `/api/products/{id}`  | -                  | 204 No Content, 404     |

### Приклади використання (cURL)

**POST (Створення валідного запису):**
```bash
curl -i -X POST http://localhost:5080/api/products -H "Content-Type: application/json" \
-d "{\"sku\":\"PYS-001\",\"name\":\"Писачок мідний (тонкий)\",\"unit\":\"шт\",\"quantity\":15}"
```
**POST (Помилка валідації 400 Bad Request):**
```
curl -i -X POST http://localhost:5080/api/products -H "Content-Type: application/json" \
-d "{\"sku\":\"\",\"name\":\"X\",\"unit\":\"\",\"quantity\":-1}" ```
