using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private GameObject attack;

    public void SetCasting(bool casting)
    {
        attack.SetActive(casting);
    }
}
