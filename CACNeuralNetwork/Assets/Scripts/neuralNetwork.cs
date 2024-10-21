using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Unity.VisualScripting;
using System.Threading;

public class neuralNetwork : MonoBehaviour
{
    [SerializeField] System.Random random;
    float destinationX = 10f;
    float destinationY = 10f;
    public float score = 0f;
    [SerializeField] int[] layers; //amount of neurons in each layer
    [SerializeField] float[][] neurons; //layer of neuron, specific neuron
    public float[][][] weights; //layer of weight, neuron weight affects, weight's value

    public int layerAmount; //number of layers in network (probably gonna be 4)
    public int[] neuronAmount; //number of nuerons in each layer

    public float bias; // starting weight for all weights

    public float mutateChance; // write as percent

    public int id;
    Rigidbody2D rb;

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
        print(id);
        UnityEngine.Random.InitState(id);
        weights = new float[layerAmount - 1][][];
        for (int i = 0; i < layerAmount - 1; i++) //create until layer before output layer
        {
            weights[i] = new float[neuronAmount[i]][]; //creates array for weights coming from layer i
            for (int j = 0; j < neuronAmount[i]; j++)
            {
                weights[i][j] = new float[neuronAmount[i + 1]]; //creates an array of weights based on the amount of neurons the weights come from

                for (int k = 0; k < neuronAmount[i + 1]; k++)
                {
                    weights[i][j][k] = UnityEngine.Random.Range(-0.500f, 0.500f); //sets each weight
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
        GameObject Child = Instantiate(transform.gameObject);
        neuralNetwork nn = Child.GetComponent<neuralNetwork>(); //gets child's neural network script
        Child.name = id.ToString();
        nn.id = this.id;
        nn.random = new System.Random();     
        nn.layers = new int[layerAmount];
        nn.layerAmount = layerAmount; 
        nn.neuronAmount = neuronAmount;
        nn.bias = bias;
        nn.destinationX = destinationX;
        nn.destinationY = destinationY;
        nn.mutateChance = mutateChance;
        for (int i = 0; i < layerAmount; i++)
        {
            nn.layers[i] = neuronAmount[i]; // Set each layer to the corresponding number of neurons
        }

        //initalize neurons and weights
        nn.initNeurons();
        nn.initWeights();

        //Child.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = id.ToString();
        for (int i = 0; i < layerAmount - 1; i++) //create until layer before output layer
        {
            nn.weights[i] = new float[neuronAmount[i]][]; //creates array for weights coming from layer i
            for (int j = 0; j < neuronAmount[i]; j++)
            {
                nn.weights[i][j] = new float[neuronAmount[i + 1]]; //creates an array of weights based on the amount of neurons the weights come from

                for (int k = 0; k < neuronAmount[i + 1]; k++)
                {
                    if(mutateChance < UnityEngine.Random.Range(1, 100))
                    {
                        weights[i][j][k] = UnityEngine.Random.Range(-0.500f, 0.500f) * (float)random.NextDouble();
                        //(float)random.NextDouble() - 0.5f; //mutates (changes) weight to new random num
                    }
                    else
                    {
                        nn.weights[i][j][k] = weights[i][j][k]; //sets weight to arent's weight
                    }
                }
            }
        }

        return Child;
    }


    // Start is called before the first frame update
    void Start()
    {
        //transform.GetComponentInChildren<TextMeshProUGUI>().text = name.ToString();
        random = new System.Random(); // Initialize the random variable        
        layers = new int[layerAmount];
        for (int i = 0; i < layerAmount; i++)
        {
            layers[i] = neuronAmount[i]; // Set each layer to the corresponding number of neurons
        }

        //initalize neurons and weights
        initNeurons();
        initWeights();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
       rb.AddForce(new Vector2(outputs(inputs())[0] * Time.deltaTime, outputs(inputs())[1] *Time.deltaTime)); //changes bots position based on outputs
                                                                                                                                                                     // score += outputs(inputs())[0];
                                                                                                                                                                     //        score += outputs(inputs())[1];
        score -= MathF.Abs(transform.position.y);
        score += transform.position.x;

    }
}
