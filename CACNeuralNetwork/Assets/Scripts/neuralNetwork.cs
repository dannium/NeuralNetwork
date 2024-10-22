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
    float moveSpeed = 3f; // Reduced move speed for more natural movement

    // New variables for improved exploration
    private Vector2 lastPosition;
    private float stuckTime = 0f;
    private float stuckThreshold = 0.5f; // Increased time before considering the bot stuck
    private float explorationTimer = 0f;
    private float explorationInterval = 3f; // Increased time between direction changes
    private Vector2 currentExplorationDirection;
    private float wallAvoidanceForce = 4f; // Reduced force to apply when avoiding walls
    private float wallDetectionDistance = 1.5f;
    private HashSet<Vector2Int> exploredCells = new HashSet<Vector2Int>();
    private float cellSize = 1f; // Size of each cell in the grid

    // New variables for smoother movement
    private float accelerationRate = 5f;
    private float decelerationRate = 3f;
    private Vector2 currentVelocity;
    private Vector2 targetVelocity;

    // New variables for improved evolution
    public static List<neuralNetwork> population = new List<neuralNetwork>();
    public static int generationCount = 0;
    public static int populationSize = 50;
    public static float elitePercentage = 0.1f;
    public static float crossoverRate = 0.7f;

    // New variable to track if this bot is part of the initial population
    private bool isInitialPopulation = false;

    // New variable for a more dynamic exploration strategy
    private float explorationRadius = 5f;
    private Vector2 explorationCenter;

    // New variables for bot separation
    private float separationDistance = 2f; // Reduced minimum distance to maintain between bots
    private float separationForce = 5f; // Reduced force to apply for separation

    // New variable to ensure minimum movement
    private float minimumMovementSpeed = 0.5f;

    // New variable for wall avoidance
    private float wallRepelForce = 2f;
    private float wallRepelDistance = 2f;

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
        if (random == null)
        {
            random = new System.Random(id); // Initialize random if it's null
        }
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
        float[] array = new float[neuronAmount[0]]; // Match input size to first layer neuron count
        array[0] = destinationX - transform.position.x;
        array[1] = destinationY - transform.position.y;
        if (neuronAmount[0] > 2)
        {
            array[2] = currentExplorationDirection.x;
            array[3] = currentExplorationDirection.y;
        }
        return array;
    }

    private float[] outputs(float[] inputs)
    {
        if (inputs.Length != neuronAmount[0])
        {
            Debug.LogError($"Input length ({inputs.Length}) does not match first layer neuron count ({neuronAmount[0]})");
            return new float[neuronAmount[layerAmount - 1]]; // Return empty output array
        }

        for (int i = 0; i < neuronAmount[0]; i++)
        {
            neurons[0][i] = inputs[i]; //sets inputs to first layer neurons
        }

        for (int i = 1; i < layerAmount; i++) // Loop through each layer starting from the first hidden layer
        {
            for (int j = 0; j < neurons[i].Length; j++) // Loop through each neuron in the current layer
            {
                float value = 0.25f;
                for (int k = 0; k < neurons[i - 1].Length; k++) // Loop through each neuron in the previous layer
                {
                    value += neurons[i - 1][k] * weights[i - 1][k][j];
                }
                neurons[i][j] = (float)Math.Tanh(value); // Set value of current layer neuron between -1 and 1
            }
        }

        return neurons[layerAmount - 1];
    }

    public GameObject createChild(int id)
    {
        score = 0;
        GameObject Child = Instantiate(transform.gameObject);
        neuralNetwork nn = Child.GetComponent<neuralNetwork>(); //gets child's neural network script
        Child.name = id.ToString();
        nn.score = 0;
        nn.id = this.id;
        nn.random = new System.Random(id); // Initialize random with the id as seed
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
        SetNewExplorationDirection();

        // Set initial exploration center
        explorationCenter = transform.position;

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
                    SetNewExplorationDirection();
                    stuckTime = 0f;
                }
            }
            else
            {
                stuckTime = 0f;
            }

            // Update exploration direction periodically
            explorationTimer += Time.deltaTime;
            if (explorationTimer > explorationInterval)
            {
                SetNewExplorationDirection();
                explorationTimer = 0f;
            }

            // Get output and normalize it to maintain constant speed
            float[] outputArray = outputs(inputs());
            if (outputArray.Length >= 2)
            {
                Vector2 movement = new Vector2(outputArray[0], outputArray[1]);
                movement.Normalize(); // Ensure movement has a constant magnitude

                // Blend the neural network output with the exploration direction
                movement = Vector2.Lerp(movement, currentExplorationDirection, 0.5f);

                // Apply wall avoidance
                Vector2 avoidanceForce = CalculateWallAvoidance();
                movement += avoidanceForce;

                // Apply a force towards the exploration center if the bot is too far
                Vector2 toCenter = explorationCenter - (Vector2)transform.position;
                if (toCenter.magnitude > explorationRadius)
                {
                    movement += toCenter.normalized * 0.3f;
                }

                // Apply separation force
                Vector2 separationForce = CalculateSeparationForce();
                movement += separationForce;

                // Apply wall repel force
                Vector2 wallRepelForce = CalculateWallRepelForce();
                movement += wallRepelForce;

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
            UpdateExploredCells();
        }

        // Increase score based on exploration and reduce it based on the distance from the destination
        score += exploredCells.Count * 0.1f * Time.deltaTime;
        score -= (Mathf.Abs(transform.position.y - destinationY) + Mathf.Abs(transform.position.x - destinationX)) * Time.deltaTime * 0.5f;

        // Check if all bots have finished or a time limit has been reached
        if (population.Count > 0 && (population.TrueForAll(bot => bot.foundPlayer) || Time.time > 60f)) // 60 seconds time limit
        {
            EvolvePopulation();
        }
    }

    private void SetNewExplorationDirection()
    {
        currentExplorationDirection = UnityEngine.Random.insideUnitCircle.normalized;
        
        // Occasionally set a new exploration center
        if (UnityEngine.Random.value < 0.2f)
        {
            explorationCenter = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * explorationRadius;
        }
    }

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

    private Vector2 CalculateSeparationForce()
    {
        Vector2 separationForce = Vector2.zero;
        int neighborCount = 0;

        foreach (var bot in population)
        {
            if (bot != this)
            {
                float distance = Vector2.Distance(transform.position, bot.transform.position);
                if (distance < separationDistance)
                {
                    Vector2 awayFromNeighbor = (Vector2)(transform.position - bot.transform.position).normalized;
                    separationForce += awayFromNeighbor * (separationDistance - distance) / separationDistance;
                    neighborCount++;
                }
            }
        }

        if (neighborCount > 0)
        {
            separationForce /= neighborCount;
            separationForce *= separationForce;
        }

        return separationForce;
    }

    private Vector2 CalculateWallRepelForce()
    {
        Vector2 repelForce = Vector2.zero;
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, wallRepelDistance);

        foreach (Collider2D collider in nearbyColliders)
        {
            if (collider.CompareTag("wall"))
            {
                Vector2 awayFromWall = (Vector2)transform.position - (Vector2)collider.ClosestPoint(transform.position);
                float distance = awayFromWall.magnitude;
                repelForce += awayFromWall.normalized * (wallRepelDistance - distance) / wallRepelDistance * wallRepelForce;
            }
        }

        return repelForce;
    }

    private void UpdateExploredCells()
    {
        Vector2Int currentCell = new Vector2Int(
            Mathf.FloorToInt(transform.position.x / cellSize),
            Mathf.FloorToInt(transform.position.y / cellSize)
        );
        exploredCells.Add(currentCell);
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.tag == "wall")
        {
            Vector2 wallNormal = col.contacts[0].normal;
            float offset = 0.1f; // Reduced offset for more natural movement

            // Move the bot slightly away from the wall
            rb.MovePosition((Vector2)transform.position + wallNormal * offset);

            // Set a new exploration direction away from the wall
            currentExplorationDirection = Vector2.Lerp(currentExplorationDirection, wallNormal.normalized, 0.5f);

            // Add an impulse force to "bounce" off the wall
            rb.AddForce(wallNormal * wallRepelForce, ForceMode2D.Impulse);

            // Reduce the score penalty for staying on walls
            score -= 5f * Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "edge")
        {
            Vector2 edgeNormal = col.contacts[0].normal;
            float offset = 0.1f; // Reduced offset for more natural movement

            // Move the bot slightly away from the edge
            rb.MovePosition((Vector2)transform.position + edgeNormal * offset);

            // Set a new exploration direction away from the edge
            currentExplorationDirection = Vector2.Lerp(currentExplorationDirection, edgeNormal.normalized, 0.5f);

            // Add an impulse force to "bounce" off the edge
            rb.AddForce(edgeNormal * wallRepelForce, ForceMode2D.Impulse);

            // Reduce the penalty for hitting edges
            score -= 20f;
        }
        else if (col.gameObject.tag == "plr")
        {
            score += 10000;
            foundPlayer = true;
        }
    }

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

    private void ResetForNewGeneration()
    {
        score = 0;
        foundPlayer = false;
        transform.position = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f), 0); // Random starting position
        rb.velocity = Vector2.zero;
        currentVelocity = Vector2.zero;
        SetNewExplorationDirection();
        exploredCells.Clear();
        explorationCenter = transform.position;
    }

    // Method to set initial population flag
    public void SetAsInitialPopulation()
    {
        isInitialPopulation = true;
    }
}
