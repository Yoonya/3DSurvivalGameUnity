using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Animal : MonoBehaviour {

    protected StatusController thePlayerStatus;

    [SerializeField] public string animalName;
    [SerializeField] protected int hp;

    [SerializeField]
    protected Item item_Prefab;
    [SerializeField]
    public int itemNumber; //아이템의 흭득 개수

    [SerializeField] protected float walkSpeed;
    [SerializeField] protected float runSpeed;

    protected Vector3 destination; //방향

    protected bool isAction;
    protected bool isWalking;
    protected bool isRunning;
    protected bool isChasing; //이것으로 리셋되지 않게
    public bool isDead;
    protected bool isAttacking;

    [SerializeField] protected float walkTime;//걷기 시간
    [SerializeField] protected float waitTime;//대기 시간
    [SerializeField] protected float runTime; //뛰기 시간
    protected float currentTime;

    [SerializeField] protected Animator anim;
    [SerializeField] protected Rigidbody rigid;
    [SerializeField] protected BoxCollider boxCol;
    protected AudioSource theAudio;
    protected NavMeshAgent nav;
    protected FieldOfViewAngle theViewAngle;

    //soundManager를 안쓰고 따로 쓰는 이유는 돼지가 거리에 따라 소리가 바뀌어야하기 때문이다.
    [SerializeField] protected AudioClip[] sound_Normal;
    [SerializeField] protected AudioClip sound_Hurt;
    [SerializeField] protected AudioClip sound_Dead;

    // Use this for initialization
    void Start()
    {
        thePlayerStatus = FindObjectOfType<StatusController>();
        theViewAngle = GetComponent<FieldOfViewAngle>();
        nav = GetComponent<NavMeshAgent>();
        theAudio = GetComponent<AudioSource>();
        currentTime = waitTime;
        isAction = true;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!isDead)
        {
            Move();
            ElapseTime();
        }
    }

    protected void Move()
    {
        if (isWalking || isRunning)
            //rigid.MovePosition(transform.position + (transform.forward * applySpeed * Time.deltaTime));
            nav.SetDestination(transform.position + destination * 5f);
    }



    protected void ElapseTime()
    {
        if (isAction)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0 && !isChasing && !isAttacking)
            {
                ReSet();
            }
        }
    }


    protected virtual void ReSet()//walking의 무한재생을 막기 위해
    {
        isWalking = false;
        isRunning = false;
        isChasing = false;
        isAttacking = false;
        isAction = true;
        nav.speed = walkSpeed;
        nav.ResetPath();
        anim.SetBool("Walking", isWalking);
        anim.SetBool("Running", isRunning);
        destination.Set(Random.Range(-0.2f, 0.2f), 0f, Random.Range(0.5f, 1f));
    }

    protected void TryWalk()
    {
        isWalking = true;
        anim.SetBool("Walking", isWalking);
        currentTime = walkTime;
        nav.speed = walkSpeed;
    }

    public virtual void Damage(int _dmg, Vector3 _targetPos)
    {
        if (!isDead)
        {
            hp -= _dmg;
            if (hp <= 0)
            {
                Dead();
                return;
            }
            PlaySE(sound_Hurt);
            anim.SetTrigger("Hurt");
        }
    }
    protected void Dead()
    {
        PlaySE(sound_Dead);
        isWalking = false;
        isRunning = true;
        isChasing = false;
        isAttacking = false;
        isDead = true;
        nav.ResetPath();
        anim.SetTrigger("Dead");
    }

    protected void RandomSound()
    {
        int _random = Random.Range(0, 3);//일상 사운드 3개
        PlaySE(sound_Normal[_random]);
    }

    protected void PlaySE(AudioClip _clip)
    {
        theAudio.clip = _clip;
        theAudio.Play();
    }

    public Item GetItem()
    {
        this.gameObject.tag = "Untagged"; //한번 해체하면 더 못해체하게 바뀌도록
        Destroy(this.gameObject, 3f);
        return item_Prefab;
    }
}
