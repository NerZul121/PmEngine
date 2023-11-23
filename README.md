# PMEngine.Core



## Что это?

Это ядро движка, предназначенного для текстовых интерактивных приложений, например для ботов Telegram, других мессенеджеров, браузерных приложений или просто консоли.
Движок умеет обрабатывать действия пользователя, управлять ими, выполнять команды, поддерживает модульность.
Демонстрация движка: текстовая полноценная ММОРПГ в Телеграмм - https://t.me/PmEngineTestBot

## Список исползуемых переменных среды

В движке используются следующие переменные среды:
```
PROVIDER_TYPE = INT от 0 до 2, где 0 - PostgreSQL, 1 - SQLite, 2 - InMemory (SQLite)
CONNECTION_STRING = Строка подключения к БД, которая будет использована в BaseContext. Должна соответствовать провайдеру, указанному в PROVIDER_TYPE
```

## Делаем Hello World в консоли

\#1 Создаем консольный проект в Visual Studio на .NET 7.0

\#2 Добавляем ссылку на PmEngine.Core

\#3 Создаем класс модуля, наследующий BaseModuleRegistrator. Он отвечает за регистрацию модуля внутри движка

```
using PmEngine.Core.BaseClasses;

namespace PmEngine.Examples
{
    internal class ExampleModule : BaseModuleRegistrator
    {
    }
}
```

\#4 Берем [IOutputManger](https://gitlab.battleofthedemigod.ru/gitlab-instance-bb639d92/pmengine.examples) из примеров / или делаем его самостоятельно

```
public class ConsoleOutput : IOutputManager
{
	// ... Реализация через Console.WriteLine()
}
```

\#5 В классе Programm.cs нужно указать используемый провайдер БД, реализацию IOutputManager, добавить созданный модуль в движок и вызывать метод конфигурирования:

```
#region configure
// Устанавливаем тип соединения с БД как InMemory
Engine.Properties.DataProvider = DataProvider.InMemory;

// Добавляем Output
Engine.Services.AddSingleton<IOutputManager>(new ConsoleOutput());

// Добавляем модуль
Engine.Modules.Add(new ExampleModule());

//Конфигурируем движок
Engine.Configure();
#endregion
```

\#6 В Programm.cs нужно добавить функционал действий. Пример можно взять [тут]() или написать самому:

```
//Регистрация пользователя
long userId = 0;
UserEntity? user = null;

using (var context = MainProcess.GetContext())
{
    Console.Write("Введите ваше имя: ");
    var name = Console.ReadLine() ?? "userName";

    user = context.Set<UserEntity>().FirstOrDefault(p => p.Name.ToLower() == name.ToLower());

    // Если такого нет, то создаем
    if (user is null)
    {
        user = new UserEntity() { Name = name };
        await context.Set<UserEntity>().AddAsync(user);
        await context.SaveChangesAsync();
    }
}

userId = user.Id;

// Процесс считывания введенного значения
while (true)
{
    var ps = await ServerSession.GetUserSession(userId);

    if (ps.NextActions.Any())
        await MainProcess.ActionProcess((ActionWrapper)await SelectFrom(ps.NextActions, ps), userId);

    Task.Delay(100).Wait();
}

// Выбор из предложенных действий
async Task<object> SelectFrom<T>(T select, UserSession ps) where T : IEnumerable<ActionWrapper>
{
    var result = MainProcess.Output.ActionsToStrings(select);

    var actN = await GetActionNumber(ps);

    return ServerSession.GetUserSession(userId).Result.NextActions[actN];
}

// Получение введенного пользователем выбора
async Task<int> GetActionNumber(UserSession ps)
{
    int i;
    string ms = Console.ReadLine() ?? "";

    while (!int.TryParse(ms, out i) || i >= ps.NextActions.Count())
    {
        if (ps.InputAction != null)
            await MainProcess.MakeAction(ps.Id, ps.InputAction);

        ms = Console.ReadLine() ?? "";
    }

    return i;
}
```

\#7 При запуске мы получим ошибку, что не указано инициализирующее пользователя действие. Надо его создать. Создаем папку Actions и добавляем туда новый класс HelloWorldAction, реализующий IAction

```
internal class HelloWorldAction : IAction
{
    public async Task<INextActionsMarkup?> DoAction(ActionWrapper currentAction, long userId, Dictionary<string, object> arguments, string inputData = "")
    {
		// Получение активной сессии пользователя
        var userSession = await ServerSession.GetUserSession(userId);
		
		// Добавляем на вывод сообщение "Привет, %UserName%!". Обратите внимание на использование CachedData!
        MainProcess.Output.AddToOutput($"Привет, {userSession.CachedData.Name}!", userId);

		// Создание разметки кнопок, которая вернется пользователю
        var result = new SingleMarkup();
		
		// Добавление пользователю кнопки "Привет!", которая запустит выполнение действия HelloWorldAction (которое выполняется сейчас)
        result.Add("Привет!", typeof(HelloWorldAction));

        return result;
            // Можно сократить до return new SingleMarkup(new ActionWrapper[] { new ActionWrapper("Привет!", typeof(HelloWorldAction)) });
    }
}
```

\#8 После этого нужно указать созданное нами действие как инициализирующее. Для этого нужно добавить его в ServerProperties.InitializationAction. Сделать это можно там же в Programm.cs на этапе заполнения данных движка, добавив строчку

```
Engine.Properties.InitializationAction = typeof(HelloWorldAction);
```

Либо указав это в классе-регистраторе модуля, сделав override метода AdditionalRegistrate

```
public override void AdditionalRegistrate(ServiceCollection services, IEnumerable<Type> allTypes)
    {
        ServerProperties.InitializationAction = typeof(HelloWorldAction);
    }
```

\#9 Готово! Осталось запустить приложение и постоянно приветствовать :)

## Как добавить свои сущности

Для добавления своих сущностей можно использовать следующие варианты:

* Использовать BaseContext
* Использовать свой DataContext

Теперь подробнее о каждом.

## BaseContext

BaseContext - это универсальный контекст, который использует для подключения данные из переменных среды. Он автоматически подгружает в себя все зарегистрированные типы сущностей, которые реализуют интерфейс IDataEntity. Для создания миграций с ним потребуется создать дополнительный контекст, который наследует BaseContext, после чего можно делать миграцию.

Если принято решение использовать BaseContext, то необходимо указать параметры подключения в переменных среды (см используемые переменные среды) либо использвовать провайдер SQLite/InMemory (они не требуют заполнения переменных сред, SQLite имеет конфиг по умолчанию, InMemory в конфиге не нуждается)
Далее все сущности, которые будут исползованы в проекте должны наследовать одну из следующих базовых сущностей:

* BaseEntity - Базовая сущность с ID и наследованием IDataEntity
* BaseNamedEntity - Базовая сущность с Name, наследующая BaseEntity
* BaseDescriptedEntity - Базова сущность с Description, наследующая BaseNamedEntity

При работе с контекстом нужно вызывать метод MainProcess.GetContext(), например:

```
using var context = MainProcess.GetContext();
var user = context.Set<UserEntity>().AsNoTracking().First(u => u.Id == 1);
```

Если вы хотите использовать какой-либо кастомный контекст (например для подключения к MSSQL), но хотите использовать его как базовый, то создайте его, наследуйте BaseContext и укажите ему аттрибут Priority, который отвечает за приоритетность использования.

```
[Priority(10)]
public class NewContext : BaseContext
{
}
```

После чего добавьте его в список контектов:
```
Engine.DataContexts.Add(typeof(NewContext));
```

При использовании нескольких контекстов метод MainProcess.GetContext() вернет тот контекст, число приоритета которого меньше всего. Т.е. если есть контекст с приоритетом 1 и контекст с приоритетом 2, то вернется контекст с приоритетом 1.
Если вы хотите гарантированно получить контекст, который соответствует типу (или является его наследником), то используйте метод MainProcess.GetContext<TContext>(), который отработает так же, как и MainProcess.GetContext(), только с проверкой на наследование/соответствование типу. Это может быть полезно при работе с несколькими разными контекстами.
MainProcess.GetContext<T>() работает на основе интерфейса IDataContext, но обращается к списку контекстов Engine.DataContexts, поэтому регистрация контекстов в этом списке обязательна.

## Свой контекст, но не BaseContext

Вы можете использовать сколько угодно контекстов, как угодно и каких угодно.
Ядро движка по прежнему будет работать в пределах BaseContext, но внутри ваших модулей вы абсолютно свободны.
Если вы собираетесь делать дополнительные модули, доступные для других пользователей - то рекоменудется использовать базовый функционал работы с контекстами, чтобы упростить другим работу с вашими модулями.
