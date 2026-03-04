using Raylib_cs;
using System.Numerics;

public enum ElementColor : byte {
	None = 0,
	Red   = 1,
	Green = 2,
	Blue  = 3
}

public enum ElementShape : byte {
	None      = 0,
	Circle    = 1,
	Rectangle = 2,
}

public class Element : Entity {
	public ElementColor Color;
	public ElementShape Shape;
	public Vector2UInt  GridPosition;

	public override void OnDeserialize(IDeserializer reader) {
		Color = reader.Read<ElementColor>(nameof(Color));
		Shape = reader.Read<ElementShape>(nameof(Shape));
	}
}