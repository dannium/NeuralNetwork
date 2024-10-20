using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimePause : MonoBehaviour
{
    public bool pause = false;
    public TimeSlider TS;
    public TMP_Text buttonTxt;
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

        }
        else
        {
            Time.timeScale = TS.scale;
            buttonTxt.text = "Pause";
        }

    }

    public void OnClick()
    {
        pause = !pause;
    }
}
