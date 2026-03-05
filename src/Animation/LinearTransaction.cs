using System;
using System.Numerics;

public class LinearTransaction : Transaction {
	public EntityHandle 		Entity;
	public Vector2      		To;
	public float                Speed;
	public event Action<Entity> OnTransactionOver = delegate {};

	private const float epsilon = 0.01f;

	public LinearTransaction(EntityHandle   entity, 
							 Vector2        to,
							 float          speed,
							 Action<Entity> onTransactionOver = null) {
		Entity = entity;
		To     = to;
		Speed  = speed;
		OnTransactionOver += onTransactionOver;
		IsOver = false;
	}

	public override void Update() {
		if (!Em.GetEntity(Entity, out var entity)) {
			IsOver = true;
			return;
		}

		var pos     = entity.Position;
		var nextPos = Mathf.MoveTowards(pos, To, Speed * Clock.Delta);
		var dir 	= To - nextPos;

		if (dir.LengthSquared() <= epsilon) {
			entity.Position = To;
			IsOver 		    = true;
			OnTransactionOver(entity);
		} else {
			entity.Position = nextPos;
		}
	}
}