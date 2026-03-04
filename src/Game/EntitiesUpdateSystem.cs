public sealed class EntitiesUpdateSystem : GameSystem {
	private EntityManager entityManager;

	public EntitiesUpdateSystem(Game game, EntityManager em) : base(game, true) {
		entityManager = em;
	}

	public override void Update() {
		entityManager.Update();
	}
}