using System.Collections.Generic;

public struct MatchResult {
	public uint HorizontalHits;
	public uint VerticalHits;

	public static readonly MatchResult Zero = new MatchResult(0, 0);

	public MatchResult(uint h, uint v) {
		HorizontalHits = h;
		VerticalHits   = v;
	}

	public bool MatchThreeOrMore() {
		return HorizontalHits >= 3 ||
			   VerticalHits >= 3;
	}

	public bool CanSpawnLine() {
		if (HorizontalHits >= 4 || VerticalHits >= 4) return true;

		return false;
	}

	public bool CanSpawnBomb(GridSystem grid, Vector2UInt pos) {
		if (HorizontalHits >= 5 || VerticalHits >= 5) return true;
		if (HorizontalHits == 3 && VerticalHits == 3) return true;

		return false;
	}
}