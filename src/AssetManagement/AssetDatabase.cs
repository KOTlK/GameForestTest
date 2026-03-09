using System.Collections.Generic;
using System.Numerics;
using System.IO;
using Raylib_cs;

using static Assertions;

namespace AssetManagement;

public enum AssetType {
	None = 0,
	Entity = 1,
	Texture = 2,
}

public struct AssetEntry : IDeserializer {
	public string 			Name;
	public List<AssetField> Fields;
	public AssetType 		Type;

	public T Read<T>(string name) {
		foreach (var field in Fields) {
			if (field.Name == name) {
				return (T)field.Content;
			}
		}

		Assert(false, "Cannot read value for %.", name);
		return default(T);
	}
}

public class AssetDatabase : ISerializer, IDeserializer {
	private Dictionary<string, Texture2D> loadedTextures = new();

	private List<Asset> 			newAssets = new();
	private Dictionary<string, int> assetByName = new();

	public void LoadFromFile(string path) {
		var text  = File.ReadAllText(path);
		var lexer = new Lexer(text);

		var token = lexer.EatToken();

		while (token.Type != (ushort)TokenType.EndOfFile) {
			if (!token.ExpectToken(TokenType.Ident)) return;

			if (token.Value.Str == "Entity") {
				var asset = ParseEntity(lexer);
				var id    = newAssets.Count;
				newAssets.Add(asset);
				assetByName.Add(asset.Name, id);
			} else if (token.Value.Str == "Texture") {
				token = lexer.EatToken();

				if (!token.ExpectToken('(')) return;

				token = lexer.EatToken();

				if (!token.ExpectToken(TokenType.String)) return;

				var asset = new TextureAsset();
				asset.Type = AssetType.Texture;

				asset.Path = token.Value.Str;
				asset.Name = token.Value.Str;

				var count = newAssets.Count;
				newAssets.Add(asset);
				assetByName.Add(asset.Path, count);

				token = lexer.EatToken();

				if (!token.ExpectToken(')')) return;
			} else {
				Assert(false, "Cannot parse assets. Can't guess the asset type");
			}

			token = lexer.EatToken();
		}
	}

	public void Write<T>(string name, T obj) {
		// @TODO: implement this if need editor
	}

	public void ReadEntity(string name, Entity entity) {
		Assert(assetByName.ContainsKey(name), "Cannot load entity with name %.", name);
		var entry = newAssets[assetByName[name]];

		Assert(entry is EntityAsset, "AssetType is incompatible (%).", 
									 entry.GetType().FullName);

		var ent = entry as EntityAsset;

		entity.Deserialize(ent);
	}

	public T Read<T>(string name) {
		Assert(assetByName.ContainsKey(name), "Cannot load entry with name %.", name);

		var entry = newAssets[assetByName[name]];

		switch(entry.Type) {
			case AssetType.Texture : {
				Assert(typeof(T) == typeof(Texture2D), 
				       "Trying to load texture, but provided % as generic type.", 
						typeof(T).FullName);

				var texture = entry as TextureAsset;

				if (loadedTextures.ContainsKey(name)) {
					return (T)(object)loadedTextures[name];
				} else {
					var tex = Raylib.LoadTexture(texture.Path);
					loadedTextures[name] = tex;

					return (T)(object)tex;
				}
			} break;
		}

		return default(T);
	}

	private Asset ParseEntity(Lexer lexer) {
		var asset = new EntityAsset();
		asset.Type = AssetType.Entity;

		// It expects lexer.Current to be ident with Entity keyword.
		var token = lexer.EatToken();

		if (!token.ExpectToken('(')) return asset;

		token = lexer.EatToken();

		if (!token.ExpectToken(TokenType.Ident)) return asset;

		var name = token.Value.Str;
		asset.Name = name;

		token = lexer.EatToken();

		if (!token.ExpectToken(')')) return asset;

		token = lexer.EatToken();

		if (!token.ExpectToken(':')) return asset;

		token = lexer.EatToken();

		if (!token.ExpectToken(TokenType.Ident)) return asset;

		while (true) {
			if (token.Type == (ushort)TokenType.EndOfFile) break;

			var next = lexer.PeekToken();
			if (next.Type != ':') break;

			var fieldName = token.Value.Str;

			lexer.EatToken();

			// Type
			token = lexer.EatToken();

			AssetField content = null;

			if (token.Is(TokenType.String)) {
				content = new AssetField();

				content.Content = token.Value.Str;
			} else {
				var type   = token.Value.Str;
				var parser = Parsers.GetParser(type);
				token = lexer.EatToken();

				if (!token.ExpectToken('(')) return null;
				content    = parser.Parse(lexer);

				token = lexer.EatToken();
				if (!token.ExpectToken(')')) return null;
			}

			content.Name = fieldName;
			asset.AddField(content);

			next = lexer.PeekToken();

			if (next.Is(TokenType.Ident)) {
				next = lexer.PeekToken(2);

				if (next.Is('(')) 
					break;
			}

			token = lexer.EatToken();
		}

		return asset;
	}
}
