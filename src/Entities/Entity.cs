using System.Runtime.CompilerServices;
using System.Numerics;

public class Entity {
    public string         Name;
    public EntityManager  Em;
    public EntityHandle   Handle;
    public EntityType     Type;
    public EntityFlags    Flags;
    public RendererHandle Renderer;

    public Vector2 Position    { get; set; }
    public float   Orientation { get; set; }
    public Vector2 Scale       { get; set; }

    public virtual void OnSerialize(ISerializer writer) { }
    public virtual void OnDeserialize(IDeserializer reader) { }
    public virtual void OnCreate(){ }
    public virtual void UpdateEntity(){ }
    public virtual void Destroy() { }
    public virtual void OnBecameDynamic() { }
    public virtual void OnBecameStatic() { }

    public void Serialize(ISerializer writer) {
        writer.Write(nameof(Type), Type);
        writer.Write(nameof(Flags), Flags);
        writer.Write(nameof(Scale), Scale);
        OnSerialize(writer);
    }

    public void Deserialize(IDeserializer reader) {
        Type  = reader.Read<EntityType>(nameof(Type));
        Flags = reader.Read<EntityFlags>(nameof(Flags));
        Scale = reader.Read<Vector2>(nameof(Scale));
        var renderer = reader.Read<Renderer>(nameof(Renderer));
        renderer.Entity = Handle;

        renderer.LoadAssets();

        var renderSystem = Services<RenderSystem>.Get();

        Renderer = renderSystem.AppendRenderer(renderer);

        OnDeserialize(reader);
    }

    // Helper methods to get and destroy entities without calling Em...
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetEntity(EntityHandle handle, out Entity e) {
        return Em.GetEntity(handle, out e);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetEntity<T>(EntityHandle handle, out T e)
    where T : Entity {
        return Em.GetEntity<T>(handle, out e);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DestroyThisEntity() {
        Em.DestroyEntity(Handle);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DestroyEntity(EntityHandle handle) {
        Em.DestroyEntity(handle);
    }
}