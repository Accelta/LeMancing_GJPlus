using System.Collections.Generic;
using UnityEngine;

public class NetController : MonoBehaviour
{
    [Header("Net References")]
    public Transform netHead;
    public LineRenderer rope;

    [Header("Swing")]
    public float swingAngle = 60f;
    public float swingSpeed = 2f;

    [Header("Movement")]
    public float shootSpeed = 5f;
    public float baseReturnSpeed = 5f;
    public float maxDistance = 5f;

    private float currentReturnSpeed;
    private Vector3 netStartLocalPos;
    private float swingTimeOffset;

    [Header("Net Catch Layout")]
public float netSpreadRadius = 0.4f;    // how far from center items can be
public bool useRandomSpread = true;     // random vs arranged

    private enum NetState { Swinging, Shooting, Returning }
    private NetState state = NetState.Swinging;

    // now supports multiple items
    private List<CatchableItem> caughtItems = new List<CatchableItem>();

    private void Start()
    {
        netStartLocalPos = netHead.localPosition;
        swingTimeOffset = Random.Range(0f, 10f);
        currentReturnSpeed = baseReturnSpeed;

        if (rope == null)
            rope = GetComponent<LineRenderer>();
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

        currentReturnSpeed = baseReturnSpeed;

        // just in case, ensure no leftover children
        if (caughtItems.Count == 0 && netHead.childCount > 0)
        {
            for (int i = netHead.childCount - 1; i >= 0; i--)
                netHead.GetChild(i).SetParent(null);
        }
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
    RecalculateReturnSpeed(); // Dynamically update return speed based on weight

    netHead.localPosition = Vector3.MoveTowards(
        netHead.localPosition,
        netStartLocalPos,
        currentReturnSpeed * Time.deltaTime
    );

    if (netHead.localPosition == netStartLocalPos)
    {
        // Resolve all caught items
        foreach (var item in caughtItems)
        {
            if (item != null)
            {
                GameManager.Instance.ResolveCatch(item);
                Destroy(item.gameObject);
            }
        }

        caughtItems.Clear();
        currentReturnSpeed = baseReturnSpeed;
        state = NetState.Swinging;
    }
}

public void CatchItem(CatchableItem item)
{
    if (state == NetState.Swinging) return;
    if (item == null || caughtItems.Contains(item)) return;

    caughtItems.Add(item);
    item.transform.SetParent(netHead);

    // mark as caught so movement scripts stop
    item.SetCaught(true);

    // place item somewhere within the net area (local space)
    Vector3 localPos;

    if (useRandomSpread)
    {
        // random position in a small circle around the center
        Vector2 rand = Random.insideUnitCircle * netSpreadRadius;
        localPos = new Vector3(rand.x, rand.y, 0f);
    }
    else
    {
        // arranged in a circle based on index
        int count = caughtItems.Count;
        float angle = (count - 1) * Mathf.PI * 2f / Mathf.Max(1, count);
        float r = netSpreadRadius;
        localPos = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f);
    }

    item.transform.localPosition = localPos;

    // Optional: reset rotation so sprites look upright on the net
    item.transform.localRotation = Quaternion.identity;

    RecalculateReturnSpeed();

    // choose behaviour: immediately return when at least 1 item is caught
    if (state == NetState.Shooting)
    {
        state = NetState.Returning;
    }
}


    private void RecalculateReturnSpeed()
    {
        if (caughtItems.Count == 0)
        {
            currentReturnSpeed = baseReturnSpeed;
            return;
        }

        float totalWeight = 0f;
        foreach (var item in caughtItems)
        {
            if (item != null && item.data != null)
                totalWeight += Mathf.Max(0.1f, item.data.weight);
        }

        if (totalWeight <= 0f) totalWeight = 1f;

        // Heavier = slower
        float Weightedspeed = baseReturnSpeed / totalWeight;
        currentReturnSpeed = Weightedspeed ;
        Debug.Log("Total weight: " + totalWeight + " returnSpeed: " + currentReturnSpeed);
    }

    private void UpdateRope()
    {
        if (rope == null) return;

        rope.positionCount = 2;
        rope.SetPosition(0, transform.position);
        rope.SetPosition(1, netHead.position);
    }
}
