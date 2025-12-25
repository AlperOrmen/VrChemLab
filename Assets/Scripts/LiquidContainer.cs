using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LiquidContainer : MonoBehaviour
{
    [Header("--- BİLEŞENLER ---")]
    [Tooltip("Sıvı shader'ına sahip mesh renderer")]
    public Renderer cupRenderer;

    [Header("--- SIVI DURUMU ---")]
    [Range(0, 1)] public float currentFillAmount = 0f;
    public LiquidData contentData;

    [Header("--- TERMAL DURUM ---")]
    public float currentTemperature = 20f;
    public float roomTemperature = 20f;
    public float maxTemperature = 300f;
    public float coolingRate = 5f;
    private bool isBoiling = false;

    [Header("--- OPTİMİZE EDİLMİŞ EFEKTLER (Play/Stop) ---")]
    public ParticleSystem boilingParticle;      
    public ParticleSystem sceneFoamParticle;    
    
    [Header("--- ELEKTRİK EFEKTİ ---")]
    public ParticleSystem electricityParticle;  
    public Light electricityLight;              
    public float electricityDuration = 3.0f;    
    private bool isElectrified = false;

    [Header("--- PATLAMA EFEKTİ ---")]
    public ParticleSystem explosionParticle; 
    public AudioClip explosionSound;
    public float explosionForce = 500f;

    [Header("--- SIVI DÖKME (LINE RENDERER) AYARLARI ---")]
    [Tooltip("Suyun akacağı uç nokta (Bardağın ağzı)")]
    public Transform spoutPoint; 
    [Tooltip("Görsel su çizgisi")]
    public LineRenderer liquidLineRenderer; 
    [Tooltip("Ne kadar eğilince dökülsün? (Derece)")]
    public float pourThreshold = 45f;
    [Tooltip("Saniyede ne kadar sıvı aksın?")]
    public float flowRate = 0.2f;
    [Tooltip("Raycast menzili (Ne kadar uzağa dökebilir?)")]
    public float maxStreamLength = 1.0f;
    public LayerMask hitLayers;

    [Header("--- RENK AYARLARI ---")]
    public float colorMixSpeed = 3.0f;
    [SerializeField] private Color currentColor = Color.clear;
    [SerializeField] private Color targetColor = Color.clear;

    [Header("--- ANLIK EFEKT AYARLARI ---")]
    public float effectCooldown = 0.2f;
    private float lastEffectTime = 0f;
    private LiquidRecipe lastReactedRecipe;

    [Header("--- KALİBRASYON ---")]
    public float minFillHeight = 0.149f;
    public float maxFillHeight = 0.798f;

    [Header("--- KÖPÜRME AYARLARI ---")]
    public float foamGrowthRate = 0.15f;
    public bool isFoaming = false;
    
    [Header("--- SIFIRLAMA (RESET) AYARLARI ---")]
    public bool autoReset = true;       
    public float resetDelay = 5.0f;     
    private bool isResetting = false;   

    [Header("--- DEBUG ---")]
    public bool enableDebugLogs = true;

    private MaterialPropertyBlock _propBlock; 
    private static readonly int FillID = Shader.PropertyToID("_Fill");
    private static readonly int SideColorID = Shader.PropertyToID("_SideColor");
    private static readonly int TopColorID = Shader.PropertyToID("_TopColor");

    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        
        if (liquidLineRenderer != null) liquidLineRenderer.enabled = false;
    }

    private void Start()
    {
        StopAllEffects();

        if (contentData != null && currentFillAmount > 0)
        {
            currentColor = contentData.LiquidColor;
            targetColor = contentData.LiquidColor;
        }
        else
        {
            ResetContainer();
        }
        UpdateShader();
    }

    private void StopAllEffects()
    {
        if (sceneFoamParticle != null) { sceneFoamParticle.Stop(); sceneFoamParticle.Clear(); }
        if (boilingParticle != null) { boilingParticle.Stop(); boilingParticle.Clear(); }
        if (electricityParticle != null) { electricityParticle.Stop(); electricityParticle.Clear(); }
        if (electricityLight != null) { electricityLight.enabled = false; }
        if (liquidLineRenderer != null) liquidLineRenderer.enabled = false;
    }

    private void Update()
    {
        if (currentColor != targetColor)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorMixSpeed);
            UpdateShader();
        }

        if (currentTemperature > roomTemperature) 
        {
            currentTemperature -= coolingRate * Time.deltaTime;
            if (currentTemperature < roomTemperature) currentTemperature = roomTemperature;
        }

        CheckBoilingState();
        CheckThermalReaction();
        
        HandlePouring();
    }

    private void HandlePouring()
    {
        if (currentFillAmount <= 0) 
        {
            if (liquidLineRenderer != null) liquidLineRenderer.enabled = false;
            return;
        }

        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);
        bool isPouring = tiltAngle > pourThreshold;

        if (isPouring)
        {
            if (liquidLineRenderer != null) liquidLineRenderer.enabled = true;
            
            Vector3 startPos = spoutPoint != null ? spoutPoint.position : transform.position;
            Vector3 direction = Vector3.down;
            
            RaycastHit hit;
            Vector3 endPos = startPos + (direction * maxStreamLength);

            if (Physics.Raycast(startPos, direction, out hit, maxStreamLength, hitLayers))
            {
                endPos = hit.point;

                LiquidContainer targetContainer = hit.collider.GetComponentInParent<LiquidContainer>();
                
                if (targetContainer != null && targetContainer != this)
                {
                    float amountToTransfer = flowRate * Time.deltaTime;

                    if (currentFillAmount >= amountToTransfer)
                    {
                        currentFillAmount -= amountToTransfer;
                        targetContainer.AddLiquid(this.contentData, amountToTransfer);
                    }
                }
            }

            if (liquidLineRenderer != null)
            {
                liquidLineRenderer.SetPosition(0, startPos);
                liquidLineRenderer.SetPosition(1, endPos);
                
                liquidLineRenderer.startColor = currentColor;
                liquidLineRenderer.endColor = currentColor;
                
                liquidLineRenderer.startWidth = 0.02f;
                liquidLineRenderer.endWidth = 0.01f;
            }
        }
        else
        {
            if (liquidLineRenderer != null) liquidLineRenderer.enabled = false;
        }
    }

    public void AddLiquid(LiquidData incomingLiquid, float amount)
    {
        if (enableDebugLogs) 
        {
            string liquidName = incomingLiquid != null ? incomingLiquid.name : "NULL (Boş Veri)";
        }

        if (isFoaming || isResetting || isElectrified) 
        {
            return;
        }

        if (amount <= 0.0001f) 
        {
             return;
        }
        
        if (incomingLiquid == null)
        {
            return;
        }


        float totalAmount = currentFillAmount + amount;
        currentTemperature = ((currentFillAmount * currentTemperature) + (amount * roomTemperature)) / totalAmount;
        
        if (currentFillAmount <= 0.01f || contentData == null)
        {

            contentData = incomingLiquid;
            targetColor = incomingLiquid.LiquidColor;
            currentColor = incomingLiquid.LiquidColor;
            lastReactedRecipe = null;
            currentFillAmount += amount;
        }
        else
        {
            LiquidRecipe recipe = ReactionRegistry.Instance.CheckReaction(contentData, incomingLiquid);
            
            if (recipe != null)
            {

                lastReactedRecipe = recipe;
                
                if (recipe.reactionType != ReactionType.Explosive)
                {
                    contentData = recipe.resultLiquid;
                    targetColor = recipe.resultLiquid.LiquidColor;
                }
                
                if (recipe.reactionType == ReactionType.Foaming)
                {
                    TriggerFoamReaction(recipe.effectDuration);
                }
                else if (recipe.reactionType == ReactionType.Electrical)
                {
                     TriggerElectricity(recipe.effectDuration);
                }
                else if (recipe.reactionType == ReactionType.Explosive)
                {
                     TriggerExplosion();
                     return;
                }
                else if (Time.time > lastEffectTime + effectCooldown)
                {
                    HandleReactionEffect(recipe);
                    lastEffectTime = Time.time;
                }
            }
            else
            {

                bool isIngredient = (lastReactedRecipe != null) && 
                                    (incomingLiquid == lastReactedRecipe.inputA || incomingLiquid == lastReactedRecipe.inputB);
                
                if (contentData != incomingLiquid && !isIngredient)
                {
                    targetColor = Color.Lerp(targetColor, incomingLiquid.LiquidColor, amount / totalAmount);
                }
            }
            currentFillAmount += amount;
        }

        currentFillAmount = Mathf.Clamp01(currentFillAmount);
        
        
        UpdateShader();
    }

    public void TriggerExplosion()
    {
        if (isResetting) return;

        if (explosionParticle != null) explosionParticle.Play();
        if (explosionSound != null) AudioSource.PlayClipAtPoint(explosionSound, transform.position);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddExplosionForce(explosionForce, transform.position - Vector3.up, 1f);
        }

        currentFillAmount = 0f;
        contentData = null;
        currentColor = Color.clear; 
        targetColor = Color.clear;
        currentTemperature = roomTemperature;
        
        UpdateShader();

        StopAllCoroutines();
        isFoaming = false;
        isElectrified = false;
        if (electricityLight != null) electricityLight.enabled = false;
        
        if (sceneFoamParticle != null) sceneFoamParticle.Stop();
        if (boilingParticle != null) boilingParticle.Stop();
        if (electricityParticle != null) electricityParticle.Stop();
    }

    public void TriggerElectricity(float duration)
    {
        if (isElectrified) return;
        StartCoroutine(ElectricityRoutine(duration));
    }

    private IEnumerator ElectricityRoutine(float duration)
    {
        isElectrified = true;
        float timer = 0f;
        float actualDuration = duration > 0 ? duration : electricityDuration;

        if (electricityParticle != null) electricityParticle.Play();
        if (electricityLight != null) electricityLight.enabled = true;
        while (timer < actualDuration)
        {
            timer += Time.deltaTime;
            if (electricityLight != null) electricityLight.intensity = Random.Range(0.5f, 4.0f);
            yield return null; 
        }

        if (electricityParticle != null) electricityParticle.Stop();
        if (electricityLight != null) electricityLight.enabled = false;
        isElectrified = false;
    }

    public void TriggerFoamReaction(float duration)
    {
        if (isFoaming || isResetting) return;
        StartCoroutine(FoamRoutine(duration));
    }

    private IEnumerator FoamRoutine(float duration)
    {
        isFoaming = true;
        if (sceneFoamParticle != null)
        {
            sceneFoamParticle.Play();
            targetColor = Color.white; 
        }

        float elapsedTime = 0f;
        float actualDuration = duration > 0 ? duration : 5f; 

        while (elapsedTime < actualDuration)
        {
            currentFillAmount += foamGrowthRate * Time.deltaTime;
            if (currentFillAmount >= 1.0f) currentFillAmount = 1.0f;
            elapsedTime += Time.deltaTime;
            UpdateShader();
            yield return null;
        }

        if (sceneFoamParticle != null) sceneFoamParticle.Stop();
        isFoaming = false;
        if (autoReset)
        {
            isResetting = true;
            yield return new WaitForSeconds(resetDelay);
            ResetContainer();
        }
    }

    void CheckBoilingState()
    {
        if (contentData == null || currentFillAmount <= 0.01f) {
            StopBoiling(); return;
        }

        if (currentTemperature >= contentData.boilingPoint) {
            if (!isBoiling) StartBoiling();
        } else {
            if (isBoiling) StopBoiling();
        }
        
        if (isBoiling && boilingParticle != null)
        {
             float surfaceY = Mathf.Lerp(minFillHeight, maxFillHeight, currentFillAmount);
             Vector3 currentPos = boilingParticle.transform.localPosition;
             boilingParticle.transform.localPosition = new Vector3(currentPos.x, surfaceY, currentPos.z);
        }
    }

    void StartBoiling()
    {
        isBoiling = true;
        if (boilingParticle != null && !boilingParticle.isPlaying) boilingParticle.Play();
    }

    void StopBoiling()
    {
        if (isBoiling)
        {
            isBoiling = false;
            if (boilingParticle != null) boilingParticle.Stop();
        }
    }

    private void CheckThermalReaction()
    {
        if (contentData == null || currentFillAmount <= 0.01f || isResetting) return;
        if (contentData.reactsToHeat && currentTemperature >= contentData.reactionTemperature)
        {
            if (contentData.heatReactionResult != null) TransformLiquid(contentData.heatReactionResult);
        }
    }

    private void TransformLiquid(LiquidData newLiquid)
    {
        contentData = newLiquid;
        targetColor = newLiquid.LiquidColor;
    }

    void HandleReactionEffect(LiquidRecipe recipe)
    {
        if (recipe.soundEffect != null) AudioSource.PlayClipAtPoint(recipe.soundEffect, transform.position);
    }

    public void ResetContainer()
    {
        StopAllCoroutines(); 
        contentData = null;
        lastReactedRecipe = null;
        currentFillAmount = 0f;
        currentTemperature = roomTemperature;
        isFoaming = false;
        isResetting = false; 
        isElectrified = false;
        currentColor = Color.clear; 
        targetColor = Color.clear;
        StopAllEffects(); 
        UpdateShader();
    }

    public void UpdateShader()
    {
        if (cupRenderer == null) return;
        cupRenderer.GetPropertyBlock(_propBlock);
        float mappedFill = Mathf.Lerp(minFillHeight, maxFillHeight, currentFillAmount);
        _propBlock.SetFloat(FillID, mappedFill);
        _propBlock.SetColor(SideColorID, currentColor);
        _propBlock.SetColor(TopColorID, currentColor);
        cupRenderer.SetPropertyBlock(_propBlock);
    }

    public void ApplyHeat(float heatAmount)
    {
        currentTemperature += heatAmount + coolingRate * Time.deltaTime;
        if (currentTemperature > maxTemperature) currentTemperature = maxTemperature;
    }

    private void OnTriggerEnter(Collider other)
    {
        SolidIngredient solid = other.GetComponent<SolidIngredient>();
        if (solid != null && solid.substanceData != null)
        {
            AddLiquid(solid.substanceData, solid.equivalentAmount);
            if (solid.entrySound != null) AudioSource.PlayClipAtPoint(solid.entrySound, transform.position);
            Destroy(other.gameObject);
        }
    }

    void OnParticleCollision(GameObject other)
    {
        PowderDispenser dispenser = other.GetComponentInParent<PowderDispenser>();
        if (dispenser != null)
        {
            AddLiquid(dispenser.powderData, dispenser.flowRate);
        }
    }
}