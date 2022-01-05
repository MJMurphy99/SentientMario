using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

//Ga that controls the width of the platforms over generations
public class GeneticPlatformController : MonoBehaviour
{
    //Size of the inital population of platforms
    public int populationSize = 6;

    //Creating an initalization of the platform prefab
    public GameObject platformPrefab;

    //Create a list of the platforms that make up the current population
    public List<GameObject> population;

    //the percentage chance that an object in the population has of randomly mutating 
    public int mutationrate;

    //Visual UI elements
    public Text currentGeneration;
    private int currentGenerationInt;
    public Text timeOnLevel;
    private float timeOnLevelTimer;

    //The position that we want the player to spawn into at the start of each generation
    public GameObject playerStartPosition;

    //used for determining position of the different platforms since they are being instantiated at start
    public Vector3[] positionArray = new Vector3[6];

    //used to keep a sorted list of the different width values of the platforms in a numeric order
    private List<GameObject> sortedList;

    //Way of meauring if the player achieves a certain "skill level" during their run
    public int skillLevelInt = 0;

    // Start is called before the first frame update
    void Start()
    {
        InitialisePlatformWidth();
    }

    // Update is called once per frame
    void Update()
    {
        //Update the UI elements as the game goes on
        currentGeneration.text = "Generation: " + currentGenerationInt;

        timeOnLevelTimer += Time.deltaTime;
        timeOnLevel.text = "Time spent: " + (timeOnLevelTimer).ToString("0");
    }

    //We only need to run this once, as it starts us off
    //This function will spawn in the objects in our population and assign them a random size for the first iteration
    private void InitialisePlatformWidth()
    {
        for (int i = 0; i < populationSize; i++)
        {
            GameObject platform = Instantiate(platformPrefab, positionArray[i], Quaternion.identity);
            PlatformWidthDNA platformWidth = platform.GetComponent<PlatformWidthDNA>();

            platformWidth.widthOfPlatformDNA = Random.Range(0.75f, 1.25f);

            platform.GetComponent<Transform>().localScale = new Vector3(platformWidth.widthOfPlatformDNA, 1f, 0);

            population.Add(platform);
        }
    }

    //Breeding function for the platforms
    public void BreedPlatforms()
    {
        //Initialize the player's position at the start of each generation
        playerStartPosition.transform.position = new Vector3(-10, 0, 0);

        //At the start of each generation, set the level timer to 0
        timeOnLevelTimer = 0;

        //create a new list of game objects
        List<GameObject> newPopulation = new List<GameObject>();

        //If the player beats the level in under 15 seconds, they will be considered "high skill"
        //this will breed the platforms starting with the smallest 
        if (timeOnLevelTimer < 15 && DeathFloorScript.deathByCollider == false)
        {
            sortedList = population.OrderByDescending(o => o.GetComponent<PlatformWidthDNA>().widthOfPlatformDNA).ToList();
            skillLevelInt = 1;
        }
        //If the player beats the level in over 15 seconds or dies, they will be considered "low skill"
        //this will breed the platforms, starting with the largest
        else if (timeOnLevelTimer >= 15 || DeathFloorScript.deathByCollider == true)
        {
            sortedList = population.OrderBy(o => o.GetComponent<PlatformWidthDNA>().widthOfPlatformDNA).ToList();
            skillLevelInt = 2;
        }

        //empty out the old platforms from the population
        population.Clear();

        //iterate through the list of the new platforms and breed them together
        for (int i = 0; i < sortedList.Count; i++)
        {
            //Add the result of the breeding into the population, repopulating the inital list
            population.Add(Breed(sortedList[i], sortedList[i], positionArray[i]));
        }

        //empty out the sorted list for the next breed function
        for (int i = 0; i < sortedList.Count; i++)
        {
            Destroy(sortedList[i]);
        }

        //increase the current generation by 1
        currentGenerationInt++;
    }

    //The breed function, we breed two parents together and use that data to create new offspring for the next population
    private GameObject Breed(GameObject parent1, GameObject parent2, Vector3 platformPosition)
    {
        //Ensure that the player won't just instantly die at the start of each breed
        DeathFloorScript.deathByCollider = false;

        //Create an offspring game object
        GameObject offspring = Instantiate(platformPrefab, platformPosition, Quaternion.identity);

        //initialize the offspringDNA
        PlatformWidthDNA offspringDNA = offspring.GetComponent<PlatformWidthDNA>();

        //Create two different DNA components for the platforms the breed with, which are pulled from the two parent objects 
        PlatformWidthDNA dna1 = parent1.GetComponent<PlatformWidthDNA>();
        PlatformWidthDNA dna2 = parent2.GetComponent<PlatformWidthDNA>();

        //How we obtain the random range for the platforms as the player progresses
        //If the program does not mutate, breed using the parent objects
        if (mutationrate <= Random.Range(0, 100))
        {
            offspringDNA.widthOfPlatformDNA = Random.Range(0, 10) < 5 ? dna1.widthOfPlatformDNA : dna2.widthOfPlatformDNA;
        }
        //If the program does mutate, breed using random numbers 
        //This keeps the system feeling fresh and prevents the platforms from homogonizing
        else
        {
            //We still check the player's skill for this section, as I wanted the randomness to feel fair and more controlled
            if (skillLevelInt == 1)
            {
                offspringDNA.widthOfPlatformDNA = Random.Range(0.75f, 0.95f);
            }
            else
            {
                offspringDNA.widthOfPlatformDNA = Random.Range(0.95f, 1.25f);
            }

        }

        //Return the offspring, which now uses the width of the platform DNA as it's new width
        offspring.GetComponent<Transform>().localScale = new Vector3(offspringDNA.widthOfPlatformDNA, 1f, 0);

        return offspring;

    }

}
