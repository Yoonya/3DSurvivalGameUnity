using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayAndNight : MonoBehaviour {

    [SerializeField] private float secondPerRealTimeSecond;//게임세계에서의 100초 = 현실1초

    [SerializeField] private float fogDensityCalc; //증감량 비율

    [SerializeField] private float nightFogDensity; //밤 상태의 Fog 밀도
    private float dayFogDensity; //낮 상태의 fog 밀도
    private float currentFogDensity; //계산

	// Use this for initialization
	void Start () {
        dayFogDensity = RenderSettings.fogDensity; //window창에 lighting 창에 fog 사용
    }
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.right, 0.1f * secondPerRealTimeSecond * Time.deltaTime);

        if (transform.eulerAngles.x >= 170) //슬슬 해짐
            GameManager.isNight = true;
        else if (transform.eulerAngles.x <= 10 )//슬슬 해뜸
            GameManager.isNight = false;

        if (GameManager.isNight)
        {
            if (currentFogDensity <= nightFogDensity)
            {
                currentFogDensity += 0.1f * fogDensityCalc * Time.deltaTime;
                RenderSettings.fogDensity = currentFogDensity;
            }
        }
        else
        {
            if (currentFogDensity >= dayFogDensity)
            {
                currentFogDensity -= 0.1f * fogDensityCalc * Time.deltaTime;
                RenderSettings.fogDensity = currentFogDensity;
            }
        }
        
	}
}
