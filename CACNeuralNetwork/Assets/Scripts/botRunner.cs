using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.UI;
using Cinemachine;

public class botRunner : MonoBehaviour
{
    public int botAmount = 32;//needs to be even
    public GameObject bot;
    public GameObject[] bots;

    //settings ui (yall im sorry about the amount of public stuff)
    public GameObject settings;
    public TextMeshProUGUI genTxt;
    public TextMeshProUGUI timerTxt;
    public TextMeshProUGUI layersTxt;
    public TextMeshProUGUI neuronsTxt;
    public TextMeshProUGUI mutateTxt;
    public TextMeshProUGUI botsTxt;
    public UnityEngine.UI.Slider layersSlider;
    public UnityEngine.UI.Slider neuronsSlider;
    public UnityEngine.UI.Slider mutateSlider;
    public UnityEngine.UI.Slider botsSlider;
    public Button resetBtn;
    public Button startBtn;
    public int layerAmount = 2;
    public int hlnAmount = 6; //hidden layer neuron amount
    public float mutateChance = 3;

    [Header("cam stuff")]
    public CinemachineVirtualCamera mapCam;

    float[] scores;
    public int botNum;
    float timer = 0;

    int gen = 1;

    [SerializeField] public bool start = false;
    void firstGen()
    {
        for (botNum = 1; botNum < bots.Length + 1; botNum++)
        {
            bots[botNum - 1] = Instantiate(bot);
            bots[botNum - 1].name = (botNum).ToString();
        }
    }
    void nextGen()
    {
        genTxt.text = "Generation " + gen;
        gen++;
        for (int i = 0; i < scores.Length; i++)
        {
            if (bots[i] == null)
            {
                Debug.LogError("bots[" + i + "] is null");
            }
            else
            {
                var neuralNetworkComponent = bots[i].GetComponent<neuralNetwork>();
                if (neuralNetworkComponent == null)
                {
                    Debug.LogError("neuralNetwork component is not found on bots[i]");
                }
                else
                {
                    float score = neuralNetworkComponent.score;
                    scores[i] = score;
                    //Debug.Log("Score: " + score);
                }
            }

            /* print(scores[i] + "a");
             print(bots[i] + "b");*/
            /*float score = bots[i].GetComponent<neuralNetwork>().score;
            scores[i] = score;
            print(i);*/
        }
        float medianScore = GetMedian(scores);
        // lists are more optimized
        List<float> scoresList = new List<float>();
        List<GameObject> botsList = new List<GameObject>();

        for (int i = 0; i < bots.Length; i++)
        {
            if (scores[i] > medianScore)
            {
                botsList.Add(bots[i]); // use Add to add  bot to  list if it survived
                scoresList.Add(scores[i]); //set score to id for testing
            }
            else
            {
                Destroy(bots[i]);
            }
        }

        scores = new float[botAmount];
        bots = new GameObject[botAmount];

        // set first half of bots array to the bots that survived
        int index = 0;
        for (int i = 0; i < botsList.Count; i++)
        {
            botsList[i].transform.position = Vector2.zero;
            bots[i] = botsList[i];
            index++;
        }
        // create children to fill the remaining spots
        for (int i = index; i < botAmount; i++)
        {
            if (i - index < botsList.Count && botsList[i - index] != null) // Make sure there is a parent bot to create a child from
            {
                scores[i] = 0;
                bots[i] = botsList[i - index].GetComponent<neuralNetwork>().createChild(botNum);
                botNum++;
            }
            else
            {
                //If no parent is available, instantiate a new bot
                bots[i] = Instantiate(bot);
                bots[i].name = "AHHHHHHHHHHHHH";
                botNum++;
            }
        }
    }


    static float GetMedian(float[] array)
    {
        //Sort the array
        float[] sortedArray = array.OrderBy(num => num).ToArray();
        int length = sortedArray.Length;
        //Calculate  median
        if (length % 2 == 1) // if odd
        {
            return sortedArray[length / 2];
        }
        else // if even
        {
            float mid1 = sortedArray[(length / 2) - 1];
            float mid2 = sortedArray[length / 2];
            return (mid1 + mid2) / 2f;
        }
    }


    // Start is called before the first frame update
    void Start()

    {
        genTxt.enabled = false;
        timerTxt.enabled = false;
        resetBtn.onClick.AddListener(resetSettings);
        startBtn.onClick.AddListener(StartRunning);
        layersSlider.onValueChanged.AddListener(delegate { sliderChanged(layersSlider); });
        neuronsSlider.onValueChanged.AddListener(delegate { sliderChanged(neuronsSlider); });
        mutateSlider.onValueChanged.AddListener(delegate { sliderChanged(mutateSlider); });
        botsSlider.onValueChanged.AddListener(delegate { sliderChanged(botsSlider); });
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && !start) { 
        settings.SetActive(true);
        }

        if (start)
        {
            genTxt.enabled = true;
            timerTxt.enabled = true;
            settings.SetActive(false);
            mapCam.Priority = 100;
            timer -= Time.deltaTime;
            if (timer >= 10)
            {
                timerTxt.text = "0:" + Mathf.Round(timer * 100) / 100;
            } else
            {
                timerTxt.text = "0:0" + Mathf.Round(timer * 100) / 100; //adds extra zero
            }
            if (timer <= 0)
        {
            timer = 15;
            timerTxt.text = "0:15.00";
            nextGen();
        }
        }
        
    }

    void StartRunning()
    {
        bots = new GameObject[botAmount];
        scores = new float[botAmount];
        firstGen();
        start = true;
    }

    void resetSettings()
    {
        layerAmount = 2;
        layersSlider.value = layerAmount;
        hlnAmount = 6; //hidden layer neuron amount
        neuronsSlider.value = hlnAmount;
        mutateChance = 3;
        mutateSlider.value = mutateChance;
        botAmount = 32;
        botsSlider.value = botAmount;
    }

    void sliderChanged(Slider slider)
    {
        if (slider == layersSlider) {
            layerAmount = (int)layersSlider.value;
            layersTxt.text = "Hidden layers: " + layerAmount;
        } else if(slider == neuronsSlider)
        {
            hlnAmount = (int)neuronsSlider.value;
            neuronsTxt.text = "Neurons: " + hlnAmount;
        } else if(slider == mutateSlider) {
            mutateChance = mutateSlider.value;
            mutateTxt.text = "Mutate chance: " + Mathf.Round(mutateChance*100)/100 + "%";
        } else if(slider == botsSlider)
        {
            botAmount = (int)botsSlider.value;
            botsTxt.text = "Bots: " + botAmount;
        }

    }
}