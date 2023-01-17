using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteoManager : MonoBehaviour
{
    //メテオプレハブ
    [SerializeField] GameObject meteoPref;
    //[SerializeField] GameObject appearMeteoEff;
    //スタミナ回復ポーション配列
    [SerializeField] GameObject[] bottles;
    GameObject meteo;
    GameObject bottle;

    //メテオ生成オブジェクトを管理するリスト
    public List<GameObject> meteoGeneratorsList = new List<GameObject>();
    public List<GameObject> meteoGeneratorsList_pool = new List<GameObject>();
    //移動オブジェクトを管理するリスト
    [SerializeField] List<GameObject> movePointList = new List<GameObject>();

    int bottleNum = 0;
    int canGenerateNum = 9;
    public float generateInterval = 0.9f;
    float elapsed = 0;
    
    

    void Start()
    {
        
        GameObject[] MG = GameObject.FindGameObjectsWithTag("MeteoGenerator");

        foreach(GameObject store in MG)
        {
            //メテオ生成オブジェクトを一括で取得し、リストイン
            meteoGeneratorsList.Add(store);
            //移動オブジェクトを一括で取得し、リストイン
            movePointList.Add(store.transform.parent.gameObject);
        }
    }

    
    void Update()
    {
        if (!GameManager.isGameStart||GameManager.isGameEnd) return;

        elapsed += Time.deltaTime;

        if(elapsed >= generateInterval)
        {
            elapsed = 0;
            //生成箇所をランダムで決定(生成オブジェクトの入ったリスト番号を決定）
            int rnd = Random.Range(0, canGenerateNum);
            
            canGenerateNum -= 1;
            
            
            //移動先のオブジェクトは生成オブジェクトの親
            GameObject targetObj = meteoGeneratorsList[rnd].transform.parent.gameObject;
            //Instantiate(appearMeteoEff, meteoGeneratorsList[rnd].transform.position, Quaternion.identity);

            

            //５％の確率でスタミナ回復ポーションを生成
            if (Random.value <= 0.05f&&bottle==null)
            {
                float rndm = Random.value;
                if(rndm <= 0.6f)
                {
                    //60％は緑ポーション
                    bottleNum = 0;

                }else if(rndm <= 0.9f)
                {
                    //３0％は青ポーション
                    bottleNum = 1;
                }
                else
                {
                    //10％は赤ポーション
                    bottleNum = 2;
                }

                bottle = Instantiate(bottles[bottleNum], meteoGeneratorsList[rnd].transform.position, Quaternion.identity);
                //生成したポーションに移動開始命令
                bottle.SendMessage("LockOnTarget", targetObj, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                

                //９５％の確率でメテオを生成
                meteo = Instantiate(meteoPref, meteoGeneratorsList[rnd].transform.position, Quaternion.identity);
                //生成したメテオに移動開始命令
                meteo.SendMessage("LockOnTarget", targetObj, SendMessageOptions.DontRequireReceiver);

                
            }
            
           
            //生成を終えたオブジェクトを他のリストにプール
            meteoGeneratorsList_pool.Add(meteoGeneratorsList[rnd]);
            //生成を終えたオブジェクトを生成オブジェクトリストから破棄
            meteoGeneratorsList.Remove(meteoGeneratorsList[rnd]);


           


            //プールしている生成オブジェクトが規定数に達したら、生成オブジェクトリストへ１つ返還
            if (meteoGeneratorsList_pool.Count >= 5)
            {
                meteoGeneratorsList.Add(meteoGeneratorsList_pool[0]);
                meteoGeneratorsList_pool.Remove(meteoGeneratorsList_pool[0]);
                canGenerateNum += 1;

            } 


        }
    }
}
