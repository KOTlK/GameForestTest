public class CountdownSystem : GameSystem {
	private float timer;
	private float timePassed;

	public bool IsTimerOver => timePassed >= timer;

	public CountdownSystem(Game game, float timer) : base(game, true) {
		this.timer = timer;
		timePassed = 0f;
	}

	public override void Update() {
		timePassed += Clock.Delta;

		var evnt = new TimeChangedEvent();

		evnt.TimeLeft = timer - timePassed;

		Events.RaiseGeneral(evnt);
	}

	public override void OnDisable() {
		timePassed = 0f;
	}

	public override void OnEnable() {
		timePassed = 0f;
		var evnt = new TimeChangedEvent();

		evnt.TimeLeft = timer - timePassed;

		Events.RaiseGeneral(evnt);
	}
}