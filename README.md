# Логистический Сервис

## Описание проекта

Этот проект представляет собой логистический сервис, который позволяет управлять доставкой товаров клиентам с помощью машин. В системе есть три типа пользователей: заказчик, менеджер и администратор. Каждый из них имеет свои роли и возможности в системе.

## Основные функции

1. **Заказчик**
   - Просмотр заказов: Заказчик может просматривать заказы, которые должны быть доставлены к нему.
   - Создание запросов: Заказчик может создавать запросы на доставку товаров в определенное время и день.
   - Просмотр схемы: Заказчик видит только те машины, которые назначены для его доставки.

2. **Менеджер**
   - Назначение машин: Менеджер может назначать свободные машины определенным заказчикам.
   - Обновление маршрутов: Менеджер может добавлять точки на маршруты машин.
   - Информирование: Обновленная информация о маршрутах и статусе машин автоматически отправляется заказчикам и администратору.

3. **Администратор**
   - Управление заказчиками и менеджерами.
   - Просмотр всех заказов: Администратор может видеть все заказы и их статусы.
   - Управление ресурсами: Администратор может добавлять новые машины в систему или как-то с ними взаимадействовать.
   - Просмотр схемы: Администратор видит визуальную карту с маршрутами всех машин на определенный день.

## Ресурсы

Под ресурсами понимаются машины, которые всегда должны быть заняты (даже если они стоят в гараже). У каждой машины есть путь на определенный день с точками, где она должна привозить товары клиентам. Этот путь создает менеджер исходя из команд администратора.

## Схема

Схема — это визуальная карта, на которой отображаются машины за выбранный день и их маршруты. На схеме видно, где сейчас находится каждая машина и куда она едет. Схема обновляется в реальном времени, чтобы все пользователи могли видеть актуальную информацию.

## Связи между таблицами

- **Users -> Customers**: Один к одному (один пользователь может быть одним заказчиком).
- **Users -> Managers**: Один к одному (один пользователь может быть одним менеджером).
- **Users -> Administrators**: Один к одному (один пользователь может быть одним администратором).

## План выполнения проекта *"Логистический Сервис"* на *ASP.NET Core* (28.10.2024 - 26.01.2025)

### Неделя 1 (28.10.2024 - 03.11.2024)
+ **Планирование и проектирование:**
  + Определение требований к системе.
  + Создание диаграммы классов.

+ **Настройка окружения:**
  + Создание репозитория на *GitHub* и настройка CI/CD.

+ **Создание базового проекта:**
  + Создание нового проекта *ASP.NET Core*.
  + Настройка базовой структуры проекта (папки, файлы).
  + Настройка файла `appsettings.json` и строки подключения к базе данных.

### Неделя 2 (04.11.2024 - 10.11.2024)
+ **Настройка Entity Framework Core:**
  + Установка необходимых пакетов *NuGet* (*Microsoft.EntityFrameworkCore*, *Microsoft.EntityFrameworkCore.SqlServer*).
  + Создание контекста базы данных и моделей данных.
  + Настройка миграций и создание тестовой базы данных.
  + Настройка маршрутизации и валидации данных (пользователей).

+ **Создание базового ядра сервера**
  + Создание схематичного представления для основного меню.
  + Создание контроллера перенаправления.
  + Создание базового логина пользователей.

### Неделя 3 (11.11.2024 - 17.11.2024)
- **Реализация аутентификации и авторизации:**
  + Установка и настройка JWT (JSON Web Tokens) для аутентификации.
  + Создание ролей пользователей (заказчик, менеджер, администратор).
  + Реализация политик авторизации для доступа к различным ресурсам.

### Неделя 4 (18.11.2024 - 24.11.2024)
- **Разработка основного функционала**
  + Создание базовых контроллеров и представлений для основных действий.
  - Создание контроллеров и действий для CRUD операций.
  - Реализация базовых API для работы с заказами, машинами и пользователями.
  - Настройка маршрутизации и валидации данных (все услуги и записи).

### Неделя 5 (25.11.2024 - 01.12.2024)
- **Реализация функционала заказчика:**
  - Создание интерфейса для просмотра заказов.
  - Реализация функционала создания запросов на доставку.
  - Отображение информации о назначенных машинах.

### Неделя 6 (02.12.2024 - 08.12.2024)
- **Реализация функционала менеджера:**
  - Создание интерфейса для назначения машин заказчикам.
  - Реализация функционала обновления маршрутов машин.
  - Отправка обновленной информации заказчикам и администратору.

### Неделя 7 (09.12.2024 - 15.12.2024)
- **Реализация функционала администратора:**
  - Создание интерфейса для просмотра всех заказов и их статусов.
  - Реализация функционала обработки заказов и дачи указаний менеджеру.
  - Добавление новых машин в систему.

### Неделя 8 (16.12.2024 - 22.12.2024)
- **Реализация визуальной карты:**
  - Интеграция с картографическим сервисом (например, *Google Maps* или *OpenStreetMap*).
  - Отображение маршрутов машин на карте.
  - Обновление информации о местоположении машин.

### Неделя 9 (23.12.2024 - 29.12.2024)
- **Тестирование и отладка:**
  - Создание модульных тестов для основных функций.
  - Проведение интеграционного тестирования.
  - Исправление обнаруженных ошибок и улучшение производительности.

### Неделя 10 (30.12.2024 - 05.01.2025)
- **Документация и подготовка к релизу:**
  - Создание документации по API (*Swagger*).
  - Написание инструкций по установке и настройке.

### Неделя 11 (06.01.2025 - 12.01.2025)
- **Релиз и развертывание:**
  - Развертывание приложения на тестовом сервере.
  - Проведение финального тестирования в реальной среде.

### Неделя 12 (13.01.2025 - 19.01.2025)
- **Поддержка и мониторинг:**
  - Настройка мониторинга и логирования.
  - Обеспечение поддержки пользователей и исправление возможных ошибок.

### Неделя 13 (20.01.2025 - 26.01.2025)
- **Завершение проекта:**
  - Подготовка отчета о выполненной работе.
  - Презентация проекта.