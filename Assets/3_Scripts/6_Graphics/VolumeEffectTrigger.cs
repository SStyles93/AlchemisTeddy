using UnityEngine;
using UnityEngine.Rendering;

public class VolumeEffectTrigger : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;
    [SerializeField] private VolumeProfile profileToApply;
    private VolumeProfile savedProfile;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            savedProfile = globalVolume.profile;
            globalVolume.profile = profileToApply;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            globalVolume.profile = savedProfile;
        }
    }
}
