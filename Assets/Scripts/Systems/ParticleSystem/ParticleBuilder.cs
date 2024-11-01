using UnityEngine;
public class ParticleBuilder
{
    readonly ParticleManager particleManager;
    ParticleData particleData;
    Vector3 position = Vector3.zero;
    Transform parent = null;

    public ParticleBuilder(ParticleManager particleManager)
    {
        this.particleManager = particleManager;
    }
    public ParticleBuilder WithParticleData(ParticleData particleData)
    {
        this.particleData = particleData;
        return this;
    }
    public ParticleBuilder WithPosition(Vector3 position)
    {
        this.position = position;
        return this;
    }
    public ParticleBuilder WithParent(Transform parent) { 
        this.parent = parent;
        return this;
    }
    public void Play()
    {
        if (!particleManager.CanPlayParticle(particleData)) return;
        ParticleEmitter particleEmitter = particleManager.Get();
        particleEmitter.Initialize(particleData);
        particleEmitter.transform.position = position;
        particleEmitter.transform.parent = parent;
        if (particleData.frequentParticle)
        {
            particleManager.FrequentParticleEmitters.Enqueue(particleEmitter);
        }
        particleEmitter.Play();
    }
}
