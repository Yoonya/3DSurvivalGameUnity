using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Kit
{
    public string KitName;
    public string kitDescription;
    public string[] needItemName;
    public int[] needItemNumber;

    public GameObject go_Kit_Prefab;
}

public class ComputerKit : MonoBehaviour {

    [SerializeField]
    private Kit[] kits;

    [SerializeField]
    private Transform tf_ItemAppear; //생성될 아이템 위치
    [SerializeField]
    private GameObject go_BaseUI;

    private bool isCraft = false;
    public bool isPowerOn = false; 

    private Inventory theInven;

    [SerializeField]
    private ComputerToolTip theToolTip;

    private AudioSource theAudio;
    [SerializeField] private AudioClip sound_buttonClick;
    [SerializeField] private AudioClip sound_Beep;
    [SerializeField] private AudioClip sound_Activated;
    [SerializeField] private AudioClip sound_Output;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //커서가 사라지면서 가운데 고정
        Cursor.visible = false;//커서 안보임, 하지만 실제로 커서는 있음

        theInven = FindObjectOfType<Inventory>();
        theAudio = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isPowerOn)
            if (Input.GetKeyDown(KeyCode.Escape))
                PowerOff();
    }

    public void PowerOn()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPowerOn = true;
        go_BaseUI.SetActive(true);
    }

    public void PowerOff()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPowerOn = false;
        theToolTip.HideToolTip();
        go_BaseUI.SetActive(false);
    }

    public void ShowToolTip(int _buttonNum)
    {
        theToolTip.ShowToolTip(kits[_buttonNum].KitName, kits[_buttonNum].kitDescription, kits[_buttonNum].needItemName, kits[_buttonNum].needItemNumber);
    }

    public void HideToolTip()
    {
        theToolTip.HideToolTip();
    }

    private void PlaySE(AudioClip _clip)
    {
        theAudio.clip = _clip;
        theAudio.Play();

    }

    public void ClickButton(int _slotNumber)
    {
        PlaySE(sound_buttonClick);
        if (!isCraft) //false면 실행
        {
            if (!Checkingredient(_slotNumber))
                return;

            isCraft = true;
            UseIngredient(_slotNumber);
            StartCoroutine(CraftCoroutine(_slotNumber));
        }
    }

    private void UseIngredient(int _slotNumber) //재료사용
    {
        for (int i = 0; i < kits[_slotNumber].needItemName.Length; i++)
        {
            theInven.SetItemCount(kits[_slotNumber].needItemName[i], kits[_slotNumber].needItemNumber[i]);
        }
    }

    private bool Checkingredient(int _slotNumber) //재료 체크
    {
        for (int i = 0; i < kits[_slotNumber].needItemName.Length; i++)
        {
            if (theInven.GetItemCount(kits[_slotNumber].needItemName[i]) < kits[_slotNumber].needItemNumber[i])
            {
                PlaySE(sound_Beep);
                return false;
            }
        }

        return true;
    }

    IEnumerator CraftCoroutine(int _slotNumber) //생성
    {
        PlaySE(sound_Activated);
        yield return new WaitForSeconds(3f);
        PlaySE(sound_Output);
        Instantiate(kits[_slotNumber].go_Kit_Prefab, tf_ItemAppear.position, Quaternion.identity);
        isCraft = false;
    }
}
