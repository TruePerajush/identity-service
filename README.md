# Identity Service

Микросервис аутентификации и авторизации, реализованный на **.NET 10**. 
Проект спроектирован с использованием **Vertical Slice Architecture** и паттерна **CQRS**.

Сервис отвечает за регистрацию пользователей, выдачу stateless JWT, обновление токенов и публикацию доменных событий в брокер сообщений для интеграции с другими микросервисами.

---

## Стек технологий

* **Платформа:** .NET 10, ASP.NET Core Web API (Minimal API)
* **Архитектура:** Vertical Slice Architecture, CQRS, Event-Driven Architecture
* **Базы данных:** PostgreSQL (Entity Framework Core 10)
* **Брокеры сообщений:** Apache Kafka (Confluent.Kafka)
* **Валидация и результаты:** FluentValidation, FluentResults
* **Pipeline:** MediatR (с использованием Pipeline Behaviors)
* **Аунтефикация:** Stateless JWT (BCrypt.Net-Next), Rate Limiting (Fixed Window)
* **Документация API:** OpenAPI, Scalar (альтернатива Swagger UI)

---

## Архитектура и ключевые особенности

### 1. Vertical Slice Architecture
Код организован вокруг **фич**. 
Каждая фича в папке `Features/` (например, `Register` или `Login`) содержит в себе всё необходимое:
* `Endpoint.cs` — точка входа (Minimal API).
* `Command.cs` / `Query.cs` — DTO и контракт для MediatR.
* `Handler.cs` — бизнес-логика.
* `Validator.cs` — правила валидации (FluentValidation).

### 2. CQRS и MediatR Pipeline
Используется паттерн CQRS. Валидация входящих данных происходит **автоматически** до попадания в Handler благодаря кастомному `ValidationBehavior`, внедренному в MediatR Pipeline.

### 3. Безопасность и Rate Limiting
* **Stateless JWT:** Access Token живет N минут. В него зашиты необходимые claims, что исключает лишние походы в БД.
* **Защита от брутфорса:** Эндпоинт `Login` защищен от подбора паролей. При 3 неудачных попытках аккаунт блокируется на 1 минуту.
* **Rate Limiting:** Настроен встроенный Rate Limiter (Fixed Window). С одного IP-адреса можно сделать не более 10 запросов к `/login` в минуту. При превышении возвращается `429 Too Many Requests`.
* **Хеширование паролей:** Пароли хешируются с использованием `BCrypt`.

### 4. Event-Driven подход (Kafka)
Сервис публикует доменные события в Apache Kafka:
* `UserRegisteredEvent` — при успешной регистрации.
* `UserLoggedInEvent` — при успешном входе.

### 5. Обработка ошибок
Используется паттерн **Result** (FluentResults) для возврата ошибок без исключений там, где это ожидаемо. Для непредвиденных ошибок настроен `GlobalExceptionHandler`, который всегда возвращает корректный `ProblemDetails` ответ.

---

## Структура проекта

```text
├── Common/             # Общие расширения, базовые классы, исключения
├── Domain/             # Доменные сущности (User, RefreshToken)
├── Features/           # Бизнес-логика (Vertical Slices)
│   ├── Login/
│   ├── Logout/
│   ├── RefreshToken/
│   └── Register/
├── Infrastructure/     # Внешние зависимости (EF Core, Kafka, JWT, Config)
├── Migrations/         # Миграции базы данных
├── Program.cs          # Точка входа, настройка DI и Middleware
└── appsettings.json    # Конфигурация (ConnectionStrings, Jwt, Kafka)