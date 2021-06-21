using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWallVertical : MonoBehaviour
{
    private Vector3 _upperBound;
    private Vector3 _bottomBound;
    private Vector3 _dir;
    private Vector3 _rotation;

    private float _speed;
    private bool _movingRestricted = false;

    void Start()
    {
        _upperBound = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
        _bottomBound = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + transform.localScale.y * 2);

        if (_upperBound.Equals(_bottomBound))
            _movingRestricted = true;

        _dir = _upperBound;
        _rotation = new Vector3(0, 0, 180);
        _speed = 0.5f;
    }

    void Update()
    {
        if (!_movingRestricted)
        {
            if (transform.localPosition == _upperBound)
            {
                _dir = _bottomBound;
                _rotation = new Vector3(0, 0, -180);
            }
            else if (transform.localPosition == _bottomBound)
            {
                _dir = _upperBound;
                _rotation = new Vector3(0, 0, 180);
            }
                

            transform.localPosition = Vector3.MoveTowards(transform.localPosition, _dir, _speed * Time.deltaTime);
        }
    }
}
