using System.Numerics;
using System.Collections.Generic;

public class GridAnimationSystem : GameSystem {
	private EntityManager em;

	public GridAnimationSystem(Game game, EntityManager entityManager) :
		base (game, true) {
		em = entityManager;
	}

	// @Speed: Switch to something sane. Now it's O(n) when removing transactions.
	public List<Transaction> Transactions = new();

	// @Cleanup: Return handle to transaction to be able to stop it later.
	public void AppendTransaction(Transaction t) {
		t.Em = em;
		Transactions.Add(t);
	}

	public override void Update() {
		for (var i = 0; i < Transactions.Count; i++) {
			var transaction = Transactions[i];
			transaction.Update();

			if (transaction.IsOver) {
				Transactions.RemoveAt(i);
			}
		}
	}
}