namespace AssetManagement;

public class FloatParser : IDataParser {
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

		var i = (float)token.Value.FPoint;

		if (negative) {
			i = -i;
		}

		return i;
	}
}