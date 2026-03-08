public class Bomb : Bonus {
	public int   Radius;
	public float Timeout;

	private GridSystem grid;

    public override void OnDeserialize(IDeserializer reader) {
    	Radius  = reader.Read<int>(nameof(Radius));
    	Timeout = reader.Read<float>(nameof(Timeout));
    }

    public override void UpdateEntity() {
    	Timeout -= Clock.Delta;

    	if (Timeout <= 0f) {
    		grid.ExplodeBomb(GridPosition, Radius);
    		DestroyThisEntity();
    	}
    }

	public override void Activate(GridSystem grid) {
		this.grid = grid;
		Em.SetFlag(Handle, EntityFlags.Dynamic);
		var renderer = Services<RenderSystem>.Get();

		renderer.RemoveRenderer(Renderer);
		Renderer = RendererHandle.Zero;
		// grid.ExplodeBomb(GridPosition, Radius);
	}
}