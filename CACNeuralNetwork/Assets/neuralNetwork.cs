using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class neuralNetwork : MonoBehaviour
{
    //idk if we need the constructor, so i didnt add it, make sure to comment a bunch
    private int[] layers; //amount of neurons in each layer
    private float[][] neurons; //layer of neuron, specific neuron
    private float[][][] weights; //layer of weight, neuron weight affects, weight's value

    public int layerAmount; //number of layers in network (probably gonna be 4)
    public int[] neuronAmount; //number of nuerons in each layer


    private void initNeurons()
    {
        neurons = new float[layerAmount][];
        for (int i = 0; i < layerAmount; i++)
        {
            neurons[i] = new float[neuronAmount[i]]; //creates an array of neurons for every layer
        }
    }

    private void initWeights()
    {
        weights = new float[layerAmount][][];
        for(int i = 0; i < layerAmount - 1; i++) //create until layer before output layer
        {
            weights[i] = new float[neuronAmount[i]][]; //creates array for weight coming from layer i
            for(int j = 0; j < neuronAmount[i]; j++)
            {
                weights[i][j] = new float[neuronAmount[i]]; //creates an array of weights based on the amount of neurons the weights come from
            }
        }
    }
     
    // Start is called before the first frame update
    void Start()
    {
        //initalize neurons and weights
        initNeurons();
        initWeights();

        layers = new int[layerAmount];
        layers[0] = 2; //amount of inputs
        layers[layers.Length - 1] = 2; // amount of outputs
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
