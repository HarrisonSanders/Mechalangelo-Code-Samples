using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public GameObject shipModel;

	[Header("Movement")]
	public float moveSpeed;
	public float knockbackStrength = 2;
	bool knockbackInProgress = false;

	[Header("Health")]
	public float maxHealth = 50;
	float currHealth;
	public bool invincible = false;

	[Header("Dodge Roll")]
	public float dodgeRollDistance = 1f;
	public float dodgeRollChargeTime = 5f;
	public GameObject[] dodgeRollIndicator;
	float dodgeRechargeTimer = 0f;
	bool canDodgeRoll = true;

	[Header("Lock On")]
    public GameObject lockOnTarget;
    public GameObject lockedOnIndicator;
    public bool canLock;
    bool lockedOn = false;

	[Header("Aim Speed")]
	public float speedH = 2.0f;
	public float speedV = 2.0f;
	float yaw = 0.0f;
	float pitch = 0.0f;

	[Header("Hit SFX")]
	public AudioClip playerHitSFX;

	//A public bool that can be changed/reacted to by external sources
	[HideInInspector]
	public bool dead = false;

	//reference to rigibody so forces can be applied
	Rigidbody rb;

	// Use this for initialization
	void Start()
	{
		ResetSensitivity();
		Physics.IgnoreLayerCollision(8, 8);
		Physics.IgnoreLayerCollision(8, 9);
		Physics.IgnoreLayerCollision(8, 20);
		Physics.IgnoreLayerCollision(20, 9);
		Physics.IgnoreLayerCollision(20, 20);
		rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			Debug.LogError("Hey! Give me a rigidbody.");
		}
		currHealth = maxHealth;
        lockedOnIndicator.SetActive(lockedOn);
	}

	private void FixedUpdate()
	{
        int i = 0;
        foreach(GameObject indicator in dodgeRollIndicator){
            if (i == 0)
            {
                indicator.transform.Rotate(Vector3.down, 7.5f);
                i++;
            }
            else 
            {
                indicator.transform.Rotate(Vector3.up, 7.5f);
            }
        }
	}

	// Update is called once per frame
	void Update()
	{
        if (currHealth <= 0 && !dead)
        {
            //Stop drift
            rb.velocity = Vector3.zero;
            dead = true;
            GameManager.gm.GameOver(false);
        }
        else if (!dead) //Cant Move if dead
        {
            if (knockbackInProgress) return;

            if (!lockedOn) // Manually aiming
            {
                yaw += speedH * Input.GetAxis("Horizontal Right" + GamepadLocator.platform + GamepadLocator.gamepadNumber);
                pitch += speedV * Input.GetAxis("Vertical Right" + GamepadLocator.platform + GamepadLocator.gamepadNumber);
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
            }
            else
            {
                //Aim at target
                transform.LookAt(lockOnTarget.transform);

            }

            float vx = Input.GetAxis("Horizontal Left" + GamepadLocator.gamepadNumber);
            float vy;
            float vz = Input.GetAxis("Vertical Left" + GamepadLocator.gamepadNumber);
            if (Input.GetButton("Raise" + GamepadLocator.platform + GamepadLocator.gamepadNumber))
            {
                if (!lockedOn || (transform.eulerAngles.x < 85 || transform.eulerAngles.x > 95))
                {
                    vy = 1;
                }
                else
                {
                    vy = 0;
                }
            }
            else if (Input.GetButton("Lower" + GamepadLocator.gamepadNumber))
            {
                if (!lockedOn || (transform.eulerAngles.x < 275 || transform.eulerAngles.x > 285))
                {
                    vy = -1;
                }
                else
                {
                    vy = 0;
                }
			}
			else
			{
				vy = 0;
			}

			Vector3 direction = new Vector3();
			direction = new Vector3(vx, vy);

			if (vx < 0 || vx > 0)
			{
				shipModel.transform.localEulerAngles = AngleLerp(shipModel.transform.localEulerAngles, new Vector3(shipModel.transform.localEulerAngles.x, shipModel.transform.localEulerAngles.y, -15 * vx), .5f);
			}
			else
			{
				shipModel.transform.localEulerAngles = AngleLerp(shipModel.transform.localEulerAngles, new Vector3(shipModel.transform.localEulerAngles.x, shipModel.transform.localEulerAngles.y, 0), .5f);
			}

			//rb.velocity = direction * moveSpeed;
			rb.velocity = transform.forward * vz * moveSpeed;
			if (vx != 0 || vy != 0)
			{
				rb.AddRelativeForce(new Vector3(vx, vy, 0) * 50, ForceMode.Force);
			}

			DodgeRollCheck(vx, vz);

            //Toggle Lock On/Off
            if(Input.GetButtonDown("Lock" + GamepadLocator.platform + GamepadLocator.gamepadNumber) && canLock)
            {
                Vector3 visTest = GetComponentInChildren<Camera>()
                    .WorldToViewportPoint(GameObject.FindWithTag("Can Lock Onto").GetComponent<Transform>().position);
                //Check if the VR player is visible, then lock on if he be
                if((visTest.x >= 0 && visTest.y >= 0) && (visTest.x <= 1 && visTest.y <= 1) && visTest.z >= 0)
                {
                    lockedOn = !lockedOn;
                    lockedOnIndicator.SetActive(lockedOn);

                    if (!lockedOn) //If unlocking, set camera to current look
                    {
                        yaw = transform.eulerAngles.y;
                        pitch = transform.eulerAngles.x;
                        //if (!quickTurning) 
                        //{
                        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                        //}
                    }
                }

                else
                {
                    Debug.Log("VR Boi not in view");
                }
            }
		}
        if(dead)
        {
            rb.velocity = Vector3.zero;
        }
    }

	void DodgeRollCheck(float vx, float vz)
	{
		//Roll Right
		if (Input.GetButtonDown("Dodge" + GamepadLocator.gamepadNumber) && canDodgeRoll)
		{
			CheckDodgeRoll(vx, vz);
		}

		//Recharge dodgeroll over time
		if (!canDodgeRoll)
		{
			dodgeRechargeTimer += Time.deltaTime;
			Debug.Log("recharging...");
			if (dodgeRechargeTimer > dodgeRollChargeTime)
			{
				Debug.Log("recharged!");
                foreach (GameObject indicator in dodgeRollIndicator){
                    indicator.SetActive(true);
                }
				//dodgeRollIndicator.SetActive(true);
				canDodgeRoll = true;
				dodgeRechargeTimer = 0f;
			}
		}
	}

	//Started the dodge roll or performs it aft the double tap
	void CheckDodgeRoll(float vx, float vz)
	{
		if (canDodgeRoll)
		{
            foreach (GameObject indicator in dodgeRollIndicator)
            {
                indicator.SetActive(false);
            }

			//Start the dodge
			StartCoroutine(Dodgeroll(vx, vz));

			//Consume dodge roll
			canDodgeRoll = false;
		}
	}

	IEnumerator Dodgeroll(float vx, float vz)
	{
		Vector3 dodgeDir = new Vector3(0, 0, 0);
		Vector3 currentRotation = new Vector3(0, 0, 0);
		Vector3 spinToRotation = new Vector3(0, 0, 0);
        if(Mathf.Abs(vx) > Mathf.Abs(vz) && vx != 0) //Roll Left/right
        {
            if (vx < 0) //Roll right
            {
                dodgeDir = new Vector3(-300, 0, 0);
                currentRotation = new Vector3(0, 0, -335);
                spinToRotation = new Vector3(0, 0, 5);
            }
            else if (vx > 0) //roll left
            {
                dodgeDir = new Vector3(300, 0, 0);
                currentRotation = new Vector3(0, 0, 5);
                spinToRotation = new Vector3(0, 0, -335);
            }
        }
		
        else //Boost/quick turn
        {
            if (vz > 0) // boost
            {
                dodgeDir = new Vector3(0, 0, 300);
            }
            else if (vz <= 0) // back juke
            {
                dodgeDir = new Vector3(0, 0, -300);
            }
        }

		//Lerp to place
		float timer = 0f;
		while (timer <= .25f)
		{

			shipModel.transform.localEulerAngles = Vector3.Lerp(
				currentRotation,
				spinToRotation,
				(timer / .25f)
			);

			rb.AddRelativeForce(dodgeDir, ForceMode.Force);
			timer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

        shipModel.transform.localEulerAngles = spinToRotation;
	}


	Vector3 AngleLerp(Vector3 StartAngle, Vector3 FinishAngle, float t)
	{
		float xLerp = Mathf.LerpAngle(StartAngle.x, FinishAngle.x, t);
		float yLerp = Mathf.LerpAngle(StartAngle.y, FinishAngle.y, t);
		float zLerp = Mathf.LerpAngle(StartAngle.z, FinishAngle.z, t);
		Vector3 Lerped = new Vector3(xLerp, yLerp, zLerp);
		return Lerped;
	}

	//Gives player a brief amount of time to recover when hit.
	public IEnumerator Invincibility()
	{
		invincible = true;
		yield return new WaitForSeconds(.5f);
		invincible = false;
	}

	void OnCollisionEnter(Collision collision)
	{
		//Collision with an explosion (bomb minon)
		if (collision.gameObject.layer == 17)
		{
			Destroy(collision.gameObject);
			if (invincible == false)
			{
				StartCoroutine(Invincibility());
				TakeDamage(25);
			}
		}
		//Collision with a bullet
		else if (collision.gameObject.layer == 14 || collision.gameObject.layer == 28)
		{
			collision.gameObject.GetComponent<Bullet>().CustomDestroy();
			if (invincible == false)
			{
				StartCoroutine(Invincibility());
				TakeDamage(collision.gameObject.GetComponent<Bullet>().damage);
			}
		}
        
        // knockback when touch shield
        if (collision.gameObject.tag == "PaintShield")
        {
            collision.gameObject.GetComponent<Shield>().Boink();
            Knockback(collision.contacts[0].point);
        }
        // knockback when touch VR head
        if (collision.gameObject.layer == 13)
        {
            Knockback(collision.contacts[0].point);
        }
	}

    public void Knockback(Vector3 hitPoint)
	{
		if (invincible || knockbackInProgress) return;

        Vector3 dir = transform.position - hitPoint;
        dir.Normalize();
        dir *= -1;
		rb.AddForce(dir * knockbackStrength, ForceMode.Impulse);
		StartCoroutine("DoKnockback");
	}

	IEnumerator DoKnockback() {
		invincible = true;
		knockbackInProgress = true;

		yield return new WaitForSeconds(0.2f);

		invincible = false;
		knockbackInProgress = false;
	}

    public void TakeDamage(int damageAmount)
    {
        currHealth -= damageAmount;
        UIManager.uim.UpdateComputerPlayerHealth((currHealth / maxHealth));
        StartCoroutine(ScreenShaker.ShakeScreenLeftRight(GetComponentInChildren<Camera>().transform, .5f));
        GetComponent<AudioSource>().PlayOneShot(playerHitSFX);
    }

	//When lock-on is turned off, we need to turn the player to reflect the lock-on angles
    public void DisengageLock() {
        lockedOn = false;
        lockedOnIndicator.SetActive(lockedOn);

        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

        //Stop drift
        rb.velocity = Vector3.zero;
    }

	//If the sensitivity has been updated in the game, load it from player prefs.
    public void ResetSensitivity()
    {
        //Get Aim Speed based on options
        speedH = PlayerPrefsManager.GetAimSensitivity()/2f;
        speedV = PlayerPrefsManager.GetAimSensitivity()/2f;
    }
}
