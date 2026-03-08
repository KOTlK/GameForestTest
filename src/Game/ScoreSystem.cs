public class ScoreSystem : GameSystem {
	private uint score = 0;

	public ScoreSystem(Game game) : base(game, true) {

	}

	public override void OnEnable() {
		Reset();
	}
	
	public override void OnDisable() {
		Reset();
	}

	public void Reset() {
		var evnt = new ScoreChangedEvent();
		evnt.OldScore = score;
		score = 0;
		evnt.NewScore = score;
		Events.RaiseGeneral(evnt);
	}

	public void Add(uint amount) {
		var evnt = new ScoreChangedEvent();
		evnt.OldScore = score;
		score += amount;
		evnt.NewScore = score;

		Events.RaiseGeneral(evnt);
	}

}