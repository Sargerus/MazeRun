using UnityEngine;
using GoogleMobileAds.Api;
using System.Collections.Generic;
using System;
using GoogleMobileAds.Common;
using System.Collections;

public class AdManager : MonoBehaviour
{
    private BannerView _bannerView;
    private InterstitialAd _interstitial;

    private void Awake()
    {
        List<String> deviceIds = new List<String>() { AdRequest.TestDeviceSimulator };
        RequestConfiguration requestConfiguration =
            new RequestConfiguration.Builder()
            .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified).build();
            //.SetTestDeviceIds(deviceIds).build();

        MobileAds.SetRequestConfiguration(requestConfiguration);
        MobileAds.Initialize(HandleInitCompleteAction);
        DontDestroyOnLoad(gameObject);

        StartCoroutine(WaitSec());
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator WaitSec()
    {
        yield return new WaitForSeconds(3.0f);
    }

    private void HandleInitCompleteAction(InitializationStatus initstatus)
    {
        MobileAdsEventExecutor.ExecuteInUpdate(() => {
            RequestBanner();
            RequestAndLoadInterstitial();
        });
    }

    private void RequestAndLoadInterstitial()
    {
        if (_interstitial != null)
        {
            _interstitial.Destroy();
        }

        string adUnitId = "/6499/example/interstitial"; 
        _interstitial = new InterstitialAd(adUnitId);
        _interstitial.OnAdClosed += OnInterstitialAddClosed;
        _interstitial.LoadAd(CreateAdRequest());

    }

    private void OnInterstitialAddClosed(object sender, EventArgs e)
    {
        RequestAndLoadInterstitial();
    }

    private void RequestBanner()
    {
        string adUnitId = "/6499/example/banner"; 
        if (_bannerView != null)
        {
            _bannerView.Destroy();
        }

        AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        _bannerView = new BannerView(adUnitId, adaptiveSize, AdPosition.Bottom);
        _bannerView.LoadAd(CreateAdRequest());
        HideBanner();
    }

    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder()
            .AddTestDevice(AdRequest.TestDeviceSimulator)
            .TagForChildDirectedTreatment(false)
            .Build();
    }

    public bool ShowBanner()
    {
        bool value = false;
        if (_bannerView != null)
        {
            _bannerView.Show();
            value = false;
        }
        else value = true;

        return value;
    }

    public void HideBanner()
    {
        _bannerView.Hide();
    }

    public void ShowInterstitial()
    {
        if (_interstitial.IsLoaded())
        {
            _interstitial.Show();
        }
        else
        {
            Debug.Log("Interstitial is not ready yet");
        }
    }
}
