using UnityEngine;

public class Collectable : MonoBehaviour
{
    private Vector3 _upperBound;
    private Vector3 _bottomBound;
    private Vector3 _dir;
    private bool _collected;
    private Renderer[] _materials;
    private float _originalDistance;

    private float _speed;
    private bool _movingRestricted = false;

    void Start()
    {
        _materials = GetComponentsInChildren<Renderer>(); 
        var bounds = GetComponent<MeshRenderer>().bounds;
        _upperBound = new Vector3(transform.localPosition.x, transform.localPosition.y + bounds.extents.y / 2, transform.localPosition.z);
        _bottomBound = new Vector3(transform.localPosition.x, transform.localPosition.y - bounds.extents.y / 2, transform.localPosition.z);
        transform.localPosition = new Vector3(transform.localPosition.x, Random.Range(_bottomBound.y, _upperBound.y), transform.localPosition.z);

        if (_upperBound.Equals(_bottomBound))
            _movingRestricted = true;

        _dir = _upperBound;
        _speed = 0.1f;
    }

    void Update()
    {
        if (!_movingRestricted && !_collected)
        {
            if (transform.localPosition == _upperBound)
                _dir = _bottomBound;
            else if (transform.localPosition == _bottomBound)
                _dir = _upperBound;

            transform.localPosition = Vector3.MoveTowards(transform.localPosition, _dir, _speed * Time.deltaTime);
        }
    }

    public void Collected()
    {
        _collected = true;
        var bounds = GetComponent<MeshRenderer>().bounds;
        _dir = new Vector3(transform.localPosition.x, (_upperBound.y + bounds.extents.y / 2) + 0.2f, transform.localPosition.z);
        _speed *= 7f;
        _originalDistance = _dir.y - transform.localPosition.y;
    }

    private void LateUpdate()
    {
        if (!_collected) return;

        if (transform.localPosition == _upperBound) Destroy(gameObject);
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, _dir, _speed * Time.deltaTime);
        foreach (var renderer in _materials)
            renderer.material.SetColor("_Color", new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, Mathf.Lerp(0, 1, (_dir.y - transform.localPosition.y) / _originalDistance)));
    }


}
