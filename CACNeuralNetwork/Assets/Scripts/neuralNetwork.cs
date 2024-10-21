using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Unity.VisualScripting;
using System.Threading;
using System.Xml;

public class neuralNetwork : MonoBehaviour
{
    [SerializeField] System.Random random;
    float destinationX;
    float destinationY;
    public float score = 0f;
    [SerializeField] int[] layers; //amount of neurons in each layer
    [SerializeField] float[][] neurons; //layer of neuron, specific neuron
    public float[][][] weights; //layer of weight, neuron weight affects, weight's value

    public int layerAmount; //number of layers in network (probably gonna be 4)
    public int[] neuronAmount; //number of neurons in each layer

    public float bias; // starting weight for all weights

    public float mutateChance; // write as percent

    public int id;
    Rigidbody2D rb;
    bool foundPlayer = false;
    float moveSpeed = 5f; // Constant move speed for the bot

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
         UnityEngine.Random.InitState(id);
        weights = new float[layerAmount - 1][][];
        for (int i = 0; i < layerAmount - 1; i++) //create until layer before output layer
        {  
            weights[i] = new float[neuronAmount[i]][]; //creates array for weights coming from layer i
            for (int j = 0; j < neuronAmount[i]; j++)
            {
                weights[i][j] = new float[neuronAmount[i+1]]; //creates an array of weights based on the amount of neurons the weights come from

                for (int k = 0; k < neuronAmount[i+1]; k++)
                {
                    weights[i][j][k] = (float)random.NextDouble() - 0.5f;
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
            neurons[0][i] = inputs[i]; //sets inputs to first layer neurons
        }

        for (int i = 1; i < layers.Length; i++) // Loop through each layer starting from the first hidden layer
        {
            for (int j = 0; j < neurons[i].Length; j++) // Loop through each neuron in the current layer
            {
                float value = 0.25f;
                for (int k = 1; k < neurons[i - 1].Length; k++) // Loop through each neuron in the previous layer
                {
                    value += neurons[i - 1][k] * weights[i - 1][k][j];
                }
                neurons[i][j] = (float)Math.Tanh(value); // Set value of current layer neuron between -1 and 1
            }
        }

        return neurons[neurons.Length - 1];
    }

    public GameObject createChild(int id)
    {
        score = 0;
        GameObject Child = Instantiate(transform.gameObject);
        neuralNetwork nn = Child.GetComponent<neuralNetwork>(); //gets child's neural network script
        Child.name = id.ToString();
        nn.score = 0;
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

        for (int i = 0; i < layerAmount - 1; i++) //create until layer before output layer
        {
            nn.weights[i] = new float[neuronAmount[i]][]; //creates array for weights coming from layer i
            for (int j = 0; j < neuronAmount[i]; j++)
            {
                nn.weights[i][j] = new float[neuronAmount[i + 1]]; //creates an array of weights based on the amount of neurons the weights come from

                for (int k = 0; k < neuronAmount[i + 1]; k++)
                {
                    if (mutateChance < UnityEngine.Random.Range(1, 100))
                    {
                        weights[i][j][k] = (float)random.NextDouble() - 0.5f;
                    }
                    else
                    {
                        nn.weights[i][j][k] = weights[i][j][k]; //sets weight to parent's weight
                    }
                }
            }
        }

        return Child;
    }

    // Start is called before the first frame update
    void Start()
    {
        destinationX = GameObject.FindGameObjectWithTag("plr").transform.position.x;
        destinationY = GameObject.FindGameObjectWithTag("plr").transform.position.y;
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
        if (!foundPlayer)
        {
            // Get output and normalize it to maintain constant speed
            Vector2 movement = new Vector2(outputs(inputs())[0], outputs(inputs())[1]);
            movement.Normalize(); // Ensure movement has a constant magnitude

            // Move the bot
            rb.MovePosition(rb.position + movement * moveSpeed * Time.deltaTime);
        }

        // Debug print for specific bot (e.g., bot with name "11")
        if (name == "11")
        {
            print("ix:" + inputs()[0] + ", iy: " + inputs()[1] + ", ox: " + outputs(inputs())[0] + ", oy: " + outputs(inputs())[1]);
        }

        // Reduce score based on the distance from the destination
        score -= (Mathf.Abs(transform.position.y - destinationY) + Mathf.Abs(transform.position.x - destinationX)) * Time.deltaTime;
    }

private void OnCollisionStay2D(Collision2D col)
{
    if (col.gameObject.tag == "wall")
    {
        float wallNormalX = col.contacts[0].normal.x;
        float wallNormalY = col.contacts[0].normal.y;
        float offset = 0.1f; // Adjust this value to control the amount of offset

        // Move the bot slightly away from the wall
        rb.MovePosition(transform.position + new Vector3(wallNormalX * offset, wallNormalY * offset, 0));

        // Adjust direction based on wall normal to continue movement
        float moveDirectionX = outputs(inputs())[0];
        float moveDirectionY = outputs(inputs())[1];

        // Check if moving directly against the wall
        if (Mathf.Abs(wallNormalX) > 0.9f)
        {
            // Reverse X direction with a small variation
            moveDirectionX = -moveDirectionX + UnityEngine.Random.Range(-0.1f, 0.1f);
        }
        if (Mathf.Abs(wallNormalY) > 0.9f)
        {
            // Reverse Y direction with a small variation
            moveDirectionY = -moveDirectionY + UnityEngine.Random.Range(-0.1f, 0.1f);
        }

        // Apply the new movement direction
        rb.MovePosition(new Vector2(transform.position.x + moveDirectionX * Time.deltaTime * 20, 
                                     transform.position.y + moveDirectionY * Time.deltaTime * 20));

        score -= 25f * Time.deltaTime;
    }
}

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "edge")
        {
            float edgeNormalX = col.contacts[0].normal.x;
            float edgeNormalY = col.contacts[0].normal.y;
            float offset = 0.1f; // Adjust this value to control the amount of offset
            if (Mathf.Abs(edgeNormalX) > 0.9f) // Check if the collision is with the left or right edge
            {
                rb.MovePosition(transform.position + new Vector3(edgeNormalX            * offset, 0, 0));
            }
            else if (Mathf.Abs(edgeNormalY) > 0.9f) // Check if the collision is with the top or bottom edge
            {
                rb.MovePosition(transform.position + new Vector3(0, edgeNormalY * offset, 0));
            }
        }
        else if (col.gameObject.tag == "plr")
        {
            score += 10000;
            foundPlayer = true;
        }
    }
}

