using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour {
    private static SoundEffectManager instance = null;
    private static AudioSource audioSource;
    private static SoundEffectLibrary soundEffectLibrary;
    [SerializeField] private Slider sfxSlider;
    private void Awake() {
        if (instance == null) {
            instance = this;
            audioSource = GetComponent<AudioSource>();
            soundEffectLibrary = GetComponent<SoundEffectLibrary>();
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        sfxSlider.onValueChanged.AddListener(delegate {OnValueChanged();});
    }

    public static void Play(string soundName) {
        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);
        audioSource.PlayOneShot(audioClip);
    }

    public static void SetVolume(float volume) {
        audioSource.volume = volume;
    }

    public void OnValueChanged() {
        SetVolume(sfxSlider.value);
    }
}
