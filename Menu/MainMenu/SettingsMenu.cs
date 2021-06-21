using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{

    private Slider _slider;
    private AudioManager _audioManager;

    // Start is called before the first frame update
    void Start()
    {
        _audioManager = FindObjectOfType<AudioManager>();

        _slider = GameObject.Find("SoundVolumeSlider").GetComponent<Slider>();
        if (_slider)
        {
            float soundsVolume = PlayerPrefs.GetFloat(PlayerPrefsConst.SoundVolume);
            _slider.value = soundsVolume;
        }

        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnToggleValueChanged(bool newValue)
    {
        if (_slider) _slider.interactable = newValue;

        PlayerPrefs.SetInt(PlayerPrefsConst.SoundOn, System.Convert.ToInt32(newValue));

        if (_audioManager)
        {
            _audioManager.ChangeVolume();
            _audioManager.Play("button_press");
        }
    }

    public void OnSliderValueChanged(float newValue)
    {
        PlayerPrefs.SetFloat(PlayerPrefsConst.SoundVolume, newValue);
    }

    public void OnSliderEndDrag()
    {
        if (_audioManager)
        {
            _audioManager.ChangeVolume();
            _audioManager.Play("button_press");
        }
    }
}
