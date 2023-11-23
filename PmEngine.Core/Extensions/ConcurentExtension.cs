using System.Collections.Concurrent;

namespace PmEngine.Core.Extensions
{
	/// <summary>
	/// Расширение для конкуретнтых коллекций
	/// </summary>
	public static class ConcurentExtension
	{
		private static object _locker = new object();

		/// <summary>
		/// Удаление из сумки
		/// </summary>
		/// <typeparam name="T">Тип</typeparam>
		/// <param name="cb">Сумка</param>
		/// <param name="obj">Объект</param>
		public static void Remove<T>(this ConcurrentBag<T> cb, T obj)
		{
			lock (_locker)
			{
				var l = cb.ToList();
				l.Remove(obj);
                cb.Clear();
				foreach (var i in l)
					cb.Add(i);
			}
		}

		/// <summary>
		/// Удалить всё
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cb"></param>
		/// <param name="func"></param>
        public static void RemoveAll<T>(this ConcurrentBag<T> cb, Func<T, bool> func)
        {
			lock (_locker)
			{
				var l = cb.ToList();
				l.RemoveAll(t => func(t));
				cb.Clear();
				foreach (var i in l)
					cb.Add(i);
			}
        }
    }
}