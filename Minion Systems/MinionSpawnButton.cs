using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionSpawnButton : MonoBehaviour {

    public float rechargeTime;
    float timer = 0f;

    public GameObject enabledGameObject;
    public GameObject disabledGameObject;
    public GameObject chargedSFXMaker;

    bool canSpawn = false;

	// Use this for initialization
	void Start () {
        enabledGameObject.SetActive(false);
        disabledGameObject.SetActive(true);
    }
	
	// Update is called once per frame
	void Update () {
        if(!canSpawn)
        {
            if(timer < rechargeTime)
            {
                timer += Time.deltaTime;
            }
            else
            {
                enabledGameObject.SetActive(true);
                disabledGameObject.SetActive(false);
                canSpawn = true;
                timer = 0f;

                Instantiate(chargedSFXMaker, transform.position, transform.rotation);
                StartCoroutine(FlashMinionReady());
            }
        }
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Mech")
        {
            if(canSpawn)
            {
                other.gameObject.transform.parent.gameObject.GetComponent<Paintbrush>().Controller.TriggerHapticPulse(1500);
                MinionSpawner[] spawners = GetComponentsInChildren<MinionSpawner>();
                foreach(MinionSpawner spawner in spawners){
                    spawner.SpawnMinion();
                }
                enabledGameObject.SetActive(false);
                disabledGameObject.SetActive(true);
                canSpawn = false;
            }
        }
    }

    IEnumerator FlashMinionReady()
    {
        VRUIManager.S.ToggleMinionReady(true);
        yield return new WaitForSeconds(1f);
        VRUIManager.S.ToggleMinionReady(false);
        yield return new WaitForSeconds(.5f);
        VRUIManager.S.ToggleMinionReady(true);
        yield return new WaitForSeconds(.5f);
        VRUIManager.S.ToggleMinionReady(false);
        yield return new WaitForSeconds(.5f);
        VRUIManager.S.ToggleMinionReady(true);
        yield return new WaitForSeconds(.5f);
        VRUIManager.S.ToggleMinionReady(false);
    }
}
