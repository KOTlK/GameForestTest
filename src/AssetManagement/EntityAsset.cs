using System.Numerics;

using static Assertions;

namespace AssetManagement;

public class EntityAsset : Asset, IDeserializer {
	private Dictionary<string, AssetField> fields = new();

	public void AddField(AssetField field) {
		fields.Add(field.Name, field);
	}

    public T Read<T>(string name) {
        Assert(fields.ContainsKey(name), "Cannot load field with name %", name);

        return fields[name].GetContent<T>();
    }
}