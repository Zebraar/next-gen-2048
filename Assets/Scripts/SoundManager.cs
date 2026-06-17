using UnityEngine;
using UnityEngine.Events;

public enum TypeOfSound
{
    def,
    cool,
    cooler,
    coolest
}
[System.Serializable]
public class SoundEvent : UnityEvent<TypeOfSound> { }
public class SoundManager : MonoBehaviour
{
    [Header("Sonds")]
    [SerializeField] private AudioClip[] bubbleSoundsDef;
    [SerializeField] private AudioClip[] bubbleSoundsCool;
    [SerializeField] private AudioClip[] bubbleSoundsCooler;
    [SerializeField] private AudioClip[] bubbleSoundsCoolest;
    [SerializeField] private AudioClip winSound;

    [Header("Other")]
    [SerializeField] private AudioSource audioSource;
    public void PlayBubbleSoundStatic(int typeIndex)
    {
        PlayBubbleSound((TypeOfSound)typeIndex);
    }

    public void PlayBubbleSound(TypeOfSound type)
    {
        if(type == TypeOfSound.def)
        {
            int koof = UnityEngine.Random.Range(0, bubbleSoundsDef.Length);
            audioSource.PlayOneShot(bubbleSoundsDef[koof]);
        } else if(type == TypeOfSound.cool)
        {
            int koof = UnityEngine.Random.Range(0, bubbleSoundsCool.Length);
            audioSource.PlayOneShot(bubbleSoundsCool[koof]);
        } else if(type == TypeOfSound.cooler)
        {
            int koof = UnityEngine.Random.Range(0, bubbleSoundsCooler.Length);
            audioSource.PlayOneShot(bubbleSoundsCooler[koof]);
        } else if(type == TypeOfSound.coolest)
        {
            int koof = UnityEngine.Random.Range(0, bubbleSoundsCoolest.Length);
            audioSource.PlayOneShot(bubbleSoundsCoolest[koof]);
        }
    }
    public void PlayWinSound()
    {
        audioSource.PlayOneShot(winSound);
    }
}
