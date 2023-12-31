# PMEngine.Core



## Что это?

Это ядро движка, предназначенного для текстовых интерактивных приложений, например для ботов Telegram, других мессенеджеров, браузерных приложений или просто консоли.
Движок умеет обрабатывать действия пользователя, управлять ими, выполнять команды, поддерживает модульность.

[![Nuget](https://img.shields.io/nuget/v/PmEngine.Core?label=PmEngine.Core)](https://www.nuget.org/packages/PmEngine.Core)  
[![Nuget](https://img.shields.io/nuget/v/PmEngine.Telegram?label=PmEngine.Telegram)](https://www.nuget.org/packages/PmEngine.Telegram)  
[![Nuget](https://img.shields.io/nuget/v/PmEngine.Vk?label=PmEngine.Vk)](https://www.nuget.org/packages/PmEngine.Vk)  

[![Telegram Chat](https://img.shields.io/badge/Telegram-2CA5E0?style=for-the-badge&logo=telegram&logoColor=white)](https://t.me/PmEngine)  

[https://github.com/NerZul121/PmEngine.Telegram](PmEngine.Telegram) — Модуль для работы с Telegram  

[https://github.com/NerZul121/PmEngine.Vk](PmEngine.VK) — Модуль для работы с VK

## Список исползуемых переменных среды

В ядре используются следующие переменные среды:
```
PROVIDER_TYPE = INT от 0 до 2, где 0 - PostgreSQL, 1 - SQLite, 2 - InMemory (SQLite)
CONNECTION_STRING = Строка подключения к БД, которая будет использована в BaseContext. Должна соответствовать провайдеру, указанному в PROVIDER_TYPE
```

# Основы

## Регистрация в DI

Для работы движка в вашем приложении его сперва необходимо добавить в DI контейнер:

```
builder.Services.AddPMEngine((e) =>
{
    e.Properties.InitializationAction = typeof(HelloWorldAction); // Указываем стартовое действие пользователя
    e.Properties.DataProvider = DataProvider.PG; // Указываем тип соединения, если не хотим исползовать переменные среды
});
```

После чего его нужно сконфигурировать:
```
var app = builder.Build();
app.ConfigureEngine();
```

## Работа с вводом и выводом

### Ввод

Ввод информации в движок можно осуществлять откуда угодно. Для этого достаточно взять пользователя и вызвать у него ActionProcess() или использовать следующую конструкцию:  
```
var processor = serviceProvider.GetRequiredService<IEngineProcessor>();
await processor.ActionProcess(session.InputAction, session, session.InputAction.Arguments);
```

### Вывод

Для вывода информации используется сервис ``IOutputManager``. Изначально он отсутсвует в движке и предполагается, что он будет добавлен внешними подключаемыми модулями.  
Чтобы с помощью него отправить что-то пользователю, достаточно вызвать метод ``IUserSession.Output.ShowContent()``.  
У каждого пользователя свой экземпляр IOutputManager. Так же для того, чтобы взять конкретную реализацию - достаточно вызвать ``IUserSession.GetOutput<TOutput>()``.  

## Работа с данными

Для работы с данными есть два пути:  

* Использовать BaseContext  
* Использовать свой DataContext  

Теперь подробнее о каждом.  

### BaseContext

BaseContext - это универсальный контекст, который использует для подключения данные из переменных среды. Он автоматически подгружает в себя все зарегистрированные типы сущностей, которые реализуют интерфейс IDataEntity. Для создания миграций с ним потребуется создать дополнительный контекст, который наследует BaseContext, после чего можно делать миграцию.  
**Важно!** При создании миграции убедитесь, что она НЕ конфликтует с имеющимися данными в БД. Для этого достаточно удалить все ссылки на UserEntity, UserLocalEntity и др.  
Так же для успешного создания миграции у вашего контекста должен быть конструктор ``public MyContext(IEngineConfigurator? configurator = null) : base(configurator)``.  

Если принято решение использовать BaseContext, то необходимо указать параметры подключения в переменных среды (см используемые переменные среды) либо использвовать провайдер SQLite/InMemory (они не требуют заполнения переменных сред, SQLite имеет конфиг по умолчанию, InMemory в конфиге не нуждается)  
Далее все сущности, которые будут исползованы в проекте должны реализовывать интерфейс ``IDataEntity`` или наследовать базовый класс ``BaseEntity``.  

Для работы с таким контекстом необходимо использовать DI. Для этого уже реализован метод, открывающий контекст, выполняющий в нем действия, а после закрываюющий его.  
```
await user.Services.InContext(async (context) => 
{
	DoSomthing ...
});
```

Если вы хотите комбинировать несколько контекстов, реализованных от BaseContext (например разные строки подключения, разделить контексты на ReadonlyContext и ReaadWriteCotnext) то вы можете использовать ``InContext<T>``  
```
await user.Services.InContext<ReadonlyContext>(async (context) => 
{
	DoSomthing ...
});
```

Все свои контексты необходимо добавить в DI в формате transient как реализацию IDataContext.  
```
services.AddTransient(typeof(IDataContext), typeof(ReadonlyContext));
```

### Свой контекст, но не BaseContext

Вы можете использовать сколько угодно контекстов, как угодно и каких угодно.  
Ядро движка по прежнему будет работать в пределах BaseContext, но внутри ваших модулей вы абсолютно свободны.  
Если вы собираетесь делать дополнительные модули, доступные для других пользователей - то рекоменудется использовать базовый функционал работы с контекстами, чтобы упростить другим работу с вашими модулями.  
