using System;

using static Assertions;

namespace AssetManagement;

public class EnumParser<T> : IDataParser 
where T : struct, Enum {
	public AssetField Parse(Lexer lexer) {
		var field = new AssetField();

		field.Content = ParseRaw(lexer);

		return field;
	}

	public object ParseRaw(Lexer lexer) {
		var token = lexer.EatToken();

		if (!token.ExpectToken(TokenType.Ident)) return null;

		var name = token.Value.Str;
		var parsed = Enum.TryParse<T>(name, true, out var res);

        Assert(parsed, 
			  "Cannot parse enum value. Provided value: %",
			  name);

		return res;
	}
}