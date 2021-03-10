﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {

    [SerializeField] private GameObject go_BaseUI;
    [SerializeField] private SaveNLoad theSaveNLoad;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!GameManager.isPause)
                CallMenu();
            else
                CloseMenu();
        }
	}

    private void CallMenu()
    {
        GameManager.isPause = true;
        go_BaseUI.SetActive(true);
        Time.timeScale = 0f; //슬로우 모션을 줄 수 있다 -> 0은 정지
    }

    private void CloseMenu()
    {
        GameManager.isPause = false;
        go_BaseUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ClickSave()
    {
        Debug.Log("세이브");
        theSaveNLoad.SaveData();
    }

    public void ClickLoad()
    {
        Debug.Log("로드");
        theSaveNLoad.LoadData();
    }

    public void ClickExit()
    {
        Debug.Log("게임종료");
        Application.Quit();
    }
}
