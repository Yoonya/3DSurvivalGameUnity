using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputNumber : MonoBehaviour {

    private bool activated = false;

    [SerializeField]
    private Text text_Preview;
    [SerializeField]
    private Text text_Input;
    [SerializeField]
    private InputField if_text;//다시 불러올때 이전 텍스트가 남아있는 것 방지

    [SerializeField]
    private GameObject go_Base;

    [SerializeField]
    private ActionController thePlayer;

    void Update()
    {
        if (activated)
        {
            if (Input.GetKeyDown(KeyCode.Return))
                OK();
            else if (Input.GetKeyDown(KeyCode.Escape));
        }
    }

    public void Call()
    {
        go_Base.SetActive(true);
        activated = true;
        if_text.text = ""; ;//다시 불러올때 이전 텍스트가 남아있는 것 방지
        text_Preview.text = DragSlot.instance.dragSlot.itemCount.ToString();
    }

    public void Cancel()
    {
        activated = false;
        go_Base.SetActive(false);
        DragSlot.instance.SetColor(0);
        DragSlot.instance.dragSlot = null;
    }

    public void OK()
    {
        DragSlot.instance.SetColor(0);

        int num;
        if (text_Input.text != "")
        {
            if (CheckNumber(text_Input.text))
            {
                num = int.Parse(text_Input.text);
                if (num > DragSlot.instance.dragSlot.itemCount)
                    num = DragSlot.instance.dragSlot.itemCount;
            }
            else
            {
                num = 1; //이상한게 들어오면 1개만 버리도록 설정함
            }
        }
        else
        {
            num = int.Parse(text_Preview.text);//아무것도 입력안하면 최대 개수 버리도록
        }

        StartCoroutine(DropItemCoroutine(num));
    }

    IEnumerator DropItemCoroutine(int _num)
    {
        for (int i = 0; i < _num; i++)
        {
            if(DragSlot.instance.dragSlot.item.itemPrefab != null) //나뭇잎같은 아이템은 즉시 파괴
                Instantiate(DragSlot.instance.dragSlot.item.itemPrefab, thePlayer.transform.position + thePlayer.transform.forward, Quaternion.identity); //카메라가 보는방향으로 생성 
            DragSlot.instance.dragSlot.SetSlotCount(-1);
            yield return new WaitForSeconds(0.05f);
        }

        //현재 아이템을 들고 있고, 그 아이템의 모든 개수를 버릴 경우 아이템 파괴
        if (int.Parse(text_Preview.text) == _num)
            if (QuickSlotController.go_HandItem != null)
                Destroy(QuickSlotController.go_HandItem);
       
        DragSlot.instance.dragSlot = null;
        go_Base.SetActive(false);
        activated = false;
    }

    private bool CheckNumber(string _argString)
    {
        char[] _tempCharArray = _argString.ToCharArray();//string의 문자들이 배열에 전부 들어감
        bool isNumber = true;
        for (int i = 0; i < _tempCharArray.Length; i++)
        {
            if (_tempCharArray[i] >= 48 && _tempCharArray[i] <= 57)//아스키0~9
                continue;
            isNumber = false;
        }
        return isNumber;
    }
}
