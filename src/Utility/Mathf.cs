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

	public static Vector2 PingPong(Vector2 start, Vector2 end, float t) {
        t = Math.Max(0f, Math.Min(1f, t));
        float pingPong = MathF.Abs(t * 2f - 1f);

        return Vector2.Lerp(start, end, pingPong);
    }
}