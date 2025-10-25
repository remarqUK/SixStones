using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages gem selection cursor - allows player to navigate with arrow keys or controller
/// Navigation: Arrow keys or D-pad/Joystick
/// Swap: Shift+Arrow keys (keyboard) or A button+Direction (controller)
/// </summary>
public class GemSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Board board;
    [SerializeField] private PlayerManager playerManager;

    [Header("Selection Settings")]
    [SerializeField] private Vector2Int startPosition = new Vector2Int(3, 3); // 4th across, 4th down (0-indexed)
    [SerializeField] private float moveDelay = 0.15f; // Delay between cursor moves
    [SerializeField] private float animationDuration = 0.15f; // Duration of cursor movement animation

    private Vector2Int currentPosition;
    private GameObject selectionIndicator;
    private SpriteRenderer indicatorRenderer;
    private float lastMoveTime;
    private Coroutine moveCoroutine;
    private Texture2D indicatorTexture; // Store texture reference for cleanup

    private void Start()
    {
        // Create selection indicator
        CreateSelectionIndicator();

        // Set initial position (no animation on startup)
        currentPosition = startPosition;
        UpdateIndicatorPosition(animate: false);

        Debug.Log($"Gem selector initialized at position ({currentPosition.x}, {currentPosition.y})");
    }

    private void OnDestroy()
    {
        // Stop any running coroutine
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        // Destroy the indicator GameObject
        if (selectionIndicator != null)
        {
            Destroy(selectionIndicator);
            selectionIndicator = null;
        }

        // Destroy the texture to prevent memory leak
        if (indicatorTexture != null)
        {
            Destroy(indicatorTexture);
            indicatorTexture = null;
        }
    }

    private void Update()
    {
        // Check if enough time has passed since last move
        if (Time.time - lastMoveTime < moveDelay)
            return;

        // Get movement input
        Vector2Int movement = GetMovementInput();

        if (movement != Vector2Int.zero)
        {
            // Check if Shift key is held (for keyboard swaps)
            bool shiftHeld = Keyboard.current != null &&
                            (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

            // Check if A button is held (for controller swaps)
            bool aButtonHeld = Gamepad.current != null && Gamepad.current.aButton.isPressed;

            if (shiftHeld || aButtonHeld)
            {
                // Perform swap in the direction pressed (will check turn validity inside)
                PerformSwap(movement);
            }
            else
            {
                // Just move the selection (allowed even during CPU's turn)
                MoveSelection(movement);
            }

            lastMoveTime = Time.time;
        }
    }

    /// <summary>
    /// Get movement input from keyboard arrow keys or controller d-pad/joystick
    /// </summary>
    private Vector2Int GetMovementInput()
    {
        Vector2 input = Vector2.zero;

        // Keyboard arrow keys
        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.isPressed) input.y += 1;
            if (Keyboard.current.downArrowKey.isPressed) input.y -= 1;
            if (Keyboard.current.leftArrowKey.isPressed) input.x -= 1;
            if (Keyboard.current.rightArrowKey.isPressed) input.x += 1;
        }

        // Gamepad input
        if (Gamepad.current != null)
        {
            // D-pad
            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            input += dpad;

            // Left stick (with deadzone)
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.magnitude > 0.5f)
            {
                input += stick;
            }
        }

        // Convert to discrete movement
        Vector2Int movement = Vector2Int.zero;
        if (input.x > 0.5f) movement.x = 1;
        else if (input.x < -0.5f) movement.x = -1;

        if (input.y > 0.5f) movement.y = 1;
        else if (input.y < -0.5f) movement.y = -1;

        return movement;
    }

    /// <summary>
    /// Move the selection in the given direction
    /// </summary>
    private void MoveSelection(Vector2Int direction)
    {
        if (board == null) return;

        Vector2Int newPosition = currentPosition + direction;

        // Clamp to board bounds
        newPosition.x = Mathf.Clamp(newPosition.x, 0, board.Width - 1);
        newPosition.y = Mathf.Clamp(newPosition.y, 0, board.Height - 1);

        // Only update if position changed
        if (newPosition != currentPosition)
        {
            currentPosition = newPosition;
            UpdateIndicatorPosition();

            // Notify player activity (e.g., for hint system)
            PlayerActivityNotifier.NotifyActivity();

            Debug.Log($"Gem selector moved to ({currentPosition.x}, {currentPosition.y})");
        }
    }

    /// <summary>
    /// Perform a swap from the current position in the given direction
    /// Triggered by: Shift+Arrow keys (keyboard) or A button+Direction (controller)
    /// </summary>
    private void PerformSwap(Vector2Int direction)
    {
        if (board == null) return;

        Vector2Int targetPosition = currentPosition + direction;

        // Use the public method to perform the swap
        PerformSwap(currentPosition, targetPosition);

        // Move cursor to the target position after swap
        SetPosition(targetPosition);
    }

    /// <summary>
    /// Unified swap method - all input methods should call this
    /// Handles swapping two gems at the given positions
    /// Only queries gems at the positions when actually performing the swap
    /// </summary>
    public void PerformSwap(Vector2Int pos1, Vector2Int pos2)
    {
        if (board == null) return;

        // Block swaps if it's not the human player's turn
        if (playerManager != null && playerManager.TwoPlayerMode)
        {
            if (playerManager.CurrentPlayer != PlayerManager.Player.Player1)
            {
                Debug.Log("Cannot swap - it's Player 2's turn");
                return;
            }
        }

        // Block swaps while board is processing animations
        if (board.IsProcessing)
        {
            Debug.Log("Cannot swap - board is processing");
            return;
        }

        // Check if both positions are within bounds
        if (board.IsValidPosition(pos1) && board.IsValidPosition(pos2))
        {
            // Only now do we check what gems are at these positions
            GamePiece piece1 = board.GetPieceAt(pos1.x, pos1.y);
            GamePiece piece2 = board.GetPieceAt(pos2.x, pos2.y);

            if (piece1 != null && piece2 != null)
            {
                Debug.Log($"Swap initiated: ({pos1.x},{pos1.y}) -> ({pos2.x},{pos2.y})");
                board.SwapPieces(pos1, pos2);
            }
            else
            {
                Debug.Log($"Cannot swap - one or both positions have no gem");
            }
        }
        else
        {
            Debug.Log($"Cannot swap - one or both positions are out of bounds");
        }
    }

    /// <summary>
    /// Update the visual indicator position to match current selection (with animation)
    /// </summary>
    private void UpdateIndicatorPosition(bool animate = true)
    {
        if (selectionIndicator == null || board == null) return;

        Vector3 targetPosition = board.GridToWorld(currentPosition.x, currentPosition.y);

        if (animate && animationDuration > 0)
        {
            // Stop any existing animation
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }
            moveCoroutine = StartCoroutine(AnimateIndicatorPosition(targetPosition));
        }
        else
        {
            // Instant movement (for initialization)
            selectionIndicator.transform.position = targetPosition;
        }
    }

    /// <summary>
    /// Coroutine to smoothly animate the cursor indicator to a new position
    /// </summary>
    private System.Collections.IEnumerator AnimateIndicatorPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = selectionIndicator.transform.position;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            // Smooth easing (same as GamePiece movement)
            t = t * t * (3f - 2f * t);
            selectionIndicator.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        selectionIndicator.transform.position = targetPosition;
        moveCoroutine = null;
    }

    /// <summary>
    /// Create the visual indicator (X overlay)
    /// </summary>
    private void CreateSelectionIndicator()
    {
        selectionIndicator = new GameObject("SelectionIndicator");
        indicatorRenderer = selectionIndicator.AddComponent<SpriteRenderer>();
        indicatorRenderer.sprite = CreateXSprite();
        indicatorRenderer.sortingOrder = 10; // On top of everything
        indicatorRenderer.color = new Color(1f, 1f, 0f, 0.8f); // Yellow with slight transparency

        selectionIndicator.transform.localScale = Vector3.one * 1.2f; // Slightly larger than gem
    }

    /// <summary>
    /// Create a simple X sprite for the selection indicator
    /// </summary>
    private Sprite CreateXSprite()
    {
        int size = 64;
        indicatorTexture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Initialize to transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        int thickness = 6;

        // Draw X (two diagonal lines)
        for (int i = 0; i < size; i++)
        {
            for (int t = -thickness / 2; t <= thickness / 2; t++)
            {
                // Top-left to bottom-right diagonal
                int y1 = size - 1 - i;
                if (i + t >= 0 && i + t < size && y1 >= 0 && y1 < size)
                {
                    pixels[y1 * size + (i + t)] = Color.white;
                }

                // Top-right to bottom-left diagonal
                int x2 = size - 1 - i;
                int y2 = size - 1 - i;
                if (x2 + t >= 0 && x2 + t < size && y2 >= 0 && y2 < size)
                {
                    pixels[y2 * size + (x2 + t)] = Color.white;
                }
            }
        }

        indicatorTexture.SetPixels(pixels);
        indicatorTexture.Apply();

        return Sprite.Create(indicatorTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>
    /// Get the currently selected position (this is the single source of truth)
    /// The cursor tracks position, not the gem itself
    /// </summary>
    public Vector2Int CurrentPosition => currentPosition;

    /// <summary>
    /// Set the selected position programmatically
    /// </summary>
    /// <param name="position">Target grid position</param>
    /// <param name="animate">If true, cursor animates to position. If false, snaps instantly.</param>
    public void SetPosition(Vector2Int position, bool animate = true)
    {
        if (board == null) return;

        position.x = Mathf.Clamp(position.x, 0, board.Width - 1);
        position.y = Mathf.Clamp(position.y, 0, board.Height - 1);

        currentPosition = position;
        UpdateIndicatorPosition(animate);
    }
}
