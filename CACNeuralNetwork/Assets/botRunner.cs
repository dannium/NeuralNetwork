using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading;

public class botRunner : MonoBehaviour
{
    public int botAmount;//needs to be even
    public GameObject bot;
    GameObject[] bots;
    float[] scores;
    int botNum;
    neuralNetwork neuralNetworkScript;
    int timer = 0;

    void firstGen()
    {
        for (botNum = 1; botNum < bots.Length; botNum++)
        {
            scores[botNum - 1] = botNum; //score set to id for testing
            bots[botNum - 1] = Instantiate(bot);
            bots[botNum - 1].name = (botNum).ToString();
        }
    }
    void nextGen()
    {
        float medianScore = GetMedian(scores);
        // Lists are more optimized
        List<float> scoresList = new List<float>();
        List<GameObject> botsList = new List<GameObject>();

        for (int i = 0; i < bots.Length; i++)
        {
            if (scores[i] > medianScore)
            {
                scoresList.Add(botNum); //set score to id for testing
                botsList.Add(bots[i]); // Use Add to add  bot to the list
            }
            else
            {
                Destroy(bots[i]);
            }
        }

        scores = new float[botAmount];
        bots = new GameObject[botAmount];

        // Set first half of bots to the bots that won
        for (int i = 0; i < botsList.Count; i++)
        {
            scores[i] = 0;
            bots[i] = botsList[i];
        }

        // Create children to fill the remaining spots
        for (int i = 0; i < botAmount - botsList.Count; i++)
        {
            scores[i + botsList.Count] = 0;
            bots[i + botsList.Count] = Instantiate(neuralNetworkScript.createChild(botNum));
            botNum++;
        }
    }


    static float GetMedian(float[] array)
    {
        //Sort the array
        float[] sortedArray = array.OrderBy(num => num).ToArray();
        int length = sortedArray.Length;
        //Calculate the median
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
        bots = new GameObject[botAmount];
        scores = new float[botAmount];
        firstGen();
    }

    // Update is called once per frame
    void Update()
    {
        timer++;
        if(timer % 2000 == 0)
        {
            nextGen();
        }
    }
}