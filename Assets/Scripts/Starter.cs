using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Starter : MonoBehaviour
{
    [SerializeField] private PostProcessVolume postProcessVolume;
    
    
    
    void Start()
    {
        postProcessVolume.enabled = true;
    }
    
    
}
