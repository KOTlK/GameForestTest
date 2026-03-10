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
    public Renderer[] Renderers         = new Renderer[InitialRenderersCount];
    public bool[]     RendererFree      = new bool[InitialRenderersCount];
    public uint[]     NextFreeRenderer  = new uint[InitialRenderersCount];
    public uint       RenderersLength   = InitialRenderersCount;
    public uint       RenderersCount    = 1;
    public uint       FirstFreeRenderer = 0;

    private EntityManager em;
    private Camera        camera;
    private GridSystem    grid;

    private Texture2D gridTexture;
    private Shader    gridShader;
    private int       gridSizeLoc;
    private int       gridCellSizeLoc;
    private int       borderColorLoc;
    private int       selectedCellLoc;
    private int       selectedCellColorLoc;
    private int       selectedCellBorderColorLoc;

    private Color     selectedCellColor       = Color.Brown;
    private Color     selectedCellBorderColor = Color.SkyBlue;
    private Color     gridColor               = Color.Beige;
    private Color     gridBorderColor         = Color.DarkGray;

    private const uint InitialRenderersCount = 128;

    public RenderSystem(Game game, EntityManager entityManager, Camera cam) : 
        base(game, true) {
        em = entityManager;
        camera = cam;
        grid = game.GetSystem<GridSystem>();

        for (var i = 0; i < InitialRenderersCount; i++) {
            RendererFree[i] = true;
        }

        gridShader = LoadShader(null, "assets/shaders/grid.fs");

        gridSizeLoc                = GetShaderLocation(gridShader, 
                                                       "gridSize");
        gridCellSizeLoc            = GetShaderLocation(gridShader, 
                                                       "cellSize");
        selectedCellLoc            = GetShaderLocation(gridShader, 
                                                        "selectedCell");
        selectedCellColorLoc       = GetShaderLocation(gridShader, 
                                                        "selectedCellColor");
        borderColorLoc             = GetShaderLocation(gridShader, 
                                                        "borderColor");
        selectedCellBorderColorLoc = GetShaderLocation(gridShader, 
                                                        "selectedCellBorderColor");

        gridTexture = AssetManagement.AssetManager.LoadAsset<Texture2D>(
                                            "assets/textures/grid.png");

        SetShaderValue<Vector2>(gridShader, 
                                gridSizeLoc, 
                                grid.Size, 
                                ShaderUniformDataType.Vec2);

        SetShaderValue<Vector2>(gridShader, 
                                gridCellSizeLoc, 
                                grid.CellSize, 
                                ShaderUniformDataType.Vec2);

        SetShaderValue<Vector4>(gridShader, 
                                selectedCellColorLoc, 
                                ColorNormalize(selectedCellColor),
                                ShaderUniformDataType.Vec4);

        SetShaderValue<Vector4>(gridShader,
                                borderColorLoc,
                                ColorNormalize(gridBorderColor),
                                ShaderUniformDataType.Vec4);

        SetShaderValue<Vector4>(gridShader,
                                selectedCellBorderColorLoc,
                                ColorNormalize(selectedCellBorderColor),
                                ShaderUniformDataType.Vec4);
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

        SetShaderValue<int>(gridShader, 
                            selectedCellLoc, 
                            grid.SelectedCell, 
                            ShaderUniformDataType.Int);

        BeginMode2D(camera.RaylibCamera);

        if (grid.Enabled) {
            var gridSize = grid.Size;
            var cellSize = grid.CellSize;

            var sx = (float)gridSize.x * cellSize.X;
            var sy = (float)gridSize.y * cellSize.Y;

            var r = new Rectangle(0, 
                                  0, 
                                  sx, 
                                  sy);
            var t  = new Rectangle(0, 
                                   0, 
                                   gridTexture.Width,
                                   gridTexture.Height);

            BeginShaderMode(gridShader);

            DrawTexturePro(gridTexture, 
                           t, 
                           r, 
                           Vector2.Zero, // pivot
                           0, 
                           gridColor);

            EndShaderMode();

#if false // Debug draw moved cells.
            for (uint y = 0; y < gridSize.y; y++) {
                for (uint x = 0; x < gridSize.x; x++) {
                    var cellIndex = grid.GetCellIndex(x, y);
                    var pos = new Vector2(x * cellSize.X,
                                          y * cellSize.Y);
                    var rect = new Rectangle(pos, cellSize);

                    if (grid.Moved[cellIndex]) {
                        DrawRectangle((int)pos.X, 
                                      (int)pos.Y, 
                                      (int)cellSize.X, 
                                      (int)cellSize.Y, 
                                      Color.Magenta);
                    }
                }
            }
#endif  
        }

        // Render entities.
        for (var i = 1; i < RenderersCount; i++) {
            if (RendererFree[i]) continue;

            var renderer = Renderers[i];

            if (renderer.Shape == ShapeType.Text) continue;

            if (!em.GetEntity(renderer.Entity, out Entity entity)) continue;

            switch (renderer.Shape) {
                case ShapeType.Sprite : {
                    var center   = entity.Position;
                    var size     = renderer.Size * entity.Scale;
                    var halfSize = size * 0.5f;
                    center      += renderer.Offset;

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
                default : break;
            }
        }

        // @Cleanup: I don't want to make render queues, 
        // so i render text after all other entities 
        // to make buttons work correctly. :)
        for (var i = 1; i < RenderersCount; i++) {
            if (RendererFree[i]) continue;

            var renderer = Renderers[i];

            if (!em.GetEntity(renderer.Entity, out Entity entity)) continue;

            if (renderer.Shape == ShapeType.Text) {
                var center   = entity.Position;
                center      += renderer.Offset;
                DrawText(renderer.Text, 
                         (int)center.X, 
                         (int)center.Y, 
                         renderer.FontSize, 
                         renderer.Color);
            }
        }

        EndMode2D();

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