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
    public float score = 0;
    public int layerAmount; //number of layers in network (probably gonna be 4)
    public int[] neuronAmount; //number of neurons in each layer
    [SerializeField] int[] layers; //amount of neurons in each layer
    [SerializeField] float[][] neurons; //layer of neuron, specific neuron
    public float[][][] weights; //layer of weight, neuron weight affects, weight's value

    public float bias; // starting weight for all weights

    public float mutateChance; // write as percent

    public int id;
    Rigidbody2D rb;
    bool foundPlayer = false;
    float moveSpeed = 3f;

    // Individual behavior variables
    private Vector2 currentDirection;
    private float directionChangeTimer = 0f;
    private float directionChangeInterval = 3f;
    private HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
    private float cellSize = 2f;

    // Simplified movement variables
    private Vector2 currentVelocity;
    private Vector2 targetVelocity;
    private float smoothTime = 0.3f;
    private float accelerationRate = 10f;
    private float decelerationRate = 5f;
    private float minimumMovementSpeed = 0.5f;

    // Wall interaction
    private float wallDetectionDistance = 2f;
    private float wallAvoidanceForce = 3f;

    // Exploration variables
    private Vector2 personalGoal;
    private float personalGoalRadius = 10f;

    // Stuck detection
    private Vector2 lastPosition;
    private float stuckTime = 0f;
    private float stuckThreshold = 1f;

    // Population variables
    public static List<neuralNetwork> population = new List<neuralNetwork>();
    public static int generationCount = 0;
    public static int populationSize = 50;
    public static float elitePercentage = 0.1f;
    public static float crossoverRate = 0.7f;

    private bool isInitialPopulation = false;

    // Initialize neurons for each layer
    private void initNeurons()
    {
        neurons = new float[layerAmount][];
        for (int i = 0; i < layerAmount; i++)
        {
            neurons[i] = new float[neuronAmount[i]];
        }
    }

    // Initialize weights for connections between neurons
    private void initWeights()
    {
        if (random == null)
        {
            random = new System.Random(id);
        }
        UnityEngine.Random.InitState(id);
        weights = new float[layerAmount - 1][][];
        for (int i = 0; i < layerAmount - 1; i++)
        {  
            weights[i] = new float[neuronAmount[i]][];
            for (int j = 0; j < neuronAmount[i]; j++)
            {
                weights[i][j] = new float[neuronAmount[i+1]];
                for (int k = 0; k < neuronAmount[i+1]; k++)
                {
                    weights[i][j][k] = (float)random.NextDouble() - 0.5f;
                }
            }
        }
    }

    // Get input values for the neural network
    private float[] inputs()
    {
        float[] array = new float[neuronAmount[0]];
        array[0] = destinationX - transform.position.x;
        array[1] = destinationY - transform.position.y;
        if (neuronAmount[0] > 2)
        {
            array[2] = currentDirection.x;
            array[3] = currentDirection.y;
        }
        return array;
    }

    // Process inputs through the neural network and return outputs
    private float[] outputs(float[] inputs)
    {
        if (inputs.Length != neuronAmount[0])
        {
            Debug.LogError($"Input length ({inputs.Length}) does not match first layer neuron count ({neuronAmount[0]})");
            return new float[neuronAmount[layerAmount - 1]];
        }

        for (int i = 0; i < neuronAmount[0]; i++)
        {
            neurons[0][i] = inputs[i];
        }

        for (int i = 1; i < layerAmount; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0.25f;
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += neurons[i - 1][k] * weights[i - 1][k][j];
                }
                neurons[i][j] = (float)Math.Tanh(value);
            }
        }

        return neurons[layerAmount - 1];
    }

    // Create a child neural network with potential mutations
    public GameObject createChild(int id)
    {
        score = 0;
        GameObject Child = Instantiate(transform.gameObject);
        neuralNetwork nn = Child.GetComponent<neuralNetwork>();
        Child.name = id.ToString();
        nn.score = 0;
        nn.id = this.id;
        nn.random = new System.Random(id);
        nn.layers = new int[layerAmount];
        nn.layerAmount = layerAmount;
        nn.neuronAmount = neuronAmount;
        nn.bias = bias;
        nn.destinationX = destinationX;
        nn.destinationY = destinationY;
        nn.mutateChance = mutateChance;
        for (int i = 0; i < layerAmount; i++)
        {
            nn.layers[i] = neuronAmount[i];
        }

        nn.initNeurons();
        nn.initWeights();

        for (int i = 0; i < layerAmount - 1; i++)
        {
            nn.weights[i] = new float[neuronAmount[i]][];
            for (int j = 0; j < neuronAmount[i]; j++)
            {
                nn.weights[i][j] = new float[neuronAmount[i + 1]];
                for (int k = 0; k < neuronAmount[i + 1]; k++)
                {
                    if (UnityEngine.Random.value < mutateChance)
                    {
                        nn.weights[i][j][k] = weights[i][j][k] + UnityEngine.Random.Range(-0.2f, 0.2f);
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
        botRunner br = GameObject.Find("generationRunner").GetComponent<botRunner>();
        id = br.botNum;
        layerAmount = br.layerAmount + 2;
        int[] neuronsAmount = new int[layerAmount];
        for (int i = 0; i < neuronsAmount.Length; i++)
        {
            if (i == 0 || i == neuronsAmount.Length - 1)
            {
                neuronsAmount[i] = 2; //sets amount of inputs and outputs
                print(i);
            }
            else
            {
                neuronsAmount[i] = br.hlnAmount; //sets amount of neurons in hidden layers
            }
        }
        neuronAmount = neuronsAmount;
        mutateChance = br.mutateChance;

        destinationX = GameObject.FindGameObjectWithTag("plr").transform.position.x;
        destinationY = GameObject.FindGameObjectWithTag("plr").transform.position.y;
        random = new System.Random(id); // Initialize the random variable with id as seed
        layers = new int[layerAmount];
        for (int i = 0; i < layerAmount; i++)
        {
            layers[i] = neuronAmount[i]; // Set each layer to the corresponding number of neurons
        }

        //initalize neurons and weights
        initNeurons();
        initWeights();
        rb = GetComponent<Rigidbody2D>();
        lastPosition = rb.position;
        SetNewDirection();

        // Set initial personal goal
        personalGoal = transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * personalGoalRadius;

        // Only add to population if this is part of the initial population
        if (isInitialPopulation)
        {
            population.Add(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!foundPlayer)
        {
            // Check if the bot is stuck
            if (Vector2.Distance(rb.position, lastPosition) < 0.01f)
            {
                stuckTime += Time.deltaTime;
                if (stuckTime > stuckThreshold)
                {
                    SetNewDirection();
                    stuckTime = 0f;
                }
            }
            else
            {
                stuckTime = 0f;
            }

            // Update direction periodically
            directionChangeTimer += Time.deltaTime;
            if (directionChangeTimer > directionChangeInterval)
            {
                SetNewDirection();
                directionChangeTimer = 0f;
            }

            // Get output and normalize it to maintain constant speed
            float[] outputArray = outputs(inputs());
            if (outputArray.Length >= 2)
            {
                Vector2 movement = new Vector2(outputArray[0], outputArray[1]);
                movement.Normalize(); // Ensure movement has a constant magnitude

                // Blend the neural network output with the current direction
                movement = Vector2.Lerp(movement, currentDirection, 0.5f);

                // Apply wall avoidance
                Vector2 avoidanceForce = CalculateWallAvoidance();
                movement += avoidanceForce;

                // Apply a force towards the personal goal if the bot is too far
                Vector2 toGoal = personalGoal - (Vector2)transform.position;
                if (toGoal.magnitude > personalGoalRadius)
                {
                    movement += toGoal.normalized * 0.3f;
                }

                // Normalize the movement vector again to maintain constant direction
                movement.Normalize();

                // Set the target velocity
                targetVelocity = movement * moveSpeed;

                // Smoothly accelerate or decelerate towards the target velocity
                currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, 
                    (currentVelocity.magnitude < targetVelocity.magnitude ? accelerationRate : decelerationRate) * Time.deltaTime);

                // Ensure minimum movement speed
                if (currentVelocity.magnitude < minimumMovementSpeed)
                {
                    currentVelocity = currentVelocity.normalized * minimumMovementSpeed;
                }

                // Apply the velocity to the rigidbody
                rb.velocity = currentVelocity;
            }
            else
            {
                Debug.LogError("Output array does not have enough elements");
            }

            lastPosition = rb.position;
            UpdateVisitedCells();
        }

        // Increase score based on exploration and reduce it based on the distance from the destination
        score += visitedCells.Count * 0.1f * Time.deltaTime;
        score -= (Mathf.Abs(transform.position.y - destinationY) + Mathf.Abs(transform.position.x - destinationX)) * Time.deltaTime * 0.5f;

        // Check if all bots have finished or a time limit has been reached
        if (population.Count > 0 && (population.TrueForAll(bot => bot.foundPlayer) || Time.time > 60f)) // 60 seconds time limit
        {
            EvolvePopulation();
        }
    }

    // Set a new random direction
    private void SetNewDirection()
    {
        currentDirection = UnityEngine.Random.insideUnitCircle.normalized;
        
        // Occasionally set a new personal goal
        if (UnityEngine.Random.value < 0.2f)
        {
            personalGoal = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * personalGoalRadius;
        }
    }

    // Calculate force to avoid walls
    private Vector2 CalculateWallAvoidance()
    {
        Vector2 avoidanceForce = Vector2.zero;
        RaycastHit2D[] hits = new RaycastHit2D[8];
        Vector2[] directions = {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            new Vector2(1, 1).normalized, new Vector2(1, -1).normalized,
            new Vector2(-1, 1).normalized, new Vector2(-1, -1).normalized
        };

        for (int i = 0; i < 8; i++)
        {
            hits[i] = Physics2D.Raycast(transform.position, directions[i], wallDetectionDistance);
            if (hits[i].collider != null && hits[i].collider.CompareTag("wall"))
            {
                avoidanceForce -= directions[i] * (wallDetectionDistance - hits[i].distance) / wallDetectionDistance * wallAvoidanceForce;
            }
        }

        return avoidanceForce;
    }

    // Update the set of visited cells
    private void UpdateVisitedCells()
    {
        Vector2Int currentCell = new Vector2Int(
            Mathf.FloorToInt(transform.position.x / cellSize),
            Mathf.FloorToInt(transform.position.y / cellSize)
        );
        visitedCells.Add(currentCell);
    }

    // Handle collision with walls
    private void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.tag == "wall")
        {
            Vector2 wallNormal = col.contacts[0].normal;
            float offset = 0.05f; // Small offset for more natural movement

            // Move the bot slightly away from the wall
            rb.MovePosition((Vector2)transform.position + wallNormal * offset);

            // Set a new direction away from the wall
            currentDirection = Vector2.Lerp(currentDirection, wallNormal.normalized, 0.7f);

            // Add a small impulse force to "bounce" off the wall
            rb.AddForce(wallNormal * wallAvoidanceForce * 0.5f, ForceMode2D.Impulse);

            // Reduce the score penalty for touching walls
            score -= 2f * Time.deltaTime;
        }
    }

    // Handle collision with edges and player
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "edge")
        {
            Vector2 edgeNormal = col.contacts[0].normal;
            float offset = 0.05f; // Small offset for more natural movement

            // Move the bot slightly away from the edge
            rb.MovePosition((Vector2)transform.position + edgeNormal * offset);

            // Set a new direction away from the edge
            currentDirection = Vector2.Lerp(currentDirection, edgeNormal.normalized, 0.7f);

            // Add a small impulse force to "bounce" off the edge
            rb.AddForce(edgeNormal * wallAvoidanceForce * 0.5f, ForceMode2D.Impulse);

            // Reduce the penalty for hitting edges
            score -= 10f;
        }
        else if (col.gameObject.tag == "plr")
        {
            score += 10000;
            foundPlayer = true;
        }
    }

    // Evolve the population to the next generation
    private static void EvolvePopulation()
    {
        generationCount++;
        Debug.Log($"Generation {generationCount} completed. Evolving population...");

        // Sort the population by score in descending order
        population.Sort((a, b) => b.score.CompareTo(a.score));

        // Keep the top performers (elites)
        int eliteCount = Mathf.Max(1, Mathf.RoundToInt(populationSize * elitePercentage));
        eliteCount = Mathf.Min(eliteCount, population.Count); // Ensure eliteCount doesn't exceed population size
        List<neuralNetwork> newPopulation = new List<neuralNetwork>(population.GetRange(0, eliteCount));

        // Create new individuals through crossover and mutation
        while (newPopulation.Count < populationSize && population.Count > 1)
        {
            neuralNetwork parent1 = SelectParent();
            neuralNetwork parent2 = SelectParent();

            neuralNetwork child = Crossover(parent1, parent2);
            Mutate(child);

            newPopulation.Add(child);
        }

        // Replace the old population with the new one
        foreach (var bot in population)
        {
            if (!newPopulation.Contains(bot))
            {
                Destroy(bot.gameObject);
            }
        }

        population = newPopulation;

        // Reset all bots for the next generation
        foreach (var bot in population)
        {
            bot.ResetForNewGeneration();
        }
    }

    // Select a parent for reproduction using tournament selection
    private static neuralNetwork SelectParent()
    {
        if (population.Count == 0)
        {
            Debug.LogError("Cannot select parent: population is empty");
            return null;
        }

        // Tournament selection
        int tournamentSize = Mathf.Min(5, population.Count);
        neuralNetwork best = null;
        for (int i = 0; i < tournamentSize; i++)
        {
            neuralNetwork contestant = population[UnityEngine.Random.Range(0, population.Count)];
            if (best == null || contestant.score > best.score)
            {
                best = contestant;
            }
        }
        return best;
    }

    // Perform crossover between two parent neural networks
    private static neuralNetwork Crossover(neuralNetwork parent1, neuralNetwork parent2)
    {
        if (parent1 == null || parent2 == null)
        {
            Debug.LogError("Cannot perform crossover: one or both parents are null");
            return null;
        }

        neuralNetwork child = Instantiate(parent1.gameObject).GetComponent<neuralNetwork>();
        child.isInitialPopulation = false; // Set this to false for new children

        // Perform crossover for each weight
        for (int i = 0; i < child.weights.Length; i++)
        {
            for (int j = 0; j < child.weights[i].Length; j++)
            {
                for (int k = 0; k < child.weights[i][j].Length; k++)
                {
                    child.weights[i][j][k] = UnityEngine.Random.value < crossoverRate ? 
                        parent1.weights[i][j][k] : parent2.weights[i][j][k];
                }
            }
        }

        return child;
    }

    // Mutate the weights of a neural network
    private static void Mutate(neuralNetwork bot)
    {
        if (bot == null)
        {
            Debug.LogError("Cannot mutate: bot is null");
            return;
        }

        for (int i = 0; i < bot.weights.Length; i++)
        {
            for (int j = 0; j < bot.weights[i].Length; j++)
            {
                for (int k = 0; k < bot.weights[i][j].Length; k++)
                {
                    if (UnityEngine.Random.value < bot.mutateChance)
                    {
                        bot.weights[i][j][k] += UnityEngine.Random.Range(-0.1f, 0.1f);
                    }
                }
            }
        }
    }

    // Reset the bot for a new generation
    private void ResetForNewGeneration()
    {
        score = 0;
        foundPlayer = false;
        transform.position = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f), 0); // Random starting position
        rb.velocity = Vector2.zero;
        currentVelocity = Vector2.zero;
        SetNewDirection();
        visitedCells.Clear();
        personalGoal = transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * personalGoalRadius;
    }

    // Set this bot as part of the initial population
    public void SetAsInitialPopulation()
    {
        isInitialPopulation = true;
    }
}
