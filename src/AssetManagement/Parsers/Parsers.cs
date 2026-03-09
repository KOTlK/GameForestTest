using System.Collections.Generic;

using static Assertions;

namespace AssetManagement;

public static class Parsers {
	private static Dictionary<string, IDataParser> parsers = new() {
		{ "int", new IntParser() },
		{ "uint", new UIntParser() },
		{ "float", new FloatParser() },
		{ "Vector2", new Vector2Parser() },
		{ "EntityType", new EnumParser<EntityType>() },
		{ "EntityFlags", new EnumParser<EntityFlags>() },
		{ "ElementColor", new EnumParser<ElementColor>() },
		{ "ElementShape", new EnumParser<ElementShape>() },
		{ "ShapeType", new EnumParser<ShapeType>() },
		{ "Color", new ColorParser() },
		{ "Renderer2D", new GenericParser<Renderer>() },
	};

	public static bool HasParser(string name) {
		return parsers.ContainsKey(name);
	}

	public static IDataParser GetParser(string name) {
		Assert(parsers.ContainsKey(name), "Cannot get parser for %.", name);
		return parsers[name];
	}
}