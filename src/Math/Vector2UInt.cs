using System.Numerics;

public struct Vector2UInt {
	public uint x;
	public uint y;

	public Vector2UInt(uint x, uint y) {
		this.x = x;
		this.y = y;
	}

	public static implicit operator Vector2(Vector2UInt v) {
		return new Vector2(v.x, v.y);
	}

	public override string ToString() {
		return $"({x}, {y})";
	}
}