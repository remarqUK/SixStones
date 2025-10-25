using UnityEngine;
using System.Collections;

/// <summary>
/// Modern GamePiece using Unity 6 best practices
/// - Data-driven (uses PieceTypeData)
/// - Pool-friendly (proper reset methods)
/// - Cached components (better performance)
/// </summary>
public class GamePieceModern : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CircleCollider2D circleCollider;

    private PieceTypeData pieceTypeData;
    private Vector2Int gridPosition;
    private Board board;
    private bool isMoving = false;

    // Cached transform reference
    private Transform cachedTransform;

    public PieceTypeData TypeData => pieceTypeData;
    public int TypeID => pieceTypeData?.typeID ?? -1;
    public Vector2Int GridPosition => gridPosition;
    public bool IsMoving => isMoving;

    private void Awake()
    {
        // Cache components
        cachedTransform = transform;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (circleCollider == null)
        {
            circleCollider = GetComponent<CircleCollider2D>();
        }
    }

    private void OnDestroy()
    {
        // Stop all coroutines to prevent memory leaks
        StopAllCoroutines();

        // Reset state flags
        isMoving = false;
    }

    /// <summary>
    /// Initialize piece with type data
    /// </summary>
    public void Initialize(PieceTypeData typeData, Vector2Int position, Board parentBoard)
    {
        pieceTypeData = typeData;
        gridPosition = position;
        board = parentBoard;

        UpdateVisuals();
    }

    public void SetGridPosition(Vector2Int position)
    {
        gridPosition = position;
    }

    /// <summary>
    /// Update visual appearance based on piece type data
    /// </summary>
    private void UpdateVisuals()
    {
        if (pieceTypeData == null || spriteRenderer == null) return;

        // Apply sprite if available
        if (pieceTypeData.sprite != null)
        {
            spriteRenderer.sprite = pieceTypeData.sprite;
        }

        // Apply color
        spriteRenderer.color = pieceTypeData.color;
    }

    /// <summary>
    /// Smooth movement animation
    /// </summary>
    public void MoveTo(Vector3 targetPosition, float duration)
    {
        if (isMoving) return;
        StartCoroutine(MoveCoroutine(targetPosition, duration));
    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition, float duration)
    {
        isMoving = true;
        Vector3 startPosition = cachedTransform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Smooth easing (smoothstep)
            t = t * t * (3f - 2f * t);

            cachedTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        cachedTransform.position = targetPosition;
        isMoving = false;
    }

    /// <summary>
    /// Highlight piece when selected
    /// </summary>
    public void Highlight(bool enable)
    {
        if (spriteRenderer == null) return;

        cachedTransform.localScale = enable ? Vector3.one * 1.1f : Vector3.one;
    }

    /// <summary>
    /// Play destruction animation and return to pool
    /// </summary>
    public void DestroyPiece(PiecePool pool, float duration = 0.3f)
    {
        StartCoroutine(DestroyAnimation(pool, duration));
    }

    private IEnumerator DestroyAnimation(PiecePool pool, float duration)
    {
        float elapsed = 0f;
        Vector3 startScale = cachedTransform.localScale;

        // Play match sound if available
        if (pieceTypeData?.matchSound != null)
        {
            AudioSource.PlayClipAtPoint(pieceTypeData.matchSound, cachedTransform.position);
        }

        // Spawn particle effect if available
        if (pieceTypeData?.matchParticle != null)
        {
            GameObject particle = Instantiate(pieceTypeData.matchParticle, cachedTransform.position, Quaternion.identity);
            particle.transform.localScale = Vector3.one * pieceTypeData.particleScale;
            Destroy(particle, 2f);
        }

        // Scale down animation
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            cachedTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        // Return to pool instead of destroying
        if (pool != null)
        {
            pool.Return(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Reset piece state when returned to pool
    /// Unity 6 best practice: Proper object pool cleanup
    /// </summary>
    public void ResetForPool()
    {
        isMoving = false;
        gridPosition = Vector2Int.zero;
        board = null;
        pieceTypeData = null;

        cachedTransform.localScale = Vector3.one;
        cachedTransform.localPosition = Vector3.zero;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            spriteRenderer.sprite = null;
        }
    }

    /// <summary>
    /// Check if two pieces can match
    /// </summary>
    public bool CanMatchWith(GamePieceModern other)
    {
        if (other == null || pieceTypeData == null || other.pieceTypeData == null)
            return false;

        return pieceTypeData.typeID == other.pieceTypeData.typeID;
    }

    /// <summary>
    /// Get points value for this piece
    /// </summary>
    public int GetPoints()
    {
        return pieceTypeData?.basePoints ?? 0;
    }
}
