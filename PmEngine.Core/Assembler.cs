using PmEngine.Core.Interfaces;
using System.Reflection;
using System.Runtime.Loader;

namespace PmEngine.Core
{
    /// <summary>
    /// This class provide possibility to work with actions in another libraries in library storage
    /// </summary>
    public static class Assembler
    {
        internal static string LibPaht { get; set; } = "./";

        /// <summary>
        /// Make action in not refferenced assembly
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<INextActionsMarkup?> InAssembly(ActionWrapper wrapper, IUserSession user)
        {
            var fullname = wrapper.ActionType?.FullName ?? wrapper.ActionTypeName;

            Console.WriteLine($"Исполнение {wrapper.DisplayName} ({fullname})");

            if (String.IsNullOrEmpty(fullname))
                return wrapper.NextActions;

            var actionClass = Get<IAction>(fullname);

            if (actionClass is not null)
                return await actionClass.DoAction(wrapper, user);

            return null;
        }

        /// <summary>
        /// Make action in not refferenced assembly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="user"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<INextActionsMarkup?> InAssembly<T>(IUserSession user, Arguments? args = null) where T : IAction
        {
            return await InAssembly(new ActionWrapper("", typeof(T).FullName, args), user);
        }

        /// <summary>
        /// Get type from assembly in library storage
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="fullname">Full name of type</param>
        /// <param name="libName">DLL name without extension</param>
        /// <param name="args">Arguments to constructor of class</param>
        /// <returns></returns>
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