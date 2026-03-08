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
	public ElementColor Color {get; private set;}
	public ElementShape Shape { get; private set;}
	public Vector2UInt  GridPosition;
	public uint         Score;
	public Vector2      SelectedScale = new Vector2(1.3f, 1.3f);
	public Vector2      UnselectedScale = new Vector2(1, 1);

	public override void OnDeserialize(IDeserializer reader) {
		Color = reader.Read<ElementColor>(nameof(Color));
		Shape = reader.Read<ElementShape>(nameof(Shape));
		Score = reader.Read<uint>(nameof(Score));
	}

	public void SetGridPosition(Vector2UInt pos) {
		GridPosition = pos;
	}

	public void Select() {
		Scale = SelectedScale;
	}

	public void Deselect() {
		Scale = UnselectedScale;
	}

	public void SetShape(ElementShape shape) {
		Shape = shape;
	}

	public void SetColor(ElementColor color) {
		Color = color;
		var render = Services<RenderSystem>.Get();

		ref var renderer = ref render.GetRenderer(Renderer);

		renderer.Color = Color switch {
			ElementColor.Green => Raylib_cs.Color.Green,
			ElementColor.Red   => Raylib_cs.Color.Red,
			ElementColor.Blue  => Raylib_cs.Color.Blue
		};
	}
}