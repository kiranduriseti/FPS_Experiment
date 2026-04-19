using UnityEngine;

public class projectile : MonoBehaviour
{
    public float speed = 15f;
    public float lifetime = 5f;
    public GameObject explosion;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += transform.forward * Time.deltaTime * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            return;

        EnemyController enemy = other.GetComponentInParent<EnemyController>();

        if (explosion != null)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}