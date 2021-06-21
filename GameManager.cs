using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static PlayersData _playersData;
    private static int _levelsCount;

    public LevelConstructor _levelConstructor;
    private SerializeManager _serializeManager;
    private Player _player;
    private AdManager _adManager;
    private MenuManager _menuManager;

    //animate menu only in game
    //private GameObject _animateMenu;
    //private bool _isMenuAnimationOn;
    //private Vector3 _animateMenuStartPos;
    //private float extentY;


    private void Awake()
    {
        var serManager = GameObject.FindGameObjectWithTag("SerializeManager");
        if (serManager)
            _serializeManager = serManager.GetComponent<SerializeManager>();
        if(_serializeManager)
            _playersData = _serializeManager.LoadProgress();

        _menuManager = FindObjectOfType<MenuManager>();

        var asd = GameObject.Find("AdManager");
        if (asd)
        {
            _adManager = asd.GetComponent<AdManager>();
            if(_adManager)
                _adManager.HideBanner();
        }
        
        //_animateMenu = GameObject.FindGameObjectWithTag("AnimateMenu");
        //if (_animateMenu)
        //{
        //    _animateMenuStartPos = _animateMenu.transform.localPosition;
        //    if(_animateMenu)
        //        extentY = _animateMenu.GetComponent<RectTransform>().rect.height;
        //}
            
    }

    private void Start()
    {
        LoadGameObjects();
        
        if (_levelsCount == 2)
        {

            if (_adManager)
                _adManager.ShowInterstitial();

            _levelsCount = 0;
        }
    }

    public void IncrementLevelsCount(int i = 1) 
    {
        _levelsCount += i;
    }
    public void FinishLevel()
    {
        IncrementLevelsCount();
        _playersData.IncrementData(1, _levelConstructor.GetCoinsCollected());

        SceneManager.LoadScene("Game");
    }

    public void LoadGameObjects()
    {
        GameObject gameObject;

        //WallsContainer
        gameObject = new GameObject("WallsContainer");
        gameObject.transform.position = Vector3.zero;
        gameObject.AddComponent<WallsContainer>();
        gameObject.transform.tag = "WallsContainer";

        //FloorContainer
        gameObject = new GameObject("FloorContainer");
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.tag = "FloorContainer";

        //ObstacleContainer
        gameObject = new GameObject("MiscContainer");
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.tag = "MiscContainer";

        //LevelCostructor
        gameObject = new GameObject("LevelConstructor");
        gameObject.transform.position = Vector3.zero;
        _levelConstructor = gameObject.AddComponent<LevelConstructor>();
        _levelConstructor.BuildMaze(ALGORITHM.Eller, 10, 5, LevelMode.Standard, 3);

        _player = GameObject.FindGameObjectWithTag("Ball").GetComponent<Player>();
        _player.SetPlayerDefaultPosition(_player.transform.localPosition);
        _player.SetPlayerDefaultScale(_player.transform.localScale);
    }
}
