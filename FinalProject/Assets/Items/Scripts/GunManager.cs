using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

public class GunManager : MonoBehaviour {

    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float cooldown = 0.5f;
    public KeyCode fireKey = KeyCode.E;
    public float damage = 1;
    private float nextFire = 0f;

    void Update () {
        ProcessBulletSpawn();
    }
    
    private void ProcessBulletSpawn() {
        Quaternion rotationOffset = Quaternion.Euler(0, 0, 90);
        if ((Input.GetKey(fireKey)) && Time.time > nextFire) {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation * rotationOffset);
            bullet.GetComponent<Bullet>().setDamage(damage);
            Rigidbody2D re = bullet.GetComponent<Rigidbody2D>();
            re.linearVelocity = transform.right * bulletSpeed;
            nextFire = Time.time + cooldown;
        }
    }
}