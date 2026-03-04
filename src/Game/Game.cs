using static Assertions;

public class Game {
	private List<GameSystem> 			 _systems = new();
	private Dictionary<Type, GameSystem> _systemByType = new();

	public void Update() {
		foreach(var system in _systems) {
			if (system.Enabled == false) continue;

			system.Update();
		}
	}

	public void Destroy() {
		foreach(var system in _systems) {
			system.Destroy();
		}
	}

	public void AppendSystem(GameSystem system) {
		_systems.Add(system);
		var type = system.GetType();

		_systemByType.Add(type, system);
	}

	public T GetSystem<T>() 
	where T : GameSystem {
		var type = typeof(T);

		Assert(_systemByType.ContainsKey(type), "Cannot get system. It is not registered.");

		return (T)_systemByType[type];
	}

	public void EnableSystem<T>() 
	where T : GameSystem {
		var type = typeof(T);
		Assert(_systemByType.ContainsKey(type), "Cannot enable system. It is not registered.");
		var system = _systemByType[type];
		Assert(system.Enabled == false, "Cannot enable system. It is already enabled.");
		system.OnEnable();
		system.Enabled = true;
	}

	public void DisableSystem<T>() 
	where T : GameSystem {
		var type = typeof(T);
		Assert(_systemByType.ContainsKey(type), "Cannot disable system. It is not registered.");
		var system = _systemByType[type];
		Assert(system.Enabled == true, "Cannot disable system. It is already disabled.");
		system.OnDisable();
		system.Enabled = false;
	}
}