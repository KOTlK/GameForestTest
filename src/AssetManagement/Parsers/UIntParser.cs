namespace AssetManagement;

public class UIntParser : IDataParser {
	public AssetField Parse(Lexer lexer) {
		var field = new AssetField();

		field.Content = ParseRaw(lexer);

		return field;
	}

	public object ParseRaw(Lexer lexer) {
		var token = lexer.EatToken();

		if (!token.ExpectToken(TokenType.IntLiteral)) return null;

		return (uint)token.Value.Integer;
	}
}