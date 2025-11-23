using UnityEngine;

[RequireComponent(typeof(CatchableItem))]
public class FishSwim : MonoBehaviour
{
    [Header("Area")]
    public BoxCollider2D swimArea;

    [Header("Behaviour")]
    public float minChangeTargetTime = 1f;
    public float maxChangeTargetTime = 3f;
    public float arriveDistance = 0.1f;

    private CatchableItem catchable;
    private SpriteRenderer sr;
    private Vector3 targetPos;
    private float baseSpeed;       // <- base speed from data
    private float nextChangeTime;
    private Bounds bounds;

    private void Awake()
    {
        catchable = GetComponent<CatchableItem>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Get base speed from ScriptableObject
        if (catchable != null && catchable.data != null && catchable.data.swimSpeed > 0f)
            baseSpeed = catchable.data.swimSpeed;
        else
            baseSpeed = 2f; // fallback

        // Auto-assign area from spawner if not set
        if (swimArea == null && catchable != null && catchable.spawner != null)
        {
            var spawner = catchable.spawner as CatchableSpawner;
            if (spawner != null)
            {
                swimArea = spawner.spawnArea;
            }
        }

        if (swimArea != null)
        {
            bounds = swimArea.bounds;
            ClampInsideBounds();
            PickNewTarget();
        }
    }

    private void Update()
    {
        if (catchable != null && catchable.IsCaught) return;
        if (swimArea == null) return;

        bounds = swimArea.bounds;

        Vector3 pos = transform.position;

        if (Vector2.Distance(pos, targetPos) <= arriveDistance || Time.time >= nextChangeTime)
        {
            PickNewTarget();
        }

        // ----------- HERE: apply difficulty multiplier -----------
        float speed = baseSpeed;
        if (GameManager.Instance != null)
        {
            speed *= GameManager.Instance.GetFishSpeedMultiplier();
        }
        // ---------------------------------------------------------

        Vector3 dir = (targetPos - pos).normalized;
        pos += dir * speed * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, bounds.min.x, bounds.max.x);
        pos.y = Mathf.Clamp(pos.y, bounds.min.y, bounds.max.y);
        transform.position = pos;

        if (sr != null && Mathf.Abs(dir.x) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (dir.x >= 0 ? 1 : -1);
            transform.localScale = scale;
        }
    }

    private void PickNewTarget()
    {
        if (swimArea == null) return;

        bounds = swimArea.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        targetPos = new Vector3(x, y, transform.position.z);

        nextChangeTime = Time.time + Random.Range(minChangeTargetTime, maxChangeTargetTime);
    }

    private void ClampInsideBounds()
    {
        if (swimArea == null) return;

        bounds = swimArea.bounds;
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, bounds.min.x, bounds.max.x);
        pos.y = Mathf.Clamp(pos.y, bounds.min.y, bounds.max.y);
        transform.position = pos;
    }
}
