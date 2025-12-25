using UnityEngine;

public class BunsenHeater : MonoBehaviour {
    [Header("Isıtıcı Ayarları")]
    public float heatPower = 30f;
    
    public bool isHeating = false;

    private void OnTriggerStay(Collider other) {
        LiquidContainer container = other.GetComponentInParent<LiquidContainer>();
        
        if (container == null) container = other.GetComponent<LiquidContainer>();
        
        if (container != null && container.currentFillAmount > 0) {
            isHeating = true;
            container.ApplyHeat(heatPower * Time.deltaTime);
        } else {
            isHeating = false;
        }
    }

    private void OnTriggerExit(Collider other) {
         isHeating = false;
    }
}