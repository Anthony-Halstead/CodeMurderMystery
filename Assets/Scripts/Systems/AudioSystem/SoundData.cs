using System;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class SoundData
{
  //Add generic strategies for how to go through array
    [Tooltip("The audio clip(s) that will be played.")]
    public AudioClip[] clips;

    [Tooltip("The Audio Mixer Group this sound will route to, allowing for grouped volume control and effects.")]
    public AudioMixerGroup mixerGroup;

    [Tooltip("Should the audio clip loop when it reaches the end?")]
    public bool loop;

    [Tooltip("Should the audio start playing automatically when the object awakes?")]
    public bool playOnAwake;

    [Tooltip("Indicates if this sound is played frequently, possibly affecting how it's managed in memory.")]
    public bool frequentSound;

    [Tooltip("Mutes the audio clip when enabled.")]
    public bool mute;

    [Tooltip("Bypasses audio effects applied directly to the AudioSource.")]
    public bool bypassEffects;

    [Tooltip("Bypasses audio effects applied at the AudioListener level.")]
    public bool bypassListenerEffects;

    [Tooltip("Ignores reverb zones in the scene for this AudioSource.")]
    public bool bypassReverbZones;

    [Tooltip("Sets the playback priority of the sound (0 is highest priority, 256 is lowest).")]
    [Range(0, 256)]
    public int priority = 128;

    [Tooltip("The volume level of the audio clip (range 0.0 to 1.0).")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("Adjusts the pitch (frequency) of the audio clip (-3.0 to 3.0, where 1.0 is normal pitch).")]
    [Range(-3f, 3f)]
    public float pitch = 1f;

    [Tooltip("Sets the left-right stereo pan of the sound (-1.0 is left, 1.0 is right).")]
    [Range(-1f, 1f)]
    public float panStereo;

    [Tooltip("Controls how much the AudioSource is affected by 3D spatialization (0.0 is 2D, 1.0 is fully 3D).")]
    [Range(0f, 1f)]
    public float spatialBlend;

    [Tooltip("Sets how much the audio is affected by reverb zones (0.0 is none, 1.1 is fully affected).")]
    [Range(0f, 1.1f)]
    public float reverbZoneMix = 1f;

    [Tooltip("Adjusts the amount of Doppler effect applied to the audio based on relative movement (0.0 disables the effect).")]
    [Range(0f, 5f)]
    public float dopplerLevel = 1f;

    [Tooltip("Controls the spread angle (in degrees) of a 3D stereo or multichannel sound in speaker space (0 to 360 degrees).")]
    [Range(0f, 360f)]
    public float spread;

    [Tooltip("The minimum distance at which the sound is heard at full volume.")]
    [Min(0f)]
    public float minDistance = 1f;

    [Tooltip("The distance beyond which the sound will no longer be heard.")]
    [Min(0f)]
    public float maxDistance = 500f;

    [Tooltip("If enabled, this AudioSource is unaffected by the AudioListener's volume setting.")]
    public bool ignoreListenerVolume;

    [Tooltip("If enabled, the AudioSource continues to play even when the AudioListener is paused.")]
    public bool ignoreListenerPause;

    [Tooltip("Defines how the volume of the audio decreases over distance (Logarithmic, Linear, or Custom).")]
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
}
