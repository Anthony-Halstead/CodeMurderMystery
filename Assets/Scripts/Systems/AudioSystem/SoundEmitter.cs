using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour {
    public SoundData Data {  get; private set; }    
   [SerializeField] AudioSource audioSource;
    Coroutine playingCoroutine;

    private void Awake()
    {
        if(audioSource == null)
        audioSource = gameObject.GetOrAddComponent<AudioSource>();
    }

    public void Play()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
        }
        audioSource.Play();
        playingCoroutine = StartCoroutine(WaitForSoundToEnd());
    }

    IEnumerator WaitForSoundToEnd()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        SoundManager.Instance.ReturnToPool(this);
    }

    public void Stop()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }

        audioSource.Stop();
        SoundManager.Instance.ReturnToPool(this);
    }

    public void Initialize(SoundData data)
    {
        Debug.Log("iNITIALIZING SOUND");
        Data = data;
        audioSource.clip = data.clips[UnityEngine.Random.Range(0,data.clips.Length)];
        audioSource.outputAudioMixerGroup = data.mixerGroup;
        audioSource.loop = data.loop;
        audioSource.playOnAwake = data.playOnAwake;

        audioSource.mute = data.mute;
        audioSource.bypassEffects = data.bypassEffects;
        audioSource.bypassListenerEffects = data.bypassListenerEffects;
        audioSource.bypassReverbZones = data.bypassReverbZones;

        audioSource.priority = data.priority;
        audioSource.volume = data.volume;
        audioSource.pitch = data.pitch;
        audioSource.panStereo = data.panStereo;
        audioSource.spatialBlend = data.spatialBlend;
        audioSource.reverbZoneMix = data.reverbZoneMix;
        audioSource.dopplerLevel = data.dopplerLevel;
        audioSource.spread = data.spread;

        audioSource.minDistance = data.minDistance;
        audioSource.maxDistance = data.maxDistance;

        audioSource.ignoreListenerVolume = data.ignoreListenerVolume;
        audioSource.ignoreListenerPause = data.ignoreListenerPause;

        audioSource.rolloffMode = data.rolloffMode;

    }

    public void WithRandomPitch(float min = -0.05f,float max = 0.05f)
    {
        audioSource.pitch += UnityEngine.Random.Range(min, max);
    }
}
