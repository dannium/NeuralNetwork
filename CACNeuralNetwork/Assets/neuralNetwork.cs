using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class neuralNetwork : MonoBehaviour
{
    System.Random random;
    float destinationX = 10f;
    float destinationY = 10f;
    //idk if we need the constructor, so i didnt add it, make sure to comment a bunch
    private int[] layers; //amount of neurons in each layer
    private float[][] neurons; //layer of neuron, specific neuron
    private float[][][] weights; //layer of weight, neuron weight affects, weight's value

    public int layerAmount; //number of layers in network (probably gonna be 4)
    public int[] neuronAmount; //number of nuerons in each layer

    public float bias; // starting weight for all weights


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

                for(int k = 0; k < neuronAmount[i]; k++)
                {
                    weights[i][j][k] = (float)random.NextDouble() - 0.5f; //sets each weight
                }
            }
        }
    }

    private float[] inputs()
    {
        float[] array = new float[2]; //makes an array with the length being the amount of inputs (2 rn)
        array[0] = destinationX - transform.position.x;
        array[1] = destinationY - transform.position.y;
        return array;
    }

    private float[] outputs(float[] inputs) {

        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        for (int i = 1; i < layers.Length; i++) {
            for(int j = 0; j < neurons[i].Length; j++)
            {
                float value = bias;
                for(int k = 0;k < neurons[i-1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                neurons[i][j] = (float)Math.Tanh(value); //sets value between -1 and 1
            }
        }
        return neurons[neurons.Length - 1];
    }

     
    // Start is called before the first frame update
    void Start()
    {
        random = new System.Random(); // Initialize the random variable
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
        transform.position = new Vector2(transform.position.x + outputs(inputs())[0], transform.position.y + outputs(inputs())[1])*Time.deltaTime; //changes bots position based on outputs
    }
}
