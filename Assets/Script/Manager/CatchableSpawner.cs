using System.Collections.Generic;
using UnityEngine;

public class CatchableSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public CatchableItem prefab;

        [Range(0f, 1f)]
        public float probability = 1f;   // slider for each item
    }

    [Header("Spawn Entries (each has its own probability)")]
    public SpawnEntry[] spawnEntries;

    [Header("Spawn Settings")]
    public int maxActiveItems = 10;
    public float spawnInterval = 2f;

    [Header("Spawn Area")]
    public BoxCollider2D spawnArea;

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
        if (spawnEntries == null || spawnEntries.Length == 0) return;
        if (spawnArea == null)
        {
            Debug.LogWarning("CatchableSpawner: spawnArea is not assigned!");
            return;
        }

        // Calculate total probability
        float totalWeight = 0;
        foreach (var entry in spawnEntries)
        {
            totalWeight += Mathf.Max(0f, entry.probability);
        }

        if (totalWeight <= 0f) return; // nothing to spawn

        // Weighted random selection
        float r = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        SpawnEntry chosen = null;
        foreach (var entry in spawnEntries)
        {
            cumulative += entry.probability;
            if (r <= cumulative)
            {
                chosen = entry;
                break;
            }
        }

        if (chosen == null || chosen.prefab == null) return;

        // Pick random position inside spawn area
        Bounds b = spawnArea.bounds;
        float x = Random.Range(b.min.x, b.max.x);
        float y = Random.Range(b.min.y, b.max.y);
        Vector3 pos = new Vector3(x, y, 0f);

        // Spawn
        CatchableItem instance = Instantiate(chosen.prefab, pos, Quaternion.identity);
        instance.spawner = this;
        activeItems.Add(instance);
    }

    public void NotifyItemDestroyed(CatchableItem item)
    {
        if (activeItems.Contains(item))
            activeItems.Remove(item);
    }
}
