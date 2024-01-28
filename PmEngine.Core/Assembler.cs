using PmEngine.Core.Interfaces;
using System.Reflection;
using System.Runtime.Loader;

namespace PmEngine.Core
{
    public static class Assembler
    {
        internal static string LibPaht { get; set; } = "./";

        public static async Task<INextActionsMarkup?> InAssembly(IActionWrapper wrapper, IUserSession user)
        {
            var fullname = wrapper.ActionType?.FullName ?? wrapper.ActionTypeName;

            Console.WriteLine($"Исполнение {wrapper.DisplayName} ({fullname})");

            if (String.IsNullOrEmpty(fullname))
                return wrapper.NextActions;

            var actionClass = Get<IAction>(fullname);

            if (actionClass is not null)
                return await actionClass.DoAction(wrapper, user, wrapper.Arguments);

            return null;
        }

        public static async Task<INextActionsMarkup?> InAssembly<T>(IUserSession user, IActionArguments? args = null) where T : IAction
        {
            return await InAssembly(new ActionWrapper("", typeof(T).FullName, args), user);
        }

        public static T? Get<T>(string fullname, string? libName = null, object?[]? args = null)
        {
            var lib = libName ?? fullname.Split('.').First();
            var dlls = Directory.GetFiles(LibPaht).Where(s => s.Contains(lib) && s.EndsWith(".dll"));

            Assembly? assembly = null;
            Type? type = null;

            var context = new AssemblyLoadContext(null, true);

            foreach (var dll in dlls)
            {
                assembly = context.LoadFromStream(new MemoryStream(File.ReadAllBytes(dll)));

                type = assembly.GetTypes().FirstOrDefault(t => t.FullName == fullname);

                if (type is not null)
                    break;
            }

            if (assembly is null)
                return default(T);

            var deps = assembly.GetReferencedAssemblies();
            foreach (var dep in deps)
            {
                if (typeof(Assembler).Assembly.GetReferencedAssemblies().Any(d => d.Name == dep.Name))
                    continue;

                var fname = LibPaht + dep.Name + ".dll";

                if (File.Exists(fname))
                    context.LoadFromStream(new MemoryStream(File.ReadAllBytes(fname)));
                else
                    Console.WriteLine($"Зависимость {dep.Name} не найдена");
            }

            if (type == null)
            {
                Console.WriteLine("Тип не найден");
                return default(T);
            }

            return (T?)Activator.CreateInstance(type, args);
        }
    }
}