﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Rail.Misc
{
    public static class Enumerables
    {

		
		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			if (source == null)
			{
				return;
			}
			foreach(T item in source)
			{
				action(item);
			}
		}

		public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
		{
			if (source == null)
			{
				return;
			}
			int index = 0;
			foreach (T item in source)
			{
				action(item, index++);
			}
		}

		public static void AddRange<T>(this ObservableCollection<T> source, IEnumerable<T> items)
		{
			foreach (T i in items)
			{
				source.Add(i);
			}
		}

		public static bool One<TSource>(this IEnumerable<TSource> source)
		{
			return source.Count<TSource>() == 1;
		}

		public static bool One<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			return source.Count<TSource>(predicate) == 1;
		}

		public static TSource IdenticalOrDefault<TSource>(this IEnumerable<TSource> source)
		{
			TSource first = source.FirstOrDefault();
			int num = source.Where(i => i.Equals(first)).Count();
			return num == source.Count() ? first : default;
		}

		//public static void ForEach<T>(this ObservableCollection<T> source, Action<T> action)
		//{
		//	foreach (T i in source)
		//	{
		//		action(i);
		//	}
		//}

		public static ObservableCollection<TSource> ToObservableCollection<TSource>(this IEnumerable<TSource> source)
		{
			return new ObservableCollection<TSource>(source);
		}
	}
}
