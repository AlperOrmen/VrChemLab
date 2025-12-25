using UnityEngine;

public class ExperimentStation : MonoBehaviour
{

    public void OnReactionSuccess()
    {
        Debug.Log("Deney Tamamlandı!");
        GameManager.Instance.CompleteCurrentLevel();
    }
}