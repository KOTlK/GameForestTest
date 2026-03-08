using System.Numerics;

public struct Vector2Int {
	public int x;
	public int y;

	public static readonly Vector2Int zero = new Vector2Int(0, 0);

	public Vector2Int(int x, int y) {
		this.x = x;
		this.y = y;
	}

	public Vector2Int(float x, float y) {
		this.x = (int)x;
		this.y = (int)y;
	}

	public Vector2Int(double x, double y) {
		this.x = (int)x;
		this.y = (int)y;
	}

	public static implicit operator Vector2(Vector2Int v) {
		return new Vector2(v.x, v.y);
	}

	public static implicit operator Vector2UInt(Vector2Int v) {
		return new Vector2UInt((uint)v.x, (uint)v.y);
	}

	public override string ToString() {
		return $"({x}, {y})";
	}

	public static bool operator==(Vector2Int lhs, Vector2Int rhs) {
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}

	public static bool operator!=(Vector2Int lhs, Vector2Int rhs) {
		return lhs == rhs;
	}

	public static Vector2Int operator+(Vector2Int lhs, Vector2Int rhs) {
		return new Vector2Int(lhs.x + rhs.x, lhs.y + rhs.y);
	}

	public static Vector2Int operator-(Vector2Int lhs, Vector2Int rhs) {
		return new Vector2Int(lhs.x - rhs.x, lhs.y - rhs.y);
	}

	public static Vector2Int operator+(Vector2Int lhs, Vector2UInt rhs) {
		return new Vector2Int(lhs.x + rhs.x, lhs.y + rhs.y);
	}

	public static Vector2Int operator-(Vector2Int lhs, Vector2UInt rhs) {
		return new Vector2Int(lhs.x - rhs.x, lhs.y - rhs.y);
	}
}