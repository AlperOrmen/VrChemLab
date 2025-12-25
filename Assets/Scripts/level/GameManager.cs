using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("VR Input Settings (KESİN ÇÖZÜM)")]
    public InputActionAsset inputActionAsset; 
    
    public string[] locomotionMapNames = new string[] { 
        "XRI LeftHand Locomotion", 
        "XRI RightHand Locomotion" 
    };

    [Header("VR Player Setup")]
    public Transform vrPlayerOrigin; 
    public Transform mainCamera;        

    [Header("UI Panels")]
    public GameObject startPanel;      
    public GameObject levelSelectPanel; 
    public GameObject inGamePanel;     
    public TextMeshProUGUI instructionText; 
    public TextMeshProUGUI infoText;        

    [Header("Level Configuration")]
    public List<LevelData> allLevels; 
    public Transform[] stationSpawnPoints; 
    public Button surpriseButton; 

    private HashSet<string> completedLevels = new HashSet<string>();
    private LevelData currentLevel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ShowStartScreen();
    }

    
    private void ToggleLocomotion(bool status)
    {
        if (inputActionAsset == null) return;

        foreach (string mapName in locomotionMapNames)
        {
            var actionMap = inputActionAsset.FindActionMap(mapName);
            if (actionMap != null)
            {
                if (status) 
                    actionMap.Enable();
                else 
                    actionMap.Disable();
            }
        }
    }


    public void ShowStartScreen()
    {
        ToggleLocomotion(false);

        startPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        inGamePanel.SetActive(false);

        RecenterUI(startPanel.transform.parent);
    }

    public void ShowLevelSelection()
    {
        startPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        inGamePanel.SetActive(false);
        
        ToggleLocomotion(false);
        
        RecenterUI(levelSelectPanel.transform.parent);
        CheckSurpriseUnlock(); 
    }

    public void StartLevel(int levelIndex)
    {
        currentLevel = allLevels[levelIndex];
        
        startPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        inGamePanel.SetActive(true);
        
        instructionText.text = currentLevel.instructions;
        infoText.text = ""; 

        vrPlayerOrigin.position = stationSpawnPoints[levelIndex].position;
        vrPlayerOrigin.rotation = stationSpawnPoints[levelIndex].rotation;

        ToggleLocomotion(true);
    }

    public void CompleteCurrentLevel()
    {
        if (currentLevel == null) return;
        StartCoroutine(LevelSuccessRoutine());
    }

    private IEnumerator LevelSuccessRoutine()
    {
        instructionText.text = "DENEY BAŞARILI!";
        infoText.text = currentLevel.successInfo;
        
        if (!completedLevels.Contains(currentLevel.levelID))
        {
            completedLevels.Add(currentLevel.levelID);
        }

        yield return new WaitForSeconds(5f);

        ShowLevelSelection();
    }

    private void CheckSurpriseUnlock()
    {
        if (completedLevels.Count >= 4)
        {
            surpriseButton.interactable = true;
            surpriseButton.GetComponentInChildren<TextMeshProUGUI>().text = "GİZLİ BÖLÜM (AÇIK)";
        }
        else
        {
            surpriseButton.interactable = false;
            surpriseButton.GetComponentInChildren<TextMeshProUGUI>().text = "??? (KİLİTLİ)";
        }
    }

    private void RecenterUI(Transform uiCanvas)
    {
        if (mainCamera == null || uiCanvas == null) return;
        Vector3 targetPos = mainCamera.position + (mainCamera.forward * 2.0f);
        targetPos.y = mainCamera.position.y; 
        uiCanvas.position = targetPos;
        uiCanvas.LookAt(new Vector3(mainCamera.position.x, uiCanvas.position.y, mainCamera.position.z));
        uiCanvas.Rotate(0, 180, 0); 
    }
}