# Neural Network Hide n' Seek for Congressional App Challenge 2024 "Hidden Data"


### - Overview -
In this game, bots are trained to seek out the hider (the player) using neural networks, improving their ability to find the hider with every generation. *Hidden Data* is designed for teens interested in computer science and machine learning, offering an engaging, hands-on experience that simulates how neural networks operate and evolve over time.
<hr>

### - How it Works -
At the beginning of each generation, 50 AI bots spawn, each tasked with moving toward a target position within a given room. Each bot is given inputs representing an X and Y distance from the goal location. 

These inputs are processed through **hidden layer neurons**, which perform a series of calculations to determine how the AI bot should move at the end of the network.

### - Learning Mechanism -
The performance of every AI bot is evaluated, rewarding a higher score for finding the player or getting to the target position, and a lower score for not finding the hider or running into walls.

At the end of the generation, bots that score in the lower 50% are terminated, while the upper 50% remain in the pool, having children which undergo a mutation process.

Due to the highest-performing bots surviving and reproducing, each generation sees bots improving their ability to find the player successfully.

### - Player Engagement - 
In addition to watching how the bots refine themselves over each generation, players are encouraged to experiment with different hiding strategies and a myriad of parameters, such as `mutation rates`, `amount of neurons`, and `number of neuron layers`. This customizability enables users to learn about how different factors affect the process of training neural networks, their performance, and overall effectiveness. 
<hr>

### - About Us -
"Hidden Data" was created by `Jacob Bancroft`, `Danny Reilly`, `Luke Evenson`, and `Eamon Brisson`, freshmen at North Kingstown High School in Rhode Island, for the *2024 Congressional App Challenge*.

