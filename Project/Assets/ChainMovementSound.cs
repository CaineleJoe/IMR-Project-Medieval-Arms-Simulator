using System.Collections;
using UnityEngine;
using Random = System.Random;

public class ChainSwingSound : MonoBehaviour
{
    [SerializeField] private AudioSource SoundSource;
    [SerializeField] private AudioClip[] ChainSounds; 
    [SerializeField] private float velocityThreshold = 2.0f;

    private Random rand = new Random();
    private bool isPlaying = false;
    private Rigidbody rb;

    void Start()
    {
        if (SoundSource == null)
            SoundSource = GetComponentInChildren<AudioSource>();

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Nu rigidbody for buzdugan!");
        }
    }

    void Update()
    {
        if (rb == null) return;

        float speed = rb.velocity.magnitude;

        if (speed >= velocityThreshold && !isPlaying)
        {
            PlayChainSound();
        }
    }

    private void PlayChainSound()
    {
        var clip = ChainSounds[rand.Next(0, ChainSounds.Length)];
        SoundSource.clip = clip;
        SoundSource.Play();

        isPlaying = true;
        StartCoroutine(WaitForClipToEnd(clip.length));
    }

    private IEnumerator WaitForClipToEnd(float duration)
    {
        yield return new WaitForSeconds(duration);
        isPlaying = false;
    }
}
