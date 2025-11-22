using UnityEngine;

[RequireComponent(typeof(CatchableItem))]
public class FishSwim : MonoBehaviour
{
    [Header("Area")]
    public BoxCollider2D swimArea;       // if null, will try to use spawner.spawnArea

    [Header("Behaviour")]
    public float minChangeTargetTime = 1f;
    public float maxChangeTargetTime = 3f;
    public float arriveDistance = 0.1f;  // when this close, pick new target

    private CatchableItem catchable;
    private SpriteRenderer sr;
    private Vector3 targetPos;
    private float speed;
    private float nextChangeTime;
    private Bounds bounds;

    private void Awake()
    {
        catchable = GetComponent<CatchableItem>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Get swim speed from data
        if (catchable != null && catchable.data != null && catchable.data.swimSpeed > 0f)
            speed = catchable.data.swimSpeed;
        else
            speed = 2f; // fallback

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
            ClampInsideBounds();       // ensure start inside
            PickNewTarget();
        }
        else
        {
            Debug.LogWarning("FishSwim: no swimArea assigned and no spawner.spawnArea found.");
        }
    }

    private void Update()
    {
        if (catchable != null && catchable.IsCaught) return; // stop if caught
        if (swimArea == null) return;

        bounds = swimArea.bounds; // update in case area moves

        Vector3 pos = transform.position;

        // Change target if close or timer elapsed
        if (Vector2.Distance(pos, targetPos) <= arriveDistance || Time.time >= nextChangeTime)
        {
            PickNewTarget();
        }

        // Move toward target
        Vector3 dir = (targetPos - pos).normalized;
        pos += dir * speed * Time.deltaTime;

        // Clamp inside area
        pos.x = Mathf.Clamp(pos.x, bounds.min.x, bounds.max.x);
        pos.y = Mathf.Clamp(pos.y, bounds.min.y, bounds.max.y);
        transform.position = pos;

        // Visual flip (left/right)
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
