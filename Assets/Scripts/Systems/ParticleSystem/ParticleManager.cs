using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityUtils;

public class ParticleManager : PersistentSingleton<ParticleManager>
{
    IObjectPool<ParticleEmitter> particleEmitterPool;
    readonly List<ParticleEmitter> activeParticleEmitters = new();

    public readonly Queue<ParticleEmitter> FrequentParticleEmitters = new();

    [Header("Settings")]
    [SerializeField] ParticleEmitter particleEmitterPrefab;
    [SerializeField] bool collectionCheck = true;
    [SerializeField] int defaultCapacity = 10;
    [SerializeField] int maxPoolSize = 100;
    [SerializeField] int maxParticleInstances = 30;

    void Start() { InitializePool(); }

    public ParticleBuilder CreateParticle() => new ParticleBuilder(this);
    public bool CanPlayParticle(ParticleData data)
    {
        if (!data.frequentParticle) return true;
        if (FrequentParticleEmitters.Count >= maxParticleInstances && FrequentParticleEmitters.TryDequeue(out var particleEmitter))
        {
            try
            {
                particleEmitter.Stop();
                return true;
            }
            catch
            {
                Debug.Log("paarticle emitter is already released");
            }
            return false;
        }
        return true;
    }

    public ParticleEmitter Get()
    {
        return particleEmitterPool.Get();
    }
    public void ReturnToPool(ParticleEmitter emitter) { particleEmitterPool.Release(emitter); }
    void OnDestroyPoolObject(ParticleEmitter emitter)
    {
        Destroy(particleEmitterPrefab.gameObject);
    }

    void OnReturnedToPool(ParticleEmitter particleEmitter)
    {
        particleEmitter.gameObject.SetActive(false);
        activeParticleEmitters.Remove(particleEmitter);
    }

    void OnTakeFromPool(ParticleEmitter particleEmitter)
    {
        particleEmitter.gameObject.SetActive(true);
        activeParticleEmitters.Add(particleEmitter);
    }

    ParticleEmitter CreateParticleEmitter()
    {
        var particleEmitter = Instantiate(particleEmitterPrefab);
        particleEmitter.gameObject.SetActive(false);
        return particleEmitter;
    }

    private void InitializePool()
    {
        particleEmitterPool = new ObjectPool<ParticleEmitter>(
              CreateParticleEmitter,
              OnTakeFromPool,
              OnReturnedToPool,
              OnDestroyPoolObject,
              collectionCheck,
              defaultCapacity,
              maxPoolSize
          );
    }

}
