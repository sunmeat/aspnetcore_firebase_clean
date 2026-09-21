# Clean Architecture + React + Firebase Cloud

Навчальний приклад вебзастосунку на **ASP.NET Core Web API + React + Firebase Cloud Firestore**, побудованого за принципами **Clean Architecture**.

Проєкт створений як початкова точка для подальшого розвитку бекенду: логування, модульного та інтеграційного тестування, а також автентифікації через **Google, GitHub та Microsoft**.

[GitHub repository](https://github.com/sunmeat/aspnetcore_firebase_clean?utm_source=chatgpt.com)

---

## Технології

### Backend

* C#
* .NET 10
* ASP.NET Core Web API
* Clean Architecture
* Firebase Cloud Firestore
* Google Cloud Firestore SDK
* Dependency Injection
* Repository Pattern
* DTO
* async/await

### Frontend

* React 19
* Vite
* JavaScript
* HTML
* CSS

### Database

Проєкт використовує **Cloud Firestore** замість Entity Framework Core та SQL Server.

Дані зберігаються у двох основних колекціях:

```text
teams
players
```

---

## Структура solution

```text
Soccer
│
├── Soccer.Domain
│   ├── Entities
│   │   ├── Player.cs
│   │   └── Team.cs
│   │
│   └── Interfaces
│       └── IRepository.cs !!! (порибрано Unit of Work / EF Core)
│
├── Soccer.Application
│   ├── DTO
│   │   ├── PlayerDTO.cs
│   │   └── TeamDTO.cs
│   │
│   ├── Interfaces
│   │   └── IEntityService.cs
│   │
│   ├── Services
│   │   ├── PlayerService.cs !!! (прибрано AutoMapper)
│   │   └── TeamService.cs !!!
│   │
│   └── DependencyInjection
│       └── ApplicationServiceExtensions.cs
│
├── Soccer.Infrastructure
│   ├── Persistence
│   │   └── FirestoreSeeder.cs !!! (замість SoccerContext.cs)
│   │
│   ├── Repositories
│   │   ├── PlayerRepository.cs !!!
│   │   └── TeamRepository.cs !!!
│   │
│   └── DependencyInjection
│       └── InfrastructureServiceExtensions.cs !!! (реєстрація сервісів без юніт оф ворк)
│
├── Soccer.Common
│   └── Exceptions
│
├── Soccer.WebAPI
│   ├── Controllers
│   │   ├── PlayersController.cs
│   │   └── TeamsController.cs
│   │
│   ├── Program.cs
│   └── appsettings.json !!! (повернуто до дефолтного значення)
│
└── react.client
    ├── src
    ├── public
    ├── package.json
    └── vite.config.js
```

---

## Clean Architecture

Основна ідея проєкту полягає в тому, що бізнес-логіка не повинна залежати від конкретної бази даних, Web API або UI.

```text
                 ┌─────────────────────┐
                 │       React         │
                 │      Frontend       │
                 └──────────┬──────────┘
                            │ HTTP
                            ▼
                 ┌─────────────────────┐
                 │     Soccer.WebAPI   │
                 │    Controllers      │
                 └──────────┬──────────┘
                            │
                            ▼
                 ┌─────────────────────┐
                 │ Soccer.Application  │
                 │      Services       │
                 │       DTOs          │
                 └──────────┬──────────┘
                            │
                            ▼
                 ┌─────────────────────┐
                 │    Soccer.Domain    │
                 │ Entities + Contracts│
                 └──────────┬──────────┘
                            ▲
                            │
                 ┌──────────┴──────────┐
                 │ Soccer.Infrastructure│
                 │    Repositories      │
                 │      Firestore       │
                 └──────────────────────┘
```

`Soccer.Domain` є центром архітектури.

Domain не знає:

* про Firestore;
* про ASP.NET Core;
* про React;
* про HTTP;
* про конкретну реалізацію Repository.

Infrastructure, навпаки, реалізує контракти Domain.

Наприклад:

```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAll();
    Task<T?> Get(int id);
    Task<T?> Get(string name);
    Task Create(T item);
    Task Update(T item);
    Task Delete(int id);
}
```

А вже `PlayerRepository` та `TeamRepository` реалізують цей інтерфейс через Firestore.

---

## Firebase / Cloud Firestore

Для зберігання даних використовується **Cloud Firestore**.

Структура даних:

```text
teams
│
├── 1
├── 2
├── 3
└── ...

players
│
├── 1
├── 2
├── 3
└── ...
```

ID документів відповідають числовим `Id` сутностей.

При створенні нового запису використовується:

```text
max(Id) + 1
```

Наприклад:

```text
1
2
3
...
26
↓
новий гравець
↓
27
```

Списки додатково сортуються за числовим `Id`, щоб уникнути стандартного лексикографічного порядку Firestore:

```text
1
2
3
...
9
10
11
```

замість:

```text
1
10
11
2
3
...
```

---

## Firebase credentials

Для локального запуску потрібен Firebase Service Account JSON.

У Firebase Console:

```text
Project settings
    ↓
Service accounts
    ↓
Generate new private key
    ↓
Download JSON
```

Файл повинен знаходитися локально в:

```text
Soccer.Infrastructure/firebase.json
```

Файл **не повинен потрапляти до Git**.

У `.gitignore` вже додано:

```text
/Soccer.Infrastructure/firebase.json
```

> Ніколи не публікуйте Service Account JSON у GitHub. Він містить приватний ключ для доступу до Firebase!

---

## Початкове заповнення бази

`FirestoreSeeder` автоматично створює початкові дані для:

* 15 футбольних команд;
* 26 гравців.

Seeder перевіряє існування документа перед створенням, тому повторний запуск застосунку не повинен перезаписувати вже існуючі документи.

---

## Web API

Для роботи з даними доступні два основні контролери:

```text
/api/players
/api/teams
```

### Players

```http
GET    /api/players
GET    /api/players/{id}
POST   /api/players
PUT    /api/players/{id}
DELETE /api/players/{id}
```

### Teams

```http
GET    /api/teams
GET    /api/teams/{id}
POST   /api/teams
PUT    /api/teams/{id}
DELETE /api/teams/{id}
```

---

## React

Frontend знаходиться в окремому проєкті:

```text
react.client
```

Для нього використовуються:

* React 19;
* Vite;
* JavaScript;
* CSS.

Під час локальної розробки ASP.NET Core використовує Vite Dev Server через `SpaProxy`.

---

## Запуск проєкту

### 1. Клонування

```bash
git clone https://github.com/sunmeat/aspnetcore_firebase_clean.git
cd aspnetcore_firebase_clean
```

### 2. Firebase

Створіть Firebase Service Account та покладіть JSON-файл у:

```text
Soccer.Infrastructure/firebase.json
```

### 3. Backend

```bash
dotnet restore
dotnet build
dotnet run --project Soccer.WebAPI
```

### 4. Frontend

В окремому terminal:

```bash
cd react.client
npm install
npm run dev
```

---

## Поточний стан

Цей репозиторій є **початковим навчальним прикладом**, на якому поступово демонструється розвиток вебзастосунку.

На поточному етапі реалізовано:

* [x] Clean Architecture
* [x] Domain entities
* [x] Application services
* [x] DTO
* [x] Repository Pattern
* [x] Dependency Injection
* [x] ASP.NET Core Web API
* [x] React + Vite
* [x] Firebase Cloud Firestore
* [x] Firebase Service Account authentication
* [x] Firestore seeding
* [x] CRUD для команд
* [x] CRUD для гравців
* [x] зв'язок `Player → Team`
* [x] ручне перетворення Entity → DTO
* [x] автоматична генерація нового числового ID

У наступних етапах планується:

* [ ] Logging
* [ ] Structured logging
* [ ] Unit tests
* [ ] Integration tests
* [ ] Authentication
* [ ] Google login
* [ ] GitHub login
* [ ] Microsoft login
* [ ] Authorization
* [ ] захист API endpoint'ів
* [ ] робота з authenticated user
* [ ] обробка помилок та глобальний exception handling

---

## Навчальна мета

Проєкт призначений не стільки для демонстрації складного футбольного менеджера, скільки для поступового вивчення архітектури сучасного вебзастосунку.

Послідовність розвитку передбачається приблизно такою:

```text
Clean Architecture
        ↓
Repository Pattern
        ↓
Firestore
        ↓
Web API
        ↓
React
        ↓
Logging
        ↓
Testing
        ↓
Authentication
        ↓
Authorization
```

Таким чином, кожен наступний етап додає до вже працюючої системи нову реальну можливість, не руйнуючи попередню архітектуру.

---

## Чому Firestore?

У попередній версії проєкту використовувався SQL Server + Entity Framework Core.

У цій версії база даних була замінена на **Firebase Cloud Firestore**.

Це дозволяє продемонструвати важливий принцип Clean Architecture:

> Application не повинна знати, яка саме технологія використовується для зберігання даних.

Application працює з:

```csharp
IRepository<Player>
IRepository<Team>
```

а конкретна реалізація знаходиться в Infrastructure:

```text
PlayerRepository
TeamRepository
```

Тому заміна SQL Server на Firestore не потребувала переписування Domain та Application шарів.

---

## License

MIT License.

Див. [`LICENSE.txt`](LICENSE.txt).
