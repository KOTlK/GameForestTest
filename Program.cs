using Raylib_cs;
using System.Numerics;
using System;

using static Raylib_cs.Raylib;

internal static class Program {
    private const int WindowWidth = 1600;
    private const int WindowHeight = 900;

    private static Vector2UInt gridSize = new Vector2UInt(8, 8);
    private static Vector2     cellSize = new Vector2(30, 30);

    [System.STAThread]
    public static void Main() {
        InitWindow(WindowWidth, WindowHeight, "Hello World");
        Events.Init();
        
        var camera = new Camera(new Vector2(WindowWidth / 2f, WindowHeight / 2f),
                                Vector2.Zero,
                                1.0f);

        var entityManager = new EntityManager();
        var game          = new Game();

        var emUpdateSystem = new EntitiesUpdateSystem(game, entityManager);
        var gridSystem = new GridSystem(game, 
                                        entityManager, 
                                        gridSize,
                                        cellSize);
        var animationSystem = new GridAnimationSystem(game, entityManager);
        var score           = new ScoreSystem(game);
        var random = new Random();

        game.AppendSystem(emUpdateSystem);
        game.AppendSystem(gridSystem);
        game.AppendSystem(animationSystem);
        game.AppendSystem(score);
        var render = new RenderSystem(game, entityManager, camera);
        game.AppendSystem(render);

        Services<Game>.Create(game);
        Services<EntityManager>.Create(entityManager);
        Services<GridSystem>.Create(gridSystem);
        Services<RenderSystem>.Create(render);
        Services<ScoreSystem>.Create(score);

        // @Temp: Now ui renders in world space.
        var scoreUI = entityManager.CreateEntity<ScoreUI>("score_ui", new Vector2(-70, 0), 0);


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
                            gridSystem.TrySwitchPositions((uint)gridSystem.SelectedCell,
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
                gridSystem.Fall();
            }

            if (IsKeyPressed(KeyboardKey.N)) {
                gridSystem.MakeNewGrid(gridSize, cellSize);
                gridSystem.FillRandom();
            }

            game.Update();
        }

        CloseWindow();
    }
}