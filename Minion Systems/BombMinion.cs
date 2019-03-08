using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Bomb Man: Chases Computer Player and blows up on impact. Also blows up when shot too much
public class BombMinion : Minion {

    //How much damage it deals if colliding with player
    public int damageAmount = 25;
    //When does it explode?
    public float explodeTime = 2f;
    //How much health does it have
    public int health = 10;

    public GameObject explosion;
    public GameObject regular;
    public GameObject flash;
    public GameObject explodeSfxMaker;
    public GameObject hitSfxMaker;

    bool armed;

    [Header("Select All things you want explosion to damage")]
    public LayerMask layerMask;

    public override void Move()
    {
        //Move forward until in specified range
        if (Vector3.Distance(player.transform.position, transform.position) > 2* hoverDistance && !armed)
        {
            base.Move();
        } 
        else
        {
            if (Vector3.Distance(player.transform.position, transform.position) > hoverDistance)
            {
                // we're attracted to the player, that's a nice boat
                Vector3 velAttract = (player.transform.position - transform.position).normalized * chaseSpeed;
                rb.velocity = velAttract;
            } 
            else
            {
                //If in range, Stop Moving and Arm
                if (!armed)
                {
                    ExplodeNow();
                }
                StopMoving();
            }
        }
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
                    UIManager.uim.UpdateComputerPlayerRockets(ComputerPlayerShooter.cps.currMissles);
                }
                if(!armed)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    public void ExplodeNow()
    {
        StartCoroutine(Explode());
    }

    //Plays Explosion effect and destroys self
    IEnumerator Explode()
    {
        armed = true;
        //Flash until timer is over
        float timer = 0f;
        while (timer < explodeTime)
        {
            //Toggle flash on and off
            flash.SetActive(true);
            regular.SetActive(false);
            yield return new WaitForSeconds(.3f);
            flash.SetActive(false);
            regular.SetActive(true);
            yield return new WaitForSeconds(.3f);
            timer += .6f;
        }

        //Charge a powerup
        PowerupManager.pm.ChargeAndSpawnGunPowerup(transform);

        Instantiate(explodeSfxMaker, transform.position, transform.rotation);

        Instantiate(explosion, transform.position, transform.rotation);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        gameObject.GetComponent<SphereCollider>().enabled = true;
        ScanForItems(gameObject.GetComponent<SphereCollider>());
    }

    //Looks at explosion radius and reacts accordingly.
    void ScanForItems(SphereCollider sCollider)
    {
        Collider[] allOverlappingColliders = Physics.OverlapSphere(sCollider.gameObject.transform.position, sCollider.radius / 3f, layerMask, QueryTriggerInteraction.Collide);
        foreach (Collider item in allOverlappingColliders)
        {
            if (item.gameObject.GetComponent<HeadPiece>() != null && item.gameObject.GetComponent<HeadPiece>()._damaged == false)
            {
                item.gameObject.GetComponent<HeadPiece>().Damage();
            }
            else if (item.gameObject.GetComponent<Bullet>() != null)
            {
                Destroy(item.gameObject);
            }
            else if (item.gameObject.GetComponent<Shield>() != null)
            {
                if (item.gameObject.GetComponent<ProtoTrapField>() == null)
                {
                    Destroy(item.gameObject);
                }
            }
            else if (item.gameObject.layer == 17)
            {
                Destroy(item.gameObject.transform.parent.gameObject);
            }
            // Insta-kill minions and spawners
            else if (item.gameObject.tag == "Minion Spawner")
            {
                item.gameObject.GetComponent<MinionSpawner>().Die();
            }
            else if (item.gameObject.tag == "Bomb Minion")
            {
                BombMinion bm = item.gameObject.GetComponent<BombMinion>();
                if (bm!=null)
                {
                    bm.ExplodeNow();
                }
            }
            else if (item.gameObject.tag == "Shooter Minion")
            {
                //Kill it
                ShooterMinion sm = item.gameObject.GetComponent<ShooterMinion>();
                if (sm != null)
                {
                    sm.Explode();
                }
            }
            else if (item.gameObject.tag == "Player")
            {
                item.gameObject.GetComponent<PlayerController>().TakeDamage(damageAmount);
            }
            else if (item.gameObject.tag == "Applied Shield")
            {
                item.gameObject.GetComponent<PowerupShield>().TakeDamage(damageAmount);
            }
        }
        GameObject spawned = Instantiate<GameObject>(explosion);
        spawned.transform.position = gameObject.transform.position;
        spawned.transform.rotation = gameObject.transform.rotation;
    }
}
