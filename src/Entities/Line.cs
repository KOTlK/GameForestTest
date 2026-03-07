public class Line : Bonus {
	private RendererHandle lineRenderer;

    public override void OnDeserialize(IDeserializer reader) {
    }

	public override void Activate(GridSystem grid) {
		Console.WriteLine("Activated line");
	}

	public override void Destroy() {
		var render = Services<RenderSystem>.Get();
		render.RemoveRenderer(lineRenderer);
	}
}