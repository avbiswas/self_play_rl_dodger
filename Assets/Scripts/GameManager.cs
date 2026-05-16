using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject simulation;
    const string GameModeKey = "GameMode";
    const int HumanVsAiMode = 0;
    const int AiVsAiMode = 1;

    public void StartHumanVsAI(){
        PlayerPrefs.SetInt(GameModeKey, HumanVsAiMode);
        SwitchToGameScene();
    }

    public void StartAIvsAI(){
        PlayerPrefs.SetInt(GameModeKey, AiVsAiMode);
        SwitchToGameScene();
    }

    public void SwitchToGameScene(){
        if (!PlayerPrefs.HasKey(GameModeKey)){
            PlayerPrefs.SetInt(GameModeKey, HumanVsAiMode);
        }
        if (simulation != null){
            Destroy(simulation);
        }
        SceneManager.LoadScene("GameEnv", LoadSceneMode.Single);
    }
}
