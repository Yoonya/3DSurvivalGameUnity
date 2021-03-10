using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {

    //활성화 여부
    public static bool isActivate = false;

    //현재 장착된 총
    [SerializeField]
    private Gun currentGun;

    //연사 속도 계산
    private float currentFireRate;

    //상태 변수
    private bool isReload = false;
    [HideInInspector]
    public bool isFineSightMode = false;

    //본래 포지션값
    [SerializeField]
    private Vector3 originPos;

    //효과음
    private AudioSource audioSource;

    //충돌 정보 받아옴
    private RaycastHit hitInfo;
    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private Camera theCam;//쏘고 맞추려고 할 때 필요
    private Crosshair theCrosshair;

    [SerializeField]
    private GameObject hit_effect_prefab; //피격 이벤트

    //시간대기 없이도 코루틴을 계속 사용하는 이유는 모두 동시에 처리해야해서 병렬처리를 해야함으로
    private void Start()
    {
        originPos = Vector3.zero;
        audioSource = GetComponent<AudioSource>();
        theCrosshair = FindObjectOfType<Crosshair>();

    }
    // Update is called once per frame
    void Update () {
        if (isActivate)
        {
            GunFireRateCalc();
            TryFire();
            TryReload();
            TryFineSight();
        }
	}

    //재계산 연사속도
    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime;
    }

    //발사 시도
    private void TryFire()
    {
        if (Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload) 
        {
            Fire();
        }
    }

    //재장전 시도
    private void TryReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount)
        {
            CancelFineSight();
            StartCoroutine(ReloadCoroutine());
        }
    }

    public void CancelReload()
    {
        if (isReload)
        {
            StopAllCoroutines();
            isReload = false;
        }
    }

    private void Fire() //발사전
    {
        if (!isReload)
        {
            if (currentGun.currentBulletCount > 0)
                Shoot();
            else
            {
                CancelFineSight();
                StartCoroutine(ReloadCoroutine());
            }
                
        }       
    }

    private void Shoot()//발사후
    {
        theCrosshair.FireAnimation();
        currentGun.currentBulletCount--;
        currentFireRate = currentGun.fireRate;//연사속도 초기화
        PlaySE(currentGun.fire_Sound);
        currentGun.muzzleFlash.Play();
        Hit();
        StopAllCoroutines();//RetroActionCoroutine의 if else 부분이 겹칠 수도 있다.
        StartCoroutine(RetroActionCoroutine());
    }

    private void Hit()
    {
        if (Physics.Raycast(theCam.transform.position, theCam.transform.forward + //전방에 더함
            new Vector3(Random.Range(-theCrosshair.GetAccuracy() - currentGun.accuracy, theCrosshair.GetAccuracy() + currentGun.accuracy),//상하좌우로 범위만큼 랜덤한 값을
                        Random.Range(-theCrosshair.GetAccuracy() - currentGun.accuracy, theCrosshair.GetAccuracy() + currentGun.accuracy), 
                        0), 
            out hitInfo, currentGun.range, layerMask))//이번엔 position -> local로하면 어느 위치에 있던 항상 같은 좌표 값을 주기 때문, layerMask 추가
        {
            //hitino.point는 실제 좌표를 반환, LootRotation은 바라보는 방향, normal은 충돌한 곳의 표면을 반환함, 표면의 방향으로 나옴
            GameObject clone = Instantiate(hit_effect_prefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            Destroy(clone, 2f);//프리펩이 계속 쌓이게 되어 삭제
        }
    }

    //재장전
    IEnumerator ReloadCoroutine()
    {
        if (currentGun.carryBulletCount > 0)
        {
            isReload = true;
            currentGun.anim.SetTrigger("Reload");

            currentGun.carryBulletCount += currentGun.currentBulletCount;//리로드를 하면 기존에 있던 총알 수와 합쳐야한다.
            currentGun.currentBulletCount = 0;//들고 있는 총알을 총알집에 넣어두고 새로 리로드를 하는 것

            yield return new WaitForSeconds(currentGun.reloadTime);

            if (currentGun.carryBulletCount >= currentGun.reloadBulletCount)
            {
                currentGun.currentBulletCount = currentGun.reloadBulletCount;
                currentGun.carryBulletCount -= currentGun.reloadBulletCount;
            }
            else
            {
                currentGun.currentBulletCount = currentGun.carryBulletCount;
                currentGun.carryBulletCount = 0;
            }

            isReload = false;
        }
        else
        {
            Debug.Log("소유한 총알이 없습니다.");
        }
    }

    //정조준 시도
    private void TryFineSight()
    {
        if (Input.GetButtonDown("Fire2") && !isReload)
        {
            FineSight();
        }
    }

    public void CancelFineSight()//정조준 취소, 리로드하면 정조준을 품
    {
        if (isFineSightMode)
            FineSight();
    }

    //정조준 로직 가동
    private void FineSight()
    {
        isFineSightMode = !isFineSightMode;
        currentGun.anim.SetBool("FineSightMode", isFineSightMode);
        theCrosshair.FineSightAnimation(isFineSightMode);

        if (isFineSightMode)
        {
            StopAllCoroutines();
            StartCoroutine(FineSightActivateCoroutine());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FineSightDeActivateCoroutine());
        }
    }

    //정조준 활성화
    IEnumerator FineSightActivateCoroutine()
    {
        while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)//정조준 위치가 될 때까지
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.2f);
            yield return null;
        }
    }
    //정조준 비활성화
    IEnumerator FineSightDeActivateCoroutine()
    {
        while (currentGun.transform.localPosition != originPos)//원래의 위치로 돌아올 때까지->정조준이 풀릴 때
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
            yield return null;
        }
    }

    //반동
    IEnumerator RetroActionCoroutine()
    {
        //주의할 점으로 holder의 위치를 잡을 때 계속 축이 바뀌어서 x,y,z축이 각각 어디를 바라보는지 유니티에서 확인 후 바꿔야한다.
        Vector3 recoilBack = new Vector3(currentGun.retroActionForce, originPos.y, originPos.z);//그렇기 때문에 원래 z축 반동이지만 x축에 값을 넣는다.
        Vector3 retroActionRecoilBack = new Vector3(currentGun.retroActionFineSightForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z); //정조준용

        if (!isFineSightMode)//정조준 상태가 아닌 경우
        {
            currentGun.transform.localPosition = originPos;

            //반동시작
            while (currentGun.transform.localPosition.x <= currentGun.retroActionForce - 0.02f)//약간 여유
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, recoilBack, 0.4f);
                yield return null;
            }

            //원위치
            while (currentGun.transform.localPosition != originPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.1f);
                yield return null;
            }
        }
        else
        {
            currentGun.transform.localPosition = currentGun.fineSightOriginPos;

            //반동시작
            while (currentGun.transform.localPosition.x <= currentGun.retroActionFineSightForce - 0.02f)//약간 여유
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionRecoilBack, 0.4f);
                yield return null;
            }

            //원위치
            while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.1f);
                yield return null;
            }
        }
    }

    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }

    public Gun GetGun()
    {
        return currentGun;
    }

    public bool GetFineSightMode()
    {
        return isFineSightMode;
    }

    public void GunChange(Gun _gun)
    {
        if (WeaponManager.currentWeapon != null)//뭔가를 들고있을 경우
            WeaponManager.currentWeapon.gameObject.SetActive(false);

        currentGun = _gun;
        WeaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentGun.anim;

        currentGun.transform.localPosition = Vector3.zero;//혹시 모를 초기화
        currentGun.gameObject.SetActive(true);
        isActivate = true;
    }
}
