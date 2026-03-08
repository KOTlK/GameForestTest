using Raylib_cs;
using System.Collections.Generic;
using System;
using System.Numerics;

using static Raylib_cs.Raylib;
using static Assertions;

public struct RendererHandle {
	public uint Id;

	public static readonly RendererHandle Zero = new RendererHandle() {Id = 0};
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
	private static readonly Vector2UInt TextureResolution = new Vector2UInt(64, 64);

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
		if (handle.Id == 0) return;
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

		var sx = (float)gridSize.x * cellSize.X;
		var sy = (float)gridSize.y * cellSize.Y;

		for (uint y = 0; y < gridSize.y; y++) {
			for (uint x = 0; x < gridSize.x; x++) {
				var cellIndex = grid.GetCellIndex(x, y);
				var color     = cellIndex == grid.SelectedCell ? Color.Green : 
																 Color.Beige;
				var pos = new Vector2(x * cellSize.X,
									  y * cellSize.Y);
				var rect = new Rectangle(pos, cellSize);

				DrawRectangleLinesEx(rect, 1, color);

#if true
				if (grid.Moved[cellIndex]) {
					DrawRectangle((int)pos.X, (int)pos.Y, (int)cellSize.X, (int)cellSize.Y, Color.Magenta);
				}
#endif 
			}
		}

		// Render entities.
		for (var i = 1; i < RenderersCount; i++) {
			if (RendererFree[i]) continue;

			var renderer = Renderers[i];

			if (!em.GetEntity(renderer.Entity, out Entity entity)) continue;

			switch (renderer.Shape) {
				// case ShapeType.Circle : {
				// 	DrawCircleV(entity.Position + renderer.Offset, 
				// 				renderer.Radius * entity.Scale.X, 
				// 				renderer.Color);
				// } break;
				// case ShapeType.Rectangle : {
				// 	var center   = entity.Position;
				// 	var halfSize = renderer.Size * entity.Scale * 0.5f;
				// 	center      -= halfSize;
				// 	center 		+= renderer.Offset;
					
				// 	DrawRectangleV(center,
				// 				   renderer.Size * entity.Scale,
				// 				   renderer.Color);
				// } break;
				case ShapeType.Text : {
					var center   = entity.Position;
					center      += renderer.Offset;
					DrawText(renderer.Text, (int)center.X, (int)center.Y, renderer.FontSize, renderer.Color);
				} break;
				case ShapeType.Sprite : {
					var center   = entity.Position;
					var size     = renderer.Size * entity.Scale;
					var halfSize = size * 0.5f;
					center 		+= renderer.Offset;

					var rect = new Rectangle(center.X, 
											 center.Y, 
											 size.X, 
											 size.Y);
					var txt  = new Rectangle(0, 
											 0, 
											 renderer.Texture.Width,
											 renderer.Texture.Height);

					DrawTexturePro(renderer.Texture, 
								   txt, 
								   rect, 
								   halfSize, // pivot
								   entity.Orientation, 
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