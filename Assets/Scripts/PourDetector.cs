using UnityEngine;

public class PourDetector : MonoBehaviour
{
    [Header("Ayarlar")]
    public int pourThreshold = 45; 
    public Transform pourOrigin;   
    public float pourRate = 0.2f;  
    public float maxStreamLength = 2.0f;
    public LayerMask hitLayers;

    [Header("Görsel Bileşenler")]
    public LineRenderer lineRenderer; 
    public LiquidContainer sourceContainer; 

    private bool isPouring = false;

    void Start()
    {
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (sourceContainer == null) sourceContainer = GetComponent<LiquidContainer>();
    }

    private void Update()
    {
        if (pourOrigin != null)
        {
            Debug.DrawRay(pourOrigin.position, Vector3.down * maxStreamLength, Color.red);
        }

        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);
        bool shouldPour = tiltAngle > pourThreshold && sourceContainer.currentFillAmount > 0;

        if (isPouring != shouldPour)
        {
            isPouring = shouldPour;
            if (lineRenderer != null) lineRenderer.enabled = isPouring;
        }

        if (isPouring)
        {
            PerformPourAndDebug();
        }
    }

    void PerformPourAndDebug()
    {
        lineRenderer.SetPosition(0, pourOrigin.position);
        
        RaycastHit hit;
        Vector3 targetPos = pourOrigin.position + Vector3.down * maxStreamLength;

        if (Physics.Raycast(pourOrigin.position, Vector3.down, out hit, maxStreamLength, hitLayers))
        {
            targetPos = hit.point;


            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                lineRenderer.SetPosition(1, targetPos);
                return;
            }

            LiquidContainer targetContainer = hit.collider.GetComponentInParent<LiquidContainer>();

            if (targetContainer == null)
            {
            }
            else
            {
                float amount = pourRate * Time.deltaTime;
                targetContainer.AddLiquid(sourceContainer.contentData, amount);
                sourceContainer.currentFillAmount -= amount;
            }
        }
        else
        {
            // HİÇBİR ŞEYE ÇARPMIYORSA
        }

        lineRenderer.SetPosition(1, targetPos);
        sourceContainer.UpdateShader();
        
        if (sourceContainer.contentData != null)
        {
             lineRenderer.startColor = sourceContainer.contentData.LiquidColor;
             lineRenderer.endColor = sourceContainer.contentData.LiquidColor;
             if(lineRenderer.material != null) lineRenderer.material.color = sourceContainer.contentData.LiquidColor;
        }
    }
}