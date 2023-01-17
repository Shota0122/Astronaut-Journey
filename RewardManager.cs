using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;



public class RewardManager : MonoBehaviour
{

    private RewardedAd rewardedAd;
    string adUnitId;
    bool rewarded = false;

    GameObject gameManager;

    public void Start()
    {

        gameManager = GameObject.Find("GameManager");

        

    }




    public void LoadRewardedAd()
    {


#if UNITY_ANDROID
            adUnitId = "ca-app-pub-8725738518650333/8500074725";
#elif UNITY_IPHONE
        adUnitId = "ca-app-pub-8725738518650333/5191385103";
#else
            adUnitId = "unexpected_platform";
#endif


        this.rewardedAd = new RewardedAd(adUnitId);


        // Called when an ad request has successfully loaded.
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;


        

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);
    }





    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        rewardedAd.Show();
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        if(rewarded) gameManager.SendMessage("RebornRewarded", SendMessageOptions.DontRequireReceiver);
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        rewarded = true;
        
    }


   
}
