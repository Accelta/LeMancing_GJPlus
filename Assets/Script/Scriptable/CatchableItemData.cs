using UnityEngine;

[CreateAssetMenu(fileName = "NewCatchableItem", menuName = "Fishing/Catchable Item")]
public class CatchableItemData : ScriptableObject
{
    public string itemName;
    public Sprite sprite;

    [Header("Score")]
    public int scoreValue = 10;

    [Header("Hazard")]
    public bool isHazard = false;
    public int damageAmount = 1;  // Only used if isHazard == true

    [Header("Gameplay")]
    public float weight = 1f;     // You could use this later to slow down return speed

    [Header("Movement")]
    public float swimSpeed = 2f;
}
