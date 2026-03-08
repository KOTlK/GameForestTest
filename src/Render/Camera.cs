using Raylib_cs;
using System.Numerics;

public class Camera {
	public Camera2D RaylibCamera;
	public Vector2  Position     { get; private set; }
	public float    Zoom 	     { get; private set; } 

	private Vector2 windowSize;

	public Camera(Vector2 windowSize, Vector2 position, float zoom) {
		this.windowSize = windowSize;
		Position 		= position;
		Zoom 			= zoom;

		RaylibCamera = new Camera2D(windowSize, position, 0, zoom);
	}

	public void Move(Vector2 newPosition) {
		Position 			= newPosition;
		RaylibCamera.Target = newPosition;
	}

	public void SetZoom(float zoom) {
		Zoom = zoom;
		RaylibCamera.Zoom = zoom;
	}

	public void AlignWithGrid(Vector2UInt gridSize, 
							  Vector2 	  cellSize, 
							  Vector2 	  gridOffset) {
		var gridCenter = gridSize * cellSize * 0.5f;
		Move(gridCenter);

		var gridWidth  = gridSize.x * cellSize.X + gridOffset.X;
		var gridHeight = gridSize.y * cellSize.Y + gridOffset.Y;

		var zoom = gridWidth > gridHeight ? gridWidth : gridHeight;

		SetZoom(windowSize.Y / (zoom * 0.5f));
	}

	public Vector2 GetPointerWorldPos() {
		return Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), RaylibCamera);
	}

	public Vector2 ScreenToWorldPoint(Vector2 screen) {
		return Raylib.GetScreenToWorld2D(screen, RaylibCamera);
	}
}