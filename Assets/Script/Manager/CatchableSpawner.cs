using System.Collections.Generic;
using UnityEngine;

public class CatchableSpawner : MonoBehaviour
{
    [Header("Prefabs (must have CatchableItem)")]
    public CatchableItem[] itemPrefabs;

    [Header("Spawn Settings")]
    public int maxActiveItems = 10;
    public float spawnInterval = 2f;

    [Header("Spawn Area")]
    public BoxCollider2D spawnArea;   // <- assign your invisible box here

    private float spawnTimer;
    private List<CatchableItem> activeItems = new List<CatchableItem>();

    private void Start()
    {
        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        // Clean null entries
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
        if (spawnArea == null)
        {
            Debug.LogWarning("CatchableSpawner: spawnArea is not assigned!");
            return;
        }

        int index = Random.Range(0, itemPrefabs.Length);
        CatchableItem prefab = itemPrefabs[index];

        Bounds b = spawnArea.bounds;
        float x = Random.Range(b.min.x, b.max.x);
        float y = Random.Range(b.min.y, b.max.y);
        Vector3 pos = new Vector3(x, y, 0f);

        CatchableItem instance = Instantiate(prefab, pos, Quaternion.identity);
        instance.spawner = this;
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
