using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWallHorizontal : MonoBehaviour
{
    private Vector3 _upperBound;
    private Vector3 _bottomBound;
    private Vector3 _dir;

    private float _speed;
    private bool _movingRestricted = false;

    public void SetUpWall(bool moveToLeft)
    {
        if (moveToLeft)
        {
            _upperBound = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
            _bottomBound = new Vector3(transform.localPosition.x - transform.localScale.x * 2, transform.localPosition.y, transform.localPosition.z);
        }
        else
        {
            _upperBound = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
            _bottomBound = new Vector3(transform.localPosition.x + transform.localScale.x * 2, transform.localPosition.y, transform.localPosition.z);
        }

        if (_upperBound.Equals(_bottomBound))
            _movingRestricted = true;

        _dir = _upperBound;
        _speed = 0.06f;
    }

    void Update()
    {
        if (!_movingRestricted)
        {
            if (transform.localPosition == _upperBound)
                _dir = _bottomBound;
            else if (transform.localPosition == _bottomBound)
                _dir = _upperBound;

            transform.localPosition = Vector3.MoveTowards(transform.localPosition, _dir, _speed * Time.deltaTime);
        }
    }
}