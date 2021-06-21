using UnityEngine;

public class Player : MonoBehaviour
{
    private static int _coins;
    private static int _levels;

    private float _speed;
    private Rigidbody _rigid;
    private Material _material;
    Vector3 _tilt;
    private Vector3 _playerDefaultPosition;
    private Vector3 _playerDefaultScale;
    private Vector3 _holeCoord;
    private bool _playInHallAnim;
    private bool _playResurectionAnim;
    private float _inHallSpeed = 0.8f;
    private float _displaySpeed = 2.5f;
    private bool _movingRestricted = false;
    private GameObject _disabledTrap;

    // Start is called before the first frame update
    void Start()
    {
        _speed = 8.2f;
        _rigid = GetComponent<Rigidbody>();
        _playInHallAnim = false;
        _playResurectionAnim = false;
        _material = transform.GetComponent<Renderer>().material;
    }

    private void Update()
    {
        if (_movingRestricted) return;

        _tilt = Input.acceleration;
        _tilt.z = 0;
    }

    private void FixedUpdate()
    {
       if (Mathf.Abs(_rigid.velocity.x) >= 2.5f)
           _tilt.x *= 1.6f;
       
       if (Mathf.Abs(_rigid.velocity.y) >= 2.5f)
           _tilt.y *= 1.6f;
       
       if (Mathf.Abs(_rigid.velocity.z) >= 2.5f)
           _tilt.z *= 1.6f;
       
       
       _rigid.AddForce(_tilt * 1.2f * _speed * Time.fixedDeltaTime, ForceMode.VelocityChange);
        
        //if (_movingRestricted) return;
        //
        //_tilt = Input.acceleration;
        //_tilt.z = 0;
        //_rigid.AddForce(_tilt * _speed * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    public Vector3 GetForceValue() => _rigid.velocity;
    public void SetPlayerDefaultPosition(Vector3 pos)
    {
        _playerDefaultPosition = pos;
    }
    public void SetPlayerDefaultScale(Vector3 scale)
    {
        _playerDefaultScale = scale;
    }
    public void OnIntoHall(Vector3 holePos, GameObject trap)
    {
        _rigid.velocity = Vector3.zero;
        _playInHallAnim = true;
        _movingRestricted = true;
        _holeCoord = holePos;
        _disabledTrap = trap;
    }

    private void LateUpdate()
    {
        if (_playInHallAnim)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, _holeCoord, 0.1f);
            transform.localScale = new Vector3(transform.localScale.x - _inHallSpeed * Time.deltaTime, transform.localScale.y - _inHallSpeed * Time.deltaTime, transform.localScale.z - _inHallSpeed * Time.deltaTime);
            if (transform.localScale.x <= 0.1f)
            {
                _playInHallAnim = false;
                _playResurectionAnim = true;
                transform.localPosition = _playerDefaultPosition;
                //_material.SetColor("_Color", new Color(_material.color.r, _material.color.g, _material.color.b, 0));
                transform.localScale = _playerDefaultScale;
            }
        }

        if (_playResurectionAnim)
        {
            _playResurectionAnim = false;
            _movingRestricted = false;
            //    _material.SetColor("_Color", new Color(_material.color.r, _material.color.g, _material.color.b, _material.color.a + _displaySpeed * Time.deltaTime));
            //    if (_material.color.a >= 1)
            //    {
            //        _material.SetColor("_Color", new Color(_material.color.r, _material.color.g, _material.color.b, 1));
            //        _playResurectionAnim = false;
            //        _movingRestricted = false;
            //        _disabledTrap.GetComponent<SphereCollider>().enabled = true;
            //        _disabledTrap = null;
            //    }
        }
    }

    public static void AddCoins(int i)
    {
        _coins += i;
    }

    public static void AddLevels(int i)
    {
        _levels += i;
    }
    public static int GetCoins() => _coins;
    public static int GetLevels() => _levels;
}
