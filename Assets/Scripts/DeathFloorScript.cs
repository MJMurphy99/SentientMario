using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Script used to determine when the player has died
public class DeathFloorScript : MonoBehaviour
{
    public GeneticPlatformController breedPlatforms;

    public static bool deathByCollider;

    //When the player collides with either the floor or a rocket, breed based on a failure
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "DeathFloor")
        {
            deathByCollider = true;
            breedPlatforms.BreedPlatforms();
        }
    }
}