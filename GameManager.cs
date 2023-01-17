using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    GameObject adManager;
    [SerializeField] GameObject characterCamera;
    [SerializeField] GameObject[] characterStorage;
    [SerializeField] GameObject[] opendCharacterStorage;
    GameObject player;
    GameObject OP;

    [SerializeField] TextMeshProUGUI TMP_Navi;
    [SerializeField] TextMeshProUGUI TMP_Score;
    [SerializeField] TextMeshProUGUI TMP_Level;
    [SerializeField] Image imgResult;
    [SerializeField] Image imgLevelUpLabel;
    [SerializeField] Image imgExpBar;
    [SerializeField] Image imgStaminaBar;
    [SerializeField] Image imgCharaGet;
    [SerializeField] Image[] imgMoveCursors;
    [SerializeField] Image imgBestScore;
    [SerializeField] Button btnHome;
    [SerializeField] Button btnReplay;
    [SerializeField] Button btnReborn;
 
    Vector2 naviStartPos;
    Vector3 charaCameraStartPos;
    Vector3 rebornPos;

    public static bool isGameStart;
    public static bool isGameEnd;
    bool openFLAG = false;
    bool openStart = false;
    bool charaGetTexting = false;
    bool reborned = false;
    public bool pushHome = false;
    public bool pushReplay = false;
    

    int selectCharacter;
    int level;
    int openedCharacter_CURRENT;
    int openedCharacter_END;
    float elapsed = 0;
    float playTime = 0;
    float playScore = 0;
    float playScore_REBORN = 0;
    float exp;
    float needExp;

    AudioSource myAudio;
    [SerializeField] AudioClip SE_Button;
    [SerializeField] AudioClip SE_CameraChange;
    [SerializeField] AudioClip SE_CharaOpen;
    [SerializeField] AudioClip SE_Nothing;



    private void Awake()
    {
        isGameStart = false;
        isGameEnd = false;
        charaCameraStartPos = characterCamera.transform.position;

        //選択しているキャラクターがいるならそれを、いないならデフォルトを表示する配列番号を設定
        selectCharacter = (PlayerPrefs.HasKey("Character")) ? PlayerPrefs.GetInt("Character") : 0;
        player = Instantiate(characterStorage[selectCharacter], new Vector3(2.0f,5.0f,0), Quaternion.identity);

        //現在の経験値量とレベルアップまでの必要経験値量を取得
        exp = (PlayerPrefs.HasKey("Exp")) ? PlayerPrefs.GetFloat("Exp") : 0;
        needExp = (PlayerPrefs.HasKey("NeedExp")) ? PlayerPrefs.GetFloat("NeedExp") : 100.0f;

        //現在のレベルを取得
        level = PlayerPrefs.GetInt("Level");
        TMP_Level.text = level + "";

        //現在のキャラクター解放数を取得
        openedCharacter_CURRENT = (PlayerPrefs.HasKey("OpenedCharacter")) ? PlayerPrefs.GetInt("OpenedCharacter") : 0;
        
        
        //ナビテキストの初期位置を記憶
        naviStartPos = TMP_Navi.rectTransform.position;
    }


    void Start()
    {
        adManager = GameObject.FindGameObjectWithTag("AdManager");
        myAudio = GetComponent<AudioSource>();

        Navigate(0);

        btnHome.onClick.AddListener(() =>
        {
            myAudio.PlayOneShot(SE_Button);

            pushHome = true;
            adManager.BroadcastMessage("ShowInterstitial", SendMessageOptions.DontRequireReceiver);
        });

        btnReplay.onClick.AddListener(() =>
        {
            myAudio.PlayOneShot(SE_Button);

            pushReplay = true;
            adManager.BroadcastMessage("ShowInterstitial", SendMessageOptions.DontRequireReceiver);

        });


        btnReborn.onClick.AddListener(() =>
        {
            

            if (!reborned)
            {
                myAudio.PlayOneShot(SE_Button);
                adManager.BroadcastMessage("LoadRewardedAd", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                myAudio.PlayOneShot(SE_Nothing);
            }

            

        });

        


    }

    //START,FINISH,REBORNの文字を表示
    void Navigate(int type)
    {
        switch (type)
        {
            case 0:
                TMP_Navi.text = "- START -";
                break;

            case 1:
                TMP_Navi.text = "- FINISH -";
                foreach (Image img in imgMoveCursors)
                {
                    img.gameObject.SetActive(false);
                }
                break;

            case 2:
                TMP_Navi.text = "- REBORNED -";
                break;

            default:
                break;
        }

        TMP_Navi.rectTransform
            .DOLocalMoveX(0, 1.0f)
            .SetEase(Ease.InQuint)
            .SetDelay(1.0f)
            .OnComplete(()=>{
                              TMP_Navi.transform
                                  .DOScale(0, 1.0f)
                                  .SetDelay(2.0f)
                                  .OnComplete(() =>
                                  {
                                      //STARTもしくはREBORNEDの表示が終了したら、移動可能なムーブカーソルを全て表示
                                      if (type != 1)
                                      {
                                          isGameStart = true;
                                          foreach(Image img in imgMoveCursors)
                                          {
                                              img.gameObject.SetActive(true);
                                          }

                                      }
                                     

                                      
                                      TMP_Navi.rectTransform.position = naviStartPos;
                                      TMP_Navi.rectTransform.localScale = Vector3.one;

                                      //フィニッシュの文字が表示され終えたら
                                      if (isGameEnd)
                                      {
                                          if (openFLAG)
                                          {
                                              StartCoroutine("ChangeCamera");
                                          }
                                          else
                                          {
                                              //キャラ解放はないため、リザルト表示
                                              imgResult.gameObject.SetActive(true);
                                          }


                                          


                                      }

                                  });

                            });
    }


    //プレイスコアを記録(リザルト画面へ反映する用と復活時用)
    void ScoreInform(float time)
    {
        playTime = time;
        playScore = playTime / 60.0f * 99999.0f;
        //復活前のスコアをプール
        if(!reborned) playScore_REBORN = playTime / 60.0f * 99999.0f;
        TMP_Score.text = (int)playScore + "km";
       

        //ベストスコアか判定し、ベストスコアであればデータベースへ書き込み
        if (PlayerPrefs.GetInt("BestScore") < (int)playScore)
        {
            imgBestScore.gameObject.SetActive(true);

            GameObject firestoreManager = GameObject.Find("FireStoreManager");
            firestoreManager.SendMessage("SetScore",(int)playScore,SendMessageOptions.DontRequireReceiver);

            PlayerPrefs.SetInt("BestScore", (int)playScore);
        }


        if (level < 100)
        {
            //復活フラグが立ってたら、このスコアを調整(復活前と復活後の差分を算出）
            if(reborned) playScore -= playScore_REBORN;

            //Lv100以下なら、スコアを120分の１にし、取得経験値に変換　→　現在の保有経験値にプラス
            exp += playScore/120.0f;
            CheckLevelUp();
        }
        else
        {
            //Lv100以上なら、経験値バーはマックスに固定
            imgExpBar.fillAmount = 1.0f;
        }

        if(!PlayerPrefs.HasKey("AppReview")) PlayerPrefs.SetInt("AppReview", 1);



    }

    //取得経験値に応じて、レベルアップ処理を実行(レベル１００になるまではここの処理を通過する）
    void CheckLevelUp()
    {
        while (true)
        {
            if(exp >= needExp&&level<100)
            {
                exp -= needExp;
                //必要経験値量が1.025倍に
                needExp *= 1.025f;
                level++;

                imgLevelUpLabel.gameObject.SetActive(true);
            }
            else
            {
                

                //レベルアップ終了時、現在の経験値量、必要な経験値量、現在のレベルを保存(レベル１００になった瞬間のEXPとNeedExpもしっかり保存されている）
                PlayerPrefs.SetFloat("Exp",exp);
                PlayerPrefs.SetFloat("NeedExp",needExp);
                PlayerPrefs.SetInt("Level", level);
                TMP_Level.text = level + "";
                imgExpBar.fillAmount = exp / needExp;

                //レベル100以上＋5体目のキャラ未解放だったら、解放処理実行
                if (level >= 100 && openedCharacter_CURRENT < 5)
                {
                    //5体目のキャラまで解放するよーフラグ(複数キャラが同時解放された時のため)
                    openedCharacter_END = 5;
                    OpenFlagging();
                    
                }
                else if (level >= 80 && openedCharacter_CURRENT < 4)
                {

                    openedCharacter_END = 4;
                    OpenFlagging();
                    
                }
                else if (level >= 60 && openedCharacter_CURRENT < 3)
                {

                    openedCharacter_END = 3;
                    OpenFlagging();
                    
                }
                else if (level >= 40 && openedCharacter_CURRENT < 2)
                {
                    openedCharacter_END = 2;
                    OpenFlagging();
                }
                else if (level >= 20 && openedCharacter_CURRENT < 1)
                {
                    openedCharacter_END = 1;
                    OpenFlagging();



                }

                break;
            }
        }
    }


    void OpenFlagging()
    {
        PlayerPrefs.SetInt("SkinOpen", 1);
        PlayerPrefs.SetInt("OpenedCharacter", openedCharacter_END);
        openedCharacter_CURRENT++;
        OP = Instantiate(opendCharacterStorage[openedCharacter_CURRENT], new Vector3(0, 0, -5396.0f), Quaternion.identity);
        OP.transform.localEulerAngles = new Vector3(0, 180.0f, 0);
        openFLAG = true;
    }


    void RebornPosInform(Vector3 pos)
    {
        rebornPos = pos;
    }

    void RebornRewarded()
    {
        //リワード広告視聴後にリザルト画面を非表示にし、続きからゲーム再開
        if (!reborned)
        {
            reborned = true;
            isGameStart = false;
            isGameEnd = false;
            imgResult.gameObject.SetActive(false);
            imgBestScore.gameObject.SetActive(false);
            btnReborn.gameObject.transform.Find("imgKey").gameObject.GetComponent<Image>().enabled = true;
            btnReborn.gameObject.transform.Find("imgReborn").GetComponent<Image>().color = Color.black;
            player = Instantiate(characterStorage[selectCharacter], rebornPos, Quaternion.identity);
            player.SendMessage("Reborn", playTime, SendMessageOptions.DontRequireReceiver);
            Navigate(2);
        }
    }
    



    IEnumerator ChangeCamera()
    {
        yield return new WaitForSeconds(1.0f);

        //キャラ解放処理を開始
        openFLAG = false;
        openStart = true;
        imgStaminaBar.gameObject.transform.parent.gameObject.SetActive(false);
        characterCamera.SetActive(true);
        myAudio.PlayOneShot(SE_CameraChange);
    }


    void Update()
    {
        
        //ムーブカーソルを回転
        foreach(Image img in imgMoveCursors)
        {
            img.rectTransform.Rotate(0, 0, -60.0f * Time.deltaTime);
        }
        

        if (!openStart) return;


        if(elapsed <= 2.0f)
        {
            elapsed += Time.deltaTime;
            return;
        }
        


        characterCamera.transform.position = Vector3.Lerp(characterCamera.transform.position, new Vector3(0,1.5f,-5400f), Time.deltaTime * 5.0f);

        

        if ((characterCamera.transform.position - OP.transform.position).sqrMagnitude <= 20.0f)
        {

            if (!charaGetTexting)
            {
                charaGetTexting = true;
                imgCharaGet.transform.DOScale(1.0f, 0.5f);
                myAudio.PlayOneShot(SE_CharaOpen);   

            }


            OP.transform.Rotate(0, 90.0f * Time.deltaTime, 0);


            if (Input.GetMouseButtonDown(0))
            {
                elapsed = 0;
                openedCharacter_CURRENT++;
                characterCamera.transform.position = charaCameraStartPos;
                Destroy(OP);

                charaGetTexting = false;
                imgCharaGet.transform.localScale = Vector3.zero;
                

                if (openedCharacter_CURRENT <= openedCharacter_END)
                {
                    myAudio.PlayOneShot(SE_CameraChange);
                    OP = Instantiate(opendCharacterStorage[openedCharacter_CURRENT], new Vector3(0, 0, -5396.0f), Quaternion.identity);
                    OP.transform.localEulerAngles = new Vector3(0, 180.0f, 0);
                }
                else
                {
                    
                    openStart = false;
                    openedCharacter_CURRENT--;
                    characterCamera.SetActive(false);
                    imgResult.gameObject.SetActive(true);
                    if (imgCharaGet.transform.localScale != Vector3.zero) imgCharaGet.transform.localScale = Vector3.zero;
                    imgStaminaBar.gameObject.transform.parent.gameObject.SetActive(true);

                }

            }
        }
        
    }
}
