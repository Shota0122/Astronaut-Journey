using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class SkinManager : MonoBehaviour
{
    [SerializeField] GameObject earth;
    GameObject displayCharacter;
    [SerializeField] GameObject[] characterStorage;
    GameObject appearLight;
    GameObject characterStatusManager;

    [SerializeField] Button btnPlay;
    [SerializeField] Button btnRank;
    [SerializeField] Button btnHome;
    [SerializeField] TextMeshProUGUI TMP_loadingPercent;
    [SerializeField] Image imgLoadingBar;
    [SerializeField] Image imgTouchLight;
    
    

    Vector2 tapScreenPos;
    Vector2 releaseScreenPos;
    Vector2 swipeDir;

    int rotateflag = 0;
    int selectCharacter;
    //int level; //現状は100まで
    int opendCharacter;

    float elapsed = 0;
    float loadingPercent = 0;
    bool pushPlay = false;
    bool pushHome = false;
    bool pushRank = false;
    bool isLoadingStart = false;

    bool changeChara = false;
    bool rightRotate = false;
    bool leftRotate = false;
    bool isErase = false;
    bool swiped = false;

    AudioSource myAudio;
    [SerializeField] AudioClip SE_Button;
    [SerializeField] AudioClip SE_Move;
    [SerializeField] AudioClip SE_Open;


    private void Awake()
    {
        //選択しているキャラクターがいるならそれを、いないならデフォルトを表示する配列番号を設定
        selectCharacter = (PlayerPrefs.HasKey("Character")) ? PlayerPrefs.GetInt("Character") : 0;
        
    }

    void Start()
    {
        myAudio = GetComponent<AudioSource>();

        appearLight = GameObject.Find("AppearLight");
        characterStatusManager = GameObject.Find("CharacterStatusManager");
        //キャラのステータスを決定し、ステータスを表すスターを調整する
        characterStatusManager.SendMessage("StatusDecision", selectCharacter, SendMessageOptions.DontRequireReceiver);

        //初期設定されているキャラを出現
        displayCharacter = Instantiate(characterStorage[selectCharacter], new Vector3(0, 1.0f, -5.0f), Quaternion.identity);
        displayCharacter.transform.localEulerAngles = new Vector3(0, 180.0f, 0);
        //保存されてるプレイヤーレベルを取得。20ごとに選べるキャラが増える。
        //level = PlayerPrefs.GetInt("Level");
        //level /= 20;
        opendCharacter = (PlayerPrefs.HasKey("OpenedCharacter")) ? PlayerPrefs.GetInt("OpenedCharacter") : 0;

        btnPlay.onClick.AddListener(() =>
        {
            pushPlay = true;
            EraseButton();
        });

        btnRank.onClick.AddListener(() =>
        {
            pushRank = true;
            EraseButton();
        });

        btnHome.onClick.AddListener(() =>
        {
            pushHome = true;
            EraseButton();
        });



        Invoke("TouchImage", 3.0f);

    }


    void EraseButton()
    {
        myAudio.PlayOneShot(SE_Button);

        //ボタンを押したら、その時表示されているキャラをプレイ時に使用
        PlayerPrefs.SetInt("Character", selectCharacter);

        btnPlay.transform.parent.gameObject.SetActive(false);
        btnRank.transform.parent.gameObject.SetActive(false);
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
        else if (pushRank)
        {
            SceneManager.LoadScene("Ranking");
        }

    }

    void TouchImage()
    {
        if (!swiped)
        {
            imgTouchLight.gameObject.SetActive(true);
            imgTouchLight.transform.DOScale(1.2f, 1)
            .OnComplete(() =>
            {
                imgTouchLight.GetComponent<Image>().enabled = true;
                imgTouchLight.transform.DOLocalMoveX(162, 1).SetDelay(1).OnComplete(() => imgTouchLight.gameObject.SetActive(false));
            });
        }
    }


    


    //地球を回転させて、回転停止後に次のキャラを出現
    IEnumerator EarthRotate()
    {
        isErase = true;

        swiped = true;
        if(imgTouchLight.gameObject != null) imgTouchLight.gameObject.SetActive(false);

        myAudio.PlayOneShot(SE_Move);

        yield return new WaitForSeconds(0.8f);
        //地球の回転停止、出現時の光をプレイ
        rotateflag = 0;
        appearLight.GetComponent<ParticleSystem>().Play();
        myAudio.PlayOneShot(SE_Open);
        //キャラのステータスを決定し、ステータスを表すスターを調整する
        characterStatusManager.SendMessage("StatusDecision", selectCharacter, SendMessageOptions.DontRequireReceiver);

        yield return new WaitForSeconds(0.35f);
        //光った後にキャラを出現
        changeChara = false;
        rightRotate = false;
        leftRotate = false;
        displayCharacter = Instantiate(characterStorage[selectCharacter], new Vector3(0,1.0f,-5.0f), Quaternion.identity);
        displayCharacter.transform.localEulerAngles = new Vector3(0, 180.0f, 0);
        
    }




    void Update()
    {
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

        if (isLoadingStart) return;



        if (Input.GetMouseButtonDown(0)) tapScreenPos = Input.mousePosition;


        if (Input.GetMouseButtonUp(0) && !changeChara)
        {
            changeChara = true;
            releaseScreenPos = Input.mousePosition;
            swipeDir = releaseScreenPos - tapScreenPos;

            
            if (swipeDir.x > 0)//右スワイプ
            {
                //地球を右回転
                rightRotate = true;
                rotateflag = -1;
                //キャラ配列の部屋番号を調整。キャラ数が増えても変更する必要なし。
                if (selectCharacter == opendCharacter)//level)
                {
                    selectCharacter = 0;
                }
                else
                {
                    selectCharacter++;
                }

                StartCoroutine("EarthRotate");
                
                
            }
            else if(swipeDir.x < 0)//左スワイプ
            {
                leftRotate = true;
                rotateflag = 1;
                if (selectCharacter == 0)
                {
                    selectCharacter = opendCharacter;//level;
                }
                else
                {
                    selectCharacter--;
                }
        
                StartCoroutine("EarthRotate");
            }
            else
            {
                changeChara = false;
            }
            
        }


        //地球を回転させる。ローテートフラッグで回転するか否かを調整。
        earth.transform.Rotate(0, 0, 1000.0f *rotateflag*Time.deltaTime);

        //キャラを小さくしていき最終的には消す処理
        if (isErase)
        {
            displayCharacter.transform.localScale *= 0.8f;

            if (displayCharacter.transform.localScale.x < 0.1f)
            {
                isErase = false;
                Destroy(displayCharacter.gameObject);
            }
        }



    }
}
