using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CatchableItem : MonoBehaviour
{
    public CatchableItemData data;

    [HideInInspector]
    public CatchableSpawner spawner;

    public bool IsCaught { get; private set; }   // <- NEW

    public void SetCaught(bool caught)          // <- NEW
    {
        IsCaught = caught;
    }

    private void Start()
    {
        if (data != null && data.sprite != null)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = data.sprite;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        NetHeadMarker netHead = other.GetComponent<NetHeadMarker>();
        if (netHead != null)
        {
            netHead.netController.CatchItem(this);
        }
    }

    private void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.NotifyItemDestroyed(this);
        }
    }
}
