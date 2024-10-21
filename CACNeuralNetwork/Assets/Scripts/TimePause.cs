using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TimePause : MonoBehaviour
{
    public bool pause = false;
    public TimeSlider TS;
    public TMP_Text buttonTxt;
    public GameObject pauseMenu;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (pause)
        {
            Time.timeScale = 0f;
            buttonTxt.text = "Play";
            pauseMenu.SetActive(true);
        }
        else
        {
            Time.timeScale = TS.scale;
            buttonTxt.text = "Pause";
            pauseMenu.SetActive(false);
        }

    }

    public void OnClick()
    {
        pause = !pause;
    }

    public void QuitButton()
    {
        SceneManager.LoadSceneAsync(0);
    }
    public void RestartButton()
    {
        SceneManager.LoadSceneAsync(1);
    }
}
