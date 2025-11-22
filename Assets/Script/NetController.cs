using UnityEngine;

public class NetController : MonoBehaviour
{
    [Header("Net References")]
    public Transform netHead;
    public LineRenderer rope;          // <- new

    [Header("Swing")]
    public float swingAngle = 60f;
    public float swingSpeed = 2f;

    [Header("Movement")]
    public float shootSpeed = 5f;
    public float baseReturnSpeed = 5f; // <- base speed when no weight
    public float minReturnSpeed = 1f;  // <- clamp so it never becomes too slow
    public float maxDistance = 5f;

    private float currentReturnSpeed;
    private Vector3 netStartLocalPos;
    private float swingTimeOffset;

    private enum NetState { Swinging, Shooting, Returning }
    private NetState state = NetState.Swinging;

    private CatchableItem caughtItem;

    private void Start()
    {
        netStartLocalPos = netHead.localPosition;
        swingTimeOffset = Random.Range(0f, 10f);

        currentReturnSpeed = baseReturnSpeed;

        // If you forgot to assign rope, try to get it
        if (rope == null)
        {
            rope = GetComponent<LineRenderer>();
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateNetState();
    }

    private void LateUpdate()
    {
        UpdateRope();
    }

    private void HandleInput()
    {
        if (state == NetState.Swinging)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                state = NetState.Shooting;
            }
        }
    }

    private void UpdateNetState()
    {
        switch (state)
        {
            case NetState.Swinging:
                UpdateSwing();
                break;
            case NetState.Shooting:
                UpdateShooting();
                break;
            case NetState.Returning:
                UpdateReturning();
                break;
        }
    }

    private void UpdateSwing()
    {
        float angle = Mathf.Sin((Time.time + swingTimeOffset) * swingSpeed) * swingAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        netHead.localPosition = netStartLocalPos;

        // No item caught = default return speed
        currentReturnSpeed = baseReturnSpeed;
    }

    private void UpdateShooting()
    {
        netHead.Translate(Vector3.down * shootSpeed * Time.deltaTime, Space.Self);

        float dist = Vector3.Distance(transform.position, netHead.position);
        if (dist >= maxDistance)
        {
            state = NetState.Returning;
        }
    }

    private void UpdateReturning()
    {
        netHead.localPosition = Vector3.MoveTowards(
            netHead.localPosition,
            netStartLocalPos,
            currentReturnSpeed * Time.deltaTime
        );

        if (netHead.localPosition == netStartLocalPos)
        {
            if (caughtItem != null)
            {
                GameManager.Instance.ResolveCatch(caughtItem);
                Destroy(caughtItem.gameObject);
                caughtItem = null;
            }

            state = NetState.Swinging;
        }
    }

    public void CatchItem(CatchableItem item)
    {
        if (state != NetState.Shooting) return;
        if (caughtItem != null) return;

        caughtItem = item;
        caughtItem.transform.SetParent(netHead);
        caughtItem.transform.localPosition = Vector3.zero;

        // Adjust return speed based on item weight
        if (caughtItem.data != null)
        {
            float weight = Mathf.Max(0.1f, caughtItem.data.weight);
            float speed = baseReturnSpeed / weight;
            currentReturnSpeed = Mathf.Max(minReturnSpeed, speed);
        }
        else
        {
            currentReturnSpeed = baseReturnSpeed;
        }

        state = NetState.Returning;
    }

    private void UpdateRope()
    {
        if (rope == null) return;

        rope.positionCount = 2;
        rope.SetPosition(0, transform.position);  // pivot
        rope.SetPosition(1, netHead.position);    // net head
    }
}
