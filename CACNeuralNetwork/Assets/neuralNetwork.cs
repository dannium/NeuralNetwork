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
        weights = new float[layerAmount - 1][][];
        for(int i = 0; i < layerAmount - 1; i++) //create until layer before output layer
        {
            weights[i] = new float[neuronAmount[i]][]; //creates array for weights coming from layer i
            for(int j = 0; j < neuronAmount[i]; j++)
            {
                weights[i][j] = new float[neuronAmount[i+1]]; //creates an array of weights based on the amount of neurons the weights come from

                for(int k = 0; k < neuronAmount[i+1]; k++)
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

    private float[] outputs(float[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        for (int i = 1; i < layerAmount; i++) // Loop through each layer starting from the first hidden layer
        {
            for (int j = 0; j < neuronAmount[i]; j++) // Loop through each neuron in the current layer
            {
                float value = bias;
                for (int k = 0; k < neuronAmount[i - 1]; k++) // Loop through each neuron in the previous layer
                {
                    value += weights[i - 1][k][j] * neurons[i - 1][k];
                }
                neurons[i][j] = (float)Math.Tanh(value); // Set value between -1 and 1
            }
        }
        return neurons[layerAmount - 1];
    }

    public GameObject createChild(int id)
    {
        GameObject Child = Instantiate(gameObject);
        Child.name = id.ToString();
        //we gotta change the neurons here or theyll be the exact same
        return Child;
    }


    // Start is called before the first frame update
    void Start()
    {
        random = new System.Random(); // Initialize the random variable        
        layers = new int[layerAmount];
        for (int i = 0; i < layerAmount; i++)
        {
            layers[i] = neuronAmount[i]; // Set each layer to the corresponding number of neurons
        }

        //initalize neurons and weights
        initNeurons();
        initWeights();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector2(transform.position.x + outputs(inputs())[0]*Time.deltaTime, transform.position.y + outputs(inputs())[1]*Time.deltaTime); //changes bots position based on outputs
        print(new Vector2(outputs(inputs())[0], outputs(inputs())[1]));
    }
}
