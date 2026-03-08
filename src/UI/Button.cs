using System;
using System.Numerics;
using Raylib_cs;

public enum ButtonPressType {
	None      = 0,
	OnClick   = 1,
	OnRelease = 2,
}

public class Button : UIElement {
	public event Action    Clicked  = delegate {};
	public event Action    Released = delegate {};
	public event Action    Pressed  = delegate {};
	public Vector2 		   Size;
	public ButtonPressType PressType = ButtonPressType.OnClick;

	private bool clicked = false;
	private RendererHandle textRenderer;

	public bool IsClicked => clicked;

    public override void OnDeserialize(IDeserializer reader) {
    	Size     = reader.Read<Vector2>("Size");
        var renderer = new Renderer();

        renderer.Entity   = Handle;
        renderer.Shape    = ShapeType.Text;
		renderer.Text     = reader.Read<string>("Text");
        renderer.Color    = reader.Read<Color>("TextColor");
        renderer.Offset   = reader.Read<Vector2>("TextOffset");
        renderer.FontSize = reader.Read<int>("FontSize");

        var render = Services<RenderSystem>.Get();

        textRenderer = render.AppendRenderer(renderer);
    }

    public override void Destroy() {
    	var render = Services<RenderSystem>.Get();

    	render.RemoveRenderer(textRenderer);
    }

	public void Click() {
		clicked = true;
		
		OnClick();
		Clicked();

		if (PressType == ButtonPressType.OnClick) {
			Pressed();
		}
	}

	public void Release() {
		if (!clicked) return;

		clicked = false;

		OnRelease();
		Released();

		if (PressType == ButtonPressType.OnRelease) {
			Pressed();
		}
	}

	public bool PointerOver(Vector2 pointerPos) {
		var pos   = Position;
		var hsize = Size * 0.5f;
		var max   = pos + hsize;
		var min   = pos - hsize;

		return pointerPos.X > min.X &&
			   pointerPos.X < max.X &&
			   pointerPos.Y > min.Y &&
			   pointerPos.Y < max.Y;
	}

	protected virtual void OnClick()   { }
	protected virtual void OnRelease() { }
}