using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;

public class TimeSlider : MonoBehaviour
{

    public float scale;
    public UnityEngine.UI.Slider slider;
    public TimePause TP;
    public TMP_Text scaleTxt;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        scale = slider.value;

        if (!TP.pause)
        {
        Time.timeScale = scale;
        }
        scale = Mathf.Round(scale * 10) / 10;
        
        
        if(scale % 1 == 0)
        {
        scaleTxt.text = "Speed: " + scale + ".0x";
        } else
        {
            scaleTxt.text = "Speed: " + scale + "x";
        }
    }
}
