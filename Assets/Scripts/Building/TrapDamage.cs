﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapDamage : MonoBehaviour {

    [SerializeField] private int damage;
    [SerializeField] private float finishTime;//계속 데미지 주는 것을 방지

    private bool isHurt = false;
    private bool isActivated = false;

    public IEnumerator ActivatedTrapCoroutine()
    {
        isActivated = true;

        yield return new WaitForSeconds(finishTime);
        isActivated = false;
        isHurt = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated)
        {
            if (!isHurt)
            {
                isHurt = true;

                if (other.transform.name == "Player")
                {
                    other.transform.GetComponent<StatusController>().DecreaseHP(damage);
                }
            }
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
