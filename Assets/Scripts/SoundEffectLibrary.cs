using System.Collections.Generic;
using UnityEngine;

public class SoundEffectLibrary : MonoBehaviour {
    [SerializeField] private SoundEffectGroup[] soundEffectGroups;
    private Dictionary<string, List<AudioClip>> soundDictionary;
    void Awake() {
        InitializeDictionary();
    }

    private void InitializeDictionary() {
        soundDictionary = new Dictionary<string, List<AudioClip>>();
        foreach (var group in soundEffectGroups) {
            soundDictionary[group.name] = group.audioClips;
        }
    }

    public AudioClip GetRandomClip(string name) {
        List<AudioClip> audioClips = soundDictionary[name];
        return audioClips[Random.Range(0, audioClips.Count)];
    }
}

[System.Serializable]
public struct SoundEffectGroup {
    public string name;
    public List<AudioClip> audioClips;
}