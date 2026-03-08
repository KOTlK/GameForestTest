using System.Numerics;

public struct Vector2UInt {
	public uint x;
	public uint y;

	public static readonly Vector2UInt zero = new Vector2UInt(0, 0);

	public Vector2UInt(uint x, uint y) {
		this.x = x;
		this.y = y;
	}

	public Vector2UInt(float x, float y) {
		this.x = (uint)x;
		this.y = (uint)y;
	}

	public Vector2UInt(double x, double y) {
		this.x = (uint)x;
		this.y = (uint)y;
	}

	public static implicit operator Vector2(Vector2UInt v) {
		return new Vector2(v.x, v.y);
	}

	public static implicit operator Vector2Int(Vector2UInt v) {
		return new Vector2Int((int)v.x, (int)v.y);
	}

	public override string ToString() {
		return $"({x}, {y})";
	}

	public static bool operator==(Vector2UInt lhs, Vector2UInt rhs) {
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}

	public static bool operator!=(Vector2UInt lhs, Vector2UInt rhs) {
		return lhs == rhs;
	}

	public static Vector2UInt operator+(Vector2UInt lhs, Vector2UInt rhs) {
		return new Vector2UInt(lhs.x + rhs.x, lhs.y + rhs.y);
	}

	public static Vector2UInt operator-(Vector2UInt lhs, Vector2UInt rhs) {
		return new Vector2UInt(lhs.x - rhs.x, lhs.y - rhs.y);
	}

	public static Vector2Int operator+(Vector2UInt lhs, Vector2Int rhs) {
		return new Vector2Int((int)lhs.x + rhs.x, (int)lhs.y + rhs.y);
	}

	public static Vector2Int operator-(Vector2UInt lhs, Vector2Int rhs) {
		return new Vector2Int((int)lhs.x - rhs.x, (int)lhs.y - rhs.y);
	}
}