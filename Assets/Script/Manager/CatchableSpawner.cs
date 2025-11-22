using System.Collections.Generic;
using UnityEngine;

public class CatchableSpawner : MonoBehaviour
{
    [Header("Prefabs (must have CatchableItem)")]
    public CatchableItem[] itemPrefabs;

    [Header("Spawn Settings")]
    public int maxActiveItems = 10;
    public float spawnInterval = 2f;

    [Tooltip("Min X,Y and Max X,Y world positions for random spawn area")]
    public Vector2 areaMin = new Vector2(-5f, -3f);
    public Vector2 areaMax = new Vector2(5f, 1f);

    private float spawnTimer;
    private List<CatchableItem> activeItems = new List<CatchableItem>();

    private void Start()
    {
        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        // Clean up null entries (destroyed items)
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            if (activeItems[i] == null)
                activeItems.RemoveAt(i);
        }

        if (activeItems.Count >= maxActiveItems) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnItem();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnItem()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0) return;

        // Choose random prefab
        int index = Random.Range(0, itemPrefabs.Length);
        CatchableItem prefab = itemPrefabs[index];

        // Random position in area
        float x = Random.Range(areaMin.x, areaMax.x);
        float y = Random.Range(areaMin.y, areaMax.y);
        Vector3 pos = new Vector3(x, y, 0f);

        CatchableItem instance = Instantiate(prefab, pos, Quaternion.identity);
        instance.spawner = this;      // so it can notify when destroyed
        activeItems.Add(instance);
    }

    public void NotifyItemDestroyed(CatchableItem item)
    {
        if (activeItems.Contains(item))
        {
            activeItems.Remove(item);
        }
    }
}
