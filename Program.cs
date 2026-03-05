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
        var gridSystem = new GridSystem(game, 
                                        entityManager, 
                                        new Vector2UInt(8, 8),
                                        new Vector2(30f, 30f));
        var animationSystem = new GridAnimationSystem(game, entityManager);
        var random = new Random();

        game.AppendSystem(emUpdateSystem);
        game.AppendSystem(gridSystem);
        game.AppendSystem(animationSystem);
        game.AppendSystem(new RenderSystem(game, entityManager, camera));

        Services<Game>.Create(game);
        Services<EntityManager>.Create(entityManager);

        InitWindow(WindowWidth, WindowHeight, "Hello World");

        camera.AlignWithGrid(gridSystem.Size, gridSystem.CellSize, new Vector2(10, 10));

        gridSystem.FillRandom();

        while (!WindowShouldClose()) {
            Clock.Update(GetFrameTime());

            var wheelSens = 5f;
            var wheel = GetMouseWheelMove();

            camera.SetZoom(camera.Zoom + wheel * wheelSens * Clock.Delta);

            if (IsMouseButtonPressed(MouseButton.Left)) {
                var pointerPos = camera.GetPointerWorldPos();

                if (gridSystem.IsPointOutsideGridBounds(pointerPos)) {
                    gridSystem.ResetSelection();
                } else {
                    var gridPos = gridSystem.WorldPointToGridPos(pointerPos);

                    if (gridSystem.CellSelected) {
                        if (gridSystem.CanSwitchWithSelected(gridPos)) {
                            Console.WriteLine("Switching");
                            gridSystem.SwitchPositions((uint)gridSystem.SelectedCell,
                                                       gridSystem.GetCellIndex(gridPos));
                        } else {
                            gridSystem.ResetSelection();
                        }
                    } else {
                        gridSystem.SelectCell(gridPos);
                    }
                }
            }

            var dir = Vector2.Zero;

            if (IsKeyPressed(KeyboardKey.Space)) {
                var (_, element) = entityManager.CreateEntity<Element>("green_circle_element", 
                                                                       Vector2.Zero, 
                                                                       0);

                var randX = (uint)random.NextInt64() % 8;
                var randY = (uint)random.NextInt64() % 8;

                gridSystem.PutElement(new Vector2UInt(randX, randY), element);
            }

            game.Update();
        }

        CloseWindow();
    }
}