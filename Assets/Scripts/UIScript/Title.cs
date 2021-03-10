using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour {

    public string sceneName = "GameStage";

    public static Title instance;

    private SaveNLoad theSaveNLoad;

    [SerializeField] GameObject go_LoadingBar;
    [SerializeField] Slider loadingBar;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(this.gameObject);
    }

    public void ClickStart()
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ClickLoad()
    {
        StartCoroutine(LoadCoroutine());
    }

    IEnumerator LoadCoroutine() //로드씬을 한 후 바로 로드 데이터를 하면 너무 빠르게 이루어져 씬이 전부 불러와지지 않았는데, 로드데이터를 하여 null이 출력될 수 있다. 동기화를 위해 텀을 줘야한다.
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName); //싱크를 맞추기 위해 로딩관련 일을 할 수 있는 것

        go_LoadingBar.SetActive(true); //로딩바 나오게

        while (!operation.isDone)//로딩이 끝날 때까지
        {        
            loadingBar.value = operation.progress; //로딩바 값
            yield return null; //대기
        }

        go_LoadingBar.SetActive(false);
        theSaveNLoad = FindObjectOfType<SaveNLoad>();
        theSaveNLoad.LoadData();

        this.gameObject.SetActive(false);
    }

    public void ClickExit()
    {
        Application.Quit();
    }
}
