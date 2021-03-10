using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Craft
{
    public string craftName;//이름
    public Sprite craftImage; //이미지
    public string craftDesc; //설명
    public string[] craftNeedItem;//필요한 아이템
    public int[] craftNeedItemCount;//필요한 아이템의 개수
    public GameObject go_Prefab; //실제 설치될 프리펩
    public GameObject go_PreviewPrefab;//미리보기 프리펩
}

public class CraftManual : MonoBehaviour {

    private bool isActivated = false;
    private bool isPreviewActivated = false;

    [SerializeField]
    private GameObject go_BaseUI; //기본 베이스 UI

    private int tabNumber = 0;
    private int page = 1;
    private int selectedSlotNumber;
    private Craft[] craft_SelectedTab;

    [SerializeField]
    private Craft[] craft_fire; //모닥불용 탭
    [SerializeField]
    private Craft[] craft_build; //건축용 탭

    private GameObject go_Preview;//미리보기 프리펩
    private GameObject go_Prefab; //실제 생성될 프리펩을 담을 변수

    [SerializeField]
    private Transform tf_player;

    //레이저를 쏘아서 쏜 지점에 생성->raycast
    private RaycastHit hitinfo;
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private float range;

    //필요한 UI Slot 요소
    [SerializeField]
    private GameObject[] go_Slots;
    [SerializeField]
    private Image[] image_Slot;
    [SerializeField]
    private Text[] text_SlotName;
    [SerializeField]
    private Text[] text_SlotDesc;
    [SerializeField]
    private Text[] text_SlotNeedItem;

    //필요 컴포넌트
    private Inventory theInventory;

    public void SlotClick(int _slotNumber)
    {
        selectedSlotNumber = _slotNumber + (page - 1) * go_Slots.Length;

        if (!CheckIngredient())//재료가 없다면
            return;

        go_Preview = Instantiate(craft_SelectedTab[selectedSlotNumber].go_PreviewPrefab, tf_player.position + tf_player.forward, Quaternion.identity);
        go_Prefab = craft_SelectedTab[selectedSlotNumber].go_Prefab;
        isPreviewActivated = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        go_BaseUI.SetActive(false);
    }
	// Use this for initialization
	void Start () {
        theInventory = FindObjectOfType<Inventory>();

        tabNumber = 0;
        page = 1;
        TabSlotSetting(craft_fire); //첫시작 불탭

    }

    public void TabSetting(int _tabNumber)
    {
        tabNumber = _tabNumber;
        page = 1;

        switch (tabNumber)
        {
            case 0:
                TabSlotSetting(craft_fire); //불세팅
                break;
            case 1:
                TabSlotSetting(craft_build); //건축세팅
                break;
        }
    }

    private void ClearSlot()
    {
        for (int i = 0; i < go_Slots.Length; i++)
        {
            image_Slot[i].sprite = null;
            text_SlotDesc[i].text = "";
            text_SlotName[i].text = "";
            text_SlotNeedItem[i].text = "";
            go_Slots[i].SetActive(false);
        }

    }

    public void RightPageSetting()
    {
        if (page < (craft_SelectedTab.Length / go_Slots.Length) + 1) //최대페이즈가 아닐때

            page++;
        else //최대 페이지시
            page = 1;

        TabSlotSetting(craft_SelectedTab);
    }

    public void LeftPageSetting()
    {
        if (page != 1)
            page--;
        else //최대 페이지시
            page = (craft_SelectedTab.Length / go_Slots.Length) + 1;

        TabSlotSetting(craft_SelectedTab);
    }

    private void TabSlotSetting(Craft[] _craft_tab)
    {
        ClearSlot();//불탭 건축탭 번갈아할때 초기화
        craft_SelectedTab = _craft_tab;

        int startSlotNumber = (page - 1) * go_Slots.Length;//0,4... 4의 배수

        for (int i = startSlotNumber; i < craft_SelectedTab.Length; i++)
        {
            if (i == page * go_Slots.Length)
                break;

            go_Slots[i - startSlotNumber].SetActive(true);

            image_Slot[i - startSlotNumber].sprite = _craft_tab[i].craftImage;
            text_SlotName[i - startSlotNumber].text = craft_SelectedTab[i].craftName;
            text_SlotDesc[i - startSlotNumber].text = craft_SelectedTab[i].craftName;

            for (int x = 0; x < craft_SelectedTab[i].craftNeedItem.Length; x++)
            {
                text_SlotNeedItem[i - startSlotNumber].text += craft_SelectedTab[i].craftNeedItem[x];
                text_SlotNeedItem[i - startSlotNumber].text += " x " + craft_SelectedTab[i].craftNeedItemCount[x] + "\n";
            }
 
        }
    }

    private bool CheckIngredient()
    {
        for (int i = 0; i < craft_SelectedTab[selectedSlotNumber].craftNeedItem.Length; i++)
        {
            //인벤토리 아이템이 제작필요 개수보다 부족할경우
            if (theInventory.GetItemCount(craft_SelectedTab[selectedSlotNumber].craftNeedItem[i]) < craft_SelectedTab[selectedSlotNumber].craftNeedItemCount[i])
                return false;
        }

        return true;
    }

    private void UseIngredient()
    {
        for (int i = 0; i < craft_SelectedTab[selectedSlotNumber].craftNeedItem.Length; i++)
        {
            theInventory.SetItemCount(craft_SelectedTab[selectedSlotNumber].craftNeedItem[i], craft_SelectedTab[selectedSlotNumber].craftNeedItemCount[i]);
        }
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Tab) && !isPreviewActivated)
            Window();

        if(isPreviewActivated)
            PreviewPositionUpdate();

        if (Input.GetButtonDown("Fire1"))
            Build();

        if (Input.GetKeyDown(KeyCode.Escape))
            Cancel();
	}

    private void Build()
    {
        if (isPreviewActivated && go_Preview.GetComponent<PreviewObject>().IsBuildable())
        {
            UseIngredient();
            Instantiate(go_Prefab, go_Preview.transform.position, go_Preview.transform.rotation);
            Destroy(go_Preview);
            isActivated = false;
            isPreviewActivated = false;
            go_Preview = null;
            go_Prefab = null;
        }
    }

    private void PreviewPositionUpdate()
    {
        if (Physics.Raycast(tf_player.position, tf_player.forward, out hitinfo, range, layerMask))
        {
            if (hitinfo.transform != null)
            {
                Vector3 _location = hitinfo.point;//레이저 맞은 부분

                if (Input.GetKeyDown(KeyCode.Q))
                    go_Preview.transform.Rotate(0f, -45f, 0f);
                else if (Input.GetKeyDown(KeyCode.E))
                    go_Preview.transform.Rotate(0f, 45f, 0f);

                _location.Set(Mathf.Round(_location.x), Mathf.Round(_location.y / 0.01f) * 0.01f, Mathf.Round(_location.z));//1단위로 움직이도록 반올림 y는 0.01단위
                
                go_Preview.transform.position = _location;
            }
        }
    }

    private void Cancel()
    {
        if (isPreviewActivated)
            Destroy(go_Preview);

        isActivated = false;
        isPreviewActivated = false;
        go_Preview = null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        go_BaseUI.SetActive(false);
    }

    private void Window()
    {
        if (!isActivated)
            OpenWindow();
        else
            CloseWindow();
    }

    private void OpenWindow()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isActivated = true;
        go_BaseUI.SetActive(true);
    }

    private void CloseWindow()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isActivated = false;
        go_BaseUI.SetActive(false);
    }
}
