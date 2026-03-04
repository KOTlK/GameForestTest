using System.Numerics;

using static Assertions;

public class GridSystem : GameSystem {
	public Vector2UInt    Size;
	public EntityHandle[] Elements;

	private EntityManager em;

	public GridSystem(Game game, EntityManager entityManager, Vector2UInt size) : 
	base (game, true) {
		em = entityManager;
		MakeNew(size);
	}

	public void MakeNew(Vector2UInt size) {
		var count = size.x * size.y;
		Size     = size;
		Elements = new EntityHandle[count];

		for (var i = 0; i < count; i++) {
			Elements[i] = EntityHandle.Zero;
		}
	}

	public void PutElement(Vector2UInt pos, Element element) {
		var index 			 = GetElementIndex(pos);
		element.Position     = pos;
		element.GridPosition = pos;

		Assert(em.IsAlive(Elements[index]) == false,
			  "Cannot put element at position %. Slot is already occupied.",
			   pos);

		Elements[index] = element.Handle;
	}

	public Element GetElement(Vector2UInt pos) {
		var index = GetElementIndex(pos);

		if (em.GetEntity(Elements[index], out Element element)) {
			return element;
		}

		return null;
	}

	public bool GetElement(Vector2UInt pos, out Element element) {
		var index = GetElementIndex(pos);

		if (em.GetEntity(Elements[index], out element)) {
			return true;
		}

		return false;
	}

	public void Cleanup() {
		foreach(var handle in Elements) {
			if (em.IsAlive(handle)) {
				em.DestroyEntity(handle);
			}
		}

		Elements = null;
	} 

	private uint GetElementIndex(Vector2UInt pos) {
		return pos.x + pos.y * Size.x;
	}
}