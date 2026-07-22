using System.Collections;
using UnityEngine;

public class WaterTankExplosionController : MonoBehaviour
{
    [SerializeField] private Transform waterVolume;
    [SerializeField] private ParticleSystem[] outletParticles;
    [SerializeField] private AudioSource floodAudioSource;
    [SerializeField] private AudioClip[] impactClips;
    [SerializeField] private Collider escapeTrigger;
    [SerializeField] private Collider damageVolume;
    [SerializeField] private PlayerMeltSystem playerMeltSystem;

    [SerializeField] private float riseSpeed = 1f;
    [SerializeField] private float maxHeight = 8f;
    [SerializeField] private float damagePerSecond = 10.0f;

    private bool isRunning;
    private bool isFinished;

    public void BeginFlood()
    {
        Debug.Log("홍수!!!");
        if (isRunning || isFinished) return;

        isRunning = true;
        StartCoroutine(RunFlood());
    }

    private IEnumerator RunFlood()
    {
        if (impactClips.Length > 0)
        {
            if (floodAudioSource != null)
            {
                floodAudioSource.PlayOneShot(impactClips[0]);
                yield return new WaitForSeconds(0.45f);
                floodAudioSource.PlayOneShot(impactClips[1]);
            }
        }

        foreach (var ps in outletParticles)
        {
            if (ps != null) ps.Play();
        }

        while (isRunning && !isFinished)
        {
            if (escapeTrigger != null && escapeTrigger.bounds.Contains(playerMeltSystem.transform.position))
            {
                break;
            }

            Vector3 pos = waterVolume.position;
            pos.y = Mathf.Min(pos.y + riseSpeed * Time.deltaTime, maxHeight);
            waterVolume.position = pos;

            if (damageVolume != null && playerMeltSystem != null)
            {
                if (damageVolume.bounds.Contains(playerMeltSystem.transform.position))
                {
                    playerMeltSystem.Damage(damagePerSecond * Time.deltaTime);
                }
            }

            if (pos.y >= maxHeight)
            {
                break;
            }

            yield return null;
        }

        foreach (var ps in outletParticles)
        {
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (floodAudioSource != null)
        {
            floodAudioSource.Stop();
        }

        isFinished = true;
        isRunning = false;
    }
}