using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] GameObject earth;
    [SerializeField] GameObject[] characterStorage;
    GameObject firestoreManager;

    [SerializeField] Image btnPlayBack;
    [SerializeField] Image btnRankingBack;
    [SerializeField] Image btnSkinBack;
    [SerializeField] Button btnPlay;
    [SerializeField] Button btnRanking;
    [SerializeField] Button btnSkin;
    [SerializeField] Image imgLoadingBar;
    [SerializeField] TextMeshProUGUI TMP_loadingPercent;
    [SerializeField] TMP_InputField inputField;

    int selectCharacter;
    int level;

    float elapsed = 0;
    float loadingPercent = 0;

    string playerName;

    bool pushPlay = false;
    bool pushRanking = false;
    bool pushSkin = false;
    bool isLoadingStart = false;

    AudioSource myAudio;
    [SerializeField] AudioClip SE_Button;
    [SerializeField] AudioClip SE_Nothing;

    private void Awake()
    {
        
        //プレイヤーネームが設定されてない場合
        if (!PlayerPrefs.HasKey("PlayerName"))
        {
            btnPlay.gameObject.transform.parent.gameObject.SetActive(false);
            btnRanking.gameObject.transform.parent.gameObject.SetActive(false);
            btnSkin.gameObject.transform.parent.gameObject.SetActive(false);
            inputField.gameObject.SetActive(true);
        }

        if (!PlayerPrefs.HasKey("SkinOpen"))
        {
            btnSkin.gameObject.transform.Find("imgKey").gameObject.GetComponent<Image>().enabled = true;
            btnSkin.gameObject.transform.Find("imgSkin").GetComponent<Image>().color = Color.black;
        }
        
        

        //選択しているキャラクターがいるならそれを、いないならデフォルトを表示する配列番号を設定
        selectCharacter = (PlayerPrefs.HasKey("Character")) ? PlayerPrefs.GetInt("Character") : 0;
        GameObject player = Instantiate(characterStorage[selectCharacter], Vector3.zero, Quaternion.identity);
        player.transform.localEulerAngles = new Vector3(0, 180.0f, 0);

        level = (PlayerPrefs.HasKey("Level")) ? PlayerPrefs.GetInt("Level") : 1;

#if UNITY_IOS

        if (PlayerPrefs.HasKey("AppReview"))
        {
            if (PlayerPrefs.GetInt("AppReview") != 0)
            {
                UnityEngine.iOS.Device.RequestStoreReview();
                PlayerPrefs.SetInt("AppReview", 0);
            } 
            
        }

#endif

    }

    void Start()
    {
        myAudio = GetComponent<AudioSource>();

        btnPlay.onClick.AddListener(() =>
        {
            pushPlay = true;
            EraseButton();

        });

        btnRanking.onClick.AddListener(() =>
        {
            pushRanking = true;
            EraseButton();

        });

        btnSkin.onClick.AddListener(() =>
        {
            if (PlayerPrefs.HasKey("SkinOpen"))
            {
                pushSkin = true;
                EraseButton();
            }
            else
            {
                Debug.Log("A");
                myAudio.PlayOneShot(SE_Nothing);
            }
            

        });

       
    }

    //プレイヤーネームの入力を終えたら
    void OnEndEdit()
    {
        //入力された文字列を保存
        playerName = inputField.GetComponent<TMP_InputField>().text;
        
        firestoreManager = GameObject.Find("FireStoreManager");
        firestoreManager.SendMessage("NameExistCheck",playerName,SendMessageOptions.DontRequireReceiver);
        


    }


    void InputFieldClose()
    {
        inputField.gameObject.SetActive(false);
        btnPlay.gameObject.transform.parent.gameObject.SetActive(true);
        btnRanking.gameObject.transform.parent.gameObject.SetActive(true);
        btnSkin.gameObject.transform.parent.gameObject.SetActive(true);
    }

    


    void EraseButton()
    {
        myAudio.PlayOneShot(SE_Button);

        btnPlay.transform.parent.gameObject.SetActive(false);
        btnRanking.transform.parent.gameObject.SetActive(false);
        btnSkin.transform.parent.gameObject.SetActive(false);
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

        }else if (pushRanking)
        {
            SceneManager.LoadScene("Ranking");
        }
        else if(pushSkin)
        {
            SceneManager.LoadScene("Skin");
        }
        
    }
    

    void Update()
    {
        if(SceneManager.GetActiveScene().name == "Title")
        {
            earth.transform.Rotate(30.0f * Time.deltaTime, 0, 0);

        }else if(SceneManager.GetActiveScene().name == "Ranking")
        {
            earth.transform.Rotate(60.0f * Time.deltaTime, 0, 0);
        }
        

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
