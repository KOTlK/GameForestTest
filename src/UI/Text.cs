using Raylib_cs;

public class Text : UIElement {
	public void SetText(string text) {
		var render = Services<RenderSystem>.Get();

		ref var renderer = ref render.GetRenderer(Renderer);

		renderer.Text = text;
	}

	public void SetColor(Color color) {
		var render = Services<RenderSystem>.Get();

		// Why don't I just use pointers...
		ref var renderer = ref render.GetRenderer(Renderer);

		renderer.Color = color;
	}
}