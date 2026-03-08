using System;
using System.Numerics;

public class DestroyerTransaction : Transaction {
	public EntityHandle Entity;
	public Vector2Int	Direction;
	public float        Duration; // Time in seconds to move to the next grid cell
	public Vector2UInt  CurrentCell;
	public Vector2UInt  NextCell;

	private GridSystem grid;
	private float      timePassed = 0f;

	public DestroyerTransaction(GridSystem   grid,
								EntityHandle entity, 
							    Vector2UInt  from,
							    Vector2Int   direction,
							    float        duration) {
		Entity      = entity;
		Direction   = direction;
		Duration    = duration;
		IsOver      = false;
		this.grid   = grid;
		CurrentCell = from;
		NextCell    = CurrentCell + Direction;
		// grid.DestroyElement(grid.GetCellIndex(NextCell));
	}

	public override void Update() {
		timePassed += Clock.Delta;

		if (!Em.GetEntity(Entity, out var entity)) {
			IsOver = true;
			return;
		}

		if (timePassed >= Duration) {
			grid.DestroyElement(grid.GetCellIndex(NextCell));
			entity.Position = grid.GetCellCenter(NextCell);

			if (CanMoveNext()) {
				CurrentCell = NextCell;
				NextCell += Direction;
				timePassed = 0f;
			} else {
				IsOver = true;
				entity.DestroyThisEntity();
				return;
			}
		}

		var pos  = grid.GetCellCenter(CurrentCell);
		var next = grid.GetCellCenter(NextCell);
		var t    = timePassed / Duration;

		entity.Position = Vector2.Lerp(pos, next, t);
	}

	private bool CanMoveNext() {
		var next = NextCell + Direction;
		if (next.x < 0 			  ||
			next.x >= grid.Size.x ||
			next.y < 0 			  ||
			next.y >= grid.Size.y) {
			return false;
		}

		return true;
	}
}