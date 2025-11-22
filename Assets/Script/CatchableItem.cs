using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CatchableItem : MonoBehaviour
{
    public CatchableItemData data;
    
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
        // If the net head hits this
        NetHeadMarker netHead = other.GetComponent<NetHeadMarker>();
        if (netHead != null)
        {
            netHead.netController.CatchItem(this);
        }
    }
}
