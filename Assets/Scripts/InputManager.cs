using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private Camera mainCamera;

    private GamePiece selectedPiece = null;
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
        if (board == null || board.IsProcessing) return;

        HandleInput();
    }

    private void HandleInput()
    {
        // Check if pointer (mouse/touch) is pressed
        bool currentPointerDown = Mouse.current != null && Mouse.current.leftButton.isPressed ||
                                   Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;

        // Pointer down (just pressed)
        if (currentPointerDown && !isPointerDown)
        {
            Vector3 worldPos = GetPointerWorldPosition();
            if (worldPos != Vector3.zero)
            {
                GamePiece piece = GetPieceAtPosition(worldPos);
                if (piece != null)
                {
                    selectedPiece = piece;
                    dragStartPosition = worldPos;
                    isDragging = false;
                    selectedPiece.Highlight(true);
                }
            }
        }

        // Pointer held (dragging)
        if (currentPointerDown && selectedPiece != null)
        {
            Vector3 worldPos = GetPointerWorldPosition();
            if (worldPos != Vector3.zero)
            {
                float distance = Vector3.Distance(dragStartPosition, worldPos);
                if (distance > DRAG_THRESHOLD && !isDragging)
                {
                    isDragging = true;
                    ProcessSwipe(worldPos);
                }
            }
        }

        // Pointer up (released)
        if (!currentPointerDown && isPointerDown)
        {
            if (selectedPiece != null)
            {
                selectedPiece.Highlight(false);
                selectedPiece = null;
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
        if (selectedPiece == null) return;

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

        Vector2Int currentPos = selectedPiece.GridPosition;
        Vector2Int targetPos = currentPos + gridDirection;

        if (board.IsValidPosition(targetPos))
        {
            board.SwapPieces(currentPos, targetPos);
            selectedPiece.Highlight(false);
            selectedPiece = null;
            isDragging = false;
        }
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
