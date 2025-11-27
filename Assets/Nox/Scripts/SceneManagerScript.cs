using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        // Works on Android "Back" button
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Back button pressed!");
            Application.Quit();
        }
    }

    public void GoToScene(string scene)
    {
        SceneManager.LoadSceneAsync(scene);
    }

    public void Logout()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadSceneAsync("LoginScene");
    }

}
