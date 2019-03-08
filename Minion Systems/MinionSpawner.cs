using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnerType
{
    shooter,
    bomb
}

//This is a system that spawns minions at a set interval.
//In the end, we dd not end up using this method, instead giong with a player-actuvated spawner
//That class is also included (MinionSpawnButton.cs)

//A Script to be placed on a spawner. Spawns enemies, tracks its own health
public class MinionSpawner : MonoBehaviour {

    public bool SpawnMinionWithKey = false;
    public KeyCode spawnKey = KeyCode.Space;

    [Space(20)]
    [Header("Minion Types (Include all you intend to spawn)")]
    public GameObject shooterMinion;
    public GameObject bombMinion;

    [Header("These objects should be children of the spwaner")]
    public GameObject enabledArt;
    public GameObject disabledArt;
    //Minions are spawned under this for Counting
    public GameObject minionHolder;

    //How often to minions spawn, and how many spawn?
    public float spawnInterval = 5f;
    float spawnTimer = 0f;
    public int maxMinions = 5;

    //Health System
    public int startingHealth = 50;
    public int repairRate = 10;
    int currentHealth;

    //State bools
    bool dead = false;

    [HideInInspector]
    public static MinionSpawner S;

	// Use this for initialization
	void Start () {
        currentHealth = startingHealth;

        enabledArt.SetActive(true);
        disabledArt.SetActive(false);

        if (S == null)
        {
            S = GetComponent<MinionSpawner>();
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (SpawnMinionWithKey && Input.GetKeyDown(spawnKey)) {
            SpawnMinion();
        }


        if(!dead)
        {
            if (minionHolder.transform.childCount < maxMinions && spawnTimer > spawnInterval)
            {
                //SpawnMinion();
                spawnTimer = 0f;
            }
            if (spawnTimer <= spawnInterval)
            {
                spawnTimer += Time.deltaTime;
            }
        }
	}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            Damage(1);
        }
    }

    //Player can hit Spawner, causing damage to be dealt
    void Damage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if(!dead)
            {
                Die();
            }
        }
    }

    //Spawner does not spawn if dead
    public void Die()
    {
        currentHealth = 0;
        dead = true;

        //Show disabled art so Mech knows to repair
        enabledArt.SetActive(false);
        disabledArt.SetActive(true);
    }

    //VR Man can repair the Spawner. If dead, It will start spawning agin when fully repaired
    public void Repair(int repairAmount)
    {
        currentHealth += repairAmount;
        if(currentHealth >= startingHealth)
        {
            currentHealth = startingHealth;
            if(dead)
            {
                Revive();
            }
        }
    }

    //Spawner can go back to causing trouble
    void Revive()
    {
        dead = false;

        enabledArt.SetActive(true);
        disabledArt.SetActive(false);

        //Reset Spawn Timer so minion is not spawned immedeatly
        spawnTimer = 0f;
    }

    //Spawn A Minion at spawn point. Based on the current SpawnerType setting.
    public void SpawnMinion()
    {
        //Random Chance
        int whichMinion = Random.Range(0, 2);

        if (whichMinion == 0)
        {
            Instantiate(shooterMinion, transform.position, Quaternion.identity, minionHolder.transform);
        }
        if (whichMinion == 1)
        {
            Instantiate(bombMinion, transform.position, Quaternion.identity, minionHolder.transform);
        }
    }

}
