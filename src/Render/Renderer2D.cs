using Raylib_cs;
using System.Numerics;

// @Cleanup: use sprites instead.
public enum ShapeType {
	None      = 0,
	// Circle    = 1,
	// Rectangle = 2,
	Text      = 3,
	Sprite    = 4,
}

public struct Renderer {
	public EntityHandle Entity;
	public Color     	Color;
	public ShapeType 	Shape;
	// public float     	Radius;
	public Vector2   	Size;
	public Vector2   	Offset;
	public int          FontSize;
	public string       Text;
	public Texture2D    Texture;
	public string       TexturePath;

	public void LoadAssets() {
		if (TexturePath != null) {
			Console.WriteLine($"Loading {TexturePath}");
			Texture = AssetManager.LoadAsset<Texture2D>(TexturePath);
		}
	}

	public void SetText(string text) {
		Text = text;
	}
}