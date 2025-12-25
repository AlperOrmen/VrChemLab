using UnityEngine;

public enum ReactionType {
    ColorChange,
    Precipitate,
    GasRelease, 
    Dissolve,   
    Exothermic,   
    Emulsion,     

    Foaming,       

    Explosion,    
    Electrical,

    Explosive
}

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Chemistry/Recipe")]
public class LiquidRecipe : ScriptableObject {
    [Header("Girdi Malzemeler")]
    public LiquidData inputA; 
    public LiquidData inputB; 

    [Header("Sonuçlar")]
    public LiquidData resultLiquid; 
    public ReactionType reactionType; 

    [Header("Görsel & İşitsel Efektler")]
    public GameObject visualEffectPrefab;
    public AudioClip soundEffect;     

    [Tooltip("Efekt kaç saniye sonra yok olsun?")]
    public float effectDuration = 2.0f; 
}