using System;
using System.Numerics;

public class FailedSwitchTransaction : Transaction {
	public EntityHandle 		First;
	public EntityHandle 		Second;
	public Vector2              FirstStart;
	public Vector2              SecondStart;
	public Vector2      		FirstTarget;
	public Vector2      		SecondTarget;
	public float                Duration;
	public event Action<Entity> OnTransactionOver = delegate {};

	private float timePassed = 0f;

	public FailedSwitchTransaction(EntityHandle   first, 
							 	   EntityHandle   second, 
							 	   Vector2        firstStart,
							 	   Vector2        secondStart,
							 	   Vector2        firstTarget,
							 	   Vector2        secondTarget,
							 	   float          duration,
							 	   Action<Entity> onTransactionOver = null) {
		First  		 = first;
		Second 		 = second;
		FirstStart   = firstStart;
		SecondStart  = secondStart;
		FirstTarget  = firstTarget;
		SecondTarget = secondTarget;
		Duration     = duration;
		IsOver 		 = false;
		OnTransactionOver += onTransactionOver;
	}

	public override void Update() {
		timePassed += Clock.Delta;

		if (!Em.GetEntity(First, out var first)) {
			IsOver = true;
			return;
		}

		if (!Em.GetEntity(Second, out var second)) {
			IsOver = true;
			return;
		}

		if (timePassed >= Duration) {
			first.Position  = FirstStart;
			second.Position = SecondStart;
			IsOver 			= true;
			return;
		}

		var t         = timePassed / Duration;

		var firstPos  = Mathf.PingPong(FirstStart, FirstTarget, t);
		var secondPos = Mathf.PingPong(SecondStart, SecondTarget, t);

		first.Position  = firstPos;
		second.Position = secondPos;
	}
}