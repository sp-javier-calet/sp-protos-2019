using UnityEngine;
using System.Collections;

public class ModelClickPoping : MonoBehaviour
{

    private Material _mat;
    private float _currentOverbrigth;
    public float _popOverbright = 2f;
    public float _ovebrightSpeed = 0.08f;
    private float _originalOverbright;

    private Vector3 _currentSize;
    public Vector3 _popSize = new Vector3(1.1f, 2.6f, 1.1f);
    public float SizeSpeed = 14f;
    private Vector3 _originalSize;

    private Vector3 _bigSize;
    private float _sizeDiference;
    private float _startTime;
    private Vector3 _targetSize;

    private int _shaderProperty;

    void Awake()
    {
        Renderer rend = gameObject.GetComponentInChildren<Renderer>();
        _mat = rend.material;
        _shaderProperty = Shader.PropertyToID("_Overbright");
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.size = rend.bounds.size;
        _originalOverbright = _mat.GetFloat(_shaderProperty);
        _originalSize = transform.localScale;
        _bigSize = new Vector3(_originalSize.x * _popSize.x, _originalSize.y * _popSize.y, _originalSize.z * _popSize.z);
        _sizeDiference = Vector3.Distance(_originalSize, _bigSize);
        _targetSize = _originalSize;
        _currentSize = _originalSize;

    }

    void OnMouseDown()
    {
        _currentOverbrigth = _originalOverbright * _popOverbright;
        _startTime = Time.time;
        _bigSize = new Vector3(_originalSize.x * _popSize.x, _originalSize.y * _popSize.y, _originalSize.z * _popSize.z);
        _sizeDiference = Vector3.Distance(_originalSize, _bigSize);
        _targetSize = _bigSize;
    }

    void Update()
    {
        if(_currentOverbrigth > _originalOverbright)
        {
            _mat.SetFloat(_shaderProperty, _currentOverbrigth);
            _currentOverbrigth = Mathf.Max(_currentOverbrigth - _ovebrightSpeed, _originalOverbright);
        }

        if(Vector3.Distance(_currentSize, _targetSize) != 0)
        {
            transform.localScale = _currentSize;

            float frame = (Time.time - _startTime) * SizeSpeed;
            float fraction = frame / _sizeDiference;
            _currentSize = Vector3.Lerp(_originalSize, _targetSize, fraction);
        }
        else if(_targetSize == _bigSize)
        {
            _targetSize = _originalSize;
            _startTime = Time.time;
        }
        else
        {
            transform.localScale = _originalSize;
        }
    }
}
