using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeComponent : MonoBehaviour {

    //깎일 나무 조각들, hp가 아니라 나무조각이 다 사라지면 쓰러지도록 한다.
    [SerializeField]
    private GameObject[] go_treePiceces;
    [SerializeField]
    private GameObject go_treeCenter;

    [SerializeField]
    private GameObject go_Log_Prefabs;

    //나무 쓰러질 때의 랜덤으로 가해질 힘의 세기
    [SerializeField]
    private float force;
    //자식트리
    [SerializeField]
    private GameObject go_ChildTree;

    [SerializeField]
    private CapsuleCollider parentCol;//부모트리 파괴되면 캡슐콜라이더 제거
    [SerializeField]
    private CapsuleCollider childCol;
    [SerializeField]
    private Rigidbody childRigid;

    [SerializeField]
    private GameObject go_hit_effect_prefab;

    [SerializeField]
    private float debrisDestroyTime;//파편제거시간
    [SerializeField]
    private float destroyTime;//나무제거시간

    [SerializeField]
    private string chop_sound;
    [SerializeField]
    private string falldown_sound;
    [SerializeField]
    private string logChange_sound;

    public void Chop(Vector3 _pos, float angleY)
    {
        Hit(_pos);

        AngleCalc(angleY);

        if (CheckTreePieces())
            return;

        FallDownTree();
    }

    private void Hit(Vector3 _pos)
    {
        SoundManager.instance.PlaySE(chop_sound);
        GameObject clone = Instantiate(go_hit_effect_prefab, _pos, Quaternion.Euler(Vector3.zero));//identity랑 비슷한 말임
        Destroy(clone, debrisDestroyTime);

    }

    private void AngleCalc(float _angleY)
    {
        Debug.Log(_angleY);
        if (0 <= _angleY && _angleY < 72)
            DestroyPiece(2);
        if (72 <= _angleY && _angleY < 144)
            DestroyPiece(3);
        if (144 <= _angleY && _angleY < 216)
            DestroyPiece(4);
        if (216 <= _angleY && _angleY < 288)
            DestroyPiece(0);
        if (288 <= _angleY && _angleY <= 360)
            DestroyPiece(1);
    }

    private void DestroyPiece(int _num)
    {
        if (go_treePiceces[_num].gameObject != null)//파괴하고 또 파괴하면 오류
        {
            GameObject clone = Instantiate(go_hit_effect_prefab, go_treePiceces[_num].transform.position, Quaternion.Euler(Vector3.zero));//identity랑 비슷한 말임
            Destroy(clone, debrisDestroyTime);
            Destroy(go_treePiceces[_num].gameObject);
        }
    }

    private bool CheckTreePieces()
    {
        for (int i = 0; i < go_treePiceces.Length; i++)
        {
            if (go_treePiceces[i].gameObject != null)
            {
                return true;
            }
        }
        return false;
    }

    private void FallDownTree()
    {
        SoundManager.instance.PlaySE(falldown_sound);
        Destroy(go_treeCenter);

        parentCol.enabled = false;//콜라이더 안겹치게
        childCol.enabled = true;
        childRigid.useGravity = true;
        childRigid.AddForce(5f, 0f, 5f);//나무가 너무 정직하게 떨어지지않게

        childRigid.AddForce(Random.Range(-force, force), 0f, Random.Range(-force, force));

        StartCoroutine(LogCoroutine());
    }

    IEnumerator LogCoroutine()
    {
        yield return new WaitForSeconds(destroyTime);

        SoundManager.instance.PlaySE(logChange_sound);

        Instantiate(go_Log_Prefabs, go_ChildTree.transform.position + (go_ChildTree.transform.up * 2.5f), Quaternion.LookRotation(go_ChildTree.transform.up));//나무가 쓰러지는 방향에 맞춰 생성하기 위해서
        Instantiate(go_Log_Prefabs, go_ChildTree.transform.position + (go_ChildTree.transform.up * 5f), Quaternion.LookRotation(go_ChildTree.transform.up));
        Instantiate(go_Log_Prefabs, go_ChildTree.transform.position + (go_ChildTree.transform.up * 7.5f), Quaternion.LookRotation(go_ChildTree.transform.up));

        Destroy(go_ChildTree.gameObject);
    }

    public Vector3 GetTreeCenterPosition()
    {
        return go_treeCenter.transform.position;
    }
}
