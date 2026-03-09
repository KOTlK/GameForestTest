using static Assertions;

public static class LexerExtensions {
	public static bool ExpectToken(this Token token, TokenType type) {
		if (token.Type == (ushort)type) {
			return true;
		}

		Assert(false, "Unexpected token at line %:%. Expected %, got %.",
					  token.Line,
					  token.Column,
					  type,
					  (TokenType)token.Type);

		return false;
	}

	public static bool ExpectToken(this Token token, char type) {
		if (token.Type == type) {
			return true;
		}

		Assert(false, "Unexpected token at line %:%. Expected %, got %.",
					  token.Line,
					  token.Column,
					  type,
					  (TokenType)token.Type);

		return false;
	}

	public static bool Is(this Token token, char type) {
		return token.Type == type;
	}

	public static bool Is(this Token token, TokenType type) {
		return token.Type == (ushort)type;
	}
}