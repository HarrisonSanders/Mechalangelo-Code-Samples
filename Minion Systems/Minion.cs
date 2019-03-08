using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The minion pathing system (function Move()) was handled by another programmer,
// but I've included this base class so that the inherited classes I wrote are easily comprehendable

public class Minion : MonoBehaviour {

    public float chaseSpeed = 5;            // how aggressively does it chase?
    public float avoidSpeed = 5;            // how quick does it run away from other minions?
    public float hoverDistance = 2;         // how close before stop and shoot/bomb?

    float avoidStrength = 10;
    float centerStrength = 0.5f;
    float attractStrength = 1;

    protected Transform player;
    protected Rigidbody rb;
    MinionNeighborDetector mnd;

    void Awake()
    {
        mnd = GetComponentInChildren<MinionNeighborDetector>();
        rb = GetComponent<Rigidbody>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        transform.LookAt(player);
        Move();
    }

    public virtual void Move() {
        Vector3 vel = rb.velocity;

        // avoid minions that get too close, we must keep to ourselves
        Vector3 velAvoid = Vector3.zero;
        Vector3 tooClosePos = mnd.avgClosePos;
        // we only care if there are minions nearby
        if (tooClosePos != Vector3.zero)
        {
            velAvoid = transform.position - tooClosePos;
            velAvoid.Normalize();
            velAvoid *= avoidSpeed;
        }

        // try to be in the middle of the formation, that's where the cool guys hang out
        Vector3 velCenter = mnd.avgPos;
        if (velCenter != Vector3.zero)
        {
            velCenter -= transform.position;
            velCenter.Normalize();
            velCenter *= chaseSpeed;
        }

        // we're attracted to the player, that's a nice boat
        Vector3 velAttract = (player.transform.position - transform.position).normalized * chaseSpeed;

        // apply velocity
        if (velAvoid != Vector3.zero)
        {
            vel = Vector3.Lerp(vel, velAvoid, avoidStrength * Time.fixedDeltaTime);
        }
        if (velCenter != Vector3.zero)
        {
            vel = Vector3.Lerp(vel, velCenter, centerStrength * Time.fixedDeltaTime);
        }
        if (velAttract != Vector3.zero)
        {
            vel = Vector3.Lerp(vel, velAttract, attractStrength * Time.fixedDeltaTime);
        }
        vel = vel.normalized * chaseSpeed;
        rb.velocity = vel;
    }

    // e brake
    protected void StopMoving()
    {
        rb.velocity = Vector3.zero;
    }

}