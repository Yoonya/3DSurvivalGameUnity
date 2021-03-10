using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class QuickSlotController : MonoBehaviour {

    private Slot[] quickSlots;
    [SerializeField]
    private Image[] img_CoolTime; //쿨타임 이미지
    [SerializeField]
    private Transform tf_parent;//퀵슬롯의 부모객체

    [SerializeField]
    private WeaponManager theWeaponManger;
    private Animator anim;

    [SerializeField]
    private Transform tf_ItemPos; //아이템이 위치할 손끝
    public static GameObject go_HandItem;//손에 든 아이템

    //쿨타임
    [SerializeField]
    private float coolTime;
    private float currentCoolTime;
    private bool isCoolTime;

    //쿨타임UI등장
    [SerializeField] private float appearTime;
    private float currentAppearTime;
    private bool isAppear;

    private int selectedSlot;//선택된 퀵슬롯.(0~7)

    [SerializeField]
    private GameObject go_SelectedImage;//선택된 퀵슬롯의 이미지

    // Use this for initialization
    void Start() {
        quickSlots = tf_parent.GetComponentsInChildren<Slot>();
        anim = GetComponent<Animator>();
        selectedSlot = 0;//0부터 시작
    }

    // Update is called once per frame
    void Update() {
        TryInputNumber();
        CoolTimeCalc();
        AppearCalc();
    }

    private void AppearReset()
    {
        currentAppearTime = appearTime;
        isAppear = true;
        anim.SetBool("Appear", isAppear);
    }

    private void AppearCalc()
    {
        if (Inventory.inventoryActivated)
            AppearReset();
        else
        {
            if (isAppear)
            {
                currentAppearTime -= Time.deltaTime;
                if (currentAppearTime <= 0)
                {
                    isAppear = false;
                    anim.SetBool("Appear", isAppear);
                }
            }
        }

    }

    private void CoolTimeReset()
    {
        currentCoolTime = coolTime;
        isCoolTime = true;
    }

    private void CoolTimeCalc()
    {
        if (isCoolTime)
        {
            currentCoolTime -= Time.deltaTime;
            for (int i = 0; i < img_CoolTime.Length; i++)
                img_CoolTime[i].fillAmount = currentCoolTime / coolTime;

            if (currentCoolTime <= 0)
                isCoolTime = false;
        }
    }

    private void TryInputNumber()
    {
        if (!isCoolTime)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                ChangeSlot(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                ChangeSlot(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                ChangeSlot(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                ChangeSlot(3);
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                ChangeSlot(4);
            else if (Input.GetKeyDown(KeyCode.Alpha6))
                ChangeSlot(5);
            else if (Input.GetKeyDown(KeyCode.Alpha7))
                ChangeSlot(6);
            else if (Input.GetKeyDown(KeyCode.Alpha8))
                ChangeSlot(7);
        }
    }

    public void isActivatedQuickSlot(int _num)
    {
        if (selectedSlot == _num)
            Execute();
        if(DragSlot.instance != null)//드래그슬롯은 인벤토리를 열때만 활성화되기 때문에
            if(DragSlot.instance.dragSlot.GetQuickSlotNumber() == selectedSlot)
                Execute();
    }
    private void ChangeSlot(int _num)
    {
        SelectedSlot(_num);
        Execute();
    }

    private void SelectedSlot(int _num)
    {
        selectedSlot = _num;
        go_SelectedImage.transform.position = quickSlots[selectedSlot].transform.position;//선택된 슬롯으로 이미지 이동
    }

    private void Execute()
    {
        CoolTimeReset();
        AppearReset();

        if (quickSlots[selectedSlot].item != null)
        {
            if (quickSlots[selectedSlot].item.itemType == Item.ItemType.Equipment)
                StartCoroutine(theWeaponManger.ChangeWeaponCoroutine(quickSlots[selectedSlot].item.weaponType, quickSlots[selectedSlot].item.itemName));
            else if (quickSlots[selectedSlot].item.itemType == Item.ItemType.Used || quickSlots[selectedSlot].item.itemType ==Item.ItemType.Kit)
                ChangeHand(quickSlots[selectedSlot].item);
            else
                StartCoroutine(theWeaponManger.ChangeWeaponCoroutine("HAND", "맨손"));
        }
        else
        {
            StartCoroutine(theWeaponManger.ChangeWeaponCoroutine("HAND", "맨손"));
        }
    }

    private void ChangeHand(Item _item = null)
    {
        StartCoroutine(theWeaponManger.ChangeWeaponCoroutine("HAND", "맨손"));

        if (_item != null)
        {
            StartCoroutine(HandItemCoroutine(_item));
        }
            
    }

    IEnumerator HandItemCoroutine(Item _item)
    {
        HandController.isActivate = false;

        yield return new WaitUntil(() => HandController.isActivate);//무기교체의 마지막과정

        if (_item.itemType == Item.ItemType.Kit)
            HandController.currentKit = _item;

        go_HandItem = Instantiate(quickSlots[selectedSlot].item.itemPrefab, tf_ItemPos.position, tf_ItemPos.rotation);
        go_HandItem.GetComponent<Rigidbody>().isKinematic = true;//중력에 영향받지 않음
        go_HandItem.GetComponent<BoxCollider>().enabled = false;//박스 아예 제거
        go_HandItem.tag = "Untagged";//아이템 습득을 하지못하게 태그를 제거
        go_HandItem.layer = 9;//Weapon
        go_HandItem.transform.SetParent(tf_ItemPos);
    }

    public void DecreaseSelectedItem()
    {
        CoolTimeReset();
        AppearReset();
        quickSlots[selectedSlot].SetSlotCount(-1);

        if (quickSlots[selectedSlot].itemCount <= 0)
            StartCoroutine(EatingCoroutine());
    }

    IEnumerator EatingCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(go_HandItem);
    }

    public bool GetIsCoolTime()
    {
        return isCoolTime;
    }

    public Slot GetSelectedSlot()
    {
        return quickSlots[selectedSlot];
    }
}
