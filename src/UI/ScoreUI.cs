using Raylib_cs;

public class ScoreUI : UIElement {
	public uint CurrentScore = 0;
	public uint TargetScore = 0;

	public override void OnCreate() {
		Events.SubGeneral<ScoreChangedEvent>(OnScoreChanged);
	}

	public override void Destroy() {
		Events.UnsubGeneral<ScoreChangedEvent>(OnScoreChanged);
	}

	public override void UpdateEntity() {
		if (CurrentScore < TargetScore) {
			CurrentScore++;

    		var render   = Services<RenderSystem>.Get();
			ref var renderer = ref render.GetRenderer(Renderer);
    		renderer.SetText($"Score: {CurrentScore}");
		}
	}

    private void OnScoreChanged(ScoreChangedEvent evnt) {
    	TargetScore = evnt.NewScore;
    	Console.WriteLine(TargetScore);
    }
}