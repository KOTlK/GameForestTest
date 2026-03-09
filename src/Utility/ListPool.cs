using System.Collections.Generic;

public static class ListPool<T> {
	public static Stack<List<T>> Pool = new();

	private const int initialCapacity = 128;

	public static List<T> Get() {
		if (Pool.Count > 0) {
			return Pool.Pop();
		}

		var list = new List<T>(initialCapacity);
		return list;
	}

	public static void Release(List<T> list) {
		list.Clear();

		Pool.Push(list);
	}
}