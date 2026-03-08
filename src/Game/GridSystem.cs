using System.Numerics;

using static Assertions;

public class GridSystem : GameSystem {
	public Vector2UInt    Size;
	public Vector2        CellSize;
	public EntityHandle[] Elements;
	public bool[] 		  Moved;
	public int            SelectedCell = -1;

	public bool CellSelected => SelectedCell >= 0;

	private Random        random;
	private EntityManager em;

	private const float switchDuration       = 0.2f;
	private const float failedSwitchDuration = 0.2f;
	private const float fallDuration         = 0.2f;
	private const float destroyerDuration    = 0.05f;

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
		Cleanup();

		var count = size.x * size.y;
		Size     = size;
		CellSize = cellSize;
		Elements = new EntityHandle[count];
		Moved    = new bool[count];

		for (var i = 0; i < count; i++) {
			Elements[i] = EntityHandle.Zero;
		}
	}

	public override void OnDisable() {
		Cleanup();
	}

	public override void OnEnable() {
		Cleanup();
		MakeNewGrid(Size, CellSize);
	}

	public override void Update() {
		var animation = Game.GetSystem<GridAnimationSystem>();

		if (animation.AllTransactionsOver) {
			Match();
		}

		if (animation.AllTransactionsOver) {
			Fall();
		}
	}

	public void FillRandom() {
		for (uint y = 0; y < Size.y; y++) {
			for (uint x = 0; x < Size.x; x++) {
				var position = GetCellCenter(x, y);
				var element = MakeRandomElement(position);
				PutElement(new Vector2UInt(x, y), element);
			}
		}
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

				// if (Moved[GetCellIndex(position)]) continue;
				var match = TryMatchAtPosition(position);

				if (match.MatchThreeOrMore()) {
					Match(position, match);
				}
			}
		}

		for (var i = 0; i < Moved.Length; i++) {
			Moved[i] = false;
		}
	}

	public void Fall() {
		var animation = Game.GetSystem<GridAnimationSystem>();

		// 0:0 is top left of the grid, so Size.y is at the bottom.
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
						var topIndex  = GetCellIndex(top);
						var currIndex = GetCellIndex(pos);
						RemoveElement(topIndex);
						SetElement(currIndex, topElement);
						var transaction = new LinearTransaction(topElement.Handle,
																GetCellCenter(top),
															    GetCellCenter(pos),
															    fallDuration);

						animation.AppendTransaction(transaction);
						fell = true;
						Moved[topIndex] = true;
						Moved[currIndex] = true;
						break;
					}
				}

				if (!fell) {
					// Spawn random element above the grid and animate it.
					var randomTargetPos = GetCellCenter(x, (uint)y);
					var randomPos = randomTargetPos - new Vector2(0, CellSize.Y);
					var element   = MakeRandomElement(randomPos);

					var transaction = new LinearTransaction(element.Handle,
															randomPos,
															randomTargetPos,
															fallDuration);
					animation.AppendTransaction(transaction);
					var index = GetCellIndex(x, (uint)y);
					SetElement(index, element);
					Moved[index] = true;
				}
			}
		}
	}

	public void DestroyElement(uint index) {
		var handle = Elements[index];
		if (!em.GetEntity(handle, out Element element)) return;

		RemoveElement(index);

		if (IsBonus(element.Type)) {
			var bonus = (Bonus)element;
			bonus.Activate(this);
		} else {
			Services<ScoreSystem>.Get().Add(element.Score);
		}

		if (element.Type == EntityType.Bomb) return;

		element.DestroyThisEntity();
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
			Moved[a] = true;
			Moved[b] = true;
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

		var originIndex = GetCellIndex(pos);
		var spawnBomb   = match.CanSpawnBomb(this, pos);
		var spawnLine   = !spawnBomb && match.CanSpawnLine();

		var lineCellPos = pos;
		var bombCellPos = pos;
		var lastFound   = spawnLine && Moved[originIndex];

		if (match.HorizontalHits >= 3) {
			DestroyInDirection(pos, origin, match.HorizontalHits,  1,  0, ref lastFound, ref lineCellPos, ref bombCellPos, spawnBomb, spawnLine);
			DestroyInDirection(pos, origin, match.HorizontalHits, -1,  0, ref lastFound, ref lineCellPos, ref bombCellPos, spawnBomb, spawnLine);
		}

		if (match.VerticalHits >= 3) {
			DestroyInDirection(pos, origin, match.VerticalHits,    0,  1, ref lastFound, ref lineCellPos, ref bombCellPos, spawnBomb, spawnLine);
			DestroyInDirection(pos, origin, match.VerticalHits,    0, -1, ref lastFound, ref lineCellPos, ref bombCellPos, spawnBomb, spawnLine);
		}

		DestroyElement(originIndex);

		if (spawnBomb) {
			var (_, bomb) = em.CreateEntity<Bomb>("bomb", GetCellCenter(bombCellPos), 0);
			bomb.SetColor(origin.Color);
			PutElement(bombCellPos, bomb);
		} else if (spawnLine) {
			var (_, line) = em.CreateEntity<Line>("line", GetCellCenter(lineCellPos), 0);
			line.SetColor(origin.Color);
			PutElement(lineCellPos, line);
		}
	}

	private void DestroyInDirection(Vector2UInt pos, 
									Element origin, 
									uint hits,
									int dx, 
									int dy,
									ref bool        lastFound,
									ref Vector2UInt lineCellPos,
									ref Vector2UInt bombCellPos,
									bool spawnBomb, 
									bool spawnLine) {
		int  x = (int)pos.x + dx;
		int  y = (int)pos.y + dy;

		while (x >= 0 && 
			   y >= 0 && 
			   x < Size.x && 
			   y < Size.y) {
			var index = GetCellIndex((uint)x, (uint)y);
			if (!em.GetEntity(Elements[index], out Element e)) break;

			if (!CanMatch(e, origin)) {
				if (!MatchColor(origin, e)) break;

				int nx = x + dx; 
				int ny = y + dy;
				if (nx < 0 || ny < 0 || nx >= Size.x || ny >= Size.y) break;
				if (!em.GetEntity(Elements[GetCellIndex((uint)nx, (uint)ny)], out Element next)) break;
				if (!IsBonus(next.Type) || !MatchColor(origin, next)) break;
			}

			if (hits >= 4 && Moved[index] && !lastFound) {
				lastFound   = true;
				lineCellPos = new Vector2UInt(x, y);
			}

			if (spawnBomb && Moved[index]) {
				bombCellPos = new Vector2UInt(x, y);
			}

			DestroyElement(index);
			x += dx;
			y += dy;
		}
	}

	public MatchResult TryMatchAtPosition(Vector2UInt pos) {
	    if (!em.GetEntity(Elements[GetCellIndex(pos)], out Element origin)) 
	    	return MatchResult.Zero;

	    uint horizontalHits = 1 + CountHits(pos, origin,  1,  0)
	                            + CountHits(pos, origin, -1,  0);
	    uint verticalHits   = 1 + CountHits(pos, origin,  0,  1)
	                            + CountHits(pos, origin,  0, -1);

	    return new MatchResult(horizontalHits, verticalHits);
	}

	public uint CountHits(Vector2UInt pos, Element origin, int dx, int dy) {
		uint hits = 0;
		int  x    = (int)pos.x + dx;
		int  y    = (int)pos.y + dy;

		while (x >= 0 && 
			   y >= 0 && 
			   x < Size.x && 
			   y < Size.y) {
			var index = GetCellIndex((uint)x, (uint)y);
			if (!em.GetEntity(Elements[index], out Element e)) break;

			if (CanMatch(origin, e)) {
				hits++;
			} else if (MatchColor(origin, e)) {
				int nx = x + dx;
				int ny = y + dy;
				if (nx < 0 || 
					ny < 0 || 
					nx >= Size.x || 
					ny >= Size.y) break;

				if (!em.GetEntity(Elements[GetCellIndex((uint)nx, (uint)ny)], out Element next)) 
					break;

				if (!IsBonus(next.Type) || !MatchColor(origin, next)) 
					break;

				hits++;
			} else {
				break;
			}

			x += dx;
			y += dy;
		}

		return hits;
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
		if (Elements == null) return;
		
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

	private Element GetLastMovedInHorizontalLine(Vector2UInt pos) {
		var originIndex = GetCellIndex(pos);
		if (!em.GetEntity(Elements[GetCellIndex(pos)], out Element origin))
			return null;

		if (Moved[originIndex]) return origin;

		for (var x = pos.x + 1; x < Size.x; x++) {
			var index = GetCellIndex(x, pos.y);
	        if (!em.GetEntity(Elements[index], out Element e)) 
	        	break;
	        if (e.Shape == origin.Shape && e.Color == origin.Color) {
	        	if (Moved[index]) {
	        		return e;
	        	}
	        }
	        else break;
	    }

	    for (var x = (int)pos.x - 1; x >= 0; x--) {
	    	var index = GetCellIndex((uint)x, pos.y);
	        if (!em.GetEntity(Elements[index], out Element e)) 
	        	break;
	        if (e.Shape == origin.Shape && e.Color == origin.Color) {
	        	if (Moved[index]) {
	        		return e;
	        	}
	        }
	        else break;
	    }

	    return null;
	}

	public void ExplodeBomb(Vector2UInt pos, int radius) {
		int ymin = Mathf.Clamp((int)pos.y - radius, 0, (int)Size.y - 1);
		int ymax = Mathf.Clamp((int)pos.y + radius, 0, (int)Size.y - 1);
		int xmin = Mathf.Clamp((int)pos.x - radius, 0, (int)Size.x - 1);
		int xmax = Mathf.Clamp((int)pos.x + radius, 0, (int)Size.x - 1);

		for (var y = ymin; y <= ymax; y++) {
			for (var x = xmin; x <= xmax; x++) {
				var p = new Vector2UInt(x, y);

				DestroyElement(GetCellIndex(p));
			}
		}
	}

	public void CreateDestroyerPair(Vector2UInt pos, LineDirection dir) {
		if (dir == LineDirection.Horizontal) {
			if (pos.x > 0) {
				CreateDestroyer(pos, new Vector2Int(-1, 0));
			}
			if (pos.x < Size.x - 1) {
				CreateDestroyer(pos, new Vector2Int(1, 0));
			}
		} else {
			if (pos.y > 0) {
				CreateDestroyer(pos, new Vector2Int(0, -1));
			}
			if (pos.y < Size.y - 1) {
				CreateDestroyer(pos, new Vector2Int(0, 1));
			}
		}
	}

	private void CreateDestroyer(Vector2UInt pos, Vector2Int dir) {
		var (handle, destroyer) = em.CreateEntity<Destroyer>("destroyer",
															 GetCellCenter(pos),
															 0f);


		var trans = new DestroyerTransaction(this,
											 handle,
											 pos,
											 dir,
											 destroyerDuration);
		var anim = Game.GetSystem<GridAnimationSystem>();

		anim.AppendTransaction(trans);
	}

	private bool IsBonus(EntityType type) {
		return type == EntityType.Line ||
			   type == EntityType.Bomb;
	}

	private bool CanMatch(Element a, Element b) {
		if (!MatchColor(a, b)) return false;

		if (IsBonus(a.Type) ||
			IsBonus(b.Type)) {
			return true;
		}

		return a.Shape == b.Shape && a.Color == b.Color;
	}

	private bool MatchColor(Element a, Element b) {
		return a.Color == b.Color;
	}
}
