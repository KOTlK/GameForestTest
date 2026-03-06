using System.Runtime.CompilerServices;
using System.Numerics;

using static Assertions;

[System.Serializable]
public struct EntityHandle {
    public uint Id;
    public uint Tag;

    public static readonly EntityHandle Zero = new EntityHandle { Id = 0, Tag = 0};

    public static bool operator==(EntityHandle lhs, EntityHandle rhs) {
        return lhs.Id == rhs.Id && lhs.Tag == rhs.Tag;
    }

    public static bool operator!=(EntityHandle lhs, EntityHandle rhs) {
        return !(lhs == rhs);
    }

    public override bool Equals(object obj) {
        return (EntityHandle)obj == this;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Id, Tag);
    }
}

public class EntityManager {
    public struct FlagsChange {
        public EntityFlags Old;
        public EntityFlags New;
        public Entity      Entity; // don't need handle here
    }

    public event Action<FlagsChange> EntityFlagsChanged = delegate {};

    public Dictionary<EntityType, IntrusiveList> EntitiesByType = new();
    public List<Entity>  BakedEntities = new();
    public Entity[]      Entities = new Entity[InitialCapacity];
    public EntityType[]  Types = new EntityType[InitialCapacity];
    public EntityFlags[] Flags = new EntityFlags[InitialCapacity];
    public uint[]        Tags = new uint[InitialCapacity];
    public bool[]        Free = new bool[InitialCapacity];
    public uint[]        NextFree = new uint[InitialCapacity];
    public uint[]        NextDynamic = new uint[InitialCapacity];
    public uint[]        PrevDynamic = new uint[InitialCapacity];

    public uint MaxEntitiesCount = 1;
    public uint FirstFree;
    public uint FirstDynamic;

    private const uint InitialCapacity = 1024;
    private const uint ResizeStep      = 1024;

    public EntityManager() {
        BakedEntities.Clear();
        EntitiesByType.Clear();
        MaxEntitiesCount = 1;
        FirstFree = 0;
        FirstDynamic = 0;
        var entityTypes = Enum.GetValues(typeof(EntityType));

        foreach(var type in entityTypes) {
            EntitiesByType.Add((EntityType)type, new IntrusiveList(InitialCapacity));
        }

        EntityFlagsChanged += CheckDynamicOnFlagsChange;

        for (var i = 0; i < InitialCapacity; i++) {
            Free[i] = true;
        }
    }

    public (EntityHandle, T) CreateEntity<T>(string  prefab,
                                             Vector2 position,
                                             float   orientation)
    where T : Entity, new() {
        uint id = GetNextFree();

        uint tag = Tags[id];

        var handle = new EntityHandle {
            Id = id,
            Tag = tag
        };

        var obj = new T();

        obj.Position    = position;
        obj.Orientation = orientation;
        obj.Scale       = Vector2.One;
        obj.Em          = this;
        obj.Handle      = handle;

        AssetManager.LoadEntity(prefab, obj);

        if(MaxEntitiesCount == Entities.Length) {
            Resize(MaxEntitiesCount << 1);
        }

        Entities[id] = obj;
        Free[id]     = false;
        Types[id]    = obj.Type;
        Tags[id]     = tag;
        Flags[id]    = obj.Flags;


        EntitiesByType[obj.Type].Add(handle.Id);

        if((obj.Flags & EntityFlags.Dynamic) == EntityFlags.Dynamic) {
            AddDynamic(id);
            obj.OnBecameDynamic();
        }

        obj.OnCreate();

        return (handle, obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DestroyEntity(EntityHandle handle, bool calledFromEcs = false) {
        if (IsValid(handle)) {
            Free[handle.Id] = true;

            var id = handle.Id;


            if ((Flags[id] & EntityFlags.Dynamic) == EntityFlags.Dynamic) {
                RemoveDynamic(id);
            }

            var entity = Entities[handle.Id];

            Assert(entity != null, "Cannot destroy null entity (%)", id);

            var renderSystem = Services<RenderSystem>.Get();
            renderSystem.RemoveRenderer(entity.Renderer);

            EntitiesByType[entity.Type].Remove(handle.Id);
            Entities[id] = null;
            entity.Destroy();
            NextFree[id] = FirstFree;
            FirstFree = id;
            Tags[id]++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DestroyAllEntities() {
        for (uint i = 0; i < Entities.Length; i++) {
            if (Free[i] == false) {
                DestroyEntity(GetHandle(i));
            }

            Types[i] = EntityType.None;
            Flags[i] = EntityFlags.None;
            Tags[i]  = 0;
            Free[i]        = true;
            NextFree[i]    = 0;
            NextDynamic[i] = 0;
            PrevDynamic[i] = 0;
        }

        MaxEntitiesCount = 1;
        FirstDynamic     = 0;
        FirstFree        = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update() {
        var next = FirstDynamic;

        while(next > 0) {
            var current = next;
            next = NextDynamic[next];
            Entities[current].UpdateEntity();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetEntity(EntityHandle handle, out Entity e) {
        if(IsValid(handle)) {
            e = Entities[handle.Id];
            return true;
        } else {
            e = null;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetEntity<T>(EntityHandle handle, out T e)
    where T : Entity {
        if(IsValid(handle)) {
            e = (T)Entities[handle.Id];
            return true;
        } else {
            e = null;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityHandle GetHandle(uint id) {
        return new EntityHandle {
            Id = id,
            Tag = Tags[id]
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAlive(EntityHandle handle) {
        return IsValid(handle) && Free[handle.Id] == false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValid(EntityHandle handle) {
        return handle != EntityHandle.Zero && handle.Tag == Tags[handle.Id];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<Entity> GetAllEntitiesWithType(EntityType type) {
        var list = EntitiesByType[type];

        var next = list.First;

        while (next > 0) {
            var nxt = list.Next[next];
            yield return Entities[next];
            next = nxt;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<T> GetAllEntitiesWithType<T>(EntityType type)
    where T : Entity {
        var list = EntitiesByType[type];

        var next = list.First;

        while (next > 0) {
            var nxt = list.Next[next];
            yield return (T)Entities[next];
            next = nxt;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetFlags(EntityHandle handle, EntityFlags flags) {
        if (!GetEntity(handle, out var entity)) return;

        var change = new FlagsChange();

        change.Old       = Flags[handle.Id];
        change.New       = flags;
        change.Entity    = entity;
        entity.Flags     = flags;
        Flags[handle.Id] = flags;

        EntityFlagsChanged(change);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetFlag(EntityHandle handle, EntityFlags flag) {
        if (!GetEntity(handle, out var entity)) return;

        if ((Flags[handle.Id] & flag) != flag) {
            var change        = new FlagsChange();
            change.Old        = Flags[handle.Id];
            entity.Flags     |= flag;
            Flags[handle.Id] |= flag;
            change.New        = Flags[handle.Id];
            change.Entity     = entity;

            EntityFlagsChanged(change);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearFlag(EntityHandle handle, EntityFlags flag) {
        if (!GetEntity(handle, out var entity)) return;

        if ((Flags[handle.Id] & flag) == flag) {
            var change        = new FlagsChange();
            change.Old        = Flags[handle.Id];
            entity.Flags     &= ~flag;
            Flags[handle.Id] &= ~flag;
            change.New        = Flags[handle.Id];
            change.Entity     = entity;

            EntityFlagsChanged(change);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ToggleFlag(EntityHandle handle, EntityFlags flag) {
        if (!GetEntity(handle, out var entity)) return;

        var change    = new FlagsChange();
        change.Old        = Flags[handle.Id];
        entity.Flags     ^= flag;
        Flags[handle.Id] ^= flag;
        change.New        = Flags[handle.Id];
        change.Entity     = entity;

        EntityFlagsChanged(change);
    }

    private void CheckDynamicOnFlagsChange(FlagsChange change) {
        // if dynamic flag was removed
        if ((change.Old & EntityFlags.Dynamic) == EntityFlags.Dynamic &&
            (change.New & EntityFlags.Dynamic) != EntityFlags.Dynamic) {
            var id = change.Entity.Handle.Id;

            RemoveDynamic(id);
            change.Entity.OnBecameStatic();
        } 
        // if dynamic flag was added
        else if ((change.Old & EntityFlags.Dynamic) != EntityFlags.Dynamic &&
                 (change.New & EntityFlags.Dynamic) == EntityFlags.Dynamic) {
            var id = change.Entity.Handle.Id;

            AddDynamic(id);
            change.Entity.OnBecameDynamic();
        }
    }

    private void Resize(uint newSize) {
        var ns = (int)newSize; // fuck you c#;
        Array.Resize(ref Entities, ns);
        Array.Resize(ref Types, ns);
        Array.Resize(ref Flags, ns);
        Array.Resize(ref Tags, ns);
        Array.Resize(ref Free, ns);
        Array.Resize(ref NextFree, ns);
        Array.Resize(ref NextDynamic, ns);
        Array.Resize(ref PrevDynamic, ns);
        foreach(var (_, list) in EntitiesByType) {
            list.Resize(newSize);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetNextFree() {
        uint id;
        if (FirstFree > 0) {
            id = FirstFree;
            FirstFree = NextFree[id];
            return id;
        }

        id = MaxEntitiesCount++;
        
        if (id >= Entities.Length) {
            Resize(id + ResizeStep);
        }

        return id;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddDynamic(uint id) {
        NextDynamic[id] = FirstDynamic;
        PrevDynamic[FirstDynamic] = id;
        FirstDynamic = id;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RemoveDynamic(uint id) {
        var prev = PrevDynamic[id];
        var next = NextDynamic[id];

        if (FirstDynamic == id) {
            FirstDynamic = next;
        } else {
            NextDynamic[prev] = next;
        }

        if (next > 0) {
            PrevDynamic[next] = prev;
        }

        NextDynamic[id] = 0;
        PrevDynamic[id] = 0;
    }
}