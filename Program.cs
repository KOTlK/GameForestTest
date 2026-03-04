using Raylib_cs;
using System.Numerics;
using System;

using static Raylib_cs.Raylib;

internal static class Program {
    private const int WindowWidth = 1600;
    private const int WindowHeight = 900;

    [System.STAThread]
    public static void Main() {
        var camera = new Camera(new Vector2(WindowWidth / 2f, WindowHeight / 2f),
                                Vector2.Zero,
                                1.0f);

        var entityManager = new EntityManager();
        var game          = new Game();

        var emUpdateSystem = new EntitiesUpdateSystem(game, entityManager);
        var gridSystem = new GridSystem(game, entityManager, new Vector2UInt(8, 8));
        var random = new Random();

        game.AppendSystem(emUpdateSystem);
        game.AppendSystem(gridSystem);
        game.AppendSystem(new RenderSystem(game, entityManager, camera));

        Services<Game>.Create(game);
        Services<EntityManager>.Create(entityManager);

        InitWindow(WindowWidth, WindowHeight, "Hello World");

        while (!WindowShouldClose()) {
            Clock.Update(GetFrameTime());

            var wheelSens = 5f;
            var wheel = GetMouseWheelMove();

            camera.SetZoom(camera.Zoom + wheel * wheelSens * Clock.Delta);

            if (IsKeyPressed(KeyboardKey.Space)) {
                var (_, element) = entityManager.CreateEntity<Element>("green_circle_element", Vector2.Zero, 0);

                var randX = (uint)random.NextInt64() % 8;
                var randY = (uint)random.NextInt64() % 8;

                gridSystem.PutElement(new Vector2UInt(randX, randY), element);
            }

            game.Update();
        }

        CloseWindow();
    }
}