using UnityEngine;

public class BallTrigger : MonoBehaviour
{
    private Player _parent;
    private AudioSource _audioSource;
    private LevelConstructor _levelConstructor;
    private GameManager _gameManager;

    public void Start()
    {
        _parent = transform.parent.GetComponent<Player>();
        _audioSource = GetComponent<AudioSource>();
        _levelConstructor = GameObject.Find("LevelConstructor").GetComponent<LevelConstructor>();
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    public void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Wall":
                Vector3 force = _parent.GetForceValue();
                if (Mathf.Abs(force.x) > 0.1f || Mathf.Abs(force.y) > 0.1f)
                {
                   //Debug.Log("Bump");
                   // _audioSource.Play();
                }
                break;

            case "Key":
                other.transform.parent.GetComponent<Collectable>().Collected();
                other.GetComponent<BoxCollider>().enabled = false;
                _levelConstructor.KeyCollected();
                break;

            case "Gate":
                _levelConstructor.UpdateCoinsCollected(20);
                _gameManager.FinishLevel();
                break;

            case "Hole":
                other.GetComponent<SphereCollider>().enabled = false;
                _parent.OnIntoHall(other.transform.localPosition, other.gameObject);
                _parent.GetComponent<Rigidbody>().velocity = Vector3.zero;
                _levelConstructor.UpdateCoinsCollected(-10);
                break;
        }
    }
}
