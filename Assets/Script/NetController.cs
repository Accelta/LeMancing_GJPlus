using UnityEngine;

public class NetController : MonoBehaviour
{
    public Transform netHead;          // The net tip object (child of NetPivot)
    public float swingAngle = 60f;     // Max angle left/right
    public float swingSpeed = 2f;      // How fast it swings
    public float shootSpeed = 5f;      // Speed when going out
    public float returnSpeed = 5f;     // Speed when coming back
    public float maxDistance = 5f;     // Max length of the rope/net

    private Vector3 netStartLocalPos;  // Local start position relative to pivot
    private float swingTimeOffset;
    
    private enum NetState { Swinging, Shooting, Returning }
    private NetState state = NetState.Swinging;

    private CatchableItem caughtItem;

    private void Start()
    {
        netStartLocalPos = netHead.localPosition;
        swingTimeOffset = Random.Range(0f, 10f); // Just to desync multiple nets if needed
    }

    private void Update()
    {
        HandleInput();
        UpdateNetState();
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
        // Rotate pivot like a pendulum
        float angle = Mathf.Sin((Time.time + swingTimeOffset) * swingSpeed) * swingAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Keep net head at start local position (so only rotation changes)
        netHead.localPosition = netStartLocalPos;
    }

    private void UpdateShooting()
    {
        // Move net along its local down direction
        netHead.Translate(Vector3.down * shootSpeed * Time.deltaTime, Space.Self);

        float dist = Vector3.Distance(transform.position, netHead.position);
        if (dist >= maxDistance)
        {
            state = NetState.Returning;
        }
    }

    private void UpdateReturning()
    {
        // Move net head back to start local position
        netHead.localPosition = Vector3.MoveTowards(
            netHead.localPosition,
            netStartLocalPos,
            returnSpeed * Time.deltaTime
        );

        if (netHead.localPosition == netStartLocalPos)
        {
            // Reached pivot => resolve catch if we have one
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
        if (caughtItem != null) return; // Already have something

        caughtItem = item;
        // Parent item to net head so it follows
        caughtItem.transform.SetParent(netHead);
        // Optional: align it
        caughtItem.transform.localPosition = Vector3.zero;

        state = NetState.Returning;
    }
}
