using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("SFX Library")]
    public List<NamedSFX> sfxList = new List<NamedSFX>();

    private Dictionary<string, AudioClip> sfxDict;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Convert List → Dictionary for fast lookup
        sfxDict = new Dictionary<string, AudioClip>();
        foreach (var entry in sfxList)
        {
            if (!sfxDict.ContainsKey(entry.name))
                sfxDict.Add(entry.name, entry.clip);
        }
    }

    /// <summary>
    /// Plays an SFX by name key ("NetLaunch", "CatchFish", etc.)
    /// </summary>
    public static void PlaySFX(string name)
    {
        if (Instance == null)
        {
            Debug.LogWarning("SoundManager not present in scene.");
            return;
        }

        if (!Instance.sfxDict.ContainsKey(name))
        {
            Debug.LogWarning("No SFX named: " + name);
            return;
        }

        Instance.sfxSource.PlayOneShot(Instance.sfxDict[name]);
    }
}

[System.Serializable]
public class NamedSFX
{
    public string name;      // SFX ID
    public AudioClip clip;   // Audio file
}
