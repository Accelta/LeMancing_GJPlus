using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class NetController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Visual rope (child of this pivot). This will be scaled to represent rope length.")]
    public Transform ropeChild;
    [Tooltip("Separate net GameObject that sits at rope tip. DO NOT make this a child of ropeChild.")]
    public Transform netAtTip;

    [Header("Swing (pivot)")]
    public float swingAngle = 60f;
    public float swingSpeed = 2f;
    public float swingTimeOffset = 0f;

    [Header("Movement / distances (world units)")]
    public float idleLength = 1f;         // resting rope length
    public float maxLength = 5f;          // maximum extension
    public float extendSpeed = 8f;        // units/sec while shooting
    public float baseReturnSpeed = 5f;    // base units/sec when returning (divided by weight)
    private float currentReturnSpeed;

    [Header("Net catch layout")]
    public float netSpreadRadius = 0.4f;
    public bool useRandomSpread = true;

    [Header("Rope visual detection")]
    [Tooltip("Visual length of ropeChild when localScale.y == 1 (in world units). If 0 the script will try to auto-detect).")]
    public float baseVisualLength = 0f;

    private float currentLength;
    private Vector3 initialRopeLocalScale;
    private Vector3 initialRopeLocalPos;

    private enum NetState { Swinging, Shooting, Returning }
    private NetState state = NetState.Swinging;

    private List<CatchableItem> caughtItems = new List<CatchableItem>();

    private void Start()
    {
        if (ropeChild == null)
            Debug.LogError("[NetController] ropeChild not assigned.");

        if (netAtTip == null)
            Debug.LogWarning("[NetController] netAtTip not assigned. You should assign a separate net object.");

        // auto-detect baseVisualLength if not set
        if (baseVisualLength <= 0.0001f && ropeChild != null)
        {
            var sr = ropeChild.GetComponent<SpriteRenderer>();
            if (sr != null)
                baseVisualLength = Mathf.Abs(sr.bounds.size.y / ropeChild.lossyScale.y);
            else
            {
                var r = ropeChild.GetComponent<Renderer>();
                if (r != null)
                    baseVisualLength = Mathf.Abs(r.bounds.size.y / ropeChild.lossyScale.y);
            }
            if (baseVisualLength <= 0.0001f)
                baseVisualLength = 1f; // fallback
        }

        initialRopeLocalScale = ropeChild != null ? ropeChild.localScale : Vector3.one;
        initialRopeLocalPos = ropeChild != null ? ropeChild.localPosition : Vector3.zero;

        currentLength = idleLength;
        currentReturnSpeed = baseReturnSpeed;

        UpdateRopeVisual();
        UpdateNetTip(); // place netAtTip initially
    }

    private void Update()
    {
        HandleInput();
        UpdateState();
        UpdateRopeVisual();
        UpdateNetTip();
    }

    // input toggles shooting only while swinging
    private void HandleInput()
    {
        if (state == NetState.Swinging)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                SoundManager.PlaySFX("NetLaunch");
                SoundManager.PlaySFX("WaterDrop");

                state = NetState.Shooting;
            }
        }
    }

    private void UpdateState()
    {
        switch (state)
        {
            case NetState.Swinging:
                DoSwing();
                currentReturnSpeed = baseReturnSpeed;
                // Cleanup any stray children on netAtTip if needed
                if (caughtItems.Count == 0 && netAtTip != null && netAtTip.childCount > 0)
                {
                    for (int i = netAtTip.childCount - 1; i >= 0; i--)
                        netAtTip.GetChild(i).SetParent(null);
                }
                break;

            case NetState.Shooting:
                // Extend rope length until reach maxLength, then switch to Returning
                currentLength += extendSpeed * Time.deltaTime;
                if (currentLength >= maxLength)
                {
                    currentLength = maxLength;
                    state = NetState.Returning;
                }
                break;

            case NetState.Returning:
                // Update return speed based on weight
                RecalculateReturnSpeed();

                currentLength = Mathf.MoveTowards(currentLength, idleLength, currentReturnSpeed * Time.deltaTime);

                // When back to idle, resolve caught items
                if (Mathf.Approximately(currentLength, idleLength))
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
                break;
        }
    }

    private void DoSwing()
    {
        float angle = Mathf.Sin((Time.time + swingTimeOffset) * swingSpeed) * swingAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void UpdateRopeVisual()
    {
        if (ropeChild == null) return;

        // compute scale factor so rope visual length equals currentLength (world units)
        float scaleY = currentLength / Mathf.Max(0.0001f, baseVisualLength);
        ropeChild.localScale = new Vector3(initialRopeLocalScale.x, initialRopeLocalScale.y * scaleY, initialRopeLocalScale.z);

        // move ropeChild so pivot stays at the top: set its localPosition down by half visual length
        float halfLengthLocal = (baseVisualLength * ropeChild.localScale.y) * 0.5f;
        ropeChild.localPosition = new Vector3(initialRopeLocalPos.x, -halfLengthLocal, initialRopeLocalPos.z);
        ropeChild.localRotation = Quaternion.identity;
    }

    private void UpdateNetTip()
    {
        if (netAtTip == null) return;

        // tip local position relative to pivot (down by full currentLength)
        Vector3 tipLocal = Vector3.down * currentLength;

        // convert to world and apply to netAtTip
        Vector3 tipWorld = transform.TransformPoint(tipLocal);
        netAtTip.position = tipWorld;

        // match rotation so the net faces the same direction as pivot
        netAtTip.rotation = transform.rotation;
    }

    // Called by NetHeadMarker (or any collision handler) when a CatchableItem touches the net tip
    public void CatchItem(CatchableItem item)
    {
        if (state == NetState.Swinging) return;        // only catch while shooting/returning
        if (item == null || caughtItems.Contains(item)) return;

        SoundManager.PlaySFX("CatchFish");

        caughtItems.Add(item);
        ComboManager.Instance.RegisterCatch();

        // parent the caught item to netAtTip so it follows the net but won't be scaled by the rope
        if (netAtTip != null)
        {
            item.transform.SetParent(netAtTip);
            item.transform.localPosition = Vector3.zero; // center on the net tip; you can offset if wanted

            // optional: spread items around the net tip instead of stacking (use local positions)
            if (useRandomSpread)
            {
                Vector2 rand = Random.insideUnitCircle * netSpreadRadius;
                item.transform.localPosition = new Vector3(rand.x, rand.y, 0f);
            }
            else
            {
                int count = caughtItems.Count;
                float angle = (count - 1) * Mathf.PI * 2f / Mathf.Max(1, count);
                float r = netSpreadRadius;
                item.transform.localPosition = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f);
            }
        }

        // stop movement of the caught item (if it has movement scripts that respect IsCaught)
        item.SetCaught(true);

        // as soon as we catch at least one item while shooting, switch to returning
        if (state == NetState.Shooting)
            state = NetState.Returning;

        RecalculateReturnSpeed();
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

        currentReturnSpeed = baseReturnSpeed / totalWeight;
        Debug.Log("[NetController] Total weight: " + totalWeight + " returnSpeed: " + currentReturnSpeed);
    }
}
