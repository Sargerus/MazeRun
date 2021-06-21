using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseBtnPanel : MonoBehaviour
{
    private const float _fadeTime = 1.5f;
    private float _timer = 0.0f;

   //private void Start()
   //{
   //    gameObject.SetActive(false);
   //}

    private void OnEnable()
    {
        ResetFadeTimer();
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        float pausetime = 0.5f;
        while(_timer < _fadeTime)
        {
            yield return new WaitForSeconds(pausetime);
            _timer += pausetime;
        }

        gameObject.SetActive(false);
    }

    public void ResetFadeTimer()
    {
        _timer = 0.0f;
    }
}
