using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatusManager : MonoBehaviour
{
    [SerializeField] Image[] imgSpeedStars;
    [SerializeField] Image[] imgHealthStars;

    int speed;
    int health;


    //プレイヤーのスピードと体力を決定する(SkinManagerから呼ばれる）
    //キャラを追加する場合はここを追加する必要あり
    void StatusDecision(int selectCharacter)
    {
        switch (selectCharacter)
        {
            case 0:　//~Lv19
                speed = 1;
                health = 1;
                break;

            case 1:　//Lv20~39
                speed = 1;
                health = 2;
                break;

            case 2:　//Lv40~59
                speed = 2;
                health = 2;
                break;

            case 3:　//Lv60~79
                speed = 2;
                health = 3;
                break;

            case 4:　//Lv80~99
                speed = 3;
                health = 3;
                break;

            case 5: //Lv100
                speed = 4;
                health = 4;
                break;

            default:
                break;
        }

        //選んだキャラのスピードと体力を保存しておく
        PlayerPrefs.SetInt("Speed", speed);
        PlayerPrefs.SetInt("Health", health);

        ChangeStars();
    }


    //ステータスを示すスター数を変更する(星は最大５）
    void ChangeStars()
    {
        for(int i = 1; i < imgSpeedStars.Length; i++)
        {
            if(i < speed)
            {
                imgSpeedStars[i].GetComponent<Image>().enabled = true;
            }
            else
            {
                imgSpeedStars[i].GetComponent<Image>().enabled = false;
            }
        }

        for (int i = 1; i < imgHealthStars.Length; i++)
        {
            if (i < health)
            {
                imgHealthStars[i].GetComponent<Image>().enabled = true;
            }
            else
            {
                imgHealthStars[i].GetComponent<Image>().enabled = false;
            }
        }
    }





    
}
