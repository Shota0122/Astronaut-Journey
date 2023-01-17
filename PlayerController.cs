using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    GameObject sunPlanets;
    [SerializeField] GameObject explosionPref;
    [SerializeField] GameObject skullExplosionPref;
    [SerializeField] GameObject distanceCanvasPref;
    GameObject distanceCanvas;
    GameObject gameManager;
    GameObject meteoManager;
    GameObject mainCamera;
    GameObject skyBoxCamera;
    GameObject meteo;
    GameObject targetMP;
    GameObject healEff;
    [SerializeField] GameObject heartBeatBoxPref;
    GameObject heartBeatBox;

    TextMeshProUGUI TMP_Distance;
    Image imgStamina;
    Image imgHeart;
    Image imgMoveCursor;
    

    Ray ray;
    RaycastHit hit;

    Vector3 toMovePos;
    Vector3 moveDir;
    Vector3 zeroPos;
    [SerializeField] float moveSpeed;
    float elapsed_vib = 0;
    float rotX=0;
    float rotY=0;
    float rotZ=0;
    float minAngle;
    float maxAngle;
    public float playTime;
    float playScore = 0;
    [SerializeField] float playerStamina;
    [SerializeField] float maxPlayerStamina;
    float hitStopTime = 0.3f;
    float panelAlpha = 0;

    [SerializeField] bool emergency = false;

    Tween emergencyTween;

    AudioSource myAudio;
    [SerializeField] AudioClip SE_Heal;
    [SerializeField] AudioClip SE_Move;
    [SerializeField] AudioClip SE_HeartBeat;
    
    PlayerStatus playerStatus;

    enum PlayerStatus
    {
        STAY,MOVE
    }

    private void Awake()
    { 

        distanceCanvas = Instantiate(distanceCanvasPref, transform.position - Vector3.up * 1.2f , Quaternion.identity);
        //進んだ距離を表示するTMPをシーンから取得
        TMP_Distance = distanceCanvas.transform.Find("TMP_Distance").GetComponent<TextMeshProUGUI>();
        TMP_Distance.text = "0km";

        //スタミナバーをシーンから取得
        imgStamina = GameObject.Find("StaminaBar").GetComponent<Image>();
        imgStamina.fillAmount = 1.0f;
        imgHeart = GameObject.Find("Img_Heart").GetComponent<Image>();
        //選択してるキャラのスピードと体力を取得
        moveSpeed = (PlayerPrefs.HasKey("Speed")) ? PlayerPrefs.GetInt("Speed") + 2.0f : 3.0f;
        minAngle = 270.0f * moveSpeed / 2.5f;
        maxAngle = 360.0f * moveSpeed / 2.5f;
        playerStamina = (PlayerPrefs.HasKey("Health")) ? PlayerPrefs.GetInt("Health") * 5.0f + 12.0f : 17.0f;
        maxPlayerStamina = playerStamina;

        healEff = transform.Find("PatHeal").gameObject;
    }


    void Start()
    {
        myAudio = GetComponent<AudioSource>();

        playerStatus = PlayerStatus.STAY;
        zeroPos = transform.position;
        toMovePos = zeroPos;

        gameManager = GameObject.Find("GameManager");
        meteoManager = GameObject.Find("MeteoManager");
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        skyBoxCamera = GameObject.FindGameObjectWithTag("SkyBoxCamera");
        sunPlanets = GameObject.FindGameObjectWithTag("SunPlanets");
        
    }



    private void OnTriggerEnter(Collider other)
    {
        

        //隕石とぶつかったら、ヒットストップ発生
        if (other.gameObject.CompareTag("Meteo"))
        {
            StartCoroutine("MeteoImpacted");
            Time.timeScale = 0;
            meteo = other.gameObject;

        }
        else if (other.gameObject.CompareTag("Bottle"))
        {
            myAudio.PlayOneShot(SE_Heal);

            //スタミナ回復ポーションを獲得した時の処理
            string bottleColor = other.gameObject.name.Substring(7, 1);
            switch (bottleColor)
            {
                case "G":
                    playerStamina += 5.0f;
                    break;

                case "B":
                    playerStamina += 10.0f;
                    break;

                case "R":
                    playerStamina += 20.0f;
                    break;

                default:
                    break;
            }

            playerStamina = Mathf.Clamp(playerStamina, 0, maxPlayerStamina);
            imgStamina.fillAmount = playerStamina / maxPlayerStamina;
            healEff.GetComponent<ParticleSystem>().Play();
            Destroy(other.gameObject);

            if (emergency)
            {
                emergency = false;
                emergencyTween.Kill();
                Destroy(heartBeatBox);
                imgHeart.transform.localScale = Vector3.one;
            }

            //imgHeart.gameObject.transform.DOScale(0.4f, 0.7f).SetRelative(true).SetEase(Ease.InBounce).SetLoops(2,LoopType.Yoyo);
        }

        


        if (!GameManager.isGameStart)
        {
            imgMoveCursor = other.gameObject.transform.Find("TouchUICanvas").gameObject.transform.Find("imgMoveCursor").GetComponent<Image>();
            imgMoveCursor.enabled = false;
        } 
    }


    

    void GameOver()
    {
        
        GameManager.isGameEnd = true;
        gameManager.SendMessage("ScoreInform",playTime,SendMessageOptions.DontRequireReceiver);
        gameManager.SendMessage("RebornPosInform", toMovePos, SendMessageOptions.DontRequireReceiver);
        gameManager.SendMessage("Navigate", 1 , SendMessageOptions.DontRequireReceiver);

        if (emergency)
        {
            emergency = false;
            emergencyTween.Kill();
            Destroy(heartBeatBox);
            imgHeart.transform.localScale = Vector3.one;
        }

        Destroy(gameObject);
        Destroy(distanceCanvas.gameObject);
    }

    void Reborn(float time)
    {
        playTime = time;
        playScore = playTime / 60.0f * 99999.0f;
        TMP_Distance.text = (playScore < 999999.0f) ? playScore.ToString("00") + "km" : "?????km";
    }

   

    //隕石とぶつかった時
    IEnumerator MeteoImpacted()
    {
        
        //ヒットストップ時間
        yield return new WaitForSecondsRealtime(hitStopTime);

        //時間停止を解除
        Time.timeScale = 1.0f;

        //ゲームオーバー
        mainCamera.transform.DOShakePosition(2f, 3f, 30, 1, false, true);
        GameObject explosion = Instantiate(explosionPref, transform.position, Quaternion.identity);
        Destroy(meteo);
        GameOver();

        

    }


    void Update()
    {
        
        
        if (!GameManager.isGameStart) return;


        playTime += Time.deltaTime;
        playScore = playTime / 60.0f * 99999.0f;
        TMP_Distance.text = (playScore < 999999.0f) ? playScore.ToString("00") + "km" : "?????km";

        //隕石を生成するインターバルを徐々に短くする
        meteoManager.GetComponent<MeteoManager>().generateInterval -= Time.deltaTime / 200.0f;
        meteoManager.GetComponent<MeteoManager>().generateInterval = Mathf.Clamp(meteoManager.GetComponent<MeteoManager>().generateInterval, 0.4f, 0.9f);




        distanceCanvas.transform.position = transform.position - Vector3.up * 1.2f;
        sunPlanets.transform.position += sunPlanets.transform.forward * Time.deltaTime * 300.0f;


        //プレイヤーを回転させる
        transform.Rotate(rotX * Time.deltaTime, rotY * Time.deltaTime, rotZ * Time.deltaTime);
        //回転を徐々に弱くする
        rotX *= 0.99f;
        rotY *= 0.99f;
        rotZ *= 0.99f;


        if (playerStamina <= 4 && !emergency)
        {
            emergency = true;
            emergencyTween = imgHeart.transform.DOScale(0.3f, 0.2f).SetRelative(true).SetLoops(-1, LoopType.Yoyo);
            heartBeatBox = Instantiate(heartBeatBoxPref, Vector3.zero, Quaternion.identity);
        }


        //タップした箇所にレイキャスト→当たり判定があれば移動
        if (Input.GetMouseButtonDown(0))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100.0f, Color.green, 5, false);

            if (Physics.Raycast(ray, out hit, 20.0f) && hit.collider.gameObject.CompareTag("MovePoint"))
            {
                
                elapsed_vib = 0;

                //移動中にタップ→ムーブポイントオブジェクトにレイが当たらなかったら、hitの中身は「何もないに上書きされる」→目的座標が「何もない」に上書きされるため、移動がその場で停止してエラー吐く
                //目的座標を取得したらtargetMPとして保持しておく→こうしておけば、レイがスカってhitの中身がなくなっても、targetMPの中身は変わらないため、移動も継続するし、エラーも吐かない
                targetMP = hit.collider.gameObject;

                playerStatus = PlayerStatus.MOVE;
                

                toMovePos = targetMP.transform.position;
                moveDir = (toMovePos - transform.position).normalized;


                //プレイヤーが無造作に回転するように調整
                rotX = Random.Range(minAngle, maxAngle);
                rotY = Random.Range(minAngle, maxAngle);
                rotZ = Random.Range(minAngle, maxAngle);
                if (Random.value < 0.5f) rotX *= -1.0f;
                if (Random.value < 0.5f) rotY *= -1.0f;
                if (Random.value < 0.5f) rotZ *= -1.0f;

                playerStamina -= 1.0f;
                imgStamina.fillAmount = playerStamina / maxPlayerStamina;

                if (playerStamina <= 0)
                {
                    GameObject skullExplosion = Instantiate(skullExplosionPref, transform.position, Quaternion.identity);
                    GameOver();
                }

                if (imgMoveCursor != null) imgMoveCursor.GetComponent<Image>().enabled = true;
                imgMoveCursor = targetMP.transform.Find("TouchUICanvas").gameObject.transform.Find("imgMoveCursor").GetComponent<Image>();
                imgMoveCursor.transform.DOScale(1.4f, 0.2f).OnComplete(() =>
                {
                    imgMoveCursor.GetComponent<Image>().enabled = false;
                    imgMoveCursor.transform.localScale = Vector3.one;
                });

                myAudio.PlayOneShot(SE_Move);

            }

            
        }



        switch (playerStatus)
        {
            case PlayerStatus.STAY:
                //アイドル中は上下にフワフワ揺れる
                elapsed_vib += Time.deltaTime;
                transform.position = zeroPos + new Vector3(0,0.3f*Mathf.Sin(elapsed_vib*Mathf.PI), 0);

                break;

            case PlayerStatus.MOVE:
                //徐々に速度が落ちるように移動
                transform.position = Vector3.Lerp(transform.position, targetMP.transform.position, Time.deltaTime * moveSpeed);
                //スカイボックスが回転
                skyBoxCamera.transform.Rotate(-moveDir.y * Time.deltaTime * 50.0f, moveDir.x * Time.deltaTime * 50.0f, 0);

                

                //目的座標到着時
                if ((targetMP.transform.position - transform.position).sqrMagnitude <= 0.01f)
                {
                    playerStatus = PlayerStatus.STAY;
                    transform.position = targetMP.transform.position;
                    zeroPos = transform.position;
                    
                }

                break;

            default:
                break;
        }

       


    }
}
