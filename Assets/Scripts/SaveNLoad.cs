using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable] //데이터 직렬화->데이터를 직렬화하면 한줄로 데이터들이 나열되어 저장장치가 읽고 쓰기 쉬워짐
public class SaveData
{
    public Vector3 playerPos;
    public Vector3 playerRot;

    //인벤토리
    public List<int> invenArrayNumber = new List<int>();
    public List<string> invenItemName = new List<string>();//slot은 직렬화 안됨-> 클래스가 안되는거였나 그럼
    public List<int> invenItemNumber = new List<int>();
}

public class SaveNLoad : MonoBehaviour {

    private SaveData saveData = new SaveData();

    private string SAVE_DATA_DIRECTORY;
    private string SAVE_FILENAME = "/SaveFile.txt";

    private PlayerController thePlayer;
    private Inventory theInven;

    // Use this for initialization
    void Start () {
        SAVE_DATA_DIRECTORY = Application.dataPath + "/Saves/"; //datapath = 현재 저장된 경로

        if (!Directory.Exists(SAVE_DATA_DIRECTORY))//경로에 존재하는가
            Directory.CreateDirectory(SAVE_DATA_DIRECTORY);

    }

    public void SaveData()
    {
        thePlayer = FindObjectOfType<PlayerController>();
        theInven = FindObjectOfType<Inventory>();

        saveData.playerPos = thePlayer.transform.position;
        saveData.playerRot = thePlayer.transform.eulerAngles;

        Slot[] slots = theInven.GetSlots();
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null)
            {
                saveData.invenArrayNumber.Add(i);
                saveData.invenItemName.Add(slots[i].item.itemName);
                saveData.invenItemNumber.Add(slots[i].itemCount);
            }
        }

        string json = JsonUtility.ToJson(saveData);

        File.WriteAllText(SAVE_DATA_DIRECTORY + SAVE_FILENAME, json);//이 경로에 json 저장

        Debug.Log("저장 완료");

    }

    public void LoadData()
    {
        if (File.Exists(SAVE_DATA_DIRECTORY + SAVE_FILENAME))
        {
            string loadJson = File.ReadAllText(SAVE_DATA_DIRECTORY + SAVE_FILENAME);
            saveData = JsonUtility.FromJson<SaveData>(loadJson); //save로 저장된 txt를 다시 사용할 수 있도록 클래스화

            thePlayer = FindObjectOfType<PlayerController>();
            theInven = FindObjectOfType<Inventory>();

            thePlayer.transform.position = saveData.playerPos;
            thePlayer.transform.eulerAngles = saveData.playerRot;

            for (int i = 0; i < saveData.invenItemName.Count; i++)
            {
                theInven.LoadToInven(saveData.invenArrayNumber[i], saveData.invenItemName[i], saveData.invenItemNumber[i]);
            }

            Debug.Log("로드 완료");
        }
        else
        {
            Debug.Log("세이브 파일이 없습니다.");
        }

    }

}
