using Raylib_cs;

namespace AssetManagement;

public class ColorParser : IDataParser {
	public AssetField Parse(Lexer lexer) {
		var field = new AssetField();

		field.Content = ParseRaw(lexer);

		return field;
	}

	public object ParseRaw(Lexer lexer) {
		var token = lexer.EatToken();

		if (!token.ExpectToken(TokenType.Ident)) return null;

		var name = token.Value.Str;

		return name switch {
			"Red"   	 => Color.Red,
			"Green" 	 => Color.Green,
			"Blue"  	 => Color.Blue,
			"Beige"      => Color.Beige,
			"Black"      => Color.Black,
			"Brown"      => Color.Brown,
			"DarkBlue"   => Color.DarkBlue,
			"DarkBrown"  => Color.DarkBrown,
			"DarkGray"   => Color.DarkGray,
			"DarkGreen"  => Color.DarkGreen,
			"DarkPurple" => Color.DarkPurple,
			"Gold" 		 => Color.Gold,
			"Gray" 		 => Color.Gray,
			"LightGray"  => Color.LightGray,
			"Lime"       => Color.Lime,
			"Magenta"    => Color.Magenta,
			"Maroon"     => Color.Maroon,
			"Orange"     => Color.Orange,
			"Pink"       => Color.Pink,
			"Purple"     => Color.Purple,
			"RayWhite"   => Color.RayWhite,
			"SkyBlue" 	 => Color.SkyBlue,
			"Violet"  	 => Color.Violet,
			"White"   	 => Color.White,
			"Yellow"  	 => Color.Yellow,
		};
	}
}