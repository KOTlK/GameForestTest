using System;
using System.Reflection;
using System.Collections.Generic;
using Raylib_cs;

using static Assertions;

namespace AssetManagement;

public class GenericParser<T> : IDataParser 
where T : new() {
	private static Dictionary<Type, FieldInfo[]> fieldsByType = new();

	public AssetField Parse(Lexer lexer) {
		var field  = new AssetField();

		field.Content = ParseRaw(lexer);

		return field;
	}

	public object ParseRaw(Lexer lexer) {
		var token  = lexer.EatToken();
		var obj    = new T();
		var fields = GetFields(typeof(T));

		while (true) {
			if (!token.ExpectToken(TokenType.Ident)) break;

			var fieldName = token.Value.Str;

			token = lexer.EatToken();

			if (!token.ExpectToken(':')) return null;

			token = lexer.EatToken();

			var objField = GetField(fieldName, fields);

			Assert(objField != null, "Cannot parse field %.", fieldName);

			if (token.Is(TokenType.String)) {
				var str      = token.Value.Str;

				var reference = __makeref(obj);
				objField.SetValueDirect(reference, str);
			} else {
				if (!token.ExpectToken(TokenType.Ident)) return null;

				var typeName = token.Value.Str;

				token = lexer.EatToken();

				if (!token.ExpectToken('(')) return null;

				var parser = Parsers.GetParser(typeName);
				var o      = parser.ParseRaw(lexer);

				var reference = __makeref(obj);

				objField.SetValueDirect(reference, o);

				token = lexer.EatToken();

				if (!token.ExpectToken(')')) return null;
			}

			var next = lexer.PeekToken();

			if (next.Is(')')) break;

			token = lexer.EatToken();
		}

		return obj;
	}

	private static FieldInfo GetField(string name, FieldInfo[] fields) {
		foreach (var field in fields) {
			if (field.Name == name) {
				return field;
			}
		}

		return null;
	}

	private static FieldInfo[] GetFields(Type type) {
		if (fieldsByType.ContainsKey(type)) {
			return fieldsByType[type];
		}

		var fields = type.GetFields();

		fieldsByType.Add(type, fields);

		return fields;
	}
}