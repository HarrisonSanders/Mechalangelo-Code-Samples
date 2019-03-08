using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class ComputerPlayerShooter : MonoBehaviour {

    public Rigidbody bullet;
    public Rigidbody missile;
    public float velocity;
    public float missileVelocity;
    public AudioClip shotLevel1SFX;
    public AudioClip shotLevel2SFX;
    public AudioClip shotLevel3SFX;
    public AudioClip missileSFX;
    public GameObject spawnPoint;

    [Space(10)]
    public Transform reticle;

    //Aiming utilities
    Ray mouseRay;
    RaycastHit enemyHit;
    public LayerMask layerMask;
    bool aimingAtEnemy;

    Rigidbody rb;
    public static ComputerPlayerShooter cps;

    //The amount of time between bullets if button is held
    public float fireDelay = 0.1f;
    bool firing;
    bool shooting;
    public int maxMissles = 3;
    public float missileRechargeTime = 10f;
    float missileRechargeTimer = 0f;
    public int currMissles;
    bool fired;

    [HideInInspector]
    public bool dead = false;

    //Used for SFX ONLY in this script. extra shooting handled on powerup
    [HideInInspector]
    public bool extraGun = false;

    // Use this for initialization
    void Start()
    {
        if (cps == null)
        {
            cps = this;
        }
        else
        {
            Destroy(this);
        }
        layerMask = ~layerMask;
        Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
        UIManager.uim.UpdateComputerPlayerRockets(currMissles);
    }

    // Update is called once per frame
    void Update()
    {
        if(!dead)
        {
            reticle.transform.LookAt(gameObject.transform);
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
            {
                reticle.gameObject.SetActive(true);
                reticle.transform.position = hit.point - transform.TransformDirection(Vector3.forward) * 0.01f;
            } else
            {
                reticle.gameObject.SetActive(false);        // disable reticle when it's not on anything so it doesn't hang out somewhere weird
            }

            if (Input.GetAxis("Fire" + GamepadLocator.platform + GamepadLocator.gamepadNumber) > .5f)
            {
                if (!firing)
                {
                    firing = true;
                    StartCoroutine(FireBullet(bullet, false));

                    if(!shooting)
                    {
                        if(extraGun)
                        {
                            GetComponent<AudioSource>().clip = shotLevel3SFX;
                        }
                        else
                        {
                            GetComponent<AudioSource>().clip = shotLevel1SFX;
                        }
                        GetComponent<AudioSource>().Play();
                        shooting = true;
                    }
                }
            }
            else if (Input.GetAxis("Fire" + GamepadLocator.platform + GamepadLocator.gamepadNumber) < .5f && shooting)
            {
                shooting = false;
                GetComponent<AudioSource>().Stop();
            }
            if (Input.GetAxis("Missile" + GamepadLocator.platform + GamepadLocator.gamepadNumber) > .5f && currMissles > 0 && !fired)
            {
                fired = true;
                Rigidbody newMissile = Instantiate(missile, spawnPoint.transform.position, gameObject.transform.rotation) as Rigidbody;
                newMissile.AddForce(gameObject.transform.forward * missileVelocity, ForceMode.VelocityChange);
                currMissles -= 1;
                UIManager.uim.UpdateComputerPlayerRockets(currMissles);
                GetComponent<AudioSource>().PlayOneShot(missileSFX);
            }
            else if (Input.GetAxis("Missile" + GamepadLocator.platform + GamepadLocator.gamepadNumber) < .5f)
            {
                fired = false;
            }

            //Add missiles over time
            if (currMissles != maxMissles && missileRechargeTimer < missileRechargeTime)
            {
                missileRechargeTimer += Time.deltaTime;
            }
            else if (currMissles != maxMissles && missileRechargeTimer >= missileRechargeTime)
            {
                missileRechargeTimer = 0f;
                currMissles++;
                UIManager.uim.UpdateComputerPlayerRockets(currMissles);
            }
        }
    }

    public IEnumerator FireBullet(Rigidbody objectToFire, bool isSpecial)
    {
        //Make new bullet just in front of player
        Rigidbody newBullet = Instantiate(objectToFire, spawnPoint.transform.position, objectToFire.rotation) as Rigidbody;
        newBullet.AddForce(gameObject.transform.forward * velocity, ForceMode.VelocityChange);

        yield return new WaitForSeconds(fireDelay);
        firing = false;
    }
}