using static Assertions;

namespace AssetManagement;

public class AssetField {
	public string Name;
	public object Content;

	public T GetContent<T>() {
		Assert(Content is T, 
			   "Cannot convert field content to %. The content is of type %.",
				typeof(T).FullName,
				Content.GetType().FullName);
		return (T)Content;
	}
}