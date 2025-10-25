using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GemSelector gemSelector;
    [SerializeField] private PlayerManager playerManager;

    private Vector3 dragStartPosition;
    private bool isDragging = false;
    private bool isPointerDown = false;
    private const float DRAG_THRESHOLD = 0.5f;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (board == null) return;

        // Allow input even during processing (cursor can move during animations)
        HandleInput();
    }

    private void HandleInput()
    {
        // Don't process gem input if pointer is over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Check if pointer (mouse/touch) is pressed
        bool currentPointerDown = Mouse.current != null && Mouse.current.leftButton.isPressed ||
                                   Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;

        // Pointer down (just pressed)
        if (currentPointerDown && !isPointerDown)
        {
            Vector3 worldPos = GetPointerWorldPosition();
            if (worldPos != Vector3.zero)
            {
                // Only query the gem at this position when clicking
                GamePiece piece = GetPieceAtPosition(worldPos);
                if (piece != null)
                {
                    // Update gem selector position to clicked gem (snap instantly, no animation)
                    if (gemSelector != null)
                    {
                        gemSelector.SetPosition(piece.GridPosition, animate: false);
                    }

                    dragStartPosition = worldPos;
                    isDragging = false;

                    // Highlight the piece we just clicked
                    piece.Highlight(true);

                    // Notify player activity (e.g., for hint system)
                    PlayerActivityNotifier.NotifyActivity();
                }
            }
        }

        // Pointer held (dragging)
        if (currentPointerDown && isDragging == false)
        {
            Vector3 worldPos = GetPointerWorldPosition();
            if (worldPos != Vector3.zero)
            {
                float distance = Vector3.Distance(dragStartPosition, worldPos);
                if (distance > DRAG_THRESHOLD)
                {
                    isDragging = true;
                    ProcessSwipe(worldPos);
                }
            }
        }

        // Pointer up (released)
        if (!currentPointerDown && isPointerDown)
        {
            // Turn off highlighting on the piece at current cursor position (if any)
            if (gemSelector != null && board != null)
            {
                Vector2Int cursorPos = gemSelector.CurrentPosition;
                GamePiece pieceAtCursor = board.GetPieceAt(cursorPos.x, cursorPos.y);
                if (pieceAtCursor != null)
                {
                    pieceAtCursor.Highlight(false);
                }
            }
            isDragging = false;
        }

        isPointerDown = currentPointerDown;
    }

    private Vector3 GetPointerWorldPosition()
    {
        Vector2 screenPos = Vector2.zero;

        // Get position from mouse or touch
        if (Mouse.current != null)
        {
            screenPos = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else
        {
            return Vector3.zero;
        }

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
        worldPos.z = 0;
        return worldPos;
    }

    private void ProcessSwipe(Vector3 currentWorldPos)
    {
        if (gemSelector == null || board == null) return;

        // Block swaps if it's not the human player's turn
        if (playerManager != null && playerManager.TwoPlayerMode)
        {
            if (playerManager.CurrentPlayer != PlayerManager.Player.Player1)
            {
                Debug.Log("Cannot swap - it's Player 2's turn");
                return; // Block the swap, but allow other interactions
            }
        }

        // Get cursor position (not the piece - cursor tracks position independently)
        Vector2Int cursorPos = gemSelector.CurrentPosition;

        Vector3 swipeDirection = currentWorldPos - dragStartPosition;
        Vector2Int gridDirection = Vector2Int.zero;

        // Determine primary swipe direction
        if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
        {
            // Horizontal swipe
            gridDirection = swipeDirection.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        else
        {
            // Vertical swipe
            gridDirection = swipeDirection.y > 0 ? Vector2Int.up : Vector2Int.down;
        }

        Vector2Int targetPos = cursorPos + gridDirection;

        // Turn off highlighting on piece at cursor (if any)
        GamePiece pieceAtCursor = board.GetPieceAt(cursorPos.x, cursorPos.y);
        if (pieceAtCursor != null)
        {
            pieceAtCursor.Highlight(false);
        }

        // Use unified swap method - it will query gems at action time
        gemSelector.PerformSwap(cursorPos, targetPos);

        // Move cursor to the target position after drag
        gemSelector.SetPosition(targetPos);

        // Keep isDragging = true until pointer is released (handled in pointer up section)
        // This prevents multiple swipes while holding down the mouse/finger
    }

    private GamePiece GetPieceAtPosition(Vector3 worldPosition)
    {
        // Raycast to find game pieces at this position
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, 0.1f);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<GamePiece>();
        }
        return null;
    }
}
