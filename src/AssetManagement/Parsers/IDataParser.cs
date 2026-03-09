namespace AssetManagement;

public interface IDataParser {
	AssetField Parse(Lexer lexer);
	object     ParseRaw(Lexer lexer);
}