using UnityEngine;
using UnityEditor;

/// <summary>
/// Creates default game mode assets
/// </summary>
public class CreateGameModes : EditorWindow
{
    [MenuItem("Tools/Create Default Game Modes")]
    public static void CreateDefaultModes()
    {
        // Create Resources folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources/GameModes"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "GameModes");
        }

        // Create Standard Mode
        GameModeData standardMode = ScriptableObject.CreateInstance<GameModeData>();
        standardMode.modeName = "Standard";
        standardMode.description = "Classic match-3 gameplay with falling pieces and new gems spawning from the top.";
        standardMode.playerCount = 2; // Two-player mode
        standardMode.player2IsCPU = true; // CPU controls Player 2
        standardMode.boardWidth = 8;
        standardMode.boardHeight = 8;
        standardMode.spawnNewPieces = true;
        standardMode.piecesCanFall = true;
        standardMode.moveLimit = 30;
        standardMode.minMatchLength = 3;
        standardMode.allowCascades = true;
        standardMode.match3Points = 3;
        standardMode.match4Points = 5;
        standardMode.match5PlusPoints = 10;
        standardMode.scoreMultiplier = 1f;
        standardMode.boardClearBonus = 0;
        standardMode.winOnBoardClear = false;
        standardMode.loseOnNoMoves = false;
        standardMode.targetScore = 0;
        standardMode.modeColor = new Color(0.3f, 0.7f, 1f); // Blue

        AssetDatabase.CreateAsset(standardMode, "Assets/Resources/GameModes/StandardMode.asset");

        // Create Solve Mode
        GameModeData solveMode = ScriptableObject.CreateInstance<GameModeData>();
        solveMode.modeName = "Solve";
        solveMode.description = "Clear all pieces from the board! No new gems spawn - plan your moves carefully.";
        solveMode.playerCount = 1; // Single-player mode
        solveMode.boardWidth = 8;
        solveMode.boardHeight = 8;
        solveMode.spawnNewPieces = false;
        solveMode.piecesCanFall = true;
        solveMode.moveLimit = 0; // Unlimited moves
        solveMode.minMatchLength = 3;
        solveMode.allowCascades = true;
        solveMode.match3Points = 3;
        solveMode.match4Points = 5;
        solveMode.match5PlusPoints = 10;
        solveMode.scoreMultiplier = 1.5f;
        solveMode.boardClearBonus = 1000;
        solveMode.winOnBoardClear = true;
        solveMode.loseOnNoMoves = true;
        solveMode.targetScore = 0;
        solveMode.modeColor = new Color(1f, 0.7f, 0.3f); // Orange

        AssetDatabase.CreateAsset(solveMode, "Assets/Resources/GameModes/SolveMode.asset");

        // Create Drop Mode
        GameModeData dropMode = ScriptableObject.CreateInstance<GameModeData>();
        dropMode.modeName = "Drop";
        dropMode.description = "Score by getting red gems to the bottom row! Red gems are removed when they reach the bottom.";
        dropMode.playerCount = 1; // Single-player mode
        dropMode.boardWidth = 8;
        dropMode.boardHeight = 8;
        dropMode.spawnNewPieces = true;
        dropMode.piecesCanFall = true;
        dropMode.moveLimit = 0; // Infinite moves
        dropMode.minMatchLength = 3;
        dropMode.allowCascades = true;
        dropMode.scoreOnBottomRowHit = true; // Drop mode specific
        dropMode.bottomRowScoreGem = GamePiece.PieceType.Red; // Red gems score
        dropMode.bottomRowHitPoints = 10; // 10 points per red gem
        dropMode.match3Points = 0; // No points for matches
        dropMode.match4Points = 0;
        dropMode.match5PlusPoints = 0;
        dropMode.scoreMultiplier = 1f;
        dropMode.boardClearBonus = 0;
        dropMode.winOnBoardClear = false;
        dropMode.loseOnNoMoves = false;
        dropMode.targetScore = 0;
        dropMode.modeColor = new Color(0.9f, 0.3f, 0.3f); // Red

        AssetDatabase.CreateAsset(dropMode, "Assets/Resources/GameModes/DropMode.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Game modes created successfully!");
        EditorUtility.DisplayDialog("Success",
            "Game modes created in Assets/Resources/GameModes/\n\n" +
            "• StandardMode.asset - Classic gameplay\n" +
            "• SolveMode.asset - No new pieces spawn\n" +
            "• DropMode.asset - Score by hitting bottom row\n\n" +
            "Assign a mode to the Board component in the Inspector.",
            "OK");
    }
}
