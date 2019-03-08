using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupManager : MonoBehaviour {
    
    [Header("Powerups Pickups to Spawn")]
    public GameObject shieldPowerup;
    public GameObject extraGunPowerup;

    [Header("Rates of Powerup Drops (%of drop gained by action)")]
    public float shieldChargeRate;
    public float extraGunChargeRate;

    public static PowerupManager pm;

    //Charges up to 100, then drops powerup
    float shieldCharge = 0f;
    float extraGunCharge = 0f;

	// Use this for initialization
	void Start () {
        if(pm == null) {
            pm = GetComponent<PowerupManager>();
        }
	}
	
    //Charges the shield powerup, and instantiates it if the shield is ready
    public void ChargeAndSpawnShieldPowerup(Transform spawnPoint)
    {
        shieldCharge += shieldChargeRate;
        if(shieldCharge >= 100f) {
            //Spawn shield at point of event and reset
            Instantiate(shieldPowerup, spawnPoint.position, Quaternion.identity);

            shieldCharge = 0f;
        }
    }

    //Charges the gun powerup, and instantiates it if the shield is ready
    public void ChargeAndSpawnGunPowerup(Transform spawnPoint)
    {
        extraGunCharge += extraGunChargeRate;
        if (extraGunCharge >= 100f)
        {
            //Spawn shield at point of event and reset
            Instantiate(extraGunPowerup, spawnPoint.position, Quaternion.identity);

            extraGunCharge = 0f;
        }
    }

}
