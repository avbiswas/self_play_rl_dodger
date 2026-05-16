using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
                pauseMenu.SetActive(false);
            }
            else
            {
                Time.timeScale = 0f;
                pauseMenu.SetActive(true);
            }
        }
        
    }

    public void GoToMainMenu(){
        Time.timeScale = 1f;
        DestroyAllGameObjects();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void Restart(){
        Time.timeScale = 1f;
        DestroyAllGameObjects();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    public void DestroyAllGameObjects()
    {
        Bullet[] GameObjects = (FindObjectsOfType<Bullet>() as Bullet[]);
    
        for (int i = 0; i < GameObjects.Length; i++)
        {
            Destroy(GameObjects[i].gameObject);
        }

        FixedWaitTimeManager[] fixedWaitTimeManagers = (FindObjectsOfType<FixedWaitTimeManager>() as FixedWaitTimeManager[]);

        for (int i = 0; i < fixedWaitTimeManagers.Length; i++){
            fixedWaitTimeManagers[i].FinishAllCoroutines(true);
        }
    }
}
