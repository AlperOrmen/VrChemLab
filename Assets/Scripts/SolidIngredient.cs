using UnityEngine;

public class SolidIngredient : MonoBehaviour
{
    [Header("--- KİMYASAL VERİ ---")]
    public LiquidData substanceData;
    public float equivalentAmount = 0.2f;

    [Header("--- EFEKTLER ---")]
    public AudioClip entrySound;
}