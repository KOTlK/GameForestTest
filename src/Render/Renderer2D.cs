using Raylib_cs;
using System.Numerics;

public enum ShapeType {
	None      = 0,
	Circle    = 1,
	Rectangle = 2,
}

public struct Renderer {
	public EntityHandle Entity;
	public Color     	Color;
	public ShapeType 	Shape;
	public float     	Radius;
	public Vector2   	Size;

	public void Deserializer(IDeserializer reader) {
		reader.Read<Color>(nameof(Color));
		reader.Read<ShapeType>(nameof(Shape));

		switch(Shape) {
			case ShapeType.Circle : {
				reader.Read<float>(nameof(Radius));
			} break;
			case ShapeType.Rectangle : {
				reader.Read<Vector2>(nameof(Size));
			} break;
		}
	}
}