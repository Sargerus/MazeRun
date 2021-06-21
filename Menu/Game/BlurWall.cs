using System.Collections;
using UnityEngine;

public class BlurWall : MonoBehaviour
{
    private const string ALPHAINSHADER = "_Alpha";
    private Renderer _renderer;
    private float _alpha;

    [SerializeField]
    float blur = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer)
            _alpha = _renderer.material.GetFloat(ALPHAINSHADER);

        //gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (!_renderer)
        {
            _renderer = GetComponent<Renderer>();
        }
            _renderer.material.SetFloat(ALPHAINSHADER, 0);
    
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        if (!_renderer) yield break;

        float localAlpha = 0;
        
        while(localAlpha < _alpha)
        {
            localAlpha += 0.001f;
            _renderer.material.SetFloat(ALPHAINSHADER, localAlpha);
            yield return new WaitForEndOfFrame();
        }
    }
}
