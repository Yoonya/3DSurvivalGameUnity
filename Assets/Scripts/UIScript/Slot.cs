using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler {

    public Item item;
    public int itemCount;
    public Image itemImage;

    [SerializeField]
    private bool isQuickSlot;//퀵슬롯 여부판단
    [SerializeField]
    private int quickSlotNumber;//퀵슬롯 번호

    [SerializeField]
    private Text text_Count;
    [SerializeField]
    private GameObject go_CountImage;

    private WeaponManager theWeaponManager;
    [SerializeField]
    private RectTransform baseRect;
    [SerializeField]
    private RectTransform quickSlotBaseRect; //퀵슬롯의 영역
    private InputNumber theInputNumber;

    private ItemEffectDatabase theItemEffectDatabase;

    void Start()
    {
        theInputNumber = FindObjectOfType<InputNumber>();
        theWeaponManager = FindObjectOfType<WeaponManager>();
        theItemEffectDatabase = FindObjectOfType<ItemEffectDatabase>();
    }

    //이미지의 투명도 조절
    private void SetColor(float _alpha)
    {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }

    //아이템 흭득
    public void AddItem(Item _item, int _count = 1)
    {
        item = _item;
        itemCount = _count;
        itemImage.sprite = item.itemImage;

        if (item.itemType != Item.ItemType.Equipment)
        {
            go_CountImage.SetActive(true);
            text_Count.text = itemCount.ToString();
        }
        else
        {
            text_Count.text = "0";
            go_CountImage.SetActive(false);
        }
       
        SetColor(1);
    }

    //아이템 개수 조정
    public void SetSlotCount(int _count)
    {
        itemCount += _count;
        text_Count.text = itemCount.ToString();

        if (itemCount <= 0)
            ClearSlot();
    }

    public int GetQuickSlotNumber()
    {
        return quickSlotNumber;
    }

    //슬롯 초기화
    private void ClearSlot()
    {
        item = null;
        itemCount = 0;
        itemImage.sprite = null;
        SetColor(0);

        text_Count.text = "0";
        go_CountImage.SetActive(false);
    }

    //마우스 우클릭 구간
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) //마우스 우클릭 가능
        {
            if (item != null)
            {
                theItemEffectDatabase.UseItem(item);
                if(item.itemType == Item.ItemType.Used)
                    SetSlotCount(-1);
            }
        }
    }

    //이후 드래그 구간
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item != null && Inventory.inventoryActivated)
        {
            DragSlot.instance.dragSlot = this;//드래그슬롯에 자신 슬롯 넣기
            DragSlot.instance.DragSetImage(itemImage);

            DragSlot.instance.transform.position = eventData.position;
        }

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item != null)
        {
            DragSlot.instance.transform.position = eventData.position;
        }

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //인벤토리도 아니고 퀵슬롯도 아니고 그럼 버림
        if (!((DragSlot.instance.transform.localPosition.x > baseRect.rect.xMin && DragSlot.instance.transform.localPosition.x < baseRect.rect.xMax
            && DragSlot.instance.transform.localPosition.y > baseRect.rect.yMin && DragSlot.instance.transform.localPosition.y < baseRect.rect.yMax)
            ||//인벤토리의 영역, 퀵슬롯의 영역
            (DragSlot.instance.transform.localPosition.x > quickSlotBaseRect.rect.xMin && DragSlot.instance.transform.localPosition.x < quickSlotBaseRect.rect.xMax
            && DragSlot.instance.transform.localPosition.y > quickSlotBaseRect.rect.yMin - 360 && DragSlot.instance.transform.localPosition.y < quickSlotBaseRect.rect.yMax - 360)))
        {
            if (DragSlot.instance.dragSlot != null)
            {
                theInputNumber.Call();
            }
           
        }
        else
        {
            DragSlot.instance.SetColor(0);
            DragSlot.instance.dragSlot = null;
        }

    }

    //슬롯에서 슬롯끼리 EndDrag되었을 때 발동
    public void OnDrop(PointerEventData eventData)
    {
        if (DragSlot.instance.dragSlot != null) //item null참조 방지
        {
            ChangeSlot();

            if (isQuickSlot)//인벤토리에서 퀵슬롯으로(혹은 퀵슬롯에서 퀵슬롯으로)
                theItemEffectDatabase.IsActivatedQuickSlot(quickSlotNumber);
            else //인벤토리->인벤토리, 퀵슬롯->인벤토리
                if (DragSlot.instance.dragSlot.isQuickSlot)//퀵슬롯->인벤토리
               theItemEffectDatabase.IsActivatedQuickSlot(DragSlot.instance.dragSlot.quickSlotNumber) ;
        }
           

    }

    private void ChangeSlot()
    {
        Item _tempItem = item;
        int _tempItemCount = itemCount;

        AddItem(DragSlot.instance.dragSlot.item, DragSlot.instance.dragSlot.itemCount);

        if (_tempItem != null)//item null참조 방지
            DragSlot.instance.dragSlot.AddItem(_tempItem, _tempItemCount);
        else
            DragSlot.instance.dragSlot.ClearSlot();
    }

    public void OnPointerEnter(PointerEventData eventData) //마우스가 슬롯에 들어갈 때
    {
        if(item != null)
            theItemEffectDatabase.ShowToolTip(item, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)//마우스가 슬롯에서 빠져나갈 때
    {
        theItemEffectDatabase.HideToolTip();
    }
}
