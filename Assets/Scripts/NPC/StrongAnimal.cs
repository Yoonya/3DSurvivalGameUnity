using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrongAnimal : Animal {

    [SerializeField]
    protected int attackDamage;
    [SerializeField]
    protected float attackDelay;
    [SerializeField]
    protected LayerMask targetMask;

    [SerializeField]
    protected float ChaseTime; //총 추격 시간
    protected float currentChaseTime;//계산
    [SerializeField]
    protected float ChaseDelayTime;//추격 딜레이

    public void Chase(Vector3 _targetPos)
    {
        isChasing = true;
        destination = _targetPos;
        nav.speed = runSpeed;
        isRunning = true;
        anim.SetBool("Running", isRunning);
        nav.SetDestination(destination);
    }

    public override void Damage(int _dmg, Vector3 _targetPos)
    {
        base.Damage(_dmg, _targetPos);
        if (!isDead)
            Chase(_targetPos);
    }

    protected IEnumerator ChaseTargetCoroutine() //일정시간 따라오다가 오래 못찾으면 그만두도록
    {
        currentChaseTime = 0;

        while (currentChaseTime < ChaseTime)
        {           
            if (!isDead)
            {
                Chase(theViewAngle.GetTargetPos());
                //충분히 가까이 있고
                if (Vector3.Distance(transform.position, theViewAngle.GetTargetPos()) <= 3f)
                {
                    if (theViewAngle.View())//눈 앞에 있을 경우(뷰자체는 플레이어가 주변에 있을 때, 하지만 위조건)
                    {
                        StartCoroutine(AttackCoroutine());
                    }
                }
            }
            yield return new WaitForSeconds(ChaseDelayTime);
            currentChaseTime += ChaseDelayTime;
        }

        isChasing = false;
        isRunning = false;
        anim.SetBool("Running", isRunning);
        nav.ResetPath();
    }

    protected IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        nav.ResetPath();
        currentChaseTime = ChaseTime;
        yield return new WaitForSeconds(0.5f);//잠깐 멈췄다가
        transform.LookAt(theViewAngle.GetTargetPos());//플레이어를 바라보게
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);//잠깐 멈췄다가(공격자세)
        RaycastHit _hit;
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out _hit, 3, targetMask))//vector3up으로 눈높이 위치로
        {
            thePlayerStatus.DecreaseHP(attackDamage);
        }
        else
        {
        }
        yield return new WaitForSeconds(attackDelay);
        isAttacking = false;      
        StartCoroutine(ChaseTargetCoroutine());
    }
}
