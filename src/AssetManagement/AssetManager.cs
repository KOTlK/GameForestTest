public static class AssetManager {
	private static AssetDatabase database = new();

	public static void LoadEntity(string name, Entity entity) {
		database.ReadEntity(name, entity);
	}

	public static T LoadAsset<T>(string name) {
		return database.Read<T>(name);
	}
}