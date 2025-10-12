using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


[System.Serializable]
public class SignpostDecal
{
    public DecalProjector decalProjector;
    public Texture2D decalTexture;
}

public class Signpost : MonoBehaviour
{
    [SerializeField] private List<SignpostDecal> postDecals = new List<SignpostDecal>();



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(var post in postDecals)
        {
            Material postMat = new Material(post.decalProjector.material);
            postMat.SetTexture("Base_Map", post.decalTexture);
            post.decalProjector.material = postMat;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
