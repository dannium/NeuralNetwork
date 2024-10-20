using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class botRunner : MonoBehaviour
{
    public int botAmount;//needs to be even
    public GameObject bot;
    GameObject[] bots;
    float[] scores;
    int botNum;

    void firstGen()
    {
        for (botNum = 1; botNum < bots.Length; botNum++)
        {
            scores[botNum - 1] = 0;
            bots[botNum - 1] = Instantiate(bot);
            bots[botNum - 1].name = (botNum).ToString();
        }
    }
    void nextGen()
    {
        float medianScore = GetMedian(scores);
        List<float> scoresList = new List<float>();
        List<GameObject> botsList = new List<GameObject>();
        int j = 0;
        for (int i = 0; i < bots.Length; i++)
        {
            if (scores[i] > medianScore)
            {
                scoresList[j] = scores[i];
                botsList[j] = bots[i];
                j++;
            }
        }

        
        ////////// create children
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
        
    }
}
