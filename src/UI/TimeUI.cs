using Raylib_cs;

public class TimeUI : UIElement {
	public override void OnCreate() {
		Events.SubGeneral<TimeChangedEvent>(OnTimeChanged);
	}

	public override void Destroy() {
		Events.UnsubGeneral<TimeChangedEvent>(OnTimeChanged);
	}

    private void OnTimeChanged(TimeChangedEvent evnt) {
    	var render       = Services<RenderSystem>.Get();
		ref var renderer = ref render.GetRenderer(Renderer);
		renderer.Text = $"Time left: {MathF.Ceiling(evnt.TimeLeft)}";
    }
}