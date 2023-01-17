using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using UnityEngine.SceneManagement;





public class InterstitialManager : MonoBehaviour
{

    private InterstitialAd interstitial;
    GameObject gameManager;

    
    void Start()
    {
        gameManager = GameObject.Find("GameManager");
        RequestInterstitial();
    }

    private void RequestInterstitial()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-8725738518650333/2126238060";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-8725738518650333/6696038469";
#else
        string adUnitId = "unexpected_platform";
#endif

        // Initialize an InterstitialAd.
        this.interstitial = new InterstitialAd(adUnitId);

        // Called when an ad request has successfully loaded.
        this.interstitial.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        this.interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        this.interstitial.OnAdOpening += HandleOnAdOpening;
        // Called when the ad is closed.
        this.interstitial.OnAdClosed += HandleOnAdClosed;


        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        this.interstitial.LoadAd(request);
    }



    public void ShowInterstitial ()
    {
        if (this.interstitial.IsLoaded())
        {
            this.interstitial.Show();
        }
        else
        {

            SelectScene();
        }
    }



    void SelectScene()
    {
        if (gameManager.GetComponent<GameManager>().pushHome)
        {
            SceneManager.LoadScene("Title");

        }
        else if (gameManager.GetComponent<GameManager>().pushReplay)
        {
            SceneManager.LoadScene("Game");
        }
    }



    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        
    }

    public void HandleOnAdOpening(object sender, EventArgs args)
    {
        
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        SelectScene();
    }



    
}
