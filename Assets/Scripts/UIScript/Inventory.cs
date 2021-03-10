using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {

    public static bool inventoryActivated = false;

    [SerializeField]
    private GameObject go_InventoryBase;
    [SerializeField]
    private GameObject go_SlotsParent;
    [SerializeField]
    private GameObject go_QuickSlotParent;
    [SerializeField]
    private QuickSlotController theQuickSlot;
    [SerializeField]
    private ItemEffectDatabase theItemEffectDatabase;

    private Slot[] slots; //인벤토리 슬롯들
    private Slot[] quickslots;//퀵슬롯들
    private bool isNotPut;
    private int slotNumber;

    public Slot[] GetSlots()
    {
        return slots;
    }

    [SerializeField] private Item[] items;

    public void LoadToInven(int _arrayNum, string _itemName, int _itemNum)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].itemName == _itemName)
                slots[_arrayNum].AddItem(items[i], _itemNum);
        }
    }

	// Use this for initialization
	void Start () {
        slots = go_SlotsParent.GetComponentsInChildren<Slot>();
        quickslots = go_QuickSlotParent.GetComponentsInChildren<Slot>();
    }
	
	// Update is called once per frame
	void Update () {
        TryOpenInventory();
	}

    private void TryOpenInventory()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryActivated = !inventoryActivated;

            if (inventoryActivated)
                OpenInventory();
            else
                CloseInventory();
        }
    }

    private void OpenInventory()
    {
        GameManager.isOpenInventory = true;
        go_InventoryBase.SetActive(true);
    }

    private void CloseInventory()
    {
        GameManager.isOpenInventory = false;
        go_InventoryBase.SetActive(false);
        theItemEffectDatabase.HideToolTip();
    }

    public void AcquireItem(Item _item, int _count = 1)
    {
        PutSlot(quickslots, _item, _count);//slot으로 넣으면 인벤토리부터 채우고, quickslot을 넣으면 퀵슬롯부터 아이템이 채워짐
        if (!isNotPut)
            theQuickSlot.isActivatedQuickSlot(slotNumber);

        if (isNotPut) //넣지못했다->퀵슬롯이 다 찼다-> 인벤토리에 넣는다.
            PutSlot(slots, _item, _count);
            

        if (isNotPut)//인벤토리에도 꽉 찼다.
            Debug.Log("퀵슬롯과 인벤토리가 꽉찼습니다.");

    }

    private void PutSlot(Slot[] _slots, Item _item, int _count)
    {
        if (Item.ItemType.Equipment != _item.itemType && Item.ItemType.Kit != _item.itemType)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].item != null) //기본으로 null이 되어있다. null이 아닐 시에 아이템 추가
                {
                    if (_slots[i].item.itemName == _item.itemName)
                    {
                        slotNumber = i;
                        _slots[i].SetSlotCount(_count);
                        isNotPut = false;
                        return;
                    }
                }
            }
        }

        for (int i = 0; i < _slots.Length; i++)//장비템이라면 or 중복되지 않은템이라면
        {
            if (_slots[i].item == null) //기본으로 null이 되어있다. null이면 아이템흭득
            {
                _slots[i].AddItem(_item, _count);
                isNotPut = false;
                return;
            }
        }
        Debug.Log("3");
        isNotPut = true; //반환하지 못할 경우, 아이템을 채워넣지 못한 경우
    }

    public int GetItemCount(string _itemName)
    {
        int temp = SearchSlotItem(slots, _itemName);

        return temp != 0 ? temp : SearchSlotItem(quickslots, _itemName);//조건문 성립 전자
    }

    private int SearchSlotItem(Slot[] _slots, string _itemName)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].item != null)
            {
                if (_itemName == _slots[i].item.itemName)
                    return _slots[i].itemCount;
            }
        }

        return 0;
    }

    public void SetItemCount(string _itemName, int _itemCount)
    {
        if (!ItemCountAdjust(slots, _itemName, _itemCount)) //인벤토리 차감
            ItemCountAdjust(quickslots, _itemName, _itemCount); //없으면 퀵슬롯차감

    }

    private bool ItemCountAdjust(Slot[] _slots, string _itemName, int _itemCount)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].item != null)
            {
                if (_itemName == _slots[i].item.itemName)
                {
                    _slots[i].SetSlotCount(-_itemCount);
                    return true;
                }
            }

        }

        return false;
    }
}
