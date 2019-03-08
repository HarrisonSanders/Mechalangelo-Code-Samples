using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterMinion : Minion
{
    public int health = 10;
    //How much damage it deals if it shoots player
    public int damageAmount = 25;
    //How often does it fire (in seconds(ish))
    public float fireRate = 1f;
    float fireTimer = 0f;
    //How fast does is fire
    public float fireVelocity = 10f;

    public Rigidbody bullet;
    public GameObject explosion;
    public GameObject explosionSfxMaker;
    public GameObject hitSfxMaker;
    public Light chargeLight;

    public AudioClip fireSfx;
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public override void Move()
    {
        //Move forward until in specified range
        if (Vector3.Distance(player.transform.position, transform.position) > hoverDistance)
        {
            base.Move();
        }
        //If in range, hover in place
        else
        {
            StopMoving();

            if (fireTimer > fireRate)
            {
                Fire();
            }
            else
            {
                fireTimer += Time.deltaTime;
            }
            chargeLight.intensity = fireTimer / fireRate;
        }
    }

    void Fire()
    {
        //Fire Bullet at player
        Rigidbody newBullet = Instantiate(bullet, transform.position, bullet.rotation) as Rigidbody;
        newBullet.AddForce((player.position - transform.position).normalized * fireVelocity, ForceMode.VelocityChange);

        //Make a sound
        audioSource.PlayOneShot(fireSfx);

        //Reset so it can fire again
        fireTimer = 0f;
    }

    //Takes Damage from computer player bullets
    void OnCollisionEnter(Collision collision)
    {
		if (collision.gameObject.tag == "Bullet" || collision.gameObject.layer == 14)
        {
            if (health > 0)
            {
                //Play damaged sfx, if not already one in scene
                if (GameObject.FindWithTag("Hit SFX") == null)
                {
                    Instantiate(hitSfxMaker, transform.position, transform.rotation);
                }
                health -= 1;
            }
            if (health <= 0)
            {
                if (ComputerPlayerShooter.cps.currMissles < ComputerPlayerShooter.cps.maxMissles)
                {
                    ComputerPlayerShooter.cps.currMissles += 1;
                }
                Explode();
            }
        }
    }

    bool exploding = false;

    //Plays Explosion effect and destroys self
    public void Explode()
    {
        if (exploding) return;
        exploding = true;

        //Charge a powerup
        PowerupManager.pm.ChargeAndSpawnGunPowerup(transform);

        Instantiate(explosion, transform.position, transform.rotation);
        Instantiate(explosionSfxMaker, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
