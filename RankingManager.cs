using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class RankingManager : MonoBehaviour
{
    [SerializeField] GameObject earth;
    [SerializeField] GameObject[] characterStorage;

    [SerializeField] Button btnPlay;
    [SerializeField] Button btnSkin;
    [SerializeField] Button btnHome;
    [SerializeField] TextMeshProUGUI TMP_loadingPercent;
    [SerializeField] Image imgLoadingBar;

    int selectCharacter;
    float elapsed = 0;
    float loadingPercent = 0;
    bool pushPlay = false;
    bool pushHome = false;
    bool pushSkin = false;
    bool isLoadingStart = false;

    AudioSource myAudio;
    [SerializeField] AudioClip SE_Button;



    private void Awake()
    {
        //ファイアーストアにアクセスし、ランキングトップ３と自分のスコアを取得する
        GameObject firestoreManager = GameObject.Find("FireStoreManager");
        firestoreManager.SendMessage("GetRanking", SendMessageOptions.DontRequireReceiver);
        firestoreManager.SendMessage("GetScore", SendMessageOptions.DontRequireReceiver);

        //選択しているキャラクターがいるならそれを、いないならデフォルトを表示する配列番号を設定
        selectCharacter = (PlayerPrefs.HasKey("Character")) ? PlayerPrefs.GetInt("Character") : 0;
        GameObject player = Instantiate(characterStorage[selectCharacter], new Vector3(0,-0.5f,0), Quaternion.identity);
        player.transform.localEulerAngles = new Vector3(0, 180.0f, 0);
    }


    void Start()
    {
        myAudio = GetComponent<AudioSource>();

        btnPlay.onClick.AddListener(() =>
        {
            pushPlay = true;
            EraseButton();
        });

        btnHome.onClick.AddListener(() =>
        {
            pushHome = true;
            EraseButton();
        });

        btnSkin.onClick.AddListener(() =>
        {
            pushSkin = true;
            EraseButton();
        });

    }

    void EraseButton()
    {
        myAudio.PlayOneShot(SE_Button);

        btnPlay.transform.parent.gameObject.SetActive(false);
        btnSkin.transform.parent.gameObject.SetActive(false);
        btnHome.transform.parent.gameObject.SetActive(false);
        imgLoadingBar.transform.parent.gameObject.SetActive(true);

        Invoke("LoadingStart", 1.0f);
    }


    void LoadingStart()
    {
        isLoadingStart = true;
    }


    void LoadingComplete()
    {
        if (pushPlay)
        {
            SceneManager.LoadScene("Game");

        }
        else if (pushHome)
        {
            SceneManager.LoadScene("Title");
        }
        else if (pushSkin)
        {
            SceneManager.LoadScene("Skin");
        }
        
    }
    

    void Update()
    {
        earth.transform.Rotate(60.0f * Time.deltaTime, 0, 0);

        if (isLoadingStart)
        {
            elapsed += Time.deltaTime;
            loadingPercent = elapsed / 3.0f;

            TMP_loadingPercent.text = (loadingPercent * 100.0f).ToString("0") + "%";
            imgLoadingBar.fillAmount = loadingPercent;

            if (loadingPercent >= 1.0f)
            {
                
                TMP_loadingPercent.text = "Completed";
                imgLoadingBar.fillAmount = 1;

                Invoke("LoadingComplete", 1.0f);
            }

        }
    }




   







}
