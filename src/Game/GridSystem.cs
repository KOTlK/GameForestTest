using System.Numerics;

using static Assertions;

public struct MatchResult {
	public uint HorizontalHits;
	public uint VerticalHits;

	public MatchResult(uint h, uint v) {
		HorizontalHits = h;
		VerticalHits   = v;
	}

	public static readonly MatchResult Zero = new MatchResult(0, 0);

	public bool MatchThreeOrMore() {
		return HorizontalHits >= 3 ||
			   VerticalHits >= 3;
	}
}

public class GridSystem : GameSystem {
	public Vector2UInt    Size;
	public Vector2        CellSize;
	public EntityHandle[] Elements;
	public int            SelectedCell = -1;

	public bool CellSelected => SelectedCell >= 0;

	private Random        random;
	private EntityManager em;

	private const float switchDuration       = 0.2f;
	private const float failedSwitchDuration = 0.2f;
	private const float fallDuration         = 0.2f;

	private string[] 	  elements = new string[] {
		"green_circle_element",
		"red_circle_element",
		"blue_circle_element",
		"green_rectangle_element",
		"red_rectangle_element",
		// "blue_rectangle_element",
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

	public override void Update() {
		// Match();
		var animation = Game.GetSystem<GridAnimationSystem>();

		if (animation.AllTransactionsOver) {
			Match();
		}
		Fall();
	}

	public void FillRandom() {
		for (uint y = 0; y < Size.y; y++) {
			for (uint x = 0; x < Size.x; x++) {
				var position = GetCellCenter(x, y);
				var element = MakeRandomElement(position);
				PutElement(new Vector2UInt(x, y), element);
			}
		}

		Match();
	}

	public Element MakeRandomElement(Vector2 position) {
		var rand     = (uint)random.NextInt64() % elements.Length;
		var name     = elements[rand];
		var (handle, element)  = em.CreateEntity<Element>(name, position, 0f);

		return element;
	}

	public void Match() {
		for (uint y = 0; y < Size.y; y++) {
			for (uint x = 0; x < Size.x; x++) {
				var position = new Vector2UInt(x, y);
				var match = TryMatchAtPosition(position);

				if (match.MatchThreeOrMore()) {
					Match(position, match);
				}
			}
		}
	}

	public void Fall() {
		var animation = Game.GetSystem<GridAnimationSystem>();

		for (int y = (int)Size.y - 1; y >= 0; y--) {
			for (uint x = 0; x < Size.x; x++) {
				var pos = new Vector2UInt(x, (uint)y);

				if (em.GetEntity(Elements[GetCellIndex(pos)], out Element e)) 
					continue;

				bool fell = false;

				for (var i = y - 1; i >= 0; i--) {
					var top = new Vector2UInt(x, (uint)i);

					if (em.GetEntity(Elements[GetCellIndex(top)], 
						 			 out Element topElement)) {
						RemoveElement(GetCellIndex(top));
						SetElement(GetCellIndex(pos), topElement);
						var transaction = new LinearTransaction(topElement.Handle,
																GetCellCenter(top),
															    GetCellCenter(pos),
															    fallDuration);

						animation.AppendTransaction(transaction);
						fell = true;
						break;
					}
				}

				if (!fell) {
					// Spawn random element above the grid and animate it.
					var randomTargetPos = GetCellCenter(x, (uint)y);
					var randomPos = randomTargetPos - new Vector2(0, CellSize.Y);
					var element = MakeRandomElement(randomPos);

					var transaction = new LinearTransaction(element.Handle,
															randomPos,
															randomTargetPos,
															fallDuration);
					animation.AppendTransaction(transaction);

					SetElement(GetCellIndex(x, (uint)y), element);
				}
			}
		}
	}

	public bool TrySwitchPositions(uint a, uint b) {
		var aHandle = Elements[a];
		var bHandle = Elements[b];
		if (!em.GetEntity(aHandle, out Element first)) return false;
		if (!em.GetEntity(bHandle, out Element second)) return false;

		if (SelectedCell == a || SelectedCell == b) {
			ResetSelection();
		}

		var animSystem = Game.GetSystem<GridAnimationSystem>();
		var aPos       = GetCellPos(a);
		var bPos       = GetCellPos(b);

		RemoveElement(a);
		RemoveElement(b);

		SetElement(a, second);
		SetElement(b, first);

		var aMatch = TryMatchAtPosition(aPos);
		var bMatch = TryMatchAtPosition(bPos);

		if (aMatch.MatchThreeOrMore() ||
			bMatch.MatchThreeOrMore()) {
			var sw = new SwitchTransaction(aHandle, 
									   	   bHandle,
									   	   first.Position,
									   	   second.Position,
									   	   second.Position,
									   	   first.Position,
									   	   switchDuration);

			animSystem.AppendTransaction(sw);
			return true;
		} else {
			RemoveElement(a);
			RemoveElement(b);

			SetElement(a, first);
			SetElement(b, second);
			var sw = new FailedSwitchTransaction(aHandle, 
									   	   	     bHandle,
									   	   	     first.Position,
									   	   	     second.Position,
									   	   	     second.Position,
									   	   	     first.Position,
									   	   	     failedSwitchDuration);
			animSystem.AppendTransaction(sw);
			return false;
		}
	}

	public void Match(Vector2UInt pos, MatchResult match) {
		if (!em.GetEntity(Elements[GetCellIndex(pos)], out Element origin)) 
			return;

		if (match.HorizontalHits >= 3) {
			for (var x = pos.x + 1; x < Size.x; x++) {
				var index = GetCellIndex(x, pos.y);
		        if (!em.GetEntity(Elements[index], out Element e)) 
		        	break;
		        if (e.Shape == origin.Shape && e.Color == origin.Color) {
		        	e.DestroyThisEntity();
		        	RemoveElement(index);
		        }
		        else break;
		    }

		    for (var x = (int)pos.x - 1; x >= 0; x--) {
		    	var index = GetCellIndex((uint)x, pos.y);
		        if (!em.GetEntity(Elements[index], out Element e)) 
		        	break;
		        if (e.Shape == origin.Shape && e.Color == origin.Color) {
		        	e.DestroyThisEntity();
		        	RemoveElement(index);
		        }
		        else break;
		    }
		}

		if (match.VerticalHits >= 3) {
			for (var y = pos.y + 1; y < Size.y; y++) {
				var index = GetCellIndex(pos.x, y);
		        if (!em.GetEntity(Elements[index], out Element e)) 
		        	break;
		        if (e.Shape == origin.Shape && e.Color == origin.Color) {
		        	e.DestroyThisEntity();
		        	RemoveElement(index);
		        }
		        else break;
		    }

		    for (var y = (int)pos.y - 1; y >= 0; y--) {
		    	var index = GetCellIndex(pos.x, (uint)y);
		        if (!em.GetEntity(Elements[index], out Element e)) 
		        	break;
		        if (e.Shape == origin.Shape && e.Color == origin.Color) {
		        	e.DestroyThisEntity();
		        	RemoveElement(index);
		        }
		        else break;
		    }
		}

		origin.DestroyThisEntity();
		RemoveElement(GetCellIndex(pos));
	}

	public MatchResult TryMatchAtPosition(Vector2UInt pos) {
	    if (!em.GetEntity(Elements[GetCellIndex(pos)], out Element origin)) 
	    	return MatchResult.Zero;

	    uint horizontalHits = 1;
	    uint verticalHits   = 1;

	    for (var x = pos.x + 1; x < Size.x; x++) {
	        if (!em.GetEntity(Elements[GetCellIndex(x, pos.y)], out Element e)) 
	        	break;
	        if (e.Shape == origin.Shape && e.Color == origin.Color) 
	        	horizontalHits++;
	        else break;
	    }

	    for (var x = (int)pos.x - 1; x >= 0; x--) {
	        if (!em.GetEntity(Elements[GetCellIndex((uint)x, pos.y)], out Element e)) 
	        	break;
	        if (e.Shape == origin.Shape && e.Color == origin.Color) 
	        	horizontalHits++;
	        else break;
	    }

	    for (var y = pos.y + 1; y < Size.y; y++) {
	        if (!em.GetEntity(Elements[GetCellIndex(pos.x, y)], out Element e)) 
	        	break;
	        if (e.Shape == origin.Shape && e.Color == origin.Color) 
	        	verticalHits++;
	        else break;
	    }

	    for (var y = (int)pos.y - 1; y >= 0; y--) {
	        if (!em.GetEntity(Elements[GetCellIndex(pos.x, (uint)y)], out Element e)) 
	        	break;
	        if (e.Shape == origin.Shape && e.Color == origin.Color) 
	        	verticalHits++;
	        else break;
	    }

	    return new MatchResult(horizontalHits, verticalHits);
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
		var y = index / Size.x;

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
