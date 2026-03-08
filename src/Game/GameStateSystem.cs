using System.Numerics;
using Raylib_cs;

using static Raylib_cs.Raylib;

public enum GameState : uint {
	None     = 0,
	MainMenu = 1,
	Gameplay = 2,
	Result   = 3,
}

public class GameStateSystem : GameSystem {
	private GameState     currentState;
	private EntityManager em;
	private EntityHandle  playButton;
	private EntityHandle  exitButton;
	private Camera        camera;

	public GameStateSystem(Game game, 
						   EntityManager entityManager,
						   Camera camera) : 
		base (game, true) {
		em           = entityManager;
		this.camera  = camera;
	}

	public void SwitchState(GameState state) {
		ExitState(currentState);
		EnterState(state);
	}

	public override void Update() {
		// Console.WriteLine(currentState);
		switch(currentState) {
			case GameState.Gameplay : {
				var countdown  = Game.GetSystem<CountdownSystem>();
				var gridSystem = Game.GetSystem<GridSystem>();

				if (countdown.IsTimerOver) {
					SwitchState(GameState.Result);
					break;
				}

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
		    } break;
		    default : break;
		}
	}

	private void EnterState(GameState state) {
		switch(state) {
			case GameState.MainMenu : {
				em.DestroyAllEntities();
				Game.DisableSystem<GridSystem>();
				Game.DisableSystem<GridAnimationSystem>();
				Game.DisableSystem<ScoreSystem>();
				Game.DisableSystem<CountdownSystem>();

				var (handle, button) = em.CreateEntity<Button>("play_button", 
														  camera.ScreenToWorldPoint(GetScreenCenter()), 
														  0);

				button.Clicked += OnGameStart;
				playButton = handle;
			} break;
			case GameState.Gameplay : {
				em.DestroyEntity(playButton);
				Game.EnableSystem<GridSystem>();
				Game.EnableSystem<GridAnimationSystem>();
				Game.EnableSystem<ScoreSystem>();
				Game.EnableSystem<CountdownSystem>();

	        	// @Cleanup: Now ui renders in world space.
				em.CreateEntity<ScoreUI>("score_ui", new Vector2(-70, 0), 0);
				em.CreateEntity<TimeUI>("time_ui", new Vector2(250, 0), 0);

        		Game.GetSystem<GridSystem>().FillRandom();
			} break;
			case GameState.Result : {
				em.DestroyAllEntities();
				var (handle, button) = em.CreateEntity<Button>("exit_button", 
														  	   camera.ScreenToWorldPoint(GetScreenCenter()), 
														  	   0);

				var gameOverPos = camera.ScreenToWorldPoint(GetScreenCenter()) + 
								  new Vector2(-25, -30);

				var (_, gameOver) = em.CreateEntity<Text>("text",
														  gameOverPos,
														  0f);

				gameOver.SetText("Game Over");
				gameOver.SetColor(Color.Black);

				button.Clicked += OnGameExit;
				exitButton = handle;
			} break;
		}

		currentState = state;
	}

	private void ExitState(GameState state) {
		switch(state) {
			case GameState.MainMenu : {
				em.DestroyEntity(playButton);
			} break;
			case GameState.Gameplay : {
				Game.DisableSystem<GridSystem>();
				Game.DisableSystem<GridAnimationSystem>();
				Game.DisableSystem<ScoreSystem>();
				Game.DisableSystem<CountdownSystem>();
			} break;
			case GameState.Result : {
				em.DestroyEntity(exitButton);
			} break;
		}
	}

	private void OnGameStart() {
		SwitchState(GameState.Gameplay);
	}

	private void OnGameExit() {
		SwitchState(GameState.MainMenu);
	}
}