using UnityEngine;

public class DualTapController : MonoBehaviour
{
    [Header("Ayarlar")]
    public LiquidData liquidLeft;  
    public LiquidData liquidRight; 

    [Header("Görsel Efekt ve Fizik")]
    public ParticleSystem sharedParticle; 
    public Collider streamCollider;

    private bool isLeftOpen = false;
    private bool isRightOpen = false;

    [HideInInspector] 
    public LiquidData currentOutputLiquid;

    void Start()
    {
        // Başlangıçta her şeyi kapat
        if(sharedParticle != null) sharedParticle.Stop();
        if(streamCollider != null) streamCollider.enabled = false;
        currentOutputLiquid = null;
    }

    public void ToggleLeftHandle()
    {
        isLeftOpen = !isLeftOpen;
        UpdateStream();
    }

    public void ToggleRightHandle()
    {
        isRightOpen = !isRightOpen;
        UpdateStream();
    }

    void UpdateStream()
    {
        if (!isLeftOpen && !isRightOpen)
        {
            if(sharedParticle != null) sharedParticle.Stop();
            if(streamCollider != null) streamCollider.enabled = false;
            currentOutputLiquid = null;
            return;
        }

        if (isLeftOpen && !isRightOpen) currentOutputLiquid = liquidLeft;
        else if (!isLeftOpen && isRightOpen) currentOutputLiquid = liquidRight;
        else currentOutputLiquid = liquidLeft;

        if (sharedParticle != null && currentOutputLiquid != null)
        {
            var main = sharedParticle.main;
            main.startColor = new ParticleSystem.MinMaxGradient(currentOutputLiquid.LiquidColor);
            
            if (!sharedParticle.isPlaying) sharedParticle.Play();
            
            if(streamCollider != null) streamCollider.enabled = true; 
        }
    }
}