using UnityEngine;
using UnityEngine.Rendering.Universal;


[System.Serializable]
public class SignpostDecal
{
    public DecalProjector decalProjector;
    public Texture2D decalTexture;
}

[RequireComponent(typeof(BoxCollider), typeof(DecalProjector))]
public class Signpost : WorldObject
{
    [Space(10)]
    [SerializeField] private SignpostDecal postDecal = new SignpostDecal();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Material postMat = new Material(postDecal.decalProjector.material);
        postMat.SetTexture("Base_Map", postDecal.decalTexture);
        postDecal.decalProjector.material = postMat;
    }
}
