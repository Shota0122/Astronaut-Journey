using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;

public class FireStoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] playerName_Rank;
    [SerializeField] TextMeshProUGUI[] score_Rank;

    [SerializeField] TextMeshProUGUI nameMain;
    [SerializeField] TextMeshProUGUI scoreMain;



    //プレイヤーネームを入力完了した時、重複する名前がないかデータベースをチェックする(ゲームマネージャーからコール）
    void NameExistCheck(string name)
    {
        var docRef = Firebase.Firestore.FirebaseFirestore.DefaultInstance.Collection("AllPlayerInfo").Document(name);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                //重複する名前があるため、キャンセル音を鳴らす
                //myAudio.PlayOneShot(SE_NameInputFailed);

            }
            else
            {
                //使用可能な名前なので、データベースに登録
                NameDicide(name);

            }

        });
    }




   　//プレイヤーネームをデータベースに登録する処理
    void NameDicide(string name)
    {
        GameObject titleManager = GameObject.Find("TitleManager");
        titleManager.SendMessage("InputFieldClose", SendMessageOptions.DontRequireReceiver);

        var docRef = Firebase.Firestore.FirebaseFirestore.DefaultInstance.Collection("AllPlayerInfo").Document(name);

        //プレイヤーネームをデータベースに追加する(スコアは０）
        var Ranking = new System.Collections.Generic.Dictionary<string, object>
                {
                    {"Score",0 }
     
                };

        docRef.SetAsync(Ranking).ContinueWithOnMainThread(task => {
            
            //プレイヤーネームがデータベースに書き込み完了したら、プレイヤープレフスを使って端末にも保存する(今後、ベストスコアを出すたびにこの名前でデータベースを参照するため）
            PlayerPrefs.SetString("PlayerName", name);

            //プレイヤーネームと同時にベストスコア０を仮登録しておく(これを参照して、スコアをデータベース登録するか判定）
            PlayerPrefs.SetInt("BestScore", 0);

            //プレイヤーネームと同時にレベル１を仮登録しておく
            PlayerPrefs.SetInt("Level", 1);
        });
    }




    //スコアが確定した時点でゲームマネージャーからコールされる(ベストスコア時のみ）
    void SetScore(int score)
    {
        var docRef = Firebase.Firestore.FirebaseFirestore.DefaultInstance.Collection("AllPlayerInfo").Document(PlayerPrefs.GetString("PlayerName"));
        var Ranking = new System.Collections.Generic.Dictionary<string, object>
        {
            { "Score",score}
            
        };

        docRef.SetAsync(Ranking).ContinueWithOnMainThread(task =>
        {
            //データベースにスコアが登録されたら、端末にもベストスコアを登録
            //PlayerPrefs.SetInt("BestScore", score);
        });


    }


    //自分のスコアをデータベースから取得
    void GetScore()
    {
        var docRef = Firebase.Firestore.FirebaseFirestore.DefaultInstance.Collection("AllPlayerInfo").Document(PlayerPrefs.GetString("PlayerName"));

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                
                Dictionary<string, object> city = snapshot.ToDictionary();
                foreach (KeyValuePair<string, object> pair in city)
                {
                    nameMain.text = PlayerPrefs.GetString("PlayerName");
                    scoreMain.text = pair.Value + "";
                }
            }
            
        });

    }


    //トップ３のスコアと名前をデータベースから取得
    void GetRanking()
    {
        //リストの中身を初期化
        List<string> playerList_Rank = new List<string>();
        List<string> scoreList_Rank = new List<string>();

        //データベースからプレイヤーのスコアを降順に１０件取得
        var docRef = Firebase.Firestore.FirebaseFirestore.DefaultInstance.Collection("AllPlayerInfo");
        Query query = docRef.OrderByDescending("Score").Limit(3);

        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {

            //リストの０番目にnullを入れとく(リスト１番目に１位のスコアとIDを入れたい
            scoreList_Rank.Add(null);
            playerList_Rank.Add(null);

            //QuerySnapshot = 複数のドキュメントデータ
            QuerySnapshot querysnapshot = task.Result;

            //DocumentSnapshot = 単一のドキュメントデータ
            foreach (DocumentSnapshot documentSnapshot in querysnapshot.Documents)
            {
                //１位のプレイヤーから順にドキュメントIDをPlayerListに入れていく
                playerList_Rank.Add(documentSnapshot.Id);

                //取得した１０件をDictionaryへ変換
                Dictionary<string, object> score = documentSnapshot.ToDictionary();

                //１位から順にランキングスコアをScoreListに入れていく
                foreach (object Value in score.Values)
                {
                    //オブジェクト型でデータを落としてくるので、文字列型へ変換してリストイン
                    scoreList_Rank.Add(Value.ToString());
                }
            }



            //データベースから取得したランキング情報を転写
            for (int i = 1; i < 4; i++)
            {
                playerName_Rank[i].text = playerList_Rank[i];
                score_Rank[i].text = scoreList_Rank[i];


            }

        });

    }
    
}
