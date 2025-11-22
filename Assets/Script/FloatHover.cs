using UnityEngine;

[RequireComponent(typeof(CatchableItem))]
public class FloatHover : MonoBehaviour
{
    public float amplitude = 0.2f;    // how high it moves up/down
    public float frequency = 1f;      // speed of bobbing

    private float startY;
    private CatchableItem catchable;

    private void Awake()
    {
        catchable = GetComponent<CatchableItem>();
    }

    private void Start()
    {
        startY = transform.position.y;
    }

    private void Update()
    {
        // Stop when caught, so it doesn't wiggle in the net
        if (catchable != null && catchable.IsCaught) return;

        Vector3 pos = transform.position;
        pos.y = startY + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = pos;
    }
}
