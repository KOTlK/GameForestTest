using Raylib_cs;
using System.Numerics;

public class Camera {
	public Camera2D RaylibCamera;
	public Vector2  Position     { get; private set; }
	public float    Zoom 	     { get; private set; } 

	public Camera(Vector2 windowSize, Vector2 position, float zoom) {
		Position = position;
		Zoom = zoom;

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
}