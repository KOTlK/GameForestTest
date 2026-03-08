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
        float pingPong = 1f - MathF.Abs((t % 1f) * 2f - 1f);
    	return Vector2.Lerp(start, end, pingPong);

    }

    public static float Radians(float degrees) {
    	 return degrees * 0.01745329251f;
    }

    public static float Degrees(float radians) {
    	return radians * 57.29577951308f;
    }

    public static int Clamp(int val, int min, int max) {
    	if (val < min) 		val = min;
    	else if (val > max) val = max;

    	return val;
    }
}