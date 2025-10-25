using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public enum PieceType
    {
        Red,
        Blue,
        Green,
        Yellow,
        Purple,
        Orange,
        White
    }

    // Pulse animation duration (one complete cycle: scale up + scale down)
    public const float PULSE_CYCLE_DURATION = 0.8f;

    [SerializeField] private PieceType pieceType;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector2Int gridPosition;
    private Board board;
    private bool isMoving = false;
    private bool isPulsating = false;
    private bool isPressed = false;
    private Vector3 baseScale = Vector3.one; // The gem's normal scale (e.g., 0.75)
    private Coroutine pulsateCoroutine;
    private Coroutine scaleAnimationCoroutine;

    public PieceType Type => pieceType;
    public Vector2Int GridPosition => gridPosition;
    public bool IsMoving => isMoving;

    /// <summary>
    /// Temporarily set type for CPU simulation (doesn't change visuals)
    /// </summary>
    public void SetTypeForSimulation(PieceType type)
    {
        pieceType = type;
    }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Capture the base scale this gem was created with (e.g., 0.75)
        baseScale = transform.localScale;
    }

    private void OnDestroy()
    {
        // Stop all coroutines to prevent memory leaks
        StopAllCoroutines();

        // Reset state flags
        isMoving = false;
        isPulsating = false;
        isPressed = false;
        pulsateCoroutine = null;
        scaleAnimationCoroutine = null;
    }

    public void Initialize(PieceType type, Vector2Int position, Board parentBoard, GameSettings settings = null)
    {
        pieceType = type;
        gridPosition = position;
        board = parentBoard;
        UpdateColor(settings);
    }

    public void SetGridPosition(Vector2Int position)
    {
        gridPosition = position;
    }

    /// <summary>
    /// Set visibility of the piece (used during initial board setup)
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = visible;
        }
    }

    public void MoveTo(Vector3 targetPosition, float duration)
    {
        if (isMoving) return;
        StartCoroutine(MoveCoroutine(targetPosition, duration));
    }

    private System.Collections.IEnumerator MoveCoroutine(Vector3 targetPosition, float duration)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Smooth easing
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }

    private void UpdateColor(GameSettings settings = null)
    {
        if (spriteRenderer == null) return;

        Color color;
        if (settings != null)
        {
            color = settings.GetColorForType(pieceType);
        }
        else
        {
            // Fallback to default colors if no settings provided
            color = pieceType switch
            {
                PieceType.Red => new Color(0.9f, 0.2f, 0.2f),
                PieceType.Blue => new Color(0.2f, 0.5f, 0.9f),
                PieceType.Green => new Color(0.2f, 0.8f, 0.3f),
                PieceType.Yellow => new Color(0.95f, 0.85f, 0.2f),
                PieceType.Purple => new Color(0.7f, 0.2f, 0.8f),
                PieceType.Orange => new Color(0.95f, 0.5f, 0.1f),
                PieceType.White => new Color(0.95f, 0.95f, 0.95f),
                _ => Color.white
            };
        }

        spriteRenderer.color = color;
    }

    /// <summary>
    /// Set whether this piece is currently pressed/clicked
    /// The piece manages its own highlight state
    /// </summary>
    public void SetPressed(bool pressed)
    {
        // Only update if state actually changed
        if (isPressed == pressed) return;

        isPressed = pressed;
        UpdateVisualState();
    }

    /// <summary>
    /// Update the visual appearance based on current state
    /// </summary>
    private void UpdateVisualState()
    {
        if (spriteRenderer == null) return;

        // Don't interfere with pulsate animation
        if (isPulsating) return;

        // Calculate target scale
        Vector3 targetScale;
        if (isPressed)
        {
            // Gems are 75% of cell width (0.75), pressed should be max 85% (0.85)
            // Scale factor: 0.85 / 0.75 = 1.133
            targetScale = baseScale * 1.133f;
        }
        else
        {
            // Return to base scale
            targetScale = baseScale;
        }

        // Animate to target scale
        AnimateToScale(targetScale);
    }

    /// <summary>
    /// Smoothly animate to a target scale
    /// </summary>
    private void AnimateToScale(Vector3 targetScale)
    {
        // Stop any existing scale animation
        if (scaleAnimationCoroutine != null)
        {
            StopCoroutine(scaleAnimationCoroutine);
        }

        scaleAnimationCoroutine = StartCoroutine(ScaleAnimationCoroutine(targetScale));
    }

    /// <summary>
    /// Coroutine to animate scale change
    /// </summary>
    private System.Collections.IEnumerator ScaleAnimationCoroutine(Vector3 targetScale)
    {
        float baseDuration = 0.2f;
        float duration = baseDuration;

        // Apply game speed multiplier
        if (GameSpeedSettings.Instance != null)
        {
            duration = GameSpeedSettings.Instance.GetAdjustedDuration(baseDuration);
        }

        Vector3 startScale = spriteRenderer.transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Smooth easing
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            spriteRenderer.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        spriteRenderer.transform.localScale = targetScale;
        scaleAnimationCoroutine = null;
    }

    /// <summary>
    /// Legacy method for compatibility - use SetPressed instead
    /// </summary>
    public void Highlight(bool enable)
    {
        SetPressed(enable);
    }

    public void DestroyPiece()
    {
        StartCoroutine(DestroyAnimation());
    }

    private System.Collections.IEnumerator DestroyAnimation()
    {
        float baseDuration = 0.3f;
        float duration = baseDuration;

        // Apply game speed multiplier
        if (GameSpeedSettings.Instance != null)
        {
            duration = GameSpeedSettings.Instance.GetAdjustedDuration(baseDuration);
        }

        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Reset piece state for object pooling
    /// </summary>
    public void ResetForPool()
    {
        isMoving = false;
        isPulsating = false;
        isPressed = false;
        gridPosition = Vector2Int.zero;
        board = null;
        transform.localScale = baseScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    /// <summary>
    /// Start pulsating animation for hint
    /// </summary>
    public void StartPulsate()
    {
        if (isPulsating) return;
        isPulsating = true;
        pulsateCoroutine = StartCoroutine(PulsateCoroutine());
    }

    /// <summary>
    /// Stop pulsating animation
    /// </summary>
    public void StopPulsate()
    {
        if (!isPulsating) return;
        isPulsating = false;
        if (pulsateCoroutine != null)
        {
            StopCoroutine(pulsateCoroutine);
            pulsateCoroutine = null;
        }
        // Restore correct visual state (respects isPressed)
        UpdateVisualState();
    }

    private System.Collections.IEnumerator PulsateCoroutine()
    {
        Vector3 normalScale = baseScale;
        // Gems are 75% of cell width (0.75), hint should be max 85% (0.85)
        // Scale factor: 0.85 / 0.75 = 1.133
        Vector3 enlargedScale = baseScale * 1.133f;

        while (isPulsating)
        {
            // Scale up
            float elapsed = 0f;
            while (elapsed < PULSE_CYCLE_DURATION / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (PULSE_CYCLE_DURATION / 2f);
                // Smooth easing
                t = Mathf.Sin(t * Mathf.PI * 0.5f);
                transform.localScale = Vector3.Lerp(normalScale, enlargedScale, t);
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < PULSE_CYCLE_DURATION / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (PULSE_CYCLE_DURATION / 2f);
                // Smooth easing
                t = Mathf.Sin(t * Mathf.PI * 0.5f);
                transform.localScale = Vector3.Lerp(enlargedScale, normalScale, t);
                yield return null;
            }
        }
    }
}
