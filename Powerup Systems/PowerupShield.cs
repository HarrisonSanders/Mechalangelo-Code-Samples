using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State {
    regular,
    flash,
    onPlayer
}

//This item exists in two states: not picked up and picked up.
//When not picked up, it exists as a game object in the world.
//Once picked up, it becomes a child of the player, and it's visual changes
public class PowerupShield : MonoBehaviour {

    [Header("Time player has to pick this up")]
    public float despawnTime;
    float timeInWorld;

    [Header("How much health is the shield worth?")]
    public float shieldHealth;
    float health;

    [Header("Visual States (Should all be children of object)")]
    public GameObject pickupRegular;
    public GameObject pickupFlash;
    public GameObject onPlayer;

    [Header("Sounds")]
    public AudioClip pickedUpSFX;
    public AudioClip hitSFX;

    GameObject player;
    [Header("Player to attract to if close")]
    public float suckDistance;
    public float suckStrength;
    bool sucking = false;

    bool flashing = false;
    bool pickedUp = false;
    BoxCollider _collider;
    Rigidbody _rigidbody;

    public float invincibilityDuration;

    private float timeInvincibilityStart;

    private bool invincible = false;

    // Use this for initialization
    void Start () {
        player = GameObject.FindGameObjectWithTag("Player");

        SetVisual(State.regular);

        _collider = GetComponent<BoxCollider>();
        _rigidbody = GetComponent<Rigidbody>();

        StartCoroutine(GoElsewhere());

        health = shieldHealth;
	}
	
    void FixedUpdate()
    {
        if (!pickedUp)
        {
            transform.Rotate(Vector3.up, 1f);
        }
        if (invincible && Time.time >= (timeInvincibilityStart + invincibilityDuration))
        {
            invincible = false;
        }
    }

	// Update is called once per frame
	void Update () {

        //Only do timer things when not picked up
        if (!pickedUp)
        {
            if(timeInWorld < despawnTime) // still timing
            {
                timeInWorld += Time.deltaTime;
                if(timeInWorld / despawnTime > .66f && !flashing)
                {
                    flashing = true;
                    //Time less than (adjustable ^)% or total time, Flash for remaining time
                    StartCoroutine(Flash(despawnTime - timeInWorld));
                }
            }
            else // Lost opportunity for pickup
            {
                Destroy(gameObject);
            }

            //If player is within suck distance, start suck
            if (!sucking && Vector3.Distance(player.transform.position, transform.position) < suckDistance)
            {
                Debug.Log("Starting Suck");
                sucking = true;
                _rigidbody.isKinematic = false;
            }
            else if (sucking)
            {
                //Attract to player
                Vector3 velAttract = (player.transform.position - transform.position).normalized * suckStrength;
                _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, velAttract, suckStrength * Time.fixedDeltaTime);
            }
        }
	}

    #region Moving Away from spawn location
    IEnumerator GoElsewhere()
    {
        _collider.isTrigger = false;

        //Get launched
        Vector3 randomforce = new Vector3(
            Random.Range(-3, 3),
            Random.Range(3, 5),
            Random.Range(-3, 3)
           
        );

        _rigidbody.AddForce(randomforce, ForceMode.VelocityChange);

        yield return new WaitForSeconds(.75f);

        //Stop
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        _collider.isTrigger = true;
    }


    #endregion

    #region Not Picked Up

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Here");
        //if collided with player, 'pick up'
        if(other.gameObject.tag == "Player")
        {
            PickUp();
            GetComponent<ParticleSystem>().enableEmission = false;
        }
    }

    //Once sheild is picked up, it becomes a child of the player.
    void PickUp()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().invincible = true;
        _rigidbody.isKinematic = true;
        pickedUp = true;

        //turn the trigger into a collider so it interacts with enemy bullets
        _collider.isTrigger = false;

        //Change collider size to size of shield visual
        _collider.size = new Vector3(6, 3f, 5f);

        //If player already has a shield, replace with this one
        if(GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PowerupShield>() != null)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PowerupShield>().TakeDamage((int)shieldHealth);

        }

        //Attach to player
        transform.parent = GameObject.FindGameObjectWithTag("ShipBody").transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        gameObject.layer = 24; //applied powerups
        SetVisual(State.onPlayer);

        //play sound
        GetComponent<AudioSource>().PlayOneShot(pickedUpSFX);
    }

    //Visual ONLY warning for pickup going away
    IEnumerator Flash(float flashTime)
    {
        //Flash until timer is over
        float timer = 0f;
        while (timer < flashTime)
        {
            if (!pickedUp)
            {
                //Toggle flash on and off
                SetVisual(State.flash);
            }
            yield return new WaitForSeconds(.3f);
            if (!pickedUp)
            {
                SetVisual(State.regular);
            }
            yield return new WaitForSeconds(.3f);

            timer += .6f;
        }
    }
    #endregion

    #region InUse

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 17) //Still in use??
        {
            Destroy(collision.gameObject);
            TakeDamage(25);
        }
        else if (collision.gameObject.layer == 14) //paint bullet
        {
            if (invincible)
            {
                return;
            }
            Debug.Log("hit");
            collision.gameObject.GetComponent<Bullet>().CustomDestroy();
            TakeDamage(collision.gameObject.GetComponent<Bullet>().damage);
            invincible = true;
            timeInvincibilityStart = Time.time;
        }

    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        GetComponent<AudioSource>().PlayOneShot(hitSFX);

        if(health <= 0) // Shield b gon
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().invincible = false;
            Destroy(gameObject);
        }
    }

    #endregion

    //set visual based on current state
    void SetVisual(State state) {
        pickupRegular.SetActive(false);
        pickupFlash.SetActive(false);
        onPlayer.SetActive(false);

        if(state == State.regular)
        {
            pickupRegular.SetActive(true);
        }
        else if (state == State.flash)
        {
            pickupFlash.SetActive(true);
        }
        else 
        {
            onPlayer.SetActive(true);
        }
    }
}
