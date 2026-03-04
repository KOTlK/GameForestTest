using Raylib_cs;
using System.Collections.Generic;
using System;

using static Raylib_cs.Raylib;

public struct RendererHandle {
	public uint Id;
}

public class RenderSystem : GameSystem {
	public Renderer[] Renderers 	    = new Renderer[InitialRenderersCount];
	public bool[]     RendererFree      = new bool[InitialRenderersCount];
	public uint[]     NextFreeRenderer  = new uint[InitialRenderersCount];
	public uint       RenderersLength   = InitialRenderersCount;
	public uint       RenderersCount    = 1;
	public uint       FirstFreeRenderer = 1;

	private EntityManager em;
	private Camera        camera;

	private const uint InitialRenderersCount = 128;

	public RenderSystem(Game game, EntityManager entityManager, Camera cam) : base(game, true) {
		em = entityManager;
		camera = cam;
	}

	public RendererHandle AppendRenderer(Renderer renderer) {
		var id = GetNextRendererId();
		Renderers[id] = renderer;

		return new RendererHandle() {
			Id = id
		};
	}

	public void RemoveRenderer(RendererHandle handle) {
		RendererFree[handle.Id] = true;
		NextFreeRenderer[handle.Id] = FirstFreeRenderer;
		FirstFreeRenderer = handle.Id;
	}

	public ref Renderer GetRenderer(RendererHandle handle) {
		// renderer at index 0 will never be rendered
		if (RendererFree[handle.Id]) {
			return ref Renderers[0];
		}
		
		return ref Renderers[handle.Id];
	}

	public override void Update() {
		BeginDrawing();
		ClearBackground(Color.White);

		BeginMode2D(camera.RaylibCamera);
		for (var i = 1; i < em.MaxEntitiesCount; i++) {
			if (em.Free[i]) continue;

			var entity = em.Entities[i];

			if (entity.Renderer.Id == 0) continue;

			var renderer = Renderers[entity.Renderer.Id];

			switch (renderer.Shape) {
				case ShapeType.Circle : {
					DrawCircleV(entity.Position, renderer.Radius, renderer.Color);
				} break;
				case ShapeType.Rectangle : {
					DrawRectangleV(entity.Position, renderer.Size, renderer.Color);
				} break;
			}
		}
		EndMode2D();

        DrawText("Hello, world!", 0, 0, 20, Color.Black);

		EndDrawing();
	}

	private uint GetNextRendererId() {
		uint id;

		if (FirstFreeRenderer > 0) {
			id = FirstFreeRenderer;
			FirstFreeRenderer = NextFreeRenderer[id];
			return id;
		}

		id = RenderersCount;

		if (RenderersCount >= RenderersLength) {
			Resize(RenderersCount + 128);
		}

		return id;
	}

	private void Resize(uint newSize) {
		Array.Resize(ref Renderers, (int)newSize);
		Array.Resize(ref RendererFree, (int)newSize);
		Array.Resize(ref NextFreeRenderer, (int)newSize);
	}
}