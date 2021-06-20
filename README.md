# Discord File Loader Bot (Юко/Yuko)
Бот предназначен для получения ссылок вложений, которые передаются клиенту. Клиент скачивает вложения по этим ссылкам. 

- **КЛИЕНТ НЕ ТРЕБУЕТ УСТАНОВКИ**
- **ДЛЯ ПОЛУЧЕНИЯ ССЫЛКИ ДЛЯ ДОБАВЛЕНИЯ БОТА НА СЕРВЕР ОБРАЩАТЬСЯ К АВТОРУ (Discord: Hlebushek#4209)**

## Содержание
- [Команды бота](#команды-бота)
  - [Доступные всем](#доступные-всем)
  - [Доступные администратору сервера](#доступные-администратору-сервера)
  - [Доступные владельцу бота](#доступные-владельцу-бота)
- [Клиент](#клиент)
  - [Настройка](#настройка)
  - [Начало работы](#начало-работы)

## Команды бота
Префикс для команд: >yuko

### Доступные всем
Команда|Описание
---|---
help|Показывает все доступные команды.
get-client-app|Выдает ссылку на скачивание актуальной версии клиента.
get-client-settings|Выдает данные для подключения клиента к боту. Поле "Id Пользователя" содержит Id пользователя выполнившего данную команду.
get-my-id|Выдает Id пользователя выполнившего данную команду.
version|Выдает текущую версию бота.

### Доступные администратору сервера
Команда|Алиасы|Описание
---|---|---
access|-|Каналы к которым у бота есть доступ на сервере где выполнена команда

### Доступные владельцу бота
Команда|Алиасы|Описание
---|---|---
active-time|-|Время которое бот активен
admin.access|adm.acc|Каналы к которым у бота есть доступ на сервере где выполнена команда
client-count|-|Количество подключенных клиентов
client-list|-|Список клиентов
shutdown|down|Выключить бота

## Клиент

### Настройка
1) Запускаем приложение.
2) Переходим в настройки.
3) Заполняем поля актуальными данными, для их получения вводим на нужном нам сервере в любом канале [команду](#доступные-всем): get-client-settings (не забываем про префикс)
4) В окне, в самом низу находится таблица, со списком каналов, с которых доступно скачивание вложений. Для их получения нажимаем в меню (находится над списком) на значок “Получить” (последний). Редактируем полученный список, если это требуется.
5) Закрываем окно.

### Начало работы
**Перед началом работы [настройте](#настройка) программу!**

При открытии программы мы видим окно, которое поделено на 2 части. Каждую из частей можно уменьшить, увеличив другую с помощью оранжевой полоски.

Для того чтобы скачать файлы делаем:
1) Добавляем правила: 1 значок в 1 меню.  Заполняем поля и жмем “Сохранить”. Для удаления правила нужно воспользоваться 2 значком в 1 меню, для экспорта и импорта 3 и 4 соответственно.
2) Получаем список файлов: 5 значок в 1 меню.
3) Редактируем полученный список при необходимости. Для удаления из списка используем 1 значок 2 меню, для экспорта и импорта 2 и 3 значки соответственно.
4) Скачиваем файлы: 4 значок 2 меню. Выбираем нужную папку и нажимаем “ОК”, после чего начинается скачивание.

**Если у вас уже есть список правил или файлов просто импортируйте его.**
