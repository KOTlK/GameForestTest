using System.Numerics;

public static class Mathf {
	public static Vector2 MoveTowards(Vector2 a, Vector2 b, float maxDelta) {
		var dir = b - a;
		var len = dir.Length();

		if (len <= maxDelta) {
			return b;
		}

		dir = dir / len;

		return a + dir * maxDelta;
	}
}