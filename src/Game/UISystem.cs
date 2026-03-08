using System.Numerics;
using Raylib_cs;

using static Raylib_cs.Raylib;

public class UISystem : GameSystem {
	private EntityManager em;
	private Camera        camera;

	public UISystem(Game 		  game, 
					EntityManager entityManager,
					Camera        camera) : base(game, true) {
		em = entityManager;
		this.camera = camera;
	}

	public override void Update() {
		if (IsMouseButtonPressed(MouseButton.Left)) {
			var pointerPos = camera.GetPointerWorldPos();

			foreach(var entity in em.GetAllEntitiesWithType<UIElement>(EntityType.UI)) {
				if (entity is Button button) {
					if (button.PointerOver(pointerPos)) {
						// lazy
						button.Click();
						button.Release();
					}
				}
			}
		}
	}
}