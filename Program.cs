using Raylib_cs;
using System.Numerics;
using System;

using static Raylib_cs.Raylib;

internal static class Program {
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;

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
        var state           = new GameStateSystem(game, entityManager, camera);
        var random = new Random();

        game.AppendSystem(emUpdateSystem);
        game.AppendSystem(state);
        game.AppendSystem(new CountdownSystem(game, 60));
        game.AppendSystem(gridSystem);
        game.AppendSystem(score);
        game.AppendSystem(animationSystem);
        game.AppendSystem(new UISystem(game, entityManager, camera));

        var render = new RenderSystem(game, entityManager, camera);
        game.AppendSystem(render);

        Services<Game>.Create(game);
        Services<EntityManager>.Create(entityManager);
        Services<GridSystem>.Create(gridSystem);
        Services<RenderSystem>.Create(render);
        Services<ScoreSystem>.Create(score);

        camera.AlignWithGrid(gridSystem.Size, gridSystem.CellSize, new Vector2(10, 10));

        state.SwitchState(GameState.MainMenu);

        while (!WindowShouldClose()) {
            Clock.Update(GetFrameTime());

            game.Update();

            // if (IsKeyPressed(KeyboardKey.N)) {
            //     gridSystem.MakeNewGrid(gridSize, cellSize);
            //     gridSystem.FillRandom();
            // }
        }

        CloseWindow();
    }
}