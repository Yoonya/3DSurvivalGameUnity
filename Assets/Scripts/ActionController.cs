using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionController : MonoBehaviour
{

    [SerializeField]
    private float range; //습득 가능한 최대 거리

    private bool pickupActivated = false; //습득 가능할 시 true
    private bool dissolvaActivated = false; // 고기 해체 가능할시 true
    private bool isDissolving = false;
    private bool fireLookActivated = false; // 불을 근접해서 바라볼시 true
    private bool lookComputer = false; //컴퓨터 바라볼시 true
    private bool lookArchemyTable = false; //연금 테이블 바라볼시 true
    private bool lookActivatedTrap = false; //가동된 함정을 바라볼시 true

    private RaycastHit hitInfo; //충돌체

    [SerializeField]
    private LayerMask layerMask;//아이템 레이어에만 반응하도록 레이어 마스크를 설정

    [SerializeField]
    private Text actionText;
    [SerializeField]
    private Inventory theInventory;
    [SerializeField]
    private WeaponManager theWeaponManager;
    [SerializeField]
    private QuickSlotController theQuickSlot;
    [SerializeField]
    private Transform tf_MeatDissolveTool; //고기 해체 툴
    [SerializeField]
    private ComputerKit theComputer;

    [SerializeField]
    private string sound_meat; //소리 재생

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckAction();
        TryAction();
    }

    private void TryAction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckAction();
            CanPickUp();
            CanMeat();
            CanDropFire();
            CanComputerPowerOn();
            CanArchemyTableOpen();
            CanReInstallTrap();
        }
    }

    private void CanArchemyTableOpen()
    {
        if (lookArchemyTable)
        {
            if (hitInfo.transform != null)
            {
                hitInfo.transform.GetComponent<ArchemyTable>().Window();
                InfoDisappear();          
            }
        }
    }

    private void CanReInstallTrap()
    {
        if (lookActivatedTrap)
        {
            if (hitInfo.transform != null)
            {
                hitInfo.transform.GetComponent<DeadTrap>().ReInstall();
                InfoDisappear();
            }
        }
    }

    private void CanComputerPowerOn()
    {
        if (lookComputer)
        {
            if (hitInfo.transform != null)
            {
                if (!hitInfo.transform.GetComponent<ComputerKit>().isPowerOn)
                {
                    hitInfo.transform.GetComponent<ComputerKit>().PowerOn();
                    InfoDisappear();
                }
            }
        }
    }

    private void CanPickUp()
    {
        if (pickupActivated)
        {
            if (hitInfo.transform != null)
            {
                theInventory.AcquireItem(hitInfo.transform.GetComponent<ItemPickUp>().item);
                Destroy(hitInfo.transform.gameObject);
                InfoDisappear();
            }
        }
    }

    private void CanDropFire()
    {
        if (fireLookActivated)
        {
            if (hitInfo.collider.tag == "Fire" && hitInfo.collider.GetComponent<Fire>().GetIsFire())
            {
                //손에 들고있는 아이템을 불에 넣음 == 선택된 퀵슬롯의 아이템
                Slot _seletedSlot = theQuickSlot.GetSelectedSlot();
                if (_seletedSlot.item != null)
                    DropAnItem(_seletedSlot);
            }
        }
    }

    private void DropAnItem(Slot _selectedSlot)
    {
        switch (_selectedSlot.item.itemType)
        {
            case Item.ItemType.Used:
                if (_selectedSlot.item.itemName.Contains("고기"))
                {
                    Instantiate(_selectedSlot.item.itemPrefab, hitInfo.transform.position + Vector3.up, Quaternion.identity);//불보다 조금 위에 생성되었으면 함
                    theQuickSlot.DecreaseSelectedItem();
                }
                break;
            case Item.ItemType.Ingredient:
                break;
        }
    }

    private void CheckAction()
    {
        //뭔가 충돌한게있다면
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hitInfo, range, layerMask))//현위치, 전방방향, 충돌체 저장, 사정거리, 지정한 레이어마스크
        {
            if (hitInfo.transform.tag == "Item")
                ItemInfoAppear();
            else if (hitInfo.transform.tag == "WeakAnimal" || hitInfo.transform.tag == "StrongAnimal")
                MeatInfoAppear();
            else if (hitInfo.collider.tag == "Fire")
                FireInfoAppear();
            else if (hitInfo.transform.tag == "Computer")
                ComputerInfoAppear();
            else if (hitInfo.transform.tag == "ArchemyTable")
                ArchemyInfoAppear();
            else if (hitInfo.transform.tag == "Trap")
                TrapInfoAppear();
            else
                InfoDisappear();
        }
        else//충돌 벗어났다면
            InfoDisappear();
    }

    private void Reset()
    {
        pickupActivated = false;
        dissolvaActivated = false;
        fireLookActivated = false;
    }

    private void CanMeat()
    {
        if (dissolvaActivated)
        {
            if ((hitInfo.transform.tag == "WeakAnimal" || hitInfo.transform.tag == "StrongAnimal") && hitInfo.transform.GetComponent<Animal>().isDead && !isDissolving)
            {
                isDissolving = true;
                InfoDisappear();
                StartCoroutine(MeatCoroutine());
            }

        }
    }

    IEnumerator MeatCoroutine()
    {
        WeaponManager.isChangeWeapon = true; //무기교체 안되도록
        WeaponSway.isActivated = false;

        WeaponManager.currentWeaponAnim.SetTrigger("Weapon_Out");
        PlayerController.isActivated = false;

        yield return new WaitForSeconds(0.2f); //애니메이션 후에 사라지도록

        WeaponManager.currentWeapon.gameObject.SetActive(false);
        tf_MeatDissolveTool.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.2f);//모션 소리 맞추도록
        SoundManager.instance.PlaySE(sound_meat);
        yield return new WaitForSeconds(1.8f); //고기 해체 애니메이션

        //돼지를 바라보며 해체할 때, hitinfo로 받아오다가 시점을 옮겨버리면 받아올 수가 없어 오류가 뜬다.
        theInventory.AcquireItem(hitInfo.transform.GetComponent<Animal>().GetItem(), hitInfo.transform.GetComponent<Animal>().itemNumber);

        WeaponManager.currentWeapon.gameObject.SetActive(true);
        tf_MeatDissolveTool.gameObject.SetActive(false);

        PlayerController.isActivated = true;
        WeaponSway.isActivated = true;
        WeaponManager.isChangeWeapon = false; //무기교체 가능
        isDissolving = false;
    }

    private void ItemInfoAppear()
    {
        Reset();//불과 아이템 동시에 봐지는것 방지
        pickupActivated = true;
        actionText.gameObject.SetActive(true);
        actionText.text = hitInfo.transform.GetComponent<ItemPickUp>().item.itemName + " 흭득" + "<color=yellow>" + "(E)" + "</color>";
    }

    private void MeatInfoAppear()
    {
        if (hitInfo.transform.GetComponent<Animal>().isDead)
        {
            Reset();//불과 아이템 동시에 봐지는것 방지
            dissolvaActivated = true;
            actionText.gameObject.SetActive(true);
            actionText.text = hitInfo.transform.GetComponent<Animal>().animalName + " 해체하기" + "<color=yellow>" + "(E)" + "</color>";
        }
    }

    private void InfoDisappear()
    {
        pickupActivated = false;
        dissolvaActivated = false;
        fireLookActivated = false;
        lookComputer = false;
        lookArchemyTable = false;
        lookActivatedTrap = false;
        actionText.gameObject.SetActive(false);
    }

    private void FireInfoAppear()
    {
        Reset();
        fireLookActivated = true;
        if (hitInfo.collider.GetComponent<Fire>().GetIsFire())
        {
            actionText.gameObject.SetActive(true);
            actionText.text = "선택된 아이템 불에 넣기" + "<color=yellow>" + "(E)" + "</color>";
        }
    }

    private void ComputerInfoAppear()
    {
        if (!hitInfo.transform.GetComponent<ComputerKit>().isPowerOn)
        {
            Reset();//불과 아이템 동시에 봐지는것 방지
            lookComputer = true;
            actionText.gameObject.SetActive(true);
            actionText.text = "컴퓨터 가동" + "<color=yellow>" + "(E)" + "</color>";
        }
    }

    private void ArchemyInfoAppear()
    {
        if (!hitInfo.transform.GetComponent<ArchemyTable>().GetIsOpen()) //ui띄어지면 안나오게
        {
            Reset();//불과 아이템 동시에 봐지는것 방지
            lookArchemyTable = true;
            actionText.gameObject.SetActive(true);
            actionText.text = "연금 테이블 조작" + "<color=yellow>" + "(E)" + "</color>";
        }
    }

    private void TrapInfoAppear()
    {
        if (hitInfo.transform.GetComponent<DeadTrap>().GetIsActivated()) 
        {
            Reset();//불과 아이템 동시에 봐지는것 방지
            lookActivatedTrap = true;
            actionText.gameObject.SetActive(true);
            actionText.text = "함정 재설치" + "<color=yellow>" + "(E)" + "</color>";
        }
    }
}
