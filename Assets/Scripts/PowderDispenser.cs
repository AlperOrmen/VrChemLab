using UnityEngine;

public class PowderDispenser : MonoBehaviour {
    [Header("Ayarlar")]
    public LiquidData powderData; 
    

    [Header("Referans")]
    public Transform pourPoint; 

    [Range(0, 180)]
    public int pourThreshold = 135;

    public float flowRate = 0.01f; 
    public ParticleSystem powderParticles; 

    private void Start()
    {
        if (pourPoint == null) pourPoint = transform;

        if (powderParticles != null)
        {
            powderParticles.Stop();
        }
    }

    private void Update() {
       if (powderParticles == null) return;

       CheckPourAngle();
    }

    void CheckPourAngle()
    {
        float tiltAngle = Vector3.Angle(pourPoint.up, Vector3.up);

        bool shouldPour = tiltAngle > pourThreshold;

        if (shouldPour)
        {
            if (!powderParticles.isPlaying)
                powderParticles.Play();
        }
        else
        {
            if (powderParticles.isPlaying)
                powderParticles.Stop();
        }
    }
}