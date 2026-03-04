public interface ISerializer {
	void Write<T>(string name, T obj);
}

public interface IDeserializer {
	T Read<T>(string name);
}