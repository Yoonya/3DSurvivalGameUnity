using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Twig : MonoBehaviour {

    [SerializeField]
    private int hp;//체력

    [SerializeField]
    private float destroyTime;//이펙트 삭제시간

    [SerializeField]
    private GameObject go_little_Twig; //작은 나뭇가지 조각들
    [SerializeField]
    private GameObject go_hit_effect_prefab;//타격 이펙트

    //사운드
    [SerializeField]
    private string hit_Sound;
    [SerializeField]
    private string broken_Sound;

    //회전값 변수
    private Vector3 originRot;
    private Vector3 wantedRot;
    private Vector3 currentRot;

	// Use this for initialization
	void Start () {
        originRot = transform.rotation.eulerAngles; //Quaternion은 rot그 값을 가져오지만 vector3는 그게 아니니, eulerAngles로 vector3화 시켜야 한다.
        currentRot = originRot;
	}

    public void Damage(Transform _playerTf)
    {
        hp--;

        Hit();

        StartCoroutine(HitSwayCoroutine(_playerTf));

        if (hp <= 0)
        {
            Destruction();//파괴
        }
    }

    private void Hit()
    {
        SoundManager.instance.PlaySE(hit_Sound);

        GameObject clone = Instantiate(go_hit_effect_prefab, gameObject.GetComponent<BoxCollider>().bounds.center + (Vector3.up * 0.5f), Quaternion.identity); //identity는 기본값

        Destroy(clone, destroyTime);
    }

    IEnumerator HitSwayCoroutine(Transform _target)
    {
        Vector3 direction = (_target.position - transform.position).normalized;//==플레이어가 보는 방향, 정규화는 xyz값 합이 1이되게하는것->성능이 좋아짐(권장)

        Vector3 rotationDir = Quaternion.LookRotation(direction).eulerAngles;

        CheckDirection(rotationDir);

        while (!CheckThreshold()) //반대쪽으로 꺾임
        {
            currentRot = Vector3.Lerp(currentRot, wantedRot, 0.25f);
            transform.rotation = Quaternion.Euler(currentRot);
            yield return null;
        }

        wantedRot = originRot;

        while (!CheckThreshold()) //제자리로 돌아옴
        {
            currentRot = Vector3.Lerp(currentRot, wantedRot, 0.15f);
            transform.rotation = Quaternion.Euler(currentRot);
            yield return null;
        }
    }

    private bool CheckThreshold()
    {
        if (Mathf.Abs(wantedRot.x - currentRot.x) <= 0.5f && Mathf.Abs(wantedRot.z - currentRot.z) <= 0.5f)
            return true;
        return false;
    }

    private void CheckDirection(Vector3 _rotationDir) //쓰러뜨릴 때 눕힐 방향 설정
    {
        Debug.Log(_rotationDir);

        if (_rotationDir.y > 180)
        {
            if (_rotationDir.y > 300)
                wantedRot = new Vector3(-50f, 0f, -50f);
            else if (_rotationDir.y > 240)
                wantedRot = new Vector3(0f, 0f, -50f);
            else
                wantedRot = new Vector3(50f, 0f, -50f);
        }
        else if (_rotationDir.y <=  180)
        {
            if (_rotationDir.y < 60)
                wantedRot = new Vector3(-50f, 0f, 50f);
            else if (_rotationDir.y > 120)
                wantedRot = new Vector3(0f, 0f, 50f);
            else
                wantedRot = new Vector3(50f, 0f, 50f);
        }
    }

    private void Destruction()
    {
        SoundManager.instance.PlaySE(broken_Sound);

        GameObject clone1 = Instantiate(go_little_Twig, gameObject.GetComponent<BoxCollider>().bounds.center + (Vector3.up * 0.5f), Quaternion.identity); //identity는 기본값
        GameObject clone2 = Instantiate(go_little_Twig, gameObject.GetComponent<BoxCollider>().bounds.center - (Vector3.up * 0.5f), Quaternion.identity); //identity는 기본값

        Destroy(clone1, destroyTime);
        Destroy(clone2, destroyTime);
        Destroy(gameObject);
    }
}
