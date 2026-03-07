using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

using static Assertions;

public enum AssetType {
	None = 0,
	Entity = 1,
	Texture = 2,
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
	// @Temp: Hardcoded entries. Parse from text
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
						Shape  = ShapeType.Sprite,
						Size   = new Vector2(20, 20),
						TexturePath = "assets/textures/circle.png"
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
				new AssetField() {
					Name    = "Score",
					Content = 1U
				},
			}
		},
		new AssetEntry() {
			Name   = "red_circle_element",
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
						Color  = Color.Red,
						Shape  = ShapeType.Sprite,
						Size   = new Vector2(20, 20),
						TexturePath = "assets/textures/circle.png"
					}
				},
				new AssetField() {
					Name    = "Color",
					Content = ElementColor.Red
				},
				new AssetField() {
					Name    = "Shape",
					Content = ElementShape.Circle
				},
				new AssetField() {
					Name    = "Score",
					Content = 1U
				},
			}
		},
		new AssetEntry() {
			Name   = "blue_circle_element",
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
						Color  = Color.Blue,
						Shape  = ShapeType.Sprite,
						Size   = new Vector2(20, 20),
						TexturePath = "assets/textures/circle.png"
					}
				},
				new AssetField() {
					Name    = "Color",
					Content = ElementColor.Blue
				},
				new AssetField() {
					Name    = "Shape",
					Content = ElementShape.Circle
				},
				new AssetField() {
					Name    = "Score",
					Content = 1U
				},
			}
		},
		new AssetEntry() {
			Name   = "green_rectangle_element",
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
						Shape  = ShapeType.Sprite,
						Size   = new Vector2(20, 20),
						TexturePath = "assets/textures/rect.png"
					}
				},
				new AssetField() {
					Name    = "Color",
					Content = ElementColor.Green
				},
				new AssetField() {
					Name    = "Shape",
					Content = ElementShape.Rectangle
				},
				new AssetField() {
					Name    = "Score",
					Content = 1U
				},
			}
		},
		new AssetEntry() {
			Name   = "red_rectangle_element",
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
						Color  = Color.Red,
						Shape  = ShapeType.Sprite,
						Size   = new Vector2(20, 20),
						TexturePath = "assets/textures/rect.png"
					}
				},
				new AssetField() {
					Name    = "Color",
					Content = ElementColor.Red
				},
				new AssetField() {
					Name    = "Shape",
					Content = ElementShape.Rectangle
				},
				new AssetField() {
					Name    = "Score",
					Content = 1U
				},
			}
		},
		new AssetEntry() {
			Name   = "blue_rectangle_element",
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
						Color  = Color.Blue,
						Shape  = ShapeType.Sprite,
						Size   = new Vector2(20, 20),
						TexturePath = "assets/textures/rect.png"
					}
				},
				new AssetField() {
					Name    = "Color",
					Content = ElementColor.Blue
				},
				new AssetField() {
					Name    = "Shape",
					Content = ElementShape.Rectangle
				},
				new AssetField() {
					Name    = "Score",
					Content = 1U
				},
			}
		},
		new AssetEntry() {
			Name = "score_ui",
			Type = AssetType.Entity,
			Fields = new List<AssetField>() {
				new AssetField() {
					Name    = "Type",
					Content = EntityType.UI
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
						Color    = Color.Black,
						Shape    = ShapeType.Text,
						Text     = "Score: 0",
						Offset   = new Vector2(0, 0),
						FontSize = 3
					}
				},
			}
		},
		new AssetEntry() {
			Name = "line",
			Type = AssetType.Entity,
			Fields = new List<AssetField>() {
				new AssetField() {
					Name    = "Type",
					Content = EntityType.Line
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
						Color       = Color.Yellow,
						Shape       = ShapeType.Sprite,
						Size        = new Vector2(10, 10),
						TexturePath = "assets/textures/line.png",
					}
				},
			}
		},
		new AssetEntry() {
			Name   = "assets/textures/circle.png",
			Type   = AssetType.Texture,
			Fields = new List<AssetField>() {
				new AssetField() {
					Name    = "Path",
					Content = "assets/textures/circle.png"
				}
			}
		},
		new AssetEntry() {
			Name   = "assets/textures/rect.png",
			Type   = AssetType.Texture,
			Fields = new List<AssetField>() {
				new AssetField() {
					Name    = "Path",
					Content = "assets/textures/rect.png"
				}
			}
		},
		new AssetEntry() {
			Name   = "assets/textures/line.png",
			Type   = AssetType.Texture,
			Fields = new List<AssetField>() {
				new AssetField() {
					Name    = "Path",
					Content = "assets/textures/line.png"
				}
			}
		}
	};
	public Dictionary<string, int> EntryByName = new() {
		{
			"green_circle_element",
			0
		},
		{
			"red_circle_element",
			1
		},
		{
			"blue_circle_element",
			2
		},
		{
			"green_rectangle_element",
			3
		},
		{
			"red_rectangle_element",
			4
		},
		{
			"blue_rectangle_element",
			5
		},
		{
			"score_ui",
			6
		},
		{
			"line",
			7
		},
		{
			"assets/textures/circle.png",
			8
		},
		{
			"assets/textures/rect.png",
			9
		},
		{
			"assets/textures/line.png",
			10
		}

	};

	private Dictionary<string, Texture2D> loadedTextures = new();

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
		Assert(EntryByName.ContainsKey(name), "Cannot load entry with name %.", name);

		var entry = Entries[EntryByName[name]];

		switch(entry.Type) {
			case AssetType.Texture : {
				Assert(typeof(T) == typeof(Texture2D), "Trying to load texture, but provided % as generic type.", typeof(T).FullName);
				if (loadedTextures.ContainsKey(name)) {
					return (T)(object)loadedTextures[name];
				} else {
					var tex = Raylib.LoadTexture(entry.Read<string>("Path"));
					loadedTextures[name] = tex;

					return (T)(object)tex;
				}
			} break;
		}

		return default(T);
	}
}