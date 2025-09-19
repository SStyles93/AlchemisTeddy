using UnityEngine;

public class ChestAnimator : MonoBehaviour
{
    [SerializeField] Animator animator;

    //Hashes of the animation parameters
    int isOpenHash = 0;

    private void OnEnable()
    {
        TreasureChest.OnChestChangeState += OpenChest;
    }

    private void OnDisable()
    {
        TreasureChest.OnChestChangeState -= OpenChest;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isOpenHash = Animator.StringToHash("isOpen");
    }

    public void OpenChest(bool value)
    {
        animator.SetBool(isOpenHash, value);
    }
}
