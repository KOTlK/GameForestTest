namespace AssetManagement;

public class IntParser : IDataParser {
	public AssetField Parse(Lexer lexer) {
		var field = new AssetField();

		field.Content = ParseRaw(lexer);

		return field;
	}

	public object ParseRaw(Lexer lexer) {
		var negative = false;
		var token = lexer.EatToken();

		if (token.Is('-')) {
			negative = true;
			token = lexer.EatToken();
		}

		if (!token.ExpectToken(TokenType.IntLiteral)) return null;

		var i = (int)token.Value.Integer;

		if (negative) {
			i = -i;
		}

		return i;
	}
}