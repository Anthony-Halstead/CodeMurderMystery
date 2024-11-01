using System.Collections;
using UnityEngine;

public class ParticleEmitter : MonoBehaviour
{
    public ParticleData Data { get; private set; }
    Coroutine playingCoroutine;

    public void Play()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
        }
        Data.vfx.Play();
        playingCoroutine = StartCoroutine(WaitForEffectToEnd());
    }

    IEnumerator WaitForEffectToEnd()
    {
        yield return new WaitWhile(() => Data.vfx.isPlaying);
        ParticleManager.Instance.ReturnToPool(this);
    }

    public void Stop()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }

        Data.vfx.Stop();
        ParticleManager.Instance.ReturnToPool(this);
    }

    public void Initialize(ParticleData data)
    {
        Data = data;
    }

}
