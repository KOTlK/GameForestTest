public static class Services<T> {
	private static T instance;

	public static void Create(T inst) {
		instance = inst;
	}

	public static T Get() {
		return instance;
	}
}