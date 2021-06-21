using UnityEngine;

public class WallsContainer : MonoBehaviour
{
    private const int _ANGLEMAX = 11;
    private Vector3 _tilt, _currentUpdateTilt;

    [SerializeField]
    private float _speed;


    // Start is called before the first frame update
    void Start()
    {
        _currentUpdateTilt = new Vector3();
        _speed = 5.0f;
    }

    void Update()
    {
        //float buf;
        //_tilt = Input.acceleration;
        //_tilt.z = 0;
        //buf = _tilt.y * _ANGLEMAX;
        //
        //_tilt.y = _tilt.x * (-_ANGLEMAX);
        //_tilt.x = buf;
        //if(_tilt.y != _currentUpdateTilt.y)
        //{
        //    _tilt.y -= _currentUpdateTilt.y;
        //    _currentUpdateTilt.y += (_tilt.y *= _speed * Time.deltaTime);
        //}
        //
        //if (_tilt.x != _currentUpdateTilt.x)
        //{
        //    _tilt.x -= _currentUpdateTilt.x;
        //    _currentUpdateTilt.x += (_tilt.x *= _speed * Time.deltaTime);
        //}
        //
        //foreach (Transform children in transform)
        //{
        //    if (children.localEulerAngles.z != 0)
        //        _tilt.z = -children.localEulerAngles.z;
        //    children.Rotate(_tilt);
        //
        //    _tilt.z = 0;
        //}
            
    }
}
