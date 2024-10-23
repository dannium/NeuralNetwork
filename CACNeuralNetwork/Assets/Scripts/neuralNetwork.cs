using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using TMPro;
using Unity.VisualScripting;
using System.Threading;
using System.Xml;
using System.Transactions;
using UnityEditor.Callbacks;
using System.Security.Cryptography;
using System.Data;

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
    float foundPlayerMoveSpeed = 5f; // New variable for speed after finding the player

    // Complex exploration variables
    private Vector2 currentExplorationDirection;
    private float explorationTimer = 0f;
    private float explorationInterval = 5f;
    private Dictionary<Vector2Int, float> exploredCells = new Dictionary<Vector2Int, float>();
    private float cellSize = 2f;
    private float explorationDecayRate = 0.95f;
    private float explorationBoostFactor = 1.5f;
    private Vector2Int currentCell;
    private Queue<Vector2Int> explorationPath = new Queue<Vector2Int>();
    private int pathLength = 10;
    private float explorationNoiseScale = 0.1f;
    private float explorationNoiseFrequency = 0.5f;

    // Simplified movement variables
    private Vector2 currentVelocity;
    private Vector2 targetVelocity;
    private float smoothTime = 0.3f;
    private float accelerationRate = 10f;
    private float decelerationRate = 5f;
    private float minimumMovementSpeed = 0.5f;

    // Simplified wall interaction
    private float wallRepelForce = 1f;
    private float wallRepelDistance = 1f;
    private float wallDetectionDistance = 1.5f;
    private float wallAvoidanceForce = 2f;

    // Exploration variables
    private Vector2 explorationCenter;
    private float explorationRadius = 10f;

    // Separation variables
    private float separationDistance = 10f; // Increased from 3f to encourage more separation
    private float separationForce = 5f; // Increased from 2f to make separation a stronger priority

    // Stuck detection
    private Vector2 lastPosition;
    private float stuckTime = 0f;
    private float stuckThreshold = 1f;

    // Population variables
    public static List<neuralNetwork> population = new List<neuralNetwork>();
    public static int generationCount = 0;
    public static int populationSize = 50;
    public static float elitePercentage = 1f;
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
                    weights[i][j][k] = UnityEngine.Random.Range(-0.5f, 0.5f);
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
            array[2] = currentExplorationDirection.x;
            array[3] = currentExplorationDirection.y;
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
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        foundPlayer = false;
        score = 0;
        GameObject Child = Instantiate(gameObject);
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
        SetNewExplorationDirection();

        // Set initial exploration center
        explorationCenter = transform.position;

        // Only add to population if this is part of the initial population
        if (isInitialPopulation)
        {
            population.Add(this);
        }

        // Spread out the initial positions
        if (isInitialPopulation)
        {
            float spreadRadius = 20f; // Increased from 10f to spread bots further apart initially
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spreadRadius;
            transform.position = new Vector3(randomOffset.x, randomOffset.y, 0);
        }

        // Initialize the exploration path
        GenerateExplorationPath();
    }

    // Update is called once per frame
    void Update()
    {
        if (!foundPlayer)
        {            
            Vector2 distToPlr = (GameObject.FindGameObjectWithTag("plr").transform.position - transform.position);
            score -= 5*Mathf.Abs(distToPlr.x + distToPlr.y);
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
                if (Selection.activeObject == gameObject)
                {
                    print(outputArray[0] + ", " + outputArray[1]);
                }
                // Blend the neural network output with the exploration direction
                movement = Vector2.Lerp(movement, currentExplorationDirection, 0.5f);

                // Apply wall avoidance (with reduced effect)
                Vector2 avoidanceForce = CalculateWallAvoidance();
                movement += avoidanceForce * 0.5f; // Reduced influence of wall avoidance

                // Apply a force towards the exploration center if the bot is too far
                Vector2 toCenter = explorationCenter - (Vector2)transform.position;
                if (toCenter.magnitude > explorationRadius)
                {
                    movement += toCenter.normalized * 0.3f;
                }

                // Apply separation force (increased priority)
                Vector2 separationForce = CalculateSeparationForce();
                movement += separationForce * 2f; // Doubled the influence of separation

                // Apply wall repel force (with reduced effect)
                Vector2 wallRepelForce = CalculateWallRepelForce();
                movement += wallRepelForce * 0.5f; // Reduced influence of wall repel

                // Apply exploration bias based on cell visitation frequency
                Vector2 explorationBias = CalculateExplorationBias();
                movement += explorationBias * 0.5f;

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
            GameObject player = GameObject.FindGameObjectWithTag("plr");
            if (player != null)
            {
                Vector2 directionToPlayer = (player.transform.position - transform.position).normalized;
                int layerMask = ~(1 << LayerMask.NameToLayer("bot"));
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, Mathf.Infinity, layerMask);

                if (hit.collider != null && hit.collider.gameObject == player && hit.distance < 10)
                {
                    score += 10*(10 - hit.distance); //sees plr
                }
            }
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        } 
        else 
        {
            // After finding the player, move faster and in all directions
            Vector2 directionToPlayer = new Vector2(destinationX - transform.position.x, destinationY - transform.position.y);
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer > 0.1f && distanceToPlayer < 10) // If  very close to the player
            {
                directionToPlayer.Normalize();
                Vector2 movement = directionToPlayer * foundPlayerMoveSpeed;
                rb.velocity = movement; // Move towards the player if close to the player
            }
            else
            {
                rb.velocity = Vector2.zero; // Stop when very close to the player
            }

            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Increase score based on separation from other bots
        float totalSeparation = 0f;
        foreach (var bot in population)
        {
            if (bot != this)
            {
                totalSeparation += Vector2.Distance(transform.position, bot.transform.position);
            }
        }
        score += totalSeparation * Time.deltaTime;

        // Reduce score based on the distance from the destination (less priority)
        score -= (Mathf.Abs(transform.position.y - destinationY) + Mathf.Abs(transform.position.x - destinationX)) * Time.deltaTime * 0.1f;

        // Check if all bots have finished or a time limit has been reached
        if (population.Count > 0 && (population.TrueForAll(bot => bot.foundPlayer) || Time.time > 60f)) // 60 seconds time limit
        {
            EvolvePopulation();
        }
    }

    // Set a new exploration direction using a complex method
    private void SetNewExplorationDirection()
    {
        // Update the current cell
        currentCell = new Vector2Int(
            Mathf.FloorToInt(transform.position.x / cellSize),
            Mathf.FloorToInt(transform.position.y / cellSize)
        );

        // If the exploration path is empty or we've reached the end, generate a new path
        if (explorationPath.Count == 0 || currentCell == explorationPath.Peek())
        {
            GenerateExplorationPath();
        }

        // Get the next cell in the path
        Vector2Int targetCell = explorationPath.Peek();

        // Calculate the direction to the next cell
        Vector2 directionToTarget = new Vector2(
            (targetCell.x - currentCell.x) * cellSize,
            (targetCell.y - currentCell.y) * cellSize
        ).normalized;

        // Add some noise to the direction for more natural movement
        float noiseX = Mathf.PerlinNoise(Time.time * explorationNoiseFrequency, 0) * 2 - 1;
        float noiseY = Mathf.PerlinNoise(0, Time.time * explorationNoiseFrequency) * 2 - 1;
        Vector2 noise = new Vector2(noiseX, noiseY) * explorationNoiseScale;

        currentExplorationDirection = (directionToTarget + noise).normalized;

        // If we've reached the target cell, move to the next one in the path
        if (Vector2.Distance(transform.position, new Vector2(targetCell.x * cellSize, targetCell.y * cellSize)) < cellSize * 0.5f)
        {
            explorationPath.Dequeue();
        }

        // Occasionally set a new exploration center
        if (UnityEngine.Random.value < 0.2f)
        {
            explorationCenter = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * explorationRadius;
        }
    }

    // Generate a new exploration path
    private void GenerateExplorationPath()
    {
        explorationPath.Clear();
        Vector2Int currentCell = new Vector2Int(
            Mathf.FloorToInt(transform.position.x / cellSize),
            Mathf.FloorToInt(transform.position.y / cellSize)
        );

        for (int i = 0; i < pathLength; i++)
        {
            Vector2Int nextCell = GetLeastExploredNeighbor(currentCell);
            explorationPath.Enqueue(nextCell);
            currentCell = nextCell;
        }
    }

    // Get the least explored neighboring cell
    private Vector2Int GetLeastExploredNeighbor(Vector2Int cell)
    {
        Vector2Int[] neighbors = new Vector2Int[]
        {
            new Vector2Int(cell.x + 1, cell.y),
            new Vector2Int(cell.x - 1, cell.y),
            new Vector2Int(cell.x, cell.y + 1),
            new Vector2Int(cell.x, cell.y - 1)
        };

        Vector2Int leastExploredCell = cell;
        float minExplorationValue = float.MaxValue;

        foreach (Vector2Int neighbor in neighbors)
        {
            if (!exploredCells.ContainsKey(neighbor))
            {
                return neighbor; // If we find an unexplored cell, immediately return it
            }

            if (exploredCells[neighbor] < minExplorationValue)
            {
                minExplorationValue = exploredCells[neighbor];
                leastExploredCell = neighbor;
            }
        }

        return leastExploredCell;
    }

    // Calculate exploration bias based on cell visitation frequency
    private Vector2 CalculateExplorationBias()
    {
        Vector2 bias = Vector2.zero;
        Vector2Int currentCell = new Vector2Int(
            Mathf.FloorToInt(transform.position.x / cellSize),
            Mathf.FloorToInt(transform.position.y / cellSize)
        );

        Vector2Int[] neighbors = new Vector2Int[]
        {
            new Vector2Int(currentCell.x + 1, currentCell.y),
            new Vector2Int(currentCell.x - 1, currentCell.y),
            new Vector2Int(currentCell.x, currentCell.y + 1),
            new Vector2Int(currentCell.x, currentCell.y - 1)
        };

        foreach (Vector2Int neighbor in neighbors)
        {
            if (!exploredCells.ContainsKey(neighbor))
            {
                // Strongly bias towards unexplored cells
                bias += new Vector2(neighbor.x - currentCell.x, neighbor.y - currentCell.y).normalized * explorationBoostFactor;
            }
            else
            {
                // Bias towards less explored cells
                float explorationValue = exploredCells[neighbor];
                bias += new Vector2(neighbor.x - currentCell.x, neighbor.y - currentCell.y).normalized * (1f - explorationValue);
            }
        }

        return bias.normalized;
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

    // Calculate force to maintain separation between bots
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
            separationForce *= this.separationForce;
        }

        return separationForce;
    }

    // Calculate force to repel from walls
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

    // Update the set of explored cells
    private void UpdateExploredCells()
    {
        Vector2Int currentCell = new Vector2Int(
            Mathf.FloorToInt(transform.position.x / cellSize),
            Mathf.FloorToInt(transform.position.y / cellSize)
        );

        if (!exploredCells.ContainsKey(currentCell))
        {
            exploredCells[currentCell] = 1f;
        }
        else
        {
            exploredCells[currentCell] += 1f;
        }

        // Apply decay to all explored cells
        List<Vector2Int> cellsToRemove = new List<Vector2Int>();
        foreach (var cell in new List<Vector2Int>(exploredCells.Keys))
        {
            exploredCells[cell] *= explorationDecayRate;
            if (exploredCells[cell] < 0.01f)
            {
                cellsToRemove.Add(cell);
            }
        }

        // Remove cells with very low exploration values
        foreach (var cell in cellsToRemove)
        {
            exploredCells.Remove(cell);
        }
    }

    // Handle collision with walls
    private void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("wall"))
        {
            Vector2 wallNormal = col.contacts[0].normal;
            float offset = 0.01f; // Reduced offset to allow touching walls

            // Move the bot slightly away from the wall
            rb.MovePosition((Vector2)transform.position + wallNormal * offset);

            // Set a new exploration direction away from the wall, but with less influence
            currentExplorationDirection = Vector2.Lerp(currentExplorationDirection, wallNormal.normalized, 0.3f);

            // Add a smaller impulse force to "bounce" off the wall
            if(!foundPlayer) {
                rb.AddForce(wallNormal * wallRepelForce * 0.2f, ForceMode2D.Impulse);
            } 

            // Reduce the score penalty for touching walls
            score -= 5f * Time.deltaTime; // Reduced penalty
        }
    }

    // Handle collision with edges and player
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("wall"))
        {
            Vector2 edgeNormal = col.contacts[0].normal;
            float offset = 0.05f; // Further reduced offset for more natural movement

            // Move the bot slightly away from the edge
            rb.MovePosition((Vector2)transform.position + edgeNormal * offset);

            // Set a new exploration direction away from the edge
            currentExplorationDirection = Vector2.Lerp(currentExplorationDirection, edgeNormal.normalized, 0.7f);

            // Add a smaller impulse force to "bounce" off the edge
            rb.AddForce(edgeNormal * wallRepelForce * 0.5f, ForceMode2D.Impulse);

            // Reduce the penalty for hitting edges
            score -= 10f;
        }
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.CompareTag("plr") && !foundPlayer)
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
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        score = 0;
        foundPlayer = false;
        
        // Spread out the bots more for the new generation
        float spreadRadius = 30f; // Increased from 15f to spread bots even further apart
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spreadRadius;
        transform.position = new Vector3(randomOffset.x, randomOffset.y, 0);
        
        rb.velocity = Vector2.zero;
        currentVelocity = Vector2.zero;
        SetNewExplorationDirection();
        exploredCells.Clear();
        explorationCenter = transform.position;
        GenerateExplorationPath();
    }

    // Set this bot as part of the initial population
    public void SetAsInitialPopulation()
    {
        isInitialPopulation = true;
    }
} 

