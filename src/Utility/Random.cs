using System;

namespace GameMath;

public static class Random {
	private static System.Random random = new();

	public static bool NextBool() {
		var i = random.Next();

		return (i & 1) == 0;
	}
}