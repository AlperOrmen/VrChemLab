using UnityEngine;

[CreateAssetMenu(fileName = "NewLiquid", menuName = "Chemistry/Liquid Data")]
public class LiquidData : ScriptableObject
{
    [Header("Temel Özellikler")]
    public string liquidName;
    public Color LiquidColor;

    [Header("Fiziksel Özellikler")]
    [Range(0, 1)] public float viscosity = 0.5f; 
    [Range(0, 1)] public float opacity = 0.8f;
    [Header("Termal Özellikler")]
    public float boilingPoint = 100f; 
    public GameObject boilingEffect;

    [Header("--- TERMAL REAKSİYON ---")]
    public bool reactsToHeat = false;        
    public float reactionTemperature = 80f;  
    public LiquidData heatReactionResult;      
    public GameObject heatReactionEffect;    

    [Header("Madde Tipi")]
    public bool isPowder = false;
}