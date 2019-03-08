using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupExtraGun : MonoBehaviour
{

    [Header("Time player has to pick this up")]
    public float despawnTime;
    float timeInWorld;

    [Header("How long does the player have it?")]
    public float gunTime;
    [HideInInspector]
    public float timer = 0f;

    [Header("Visual States (Should all be children of object)")]
    public GameObject pickupRegular;
    public GameObject pickupFlash;
    public GameObject onPlayer;

    [Header("Sounds")]
    public AudioClip pickedUpSFX;

    bool flashing = false;
    bool pickedUp = false;
    BoxCollider _collider;
    Rigidbody _rigidbody;

    [Header("Things for shooting")]
    public Rigidbody bullet;
    public float fireDelay = 0.1f;
    public float velocity = 30f;
    public GameObject spawnPoint1;
    public GameObject spawnPoint2;
    bool firing = false;

    GameObject player;
    [Header("Player to attract to if close")]
    public float suckDistance;
    public float suckStrength;
    bool sucking = false;

    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        SetVisual(State.regular);

        _collider = GetComponent<BoxCollider>();
        _rigidbody = GetComponent<Rigidbody>();

    }

    void FixedUpdate()
    {
        if (!pickedUp)
        {
            transform.Rotate(Vector3.up, 1f);
        }
    }

    // Update is called once per frame
    void Update()
    {

        //Only do timer things when not picked up
        if (!pickedUp)
        {
            if (timeInWorld < despawnTime) // still timing
            {
                timeInWorld += Time.deltaTime;
                if (timeInWorld / despawnTime > .66f && !flashing)
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
            if(!sucking && Vector3.Distance(player.transform.position, transform.position) < suckDistance)
            {
                Debug.Log("Starting Suck");
                sucking = true;
                _rigidbody.isKinematic = false;
            }
            else if(sucking)
            {
                //Attract to player
                Vector3 velAttract = (player.transform.position - transform.position).normalized * suckStrength;
                _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, velAttract, suckStrength * Time.fixedDeltaTime);
            }
        }

        else //exist for time
        {
            if(timer < gunTime)
            {
                //Debug.Log(timer);
                timer += Time.deltaTime;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        //if player shooting, shoot more
        if (Input.GetAxis("Fire" + GamepadLocator.platform + GamepadLocator.gamepadNumber) > .5f && pickedUp)
        {
            if (!firing)
            {
                firing = true;
                StartCoroutine(FireBullet(bullet));
            }
        }
    }

    #region Not Picked Up

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Here");
        //if collided with player, 'pick up'
        if (other.gameObject.tag == "Player")
        {
            PickUp();
            GetComponent<ParticleSystem>().enableEmission = false;
        }
    }

    //Once sheild is picked up, it becomes a child of the player.
    void PickUp()
    {
        _rigidbody.isKinematic = true;
        pickedUp = true;

        //turn the trigger into a collider so it interacts with enemy bullets
        _collider.isTrigger = false;

        //Change collider size to size of shield visual
        _collider.size = new Vector3(6, 3f, 5f);

        //If player already has a shield, replace with this one
        if (GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PowerupExtraGun>() != null)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PowerupExtraGun>().timer += gunTime;
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
        float flashtimer = 0f;
        while (flashtimer < flashTime)
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

    public IEnumerator FireBullet(Rigidbody objectToFire)
    {
        Debug.Log("Here");
        //Make new bullet just in front of new barrels
        Rigidbody newBullet = Instantiate(objectToFire, spawnPoint1.transform.position, objectToFire.rotation) as Rigidbody;
        newBullet.AddForce(gameObject.transform.forward * velocity, ForceMode.VelocityChange);

        newBullet = Instantiate(objectToFire, spawnPoint2.transform.position, objectToFire.rotation) as Rigidbody;
        newBullet.AddForce(gameObject.transform.forward * velocity, ForceMode.VelocityChange);

        yield return new WaitForSeconds(fireDelay);
        firing = false;
    }

    #endregion

    //set visual based on current state
    void SetVisual(State state)
    {
        pickupRegular.SetActive(false);
        pickupFlash.SetActive(false);
        onPlayer.SetActive(false);

        if (state == State.regular)
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
