using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioRandomizer : MonoBehaviour
{
    public float startDelay = 0;
    public float randomPith = 0.2f;
    public float randomVol = 0.2f;

    IEnumerator Start()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.Stop();

        yield return new WaitForSeconds(startDelay);

        audio.Play();
        audio.pitch = audio.pitch + Random.Range(-randomPith, randomPith);
        audio.volume = audio.volume + Random.Range(-randomVol, randomVol);
    }

}
