using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour {

    public static bool isActivated = true;

    private Vector3 originPos; //기존위치

    private Vector3 currentPos;//현재 위치

    [SerializeField]
    private Vector3 limitPos; //sway한계

    [SerializeField]
    private Vector3 fineSightLimiPos;//정조준 sway 한계

    [SerializeField]
    private Vector3 smoothSway; //부드러운 움직임 정도

    [SerializeField]
    private GunController theGunController;


	// Use this for initialization
	void Start () {
        originPos = this.transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
        if (GameManager.canPlayerMove && isActivated)
        {
            TrySway();
        }
	}

    private void TrySway()
    {
        if (Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0)
        {
            Swaying();
        }
        else
            BackToOriginPos();
    }

    private void Swaying() //무기 흔들림
    {
        float _moveX = Input.GetAxisRaw("Mouse X");
        float _moveY = Input.GetAxisRaw("Mouse Y");

        if (!theGunController.isFineSightMode) //평소
        {
            currentPos.Set(Mathf.Clamp(Mathf.Lerp(currentPos.x, -_moveX, smoothSway.x), -limitPos.x, limitPos.x),
            Mathf.Clamp(Mathf.Lerp(currentPos.y, -_moveY, smoothSway.y), -limitPos.y, limitPos.y),
            originPos.z); 
        }
        else //정조준
        {
            currentPos.Set(Mathf.Clamp(Mathf.Lerp(currentPos.x, -_moveX, smoothSway.y), -fineSightLimiPos.x, fineSightLimiPos.x),
            Mathf.Clamp(Mathf.Lerp(currentPos.y, -_moveY, smoothSway.y), -fineSightLimiPos.y, fineSightLimiPos.y),
            originPos.z);
        }
        
        transform.localPosition = currentPos; //적용
    }

    private void BackToOriginPos()
    {
        currentPos = Vector3.Lerp(currentPos, originPos, smoothSway.x);
        transform.localPosition = currentPos;
    }
}
