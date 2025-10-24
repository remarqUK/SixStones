# Match 3 Puzzle Game

A complete Match 3 puzzle game implementation for Unity 2D.

## Setup Instructions

### Automatic Setup (Recommended)

1. Open your Unity project
2. Go to the menu: **Tools > Setup Match 3 Game**
3. The setup will automatically create:
   - A new scene called "Match3.unity"
   - All necessary prefabs (GamePiece, CellBackground)
   - Sprites for the game pieces
   - Complete UI (Score and Moves counter)
   - Properly configured game objects

4. Press **Play** to test the game immediately!

## How to Play

- **Click and drag** a game piece in any direction (up, down, left, right)
- Match **3 or more** pieces of the same color in a row (horizontally or vertically)
- Matched pieces will disappear and new pieces will fall from the top
- Chain reactions create combo matches automatically
- You have **30 moves** to get the highest score possible

## Game Features

### Core Gameplay
- 8x8 game board
- 6 different colored pieces (Red, Blue, Green, Yellow, Purple, Orange)
- Smooth animations for swapping, falling, and destroying pieces
- Automatic chain reaction detection
- Invalid move prevention (swaps back if no match)

### Visual Feedback
- Pieces highlight when selected
- Smooth movement animations
- Scale-down destruction effect
- Checkerboard background pattern

### Game Rules
- Minimum 3 pieces required for a match
- Both horizontal and vertical matches supported
- Pieces fall to fill empty spaces
- New pieces spawn from the top
- Board guaranteed to have no initial matches

## Project Structure

```
Assets/
├── Scripts/
│   ├── GamePiece.cs         - Individual game piece behavior
│   ├── Board.cs             - Grid management and game logic
│   ├── MatchDetector.cs     - Match detection algorithm
│   ├── InputManager.cs      - Touch/mouse input handling
│   └── GameManager.cs       - Score and game state management
├── Editor/
│   └── Match3Setup.cs       - Automated scene setup tool
├── Prefabs/
│   ├── GamePiece.prefab     - Game piece prefab
│   └── CellBackground.prefab - Board cell background
├── Sprites/
│   ├── CircleSprite.png     - Piece sprite
│   └── SquareSprite.png     - Cell background sprite
└── Scenes/
    └── Match3.unity         - Main game scene
```

## Customization

### Board Settings (Board.cs)
- `width`: Board width (default: 8)
- `height`: Board height (default: 8)
- `cellSize`: Size of each cell (default: 1.0)
- `swapDuration`: Speed of piece swapping (default: 0.2s)
- `fallDuration`: Speed of falling pieces (default: 0.3s)

### Game Settings (GameManager.cs)
- `pointsPerPiece`: Points awarded per matched piece (default: 10)
- `maxMoves`: Number of moves allowed (default: 30)

### Adding New Piece Types
1. Open `GamePiece.cs`
2. Add new type to the `PieceType` enum
3. Add corresponding color in the `UpdateColor()` method
4. The system will automatically include it in gameplay

## Technical Details

### Scripts Overview

**GamePiece.cs**
- Manages individual piece state and type
- Handles movement animations
- Provides visual feedback (highlighting, destruction)
- Stores grid position

**Board.cs**
- Creates and manages the game grid
- Handles piece swapping logic
- Manages piece falling and refilling
- Coordinates match processing
- Prevents initial matches at game start

**MatchDetector.cs**
- Detects horizontal and vertical matches
- Supports matches of 3 or more pieces
- Efficiently finds all matches on the board
- Uses HashSet to prevent duplicate matches

**InputManager.cs**
- Handles mouse and touch input
- Implements drag-to-swap mechanics
- Uses raycast for piece selection
- Validates moves before executing

**GameManager.cs**
- Tracks score and remaining moves
- Updates UI display
- Handles game over state
- Provides game reset functionality

## Performance Considerations

- Uses object pooling-friendly architecture
- Efficient match detection algorithm (O(n) where n = board size)
- Minimal garbage allocation during gameplay
- Sprite-based rendering for 2D performance

## Future Enhancement Ideas

- Power-ups (bombs, row/column clearers)
- Multiple game modes (timed, endless, puzzle)
- Level system with increasing difficulty
- Particle effects for matches
- Sound effects and music
- Save/load high scores
- Tutorial system
- Special piece combinations (L-shapes, T-shapes)

## Troubleshooting

**Issue: Pieces don't respond to clicks**
- Ensure the Main Camera is tagged as "MainCamera"
- Check that GamePiece prefab has a CircleCollider2D component
- Verify InputManager has the correct layer mask

**Issue: No matches being detected**
- Check that Board has proper width/height settings
- Ensure pieces are properly initialized with types
- Verify MatchDetector is created in Board.Start()

**Issue: UI not showing**
- Make sure TextMeshPro package is installed
- Check Canvas render mode is set to Screen Space - Overlay
- Verify GameManager references are properly set

## Credits

Created as a demonstration of Match 3 game mechanics in Unity 2D using Universal Render Pipeline.
