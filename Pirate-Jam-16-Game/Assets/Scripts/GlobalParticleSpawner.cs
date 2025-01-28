using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalParticleSpawner : MonoBehaviour
{
    public static GlobalParticleSpawner instance;
    [SerializeField] private ParticleSystem explosionParticlePrefab;
    [SerializeField] private AudioClip explosionNoise;
    [SerializeField] private AudioClip chipDamageNoise;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public void PlayParticleAtLocation(Transform location, int typeOfParticle)
    {
        ParticleSystem particleToSpawn = null;

        switch (typeOfParticle)
        {
            case 1: //final kill feedback
                particleToSpawn = explosionParticlePrefab;
                break;
            case 2: //chip damage feedback
                particleToSpawn = explosionParticlePrefab;
                break;
            default:
                Debug.Log("something went wrong in the global particle spawner method! Check the typeOfParticle parameter you passed in!");
                break;
        }

        if (particleToSpawn != null)
        {
            ParticleSystem spawnedParticle = Instantiate(particleToSpawn, location.position, Quaternion.identity);
            switch (typeOfParticle)
            {
                case 1:
                    AudioManager.instance.PlaySoundAtLocation(location, explosionNoise);
                    break;
                case 2:
                    AudioManager.instance.PlaySoundAtLocation(location, chipDamageNoise);
                    break;
                
            }
            spawnedParticle.Play();
        }
    }

    public void PlayParticleAtLocationWithCustomRotation(Transform location, int typeOfParticle, Quaternion customRotation)
    {
        ParticleSystem particleToSpawn = null;

        switch (typeOfParticle)
        {
            case 1:
                particleToSpawn = explosionParticlePrefab;
                break;
            default:
                Debug.Log("Something went wrong in the global particle spawner method! Check the typeOfParticle parameter you passed in!");
                break;
        }

        if (particleToSpawn != null)
        {
            ParticleSystem spawnedParticle = Instantiate(particleToSpawn, location.position, customRotation);
            spawnedParticle.Play();
        }
    }
}
