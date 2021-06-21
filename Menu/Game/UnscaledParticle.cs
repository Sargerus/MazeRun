using UnityEngine;

public class UnscaledParticle : MonoBehaviour
{
    private ParticleSystem _particle;

    private void Start()
    {
        _particle = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if(gameObject.activeSelf && _particle && Time.timeScale < 1)
        {
            _particle.Simulate(Time.unscaledDeltaTime, true, false);
        }
    }
}
