Тестовое заданиe на стажировку в ИнфоТеКС

# Настройка и запуск

## 1. Восстановление зависимостей

Скачайте все необходимые NuGet-пакеты:
```
dotnet restore
```
## 2. Конфигурация подключения

Перейдите в папку проекта **TimescaleDataProcessingApi**.
Для хранения паролей используются User Secrets. [ссылка](https://learn.microsoft.com/ru-ru/aspnet/core/security/app-secrets?view=aspnetcore-10.0&tabs=windows%2Cpowershell) 
Выполните команду ниже:

```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=YOUR_DATABASE;Username=postgres;Password=YOUR_PASSWORD"
```

## 2. Миграция базы данных

Создайте таблицы в базе данных:

```
dotnet ef database update
```

## 3. Запуск приложения

```
dotnet run
```

По адресу [http://localhost:5276/swagger/index.html](http://localhost:5276/swagger/index.html) будет доступен Swagger

## Запуск тестов

Команда выполняется в папке проекта **TimescaleDataProcessingApi.Tests**

```
dotnet test
```