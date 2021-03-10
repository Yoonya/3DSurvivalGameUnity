using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FieldOfViewAngle : MonoBehaviour {

    [SerializeField] private float viewAngle; //시야각
    [SerializeField] private float viewDisance;
    [SerializeField] private LayerMask targetmask;

    private PlayerController thePlayer;
    private NavMeshAgent nav;

    void Start()
    {
        thePlayer = FindObjectOfType<PlayerController>();
        nav = GetComponent<NavMeshAgent>();
    }

    public Vector3 GetTargetPos()
    {
        return thePlayer.transform.position;
    }

    public bool View()
    {
        Collider[] _target = Physics.OverlapSphere(transform.position, viewDisance, targetmask);//주변에 있는 컬라이더들을 뽑아내서 저장시키는데 사용

        for (int i = 0; i < _target.Length; i++)
        {
            Transform _targetTf = _target[i].transform;
            if (_targetTf.name == "Player")//하이어라키에 있는 이름
            {
                Vector3 _direction = (_targetTf.position - transform.position).normalized;
                float _angle = Vector3.Angle(_direction, transform.forward);

                if (_angle < viewAngle * 0.5f)
                {
                    RaycastHit _hit;
                    if (Physics.Raycast(transform.position + transform.up, _direction, out _hit, viewDisance))
                    {
                        if (_hit.transform.name == "Player")
                        {
                            Debug.DrawRay(transform.position + transform.up, _direction, Color.blue);
                            return true;
                        }
                    }
                }
            }

            if (thePlayer.GetRun())
            {
                if (ClacPathLength(thePlayer.transform.position) <= viewDisance)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private float ClacPathLength(Vector3 _targetPos)
    {
        NavMeshPath _path = new NavMeshPath();
        nav.CalculatePath(_targetPos, _path); //네비게이션 계산, 코너 좌표

        Vector3[] _wayPoint = new Vector3[_path.corners.Length + 2];

        _wayPoint[0] = transform.position;
        _wayPoint[_path.corners.Length + 1] = _targetPos; //처음과 끝을 자신과 상대방의 좌표로 기억->중간경로는 전부 벽을 돌아가는 코너

        float _pathLength = 0;
        for (int i = 0; i < _path.corners.Length; i++)
        {
            _wayPoint[i + 1] = _path.corners[i];//웨이포인트에 경로를 넣음
            _pathLength += Vector3.Distance(_wayPoint[i], _wayPoint[i + 1]);//경로길이계산
        }

        return _pathLength;
    }
}
