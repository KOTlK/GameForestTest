using System.Numerics;

namespace AssetManagement;

public class Vector2Parser : IDataParser {
	public AssetField Parse(Lexer lexer) {
		var field = new AssetField();

		field.Content = ParseRaw(lexer);

		return field;
	}

	public object ParseRaw(Lexer lexer) {
		var token = lexer.EatToken();

		var negative = false;

		if (token.Is('-')) {
			negative = true;
			token = lexer.EatToken();
		}

		if (!token.ExpectToken(TokenType.FloatLiteral)) return null;

		var x = (float)token.Value.FPoint;

		if (negative) {
			x = -x;
		}

		token = lexer.EatToken();

		if (!token.ExpectToken(',')) return null;

		token = lexer.EatToken();

		negative = false;

		if (token.Is('-')) {
			negative = true;
			token = lexer.EatToken();
		}

		if (!token.ExpectToken(TokenType.FloatLiteral)) return null;

		var y = (float)token.Value.FPoint;

		if (negative) {
			y = -y;
		}

		return new Vector2(x, y);
	}
}