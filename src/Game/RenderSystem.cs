using Raylib_cs;
using System.Collections.Generic;
using System;
using System.Numerics;

using static Raylib_cs.Raylib;
using static Assertions;

public struct RendererHandle {
	public uint Id;
}

public class RenderSystem : GameSystem {
	public Renderer[] Renderers 	    = new Renderer[InitialRenderersCount];
	public bool[]     RendererFree      = new bool[InitialRenderersCount];
	public uint[]     NextFreeRenderer  = new uint[InitialRenderersCount];
	public uint       RenderersLength   = InitialRenderersCount;
	public uint       RenderersCount    = 1;
	public uint       FirstFreeRenderer = 0;

	private EntityManager em;
	private Camera        camera;
	private GridSystem    grid;

	private const uint InitialRenderersCount = 128;

	public RenderSystem(Game game, EntityManager entityManager, Camera cam) : base(game, true) {
		em = entityManager;
		camera = cam;
		grid = game.GetSystem<GridSystem>();

		for (var i = 0; i < InitialRenderersCount; i++) {
			RendererFree[i] = true;
		}
	}

	public RendererHandle AppendRenderer(Renderer renderer) {
		var id = GetNextRendererId();
		RendererFree[id] = false;
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

		// @Cleanup: For now it's just lines, switch to shader later.
		var gridSize = grid.Size;
		var cellSize = grid.CellSize;

		for (uint y = 0; y < gridSize.y; y++) {
			for (uint x = 0; x < gridSize.x; x++) {
				var cellIndex = grid.GetCellIndex(x, y);
				var color     = cellIndex == grid.SelectedCell ? Color.Green : 
																 Color.Red;
				var pos = new Vector2(x * cellSize.X,
									  y * cellSize.Y);
				var rect = new Rectangle(pos, cellSize);

				DrawRectangleLinesEx(rect, 1, color);
			}
		}

		// Render entities
		for (var i = 1; i < em.MaxEntitiesCount; i++) {
			if (em.Free[i]) continue;

			var entity = em.Entities[i];

			if (entity.Renderer.Id == 0) continue;

			Assert(RendererFree[entity.Renderer.Id] == false, 
				   "Trying to draw empty renderer. (%)", entity.Renderer.Id);

			var renderer = Renderers[entity.Renderer.Id];


			switch (renderer.Shape) {
				case ShapeType.Circle : {
					DrawCircleV(entity.Position, 
								renderer.Radius * entity.Scale.X, 
								renderer.Color);
				} break;
				case ShapeType.Rectangle : {
					var center   = entity.Position;
					var halfSize = renderer.Size * entity.Scale * 0.5f;
					center      -= halfSize;

					DrawRectangleV(center,
								   renderer.Size * entity.Scale,
								   renderer.Color);
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

		id = RenderersCount++;

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