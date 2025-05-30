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
   - Просмотр карты и её черновое редактирование: Менеджер может просматривать карту со всеми текущими заказами и создавать черновые маршруты для своего заказа.
   - Принятие заказа: Менеджер может принять заказ для его обработки.
   - Отправка заказа: Менеджер может отправить обработанный заказ для одобрения администратором.

3. **Администратор**
   - Управление заказчиками и менеджерами.
   - Просмотр всех заказов: Администратор может видеть все заказы и их статусы.
   - Управление ресурсами: Администратор может добавлять новые машины в систему или как-то с ними взаимадействовать.
   - Просмотр схемы: Администратор видит визуальную карту с маршрутами всех машин.
   - Одобрение заказа: Администратор может принять заказ от менеджера.

## Ресурсы

Под ресурсами понимаются машины, которые всегда должны быть заняты (даже если они стоят в гараже). У каждой машины есть путь на определенный день с точками, где она должна привозить товары клиентам. Этот путь создает менеджер и одобряет администратор.

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
+ **Реализация аутентификации и авторизации:**
  + Установка и настройка Дайджеста для аутентификации.
  + Создание ролей пользователей (заказчик, менеджер, администратор).
  + Реализация политик авторизации для доступа к различным ресурсам.

### Неделя 4 (18.11.2024 - 24.11.2024)
+ **Разработка основного функционала**
  + Создание базовых контроллеров и представлений для основных действий.
  + Создание контроллеров и действий для авторизации пользователей.
  + Настройка маршрутизации и валидации данных.

### Неделя 5 (25.11.2024 - 01.12.2024)
+ **Реализация функционала работы с пользователями:**
  + Создание контроллера для CRUD операций.
  + Реализация функционала управления пользователями администратором.
  + Отображение информации о пользователях.

### Неделя 6 (02.12.2024 - 08.12.2024)
+ **Реализация функционала карты:**
  + Создание карты по реальной местности.
  + Создание точек на карте и отображения ребер (дорог) между ними и добавление их в БД.
  + Различное отображение карты в зависимости от роли.

### Неделя 7 (09.12.2024 - 15.12.2024)
+ **Реализация функционала маршрута:**
  + Создание класса, находящего минимальный путь между двумя точками по алгоритму Дейкстры.
  + Отображение пути на карте.
  + Возможность динамически состовлять путь менеджеру.

### Неделя 8 (16.12.2024 - 22.12.2024)
+ **Реализация обработки заказа менеджером:**
  + Создание оформления и отправки менеджеру нового заказа.
  + Создание базы автомобилей для перевозки.
  + Принятие, обработка и отправка заказа администратору.

### Неделя 9 (23.12.2024 - 29.12.2024)
+ **Реализация обработки заказа администратором**
  + Получение и одобрение заказа администратором.
  + Управление автомобилями из базы.
  + Создание маршрута на карте и уведомление клиента.

### Неделя 10 (30.12.2024 - 05.01.2025)
+ **Реалтзация автомобиля на маршруте:**
  + Обновление положения автомобиля в реальном времени.
  + Изменение состояния при получении и завершении заказа.
  + Изменение состояния заказа при его завершении.

### Неделя 11 (06.01.2025 - 12.01.2025)
+ **Доработка визуальной части:**
  + Оформление дизайна для приложения.
  + Добавление различных дополнительных фишек.
  + Доработка базы заказов.

### Неделя 12 (13.01.2025 - 19.01.2025)
+ **Релиз, развертывание и исправление ошибок:**
  + Создание документации для установки.
  + Тестирование и исправление ошибок.
