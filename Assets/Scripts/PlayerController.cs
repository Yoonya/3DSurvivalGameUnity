using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    static public bool isActivated = true;

    //스피드 조정 변수
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;
    [SerializeField]
    private float swimSpeed;
    [SerializeField]
    private float swimFastSpeed;
    [SerializeField]
    private float upSwimSpeed;

    private float applySpeed; //대입용

    [SerializeField]
    private float jumpForce;

    //상태 변수
    private bool isWalk = false;
    private bool isRun = false;
    private bool isCrouch = false;
    private bool isGround = true;

    //움직임 체크 변수
    private Vector3 lastPos;//이전프레임 위치


    //앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    private CapsuleCollider capsuleCollider;//땅 착지 여부

    [SerializeField]
    private float lookSensitivity; //카메라 민감도

    [SerializeField]
    private float cameraRotationLimit;//고개를 돌릴 때, 360도 회전하면 이상하니까 제한두기 위해서
    private float currentCamaerRotationX = 0f;//정면, 기본값이 0


    [SerializeField]
    private Camera theCamera;
    private Rigidbody myRigid; //몸체, collider의 물리화
    private GunController theGunController;
    private Crosshair theCrosshair;
    private StatusController theStatusController;


	// Use this for initialization
	void Start () {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        theGunController = FindObjectOfType<GunController>();
        theCrosshair = FindObjectOfType<Crosshair>();
        theStatusController = FindObjectOfType<StatusController>();

        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y;//캐릭터를 내리는게 아니라 카메라를 내린다. position은 월드 기준, localposition은 부모 기준
        applyCrouchPosY = originPosY;
	}
	
	// Update is called once per frame
	void Update () {
        if (isActivated && GameManager.canPlayerMove)
        {
            WaterCheck();
            IsGround();
            TryJump();
            if (!GameManager.isWater)
            {
                TryRun();
            }   
            TryCrouch();
            Move();
            CameraRotation();
            CharacterRotation();
        }
	}

    private void WaterCheck()
    {
        if (GameManager.isWater)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
                applySpeed = swimFastSpeed;
            else
                applySpeed = swimSpeed;
        }
    }

    private void FixedUpdate()
    {
        if (isActivated && GameManager.canPlayerMove)
            MoveCheck();//업데이트에 넣으니 idle과 서로 번갈아 실행되는 문제가 있다.
    }

    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround && theStatusController.GetCurrentSP() > 0 && !GameManager.isWater)
            Jump();
        else if (Input.GetKeyDown(KeyCode.Space) && GameManager.isWater)
            UpSwim();

    }

    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift) && theStatusController.GetCurrentSP() > 0)//키 누르고 있으면 계속 적용
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) || theStatusController.GetCurrentSP() <= 0)
        {
            RunningCancel();
        }
    }

    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");//방향키 또는 wasd 오른쪽 입력시 1, 왼쪽 -1, 입력없으면 0 
        float _moveDirZ = Input.GetAxisRaw("Vertical"); //위아래

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;//normalized는 현재(1,0,1)을 (0.5, 0, 0.5)로 바꿔줌-> 합이 1이 되도록 바꿔줌 그러면 계산이 빨라진데

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }

    private void MoveCheck()
    {
        if (!isRun && !isCrouch && isGround)
        {
            if (Vector3.Distance(lastPos, transform.position) >= 0.01f) //경사면에서 쓸려져 내려오는 것 같은 사례를 없애기 위해 
                isWalk = true;
            else
                isWalk = false;

            theCrosshair.WalkingAnimation(isWalk);
            lastPos = transform.position;
        }     
    }

    private void Crouch()
    {
        isCrouch = !isCrouch;
        theCrosshair.CrouchingAnimation(isCrouch);

        if (isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        StartCoroutine(CrouchCoroutin());
    }

    IEnumerator CrouchCoroutin()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;//반복문 조건을 그냥 두면 무한반복하게됨

        while (_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f); //보관함수-> ex) (1, 2, 0.5f)면 1에서 2까지 1.5 1.75 1.87... 이런식으로 증가함
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);  //x,y,z 따로따로 변경은 불가능-> 벡터 자체를 넣어줘야 함
            if(count > 15)//무한반복 방지
                break;
            yield return null;//null은 1프레임 대기
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0);
    }

    private void IsGround()
    {
        //레이저를 쏘아서 감지
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f); //현재위치, 아래방향, 캡슐콜라이더.바운드.절반.y크기+약간의 여유오차
        theCrosshair.JumpingAnimation(!isGround);//점프할 때도 조준점 없애기 위해 
    }

    private void UpSwim()
    {
        myRigid.velocity = transform.up * upSwimSpeed;
    }

    private void Jump()
    {
        //앉은 상태에서 점프시 앉은 상태 해제
        if (isCrouch)
            Crouch();

        theStatusController.DecreaseStamina(100);
        myRigid.velocity = transform.up * jumpForce; // transform.up = (0,1,0)
    }

    private void Running()
    {
        //앉은 상태에서 달릴시 앉은 상태 해제
        if (isCrouch)
            Crouch();

        theGunController.CancelFineSight(); //뛸 때 정조준 해제

        isRun = true;
        theCrosshair.RunningAnimation(isRun);
        theStatusController.DecreaseStamina(10);
        applySpeed = runSpeed;
    }

    private void RunningCancel()
    {
        isRun = false;
        theCrosshair.RunningAnimation(isRun);
        applySpeed = walkSpeed;
    }

    private void CharacterRotation()
    {
        //좌우 캐릭터 회전
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));//아래와 방식이 다른 이유는 그냥 다양하게 써보는 것, 좌우는 상하와 다르게 각도 범위를 지정할 필요가 없기도하고
    }

    public bool GetRun()
    {
        return isRun;
    }

    private void CameraRotation()
    {
        if (!PauseCameraRotaion) //나무를 팰 때 시야조정을 하기 위해서
        {
            //상하 카메라 회전
            //카메라의 x값은 위아래
            float _xRotation = Input.GetAxis("Mouse Y");
            float _cameraRotationX = _xRotation * lookSensitivity;

            currentCamaerRotationX -= _cameraRotationX; // +=으로 하니 마우스가 반전됨
            currentCamaerRotationX = Mathf.Clamp(currentCamaerRotationX, -cameraRotationLimit, cameraRotationLimit);//-cameraRotationLimit와 cameraRotationLimit 사이로 값을 고정, 
                                                                                                                    //만약 currentCamaerRotationX가 값 범위를 벗어나면 최대 최소로 고정
            theCamera.transform.localEulerAngles = new Vector3(currentCamaerRotationX, 0f, 0f); //마우스 위아래만 이기 때문에 이것만 설정
        }
    }

    private bool PauseCameraRotaion = false; //나무를 팰 때 시야조정을 하기 위해서

    public IEnumerator TreeLookCoroutine(Vector3 _target)//나무 팰 때 시야조정
    {
        PauseCameraRotaion = true;

        Quaternion direction = Quaternion.LookRotation(_target - theCamera.transform.position);
        Vector3 eulerValue = direction.eulerAngles;
        float destinationX = eulerValue.x;

        while (Mathf.Abs(destinationX - currentCamaerRotationX) >= 0.5f)
        {
            eulerValue = Quaternion.Lerp(theCamera.transform.localRotation, direction, 0.3f).eulerAngles;
            theCamera.transform.localRotation = Quaternion.Euler(eulerValue.x, 0f, 0f);
            currentCamaerRotationX = theCamera.transform.localEulerAngles.x;
            yield return null;
        }

        PauseCameraRotaion = false;
    }
}
