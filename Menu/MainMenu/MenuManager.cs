using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    private AdManager _adManager;
    private bool _checkForBanner;
    private GeneralMenu _generalMenu;
    private SettingsMenu _settingsMenu;
    private PauseMenu _pauseMenu;
    private InGameMenu _inGameMenu;
    private GameObject _rateBtn;
    private PauseBtnPanel _pauseBtnPanel;
    private AudioManager _audioManager;

    private void Awake()
    {
        _adManager = FindObjectOfType<AdManager>();
        _generalMenu = FindObjectOfType<GeneralMenu>();
        _settingsMenu = FindObjectOfType<SettingsMenu>();
        _pauseMenu = FindObjectOfType<PauseMenu>();
        _inGameMenu = FindObjectOfType<InGameMenu>();
        _rateBtn = GameObject.Find("RateBtn");
        _audioManager = gameObject.GetComponent<AudioManager>();

        //Check previous set settings
        if(PlayerPrefs.GetInt(PlayerPrefsConst.SoundOn, -1) == -1)
            PlayerPrefs.SetInt(PlayerPrefsConst.SoundOn, 1);
        
        if(PlayerPrefs.GetFloat(PlayerPrefsConst.SoundVolume, -1.0f) == - 1.0f)
            PlayerPrefs.SetFloat(PlayerPrefsConst.SoundVolume, 1.0f);
    }

    private void Start()
    {
        // we are in main menu at game startup
        if (_adManager && _generalMenu)
        {
            _checkForBanner = _adManager.ShowBanner();
        }

        //PlayersData dsa;
        //GameObject asd = GameObject.FindGameObjectWithTag("SerializeManager");
        //if(asd) dsa = GetComponent<SerializeManager>().LoadProgress();
        //Text textField = GameObject.Find("111").GetComponent<Text>();
        //if(asd != null)
        //    textField.text = asd._coinsTotal + " " + asd._levelPassed;
    }

    private void PlayButtonSound()
    {
        if (_audioManager)
            _audioManager.Play("button_press");
    }

    private void Update()
    {
        if (_checkForBanner && _adManager)
            _checkForBanner = _adManager.ShowBanner();

        if (_rateBtn)
            _rateBtn.GetComponentInChildren<Button>().transform.Rotate(new Vector3(0, 0, 1), 2f, Space.Self);

        if (!_pauseBtnPanel)
        {
            if ((_pauseBtnPanel = FindObjectOfType<PauseBtnPanel>()) != null)
                _pauseBtnPanel.gameObject.SetActive(false);
        }

        if (_pauseBtnPanel && Input.touchCount == 1)
        {
            if (!_pauseBtnPanel.gameObject.activeSelf)
                _pauseBtnPanel.gameObject.SetActive(true);
            else if (_pauseBtnPanel.gameObject.activeSelf)
                _pauseBtnPanel.ResetFadeTimer();
        }
    }

    public void LoadScene(string sceneName)
    {
        PlayButtonSound();

        Time.timeScale = 1;

        SceneManager.LoadSceneAsync(sceneName);
    }

    public void OpenSettigs()
    {
        PlayButtonSound();

        if (_generalMenu && _settingsMenu)
        {
            _generalMenu.gameObject.SetActive(false);
            _settingsMenu.gameObject.SetActive(true);
        }
    }

    public void OnRestartBtnClick()
    {
        PlayButtonSound();

        GameManager GM = FindObjectOfType<GameManager>();
        if (GM)
            GM.IncrementLevelsCount();

        Time.timeScale = 1;
        SceneManager.LoadSceneAsync("Game");
    }

    public void OnBackBtnClick()
    {
        PlayButtonSound();

        if (_settingsMenu)
            _settingsMenu.gameObject.SetActive(false);

        if (_generalMenu)
            _generalMenu.gameObject.SetActive(true);

        if(_audioManager)
            _audioManager.ChangeVolume();
    }

    public void OnPauseGameBtnClick()
    {
        //StartCoroutine(LoadMenuBackground());

        Time.timeScale = 0;

        PlayButtonSound();

        if (_inGameMenu)
            _inGameMenu.gameObject.SetActive(false);

        if (_pauseMenu)
            _pauseMenu.gameObject.SetActive(true);

        if (_adManager)
            _checkForBanner = _adManager.ShowBanner();
    }

    public void OnPlayBtnClick()
    {
        PlayButtonSound();

        if (_pauseMenu)
            _pauseMenu.gameObject.SetActive(false);

        if (_inGameMenu)
            _inGameMenu.gameObject.SetActive(true);

        if (_adManager)
            _adManager.HideBanner();

        Time.timeScale = 1;
    }

    ///////////////////////////// share button logic////////////////////////////
#if UNITY_ANDROID
    private bool isFocus = false;

    private string shareSubject, shareMessage;
    private bool isProcessing = false;
    private string screenshotName;

    void OnApplicationFocus(bool focus)
    {
        isFocus = focus;
    }

    public void OnShareButtonClick()
    {
        PlayButtonSound();

        screenshotName = "Share";
        shareSubject = "PenguinTeamDev";
        shareMessage = "Try out new cool maze-like game: \n" + "https://t.me/devilbyday";

        ShareScreenshot();
    }

    private void ShareScreenshot()
    {
        if (!isProcessing)
        {
            StartCoroutine(ShareScreenshotInAnroid());
        }
    }

    public IEnumerator ShareScreenshotInAnroid()
    {
        isProcessing = true;
        yield return new WaitForEndOfFrame();
        Texture2D screenTexture = new Texture2D(512, 512, TextureFormat.RGB24, true);
        screenTexture.Apply();
        byte[] dataToSave = Resources.Load<TextAsset>(screenshotName).bytes;
        string destination = Path.Combine(Application.persistentDataPath, System.DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".png");
        Debug.Log(destination);
        File.WriteAllBytes(destination, dataToSave);

        if (!Application.isEditor)
        {
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));

            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + destination);

            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
            intentObject.Call<AndroidJavaObject>("setType", "image/png");
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), shareSubject);
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareMessage);

            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share your high score");
            currentActivity.Call("startActivity", chooser);
        }

        yield return new WaitUntil(() => isFocus);
        isProcessing = false;
    }
#endif
}
