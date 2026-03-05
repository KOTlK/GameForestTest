public abstract class Transaction {
	public EntityManager Em;

	public bool IsOver { get; protected set; }

	public abstract void Update();
}