public static class AssetManager {
	private static AssetDatabase database = new();

	public static void LoadEntity(string name, Entity entity) {
		database.ReadEntity(name, entity);
	}
}