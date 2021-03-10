using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour {

    [SerializeField]
    private string fireName;
    [SerializeField]
    private int damage;
    [SerializeField]
    private float damageTime; //데미지 딜레이
    private float currentDamageTime;

    [SerializeField]
    private float durationTime;//불의 지속시간
    private float currentDurationTime;

    [SerializeField]
    private ParticleSystem ps_Flame; //파티클시스템

    private bool isFire = true;

    private StatusController thePlayerStatus;

    void Start()
    {
        thePlayerStatus = FindObjectOfType<StatusController>();
        currentDurationTime = durationTime;
    }

    // Update is called once per frame
    void Update () {
        if (isFire)
        {
            ElapseTime();
        }
	}

    private void ElapseTime()
    {
        currentDurationTime -= Time.deltaTime;

        if (currentDamageTime > 0)
            currentDamageTime -= Time.deltaTime;

        if (currentDurationTime <= 0)
        {
            Off();
        }
    }

    private void Off()
    {
        ps_Flame.Stop(); //파티클 스탑
        isFire = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (isFire && other.transform.tag == "Player")
        {
            if (currentDamageTime <= 0)
            {
                other.GetComponent<Burn>().StartBurning();
                thePlayerStatus.DecreaseHP(damage);
                currentDamageTime = damageTime;
            }

        }
    }

    public bool GetIsFire()
    {
        return isFire;
    }
}
