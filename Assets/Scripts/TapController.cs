using UnityEngine;

public class TapController : MonoBehaviour
{
    [Header("Ayarlar")]
    public LiquidData currentLiquid; 

    [Header("Görsel Efekt")]
    public ParticleSystem liquidParticles; 

    private bool isOpen = false;

    public void ToggleTap()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            StartPouring();
        }
        else
        {
            StopPouring();
        }
    }

void StartPouring()
{
    if (liquidParticles != null && currentLiquid != null)
    {
        var main = liquidParticles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(currentLiquid.LiquidColor); 
        liquidParticles.Play();
    }
}

    void StopPouring()
    {
        if (liquidParticles != null)
        {
            liquidParticles.Stop();
        }
    }
}