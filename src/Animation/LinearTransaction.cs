using System;
using System.Numerics;

public class LinearTransaction : Transaction {
	public EntityHandle 		Entity;
	public Vector2              From;
	public Vector2      		To;
	public float                Duration;
	public event Action<Entity> OnTransactionOver = delegate {};

	private float timePassed = 0f;
	private const float epsilon = 0.01f;

	public LinearTransaction(EntityHandle   entity, 
							 Vector2        from,
							 Vector2        to,
							 float          duration,
							 Action<Entity> onTransactionOver = null) {
		Entity   = entity;
		From     = from;
		To       = to;
		Duration = duration;
		OnTransactionOver += onTransactionOver;
		IsOver   = false;
	}

	public override void Update() {
		timePassed += Clock.Delta;

		if (!Em.GetEntity(Entity, out var entity)) {
			IsOver = true;
			return;
		}

		if (timePassed >= Duration) {
			entity.Position = To;
			IsOver = true;
			return;
		}

		var t = timePassed / Duration;

		entity.Position = Vector2.Lerp(From, To, t);
	}
}