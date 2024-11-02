using System.Collections;
using UnityEngine;

public class ParticleEmitter : MonoBehaviour
{
    private GameObject vfxInstance;
    public ParticleData Data { get; private set; }
    private Coroutine playingCoroutine;

    public void Play()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
        }
        vfxInstance.GetComponent<ParticleSystem>().Play();
        playingCoroutine = StartCoroutine(WaitForEffectToEnd());
    }

    public void Initialize(ParticleData data)
    {
        Data = data;

            vfxInstance = Instantiate(Data.vfxPrefab,transform.position+ Data.particlePositionOffset, transform.rotation * Quaternion.Euler(Data.particleRotationOffset),transform);
    }

    IEnumerator WaitForEffectToEnd()
    {
        yield return new WaitWhile(() => vfxInstance.GetComponent<ParticleSystem>().isPlaying);
        ParticleManager.Instance.ReturnToPool(this);
    }

    public void Stop()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }
        vfxInstance.GetComponent<ParticleSystem>().Stop();
        ParticleManager.Instance.ReturnToPool(this);
    }
}
