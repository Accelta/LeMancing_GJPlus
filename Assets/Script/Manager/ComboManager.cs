using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ComboEntry
{
    [Tooltip("Icon shown when this combo level is active")]
    public Sprite icon;

    [Tooltip("Text shown when this combo level is active (can include {0} for count)")]
    public string text;

    [Tooltip("Optional AudioClip to play when this combo level is reached")]
    public AudioClip clip;

    [Header("Optional per-entry animation")]
    [Tooltip("Optional Animator trigger name to fire when this entry becomes active. If empty, the global defaultTrigger is used.")]
    public string animatorTrigger;

    [Tooltip("Optional legacy Animation clip name to play on the combo UI (used if Animator isn't assigned).")]
    public string legacyAnimationClipName;
}

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    [Header("Combo Entries (index 0 => Combo x1, index 1 => Combo x2, ... )")]
    public List<ComboEntry> comboEntries = new List<ComboEntry>();

    [Header("UI")]
    public Image comboIcon;                 // optional, assign a UI Image to show entry icon
    public TextMeshProUGUI comboText;       // main combo text (e.g., "Combo x3")

    [Header("Behaviour")]
    [Tooltip("Seconds until combo resets after last catch")]
    public float comboResetTime = 2f;

    [Tooltip("How big the UI pulses on increment.")]
    public float uiPulseScale = 1.4f;
    [Tooltip("How fast the pulse eases back.")]
    public float uiPulseSpeed = 6f;

    [Header("Animation (optional)")]
    [Tooltip("Animator that controls your combo UI. If assigned, triggers will be fired on increment.")]
    public Animator comboAnimator;
    [Tooltip("Default trigger name to use when a ComboEntry doesn't specify one.")]
    public string defaultAnimatorTrigger = "Play";

    [Tooltip("If no Animator is assigned, this will try to play legacy Animation clips on the comboText or comboIcon GameObjects.")]
    public Animation comboLegacyAnimation; // optional: a legacy Animation component you may attach

    [Header("Fallback audio (if an entry doesn't have a clip)")]
    public AudioClip fallbackIncrementClip;
    public AudioClip fallbackMaxClip;
    public AudioSource fallbackSource; // optional, will be created if null

    private int currentCombo = 0;          // 0 = no combo; 1 = first catch => index 0 in comboEntries
    private float lastCatchTime = -999f;
    private Vector3 comboTextDefaultScale;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (comboText != null)
            comboTextDefaultScale = comboText.rectTransform.localScale;
        else
            comboTextDefaultScale = Vector3.one;

        if (fallbackSource == null)
        {
            GameObject go = new GameObject("ComboFallbackAudioSource");
            go.transform.SetParent(transform);
            fallbackSource = go.AddComponent<AudioSource>();
            fallbackSource.playOnAwake = false;
        }

        // hide UI initially
        if (comboText != null)
            comboText.gameObject.SetActive(false);
        if (comboIcon != null)
            comboIcon.gameObject.SetActive(false);
    }

    private void Update()
    {
        // auto-reset after timeout
        if (currentCombo > 0 && Time.time - lastCatchTime >= comboResetTime)
        {
            ResetCombo();
        }

        // smooth UI pulse back to normal if active
        if (comboText != null && comboText.gameObject.activeSelf)
        {
            comboText.rectTransform.localScale = Vector3.Lerp(
                comboText.rectTransform.localScale,
                comboTextDefaultScale,
                Time.deltaTime * uiPulseSpeed
            );
        }
    }

    /// <summary>
    /// Call this when a fish is caught by the net.
    /// Preferably call from NetController.CatchItem(...) right after a successful catch.
    /// </summary>
    public void RegisterCatch()
    {
        lastCatchTime = Time.time;

        // increment and clamp to available entries (max combo is comboEntries.Count)
        int maxCombo = Mathf.Max(1, comboEntries.Count);
        currentCombo = Mathf.Min(maxCombo, currentCombo + 1);

        // update UI and play SFX for this entry
        ShowComboUI();

        // play clip defined for this entry or fallback
        PlayEntryClip(currentCombo);

        // play animation for this combo entry
        PlayComboAnimation(currentCombo);
    }

    private void ShowComboUI()
    {
        if (comboText == null && comboIcon == null) return;

        int displayCount = Mathf.Max(1, currentCombo);
        int entryIndex = Mathf.Clamp(currentCombo - 1, 0, comboEntries.Count - 1);

        // update text
        if (comboText != null)
        {
            string t = (entryIndex >= 0 && entryIndex < comboEntries.Count && !string.IsNullOrEmpty(comboEntries[entryIndex].text))
                ? string.Format(comboEntries[entryIndex].text, displayCount)
                : $"Combo x{displayCount}";

            comboText.gameObject.SetActive(true);
            comboText.text = t;
            comboText.rectTransform.localScale = comboTextDefaultScale * uiPulseScale;
        }

        // update icon
        if (comboIcon != null)
        {
            if (entryIndex >= 0 && entryIndex < comboEntries.Count && comboEntries[entryIndex].icon != null)
            {
                comboIcon.gameObject.SetActive(true);
                comboIcon.sprite = comboEntries[entryIndex].icon;
            }
            else
            {
                comboIcon.gameObject.SetActive(false);
            }
        }
    }

    private void PlayEntryClip(int comboCount)
    {
        if (comboEntries == null || comboEntries.Count == 0)
        {
            // fallback behaviour: play generic increment or max if at cap
            if (comboCount >= comboEntries.Count && fallbackMaxClip != null)
            {
                fallbackSource.PlayOneShot(fallbackMaxClip);
            }
            else if (fallbackIncrementClip != null)
            {
                fallbackSource.PlayOneShot(fallbackIncrementClip);
            }
            return;
        }

        int entryIndex = Mathf.Clamp(comboCount - 1, 0, comboEntries.Count - 1);
        var entry = comboEntries[entryIndex];

        // Prefer AudioClip defined on entry
        if (entry != null && entry.clip != null)
        {
            fallbackSource.PlayOneShot(entry.clip);
            return;
        }

        // If no entry clip, fallback to SoundManager by expected name "ComboX" (optional)
        string expectedName = $"Combo{comboCount}";
        bool played = false;
        try
        {
            // try using SoundManager if present and has PlaySFX(string)
            if (SoundManager.Instance != null)
            {
                SoundManager.PlaySFX(expectedName);
                played = true;
            }
        }
        catch { played = false; }

        if (!played)
        {
            // use fallback clips
            if (comboCount >= comboEntries.Count && fallbackMaxClip != null)
                fallbackSource.PlayOneShot(fallbackMaxClip);
            else if (fallbackIncrementClip != null)
                fallbackSource.PlayOneShot(fallbackIncrementClip);
        }
    }

    private void PlayComboAnimation(int comboCount)
    {
        if (comboEntries == null || comboEntries.Count == 0) return;

        int entryIndex = Mathf.Clamp(comboCount - 1, 0, comboEntries.Count - 1);
        var entry = comboEntries[entryIndex];

        // 1) Animator path (recommended)
        if (comboAnimator != null)
        {
            string triggerToUse = !string.IsNullOrEmpty(entry.animatorTrigger) ? entry.animatorTrigger : defaultAnimatorTrigger;

            // Defensive: check that parameter exists (best-effort)
            // Note: Animator.HasState can't check triggers directly in a portable way, we will just SetTrigger.
            comboAnimator.SetTrigger(triggerToUse);
            return;
        }

        // 2) Legacy Animation component on a provided Animation component reference
        if (comboLegacyAnimation != null)
        {
            string clipName = !string.IsNullOrEmpty(entry.legacyAnimationClipName) ? entry.legacyAnimationClipName : null;
            if (!string.IsNullOrEmpty(clipName) && comboLegacyAnimation.GetClip(clipName) != null)
            {
                comboLegacyAnimation.Play(clipName);
                return;
            }
        }

        // 3) Try an Animation component on the comboText GameObject
        if (comboText != null)
        {
            var anim = comboText.GetComponent<Animation>();
            if (anim != null)
            {
                string clipName = !string.IsNullOrEmpty(entry.legacyAnimationClipName) ? entry.legacyAnimationClipName : null;
                if (!string.IsNullOrEmpty(clipName) && anim.GetClip(clipName) != null)
                {
                    anim.Play(clipName);
                    return;
                }

                // otherwise try playing the default clip if available
                if (anim.clip != null)
                {
                    anim.Play();
                    return;
                }
            }
        }

        // 4) Try an Animation component on the comboIcon GameObject
        if (comboIcon != null)
        {
            var anim = comboIcon.GetComponent<Animation>();
            if (anim != null)
            {
                string clipName = !string.IsNullOrEmpty(entry.legacyAnimationClipName) ? entry.legacyAnimationClipName : null;
                if (!string.IsNullOrEmpty(clipName) && anim.GetClip(clipName) != null)
                {
                    anim.Play(clipName);
                    return;
                }

                if (anim.clip != null)
                {
                    anim.Play();
                    return;
                }
            }
        }

        // If nothing found, do nothing — UI pulse already provides visual feedback
    }

    private void ResetCombo()
    {
        currentCombo = 0;
        lastCatchTime = -999f;

        if (comboText != null)
            comboText.gameObject.SetActive(false);
        if (comboIcon != null)
            comboIcon.gameObject.SetActive(false);
    }

    public void ClearCombo()
    {
        ResetCombo();
    }

    public int GetCurrentCombo() => currentCombo;
}
