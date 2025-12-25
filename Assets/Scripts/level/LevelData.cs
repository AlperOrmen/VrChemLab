using UnityEngine;

[CreateAssetMenu(fileName = "NewExperiment", menuName = "ChemistryVR/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;     
    public string levelID;     
    [TextArea] public string instructions;
    [TextArea] public string successInfo;
    
}