using UnityEngine;

public class LiquidStream : MonoBehaviour
{
    public DualTapController dualTapController;

    private void OnTriggerStay(Collider other)
    {
        LiquidContainer container = other.GetComponent<LiquidContainer>();

        if (container != null && dualTapController != null && dualTapController.currentOutputLiquid != null)
        {
            container.AddLiquid(dualTapController.currentOutputLiquid, 0.1f * Time.deltaTime);
        }
    }
}