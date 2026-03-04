using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

using static Assertions;

public enum AssetType {
	None = 0,
	Entity = 1,
}

public class AssetField {
	public string Name;
	public object Content;
}

public struct AssetEntry : IDeserializer {
	public string 			Name;
	public List<AssetField> Fields;
	public AssetType 		Type;

	public T Read<T>(string name) {
		foreach (var field in Fields) {
			if (field.Name == name) {
				return (T)field.Content;
			}
		}

		Assert(false, "Cannot read value for %.", name);
		return default(T);
	}
}

public class AssetDatabase : ISerializer, IDeserializer {
	public List<AssetEntry> Entries = new() {
		new AssetEntry() {
			Name   = "green_circle_element",
			Type   = AssetType.Entity,
			Fields = new List<AssetField>() {
				new AssetField() {
					Name    = "Type",
					Content = EntityType.Element
				},
				new AssetField() {
					Name    = "Flags",
					Content = EntityFlags.Dynamic
				},
				new AssetField() {
					Name    = "Scale",
					Content = Vector2.One
				},
				new AssetField() {
					Name    = "Renderer",
					Content = new Renderer {
						Color  = Color.Green,
						Shape  = ShapeType.Circle,
						Radius = 1f
					}
				},
				new AssetField() {
					Name    = "Color",
					Content = ElementColor.Green
				},
				new AssetField() {
					Name    = "Shape",
					Content = ElementShape.Circle
				},
			}
		}
	};
	public Dictionary<string, int> EntryByName = new() {
		{
			"green_circle_element",
			0
		}

	};

	public void Write<T>(string name, T obj) {
		// @TODO: implement this if need editor
	}

	public void ReadEntity(string name, Entity entity) {
		Assert(EntryByName.ContainsKey(name), "Cannot load entity with name %.", name);
		var entry = Entries[EntryByName[name]];

		Assert(entry.Type == AssetType.Entity, "Asset type is incompatible (%)", entry.Type);

		entity.Deserialize(entry);
	}

	public T Read<T>(string name) {
		return default(T);
	}
}