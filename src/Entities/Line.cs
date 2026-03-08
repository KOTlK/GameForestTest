using Random = GameMath.Random;

public enum LineDirection {
	None 	   = 0,
	Vertical   = 1,
	Horizontal = 2,
}

public class Line : Bonus {
	public LineDirection Direction;

	// private RendererHandle lineRenderer;

    public override void OnDeserialize(IDeserializer reader) {
    }

	public override void Activate(GridSystem grid) {
		grid.CreateDestroyerPair(GridPosition, Direction);
		// grid.DestroyElement(grid.GetCellIndex(GridPosition));
	}

	public override void OnCreate() {
		var vertical = Random.NextBool();

		if (vertical) {
			Orientation = 90f;
			Direction = LineDirection.Vertical;
		} else {
			Direction = LineDirection.Horizontal;
		}
	}
}