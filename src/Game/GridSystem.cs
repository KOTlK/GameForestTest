using System.Numerics;

using static Assertions;

public class GridSystem : GameSystem {
	public Vector2UInt    Size;
	public Vector2        CellSize;
	public EntityHandle[] Elements;
	public int            SelectedCell = -1;

	public bool CellSelected => SelectedCell >= 0;

	private Random        random;
	private EntityManager em;

	private const float switchSpeed = 100f;

	private string[] 	  elements = new string[] {
		"green_circle_element",
		"red_circle_element",
		"blue_circle_element",
		"green_rectangle_element",
		"red_rectangle_element",
		"blue_rectangle_element",
	};

	public GridSystem(Game 			game, 
					  EntityManager entityManager,
					  Vector2UInt   size, 
					  Vector2       cellSize) : 
	base (game, true) {
		em     = entityManager;
		random = new Random();
		MakeNewGrid(size, cellSize);
	}

	public void MakeNewGrid(Vector2UInt size, Vector2 cellSize) {
		if (Elements != null) {
			Cleanup();
		}

		var count = size.x * size.y;
		Size     = size;
		CellSize = cellSize;
		Elements = new EntityHandle[count];

		for (var i = 0; i < count; i++) {
			Elements[i] = EntityHandle.Zero;
		}
	}

	public void FillRandom() {
		for (uint y = 0; y < Size.y; y++) {
			for (uint x = 0; x < Size.x; x++) {
				var position = GetCellCenter(x, y);
				var rand     = (uint)random.NextInt64() % elements.Length;
				var name     = elements[rand];
				var (handle, element)  = em.CreateEntity<Element>(name, position, 0f);
				PutElement(new Vector2UInt(x, y), element);
			}
		}
	}

	public void SwitchPositions(uint a, uint b) {
		if (!em.GetEntity(Elements[a], out Element first)) return;
		if (!em.GetEntity(Elements[b], out Element second)) return;

		if (SelectedCell == a || SelectedCell == b) {
			ResetSelection();
		}

		var animSystem = Game.GetSystem<GridAnimationSystem>();

		var aToB = new LinearTransaction(Elements[a], 
										 second.Position, 
										 switchSpeed);
		var bToA = new LinearTransaction(Elements[b], 
										 first.Position, 
										 switchSpeed);

		animSystem.AppendTransaction(aToB);
		animSystem.AppendTransaction(bToA);
		
		RemoveElement(a);
		RemoveElement(b);

		SetElement(a, second);
		SetElement(b, first);
	}

	public void RemoveElement(uint index) {
		Elements[index] = EntityHandle.Zero;
	}

	public void SetElement(uint index, Element element) {
		var pos = GetCellPos(index);
		element.SetGridPosition(pos);

		Assert(em.IsAlive(Elements[index]) == false,
			  "Cannot put element at position %. Slot is already occupied.",
			   pos);

		Elements[index] = element.Handle;
	}

	public void PutElement(Vector2UInt pos, Element element) {
		var index 			 = GetCellIndex(pos);
		element.Position     = GetCellCenter(pos);
		element.SetGridPosition(pos);

		Assert(em.IsAlive(Elements[index]) == false,
			  "Cannot put element at position %. Slot is already occupied.",
			   pos);

		Elements[index] = element.Handle;
	}

	public void PutElement(uint index, Element element) {
		var pos = GetCellPos(index);
		element.Position = GetCellCenter(pos);
		element.SetGridPosition(pos);

		Assert(em.IsAlive(Elements[index]) == false,
			  "Cannot put element at position %. Slot is already occupied.",
			   pos);

		Elements[index] = element.Handle;
	}

	public void ResetSelection() {
		if (SelectedCell < 0) return;

		if (em.GetEntity(Elements[SelectedCell], out Element selected)) {
			selected.Deselect();
		}

		SelectedCell = -1;
	}

	public void SelectCell(Vector2UInt pos) {
		if (SelectedCell >= 0) return;

		var index = GetCellIndex(pos);

		if (SelectedCell == index) return;

		if (SelectedCell > 0) {
			if (em.GetEntity(Elements[SelectedCell], out Element selected)) {
				selected.Deselect();
			}
		}

		if (em.GetEntity(Elements[index], out Element newSelected)) {
			newSelected.Select();
		}

		SelectedCell = (int)index;
	}

	public Element GetElement(Vector2UInt pos) {
		var index = GetCellIndex(pos);

		if (em.GetEntity(Elements[index], out Element element)) {
			return element;
		}

		return null;
	}

	public bool GetElement(Vector2UInt pos, out Element element) {
		var index = GetCellIndex(pos);

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

	public Vector2 GetCellCenter(Vector2UInt cellPos) {
		var center = Vector2.Zero;

		center.X = (CellSize.X * cellPos.x) + (CellSize.X * 0.5f);
		center.Y = (CellSize.Y * cellPos.y) + (CellSize.Y * 0.5f);

		return center;
	}

	public Vector2 GetCellCenter(uint x, uint y) {
		var center = Vector2.Zero;

		center.X = (CellSize.X * x) + (CellSize.X * 0.5f);
		center.Y = (CellSize.Y * y) + (CellSize.Y * 0.5f);

		return center;
	}

	public uint GetCellIndex(uint x, uint y) {
		Assert(x < Size.x, "X coordinate is out of grid range. (%)", x);
		Assert(y < Size.y, "Y coordinate is out of grid range. (%)", y);
		return x + y * Size.x;
	}

	public uint GetCellIndex(Vector2UInt pos) {
		Assert(pos.x < Size.x, "X coordinate is out of grid range. (%)", pos.x);
		Assert(pos.y < Size.y, "Y coordinate is out of grid range. (%)", pos.y);
		return pos.x + pos.y * Size.x;
	}

	public Vector2UInt GetCellPos(uint index) {
		var x = index % Size.x;
		var y = index / Size.y;

		return new (x, y);
	}

	public Vector2UInt WorldPointToGridPos(Vector2 point) {
		var x = point.X / CellSize.X;
		var y = point.Y / CellSize.Y;

		x = (float)Math.Floor(x);
		y = (float)Math.Floor(y);

		return new Vector2UInt(x, y);
	}

	public bool IsPointOutsideGridBounds(Vector2 point) {
		var min = Vector2.Zero;
		var max = new Vector2(Size.x * CellSize.X, Size.y * CellSize.Y);

		return (point.X < min.X ||
				point.Y < min.Y ||
				point.X > max.X ||
				point.Y > max.Y);
	}

	public bool CanSwitchWithSelected(Vector2UInt pos) {
		if (SelectedCell < 0) return false;

		var selectedPos = GetCellPos((uint)SelectedCell);

		if (pos == selectedPos) return false;

		int rowDiff = Math.Abs((int)selectedPos.y - (int)pos.y);
	    int colDiff = Math.Abs((int)selectedPos.x - (int)pos.x);

	    return (rowDiff == 1 && colDiff == 0) || 
	           (rowDiff == 0 && colDiff == 1);
	}

}
