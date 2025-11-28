using System.Collections;
using Gamekit2D;
using UnityEngine;

public class KaitoSpecial : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public GameObject bulletPrefab;
    public float fireRate = 2f;
    public float bulletSpeed = 8f;

    [Header("Bolitas Celestes")]
    public Color bulletColor = new Color(0.4f, 0.8f, 1f, 1f);
    public int bulletsPerShot = 3;
    public float spreadAngle = 45f;

    [Header("Referencias")]
    public Transform shootPoint;

    private float nextFireTime;
    private Transform player;
    private SpriteRenderer spriteRenderer;

    [Header("Comportamiento BT")]
    public float stoppingDistance = 3f;
    public float detectionRange = 8f;

    void Start()
    {
        // Buscar punto de disparo
        if (shootPoint == null)
        {
            shootPoint = transform.Find("ShootingPosition");
            if (shootPoint == null)
            {
                // Crear uno temporal si no existe
                GameObject shootObj = new GameObject("ShootPoint");
                shootPoint = shootObj.transform;
                shootPoint.SetParent(transform);
                shootPoint.localPosition = new Vector3(-0.25f, 0.4f, 0f);
            }
        }

        // Buscar jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Referencia al sprite para determinar dirección
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (bulletPrefab == null) return;

        // Disparar automáticamente cada X segundos
        if (Time.time >= nextFireTime)
        {
            ShootSpread();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void ShootSpread()
    {
        StartCoroutine(ShootSpreadRoutine());
    }

    IEnumerator ShootSpreadRoutine()
    {
        for (int i = 0; i < bulletsPerShot; i++)
        {
            ShootSingleBullet(i);
            yield return new WaitForSeconds(0.1f);
        }
    }

    void ShootSingleBullet(int bulletIndex)
    {
        // Calcular dirección base (según sprite o hacia jugador)
        Vector2 baseDirection = GetBaseDirection();

        // Calcular dispersión
        float angle = (bulletIndex - (bulletsPerShot - 1) / 2f) * spreadAngle;
        Vector2 shootDirection = Quaternion.Euler(0, 0, angle) * baseDirection;

        // Crear proyectil
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);

        // Configurar color celeste
        SetBulletColor(bullet);

        // Configurar movimiento - MÚLTIPLES MÉTODOS
        SetupBulletMovement(bullet, shootDirection);

        // Auto-destrucción después de tiempo
        Destroy(bullet, 3f);
    }

    Vector2 GetBaseDirection()
    {
        // Prioridad 1: Hacia el jugador si está cerca
        if (player != null && Vector2.Distance(transform.position, player.position) < 10f)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            // Solo disparar horizontalmente si el jugador está al mismo nivel
            if (Mathf.Abs(directionToPlayer.y) < 0.3f)
                return new Vector2(directionToPlayer.x, 0).normalized;
        }

        // Prioridad 2: Dirección según el sprite
        if (spriteRenderer != null && spriteRenderer.flipX)
            return Vector2.right; // Mirando hacia la derecha
        else
            return Vector2.left; // Mirando hacia la izquierda (por defecto)
    }

    void SetBulletColor(GameObject bullet)
    {
        SpriteRenderer bulletRenderer = bullet.GetComponent<SpriteRenderer>();
        if (bulletRenderer != null)
        {
            bulletRenderer.color = bulletColor;
        }

        // También intentar cambiar color en hijos por si el sprite está anidado
        SpriteRenderer[] childRenderers = bullet.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in childRenderers)
        {
            renderer.color = bulletColor;
        }
    }

    void SetupBulletMovement(GameObject bullet, Vector2 direction)
    {
        // MÉTODO 1: Usar Rigidbody2D (recomendado)
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
        else
        {
            // MÉTODO 2: Agregar Rigidbody2D si no existe
            rb = bullet.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Sin gravedad
            rb.linearVelocity = direction * bulletSpeed;
        }

        // MÉTODO 3: Rotar bullet hacia la dirección
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // MÉTODO 4: Usar componente personalizado como fallback
        BulletMovement customMover = bullet.GetComponent<BulletMovement>();
        if (customMover == null)
            customMover = bullet.AddComponent<BulletMovement>();

        customMover.direction = direction;
        customMover.speed = bulletSpeed;
    }

    // Método para probar fácilmente
    [ContextMenu("Probar Disparo")]
    public void TestShoot()
    {
        ShootSpread();
    }

    // Método para llamar desde otros scripts
    public void ForceShoot()
    {
        nextFireTime = Time.time;
        ShootSpread();
    }
}

// Componente simple para movimiento de balas
public class BulletMovement : MonoBehaviour
{
    public Vector2 direction = Vector2.left;
    public float speed = 8f;
    public float lifetime = 3f;

    private Rigidbody2D rb;
    private float spawnTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnTime = Time.time;

        // Configurar movimiento inicial
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
            rb.gravityScale = 0f;
        }
    }

    void Update()
    {
        // Movimiento por transform como fallback
        if (rb == null)
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }

        // Auto-destrucción
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Destruir al chocar
        if (!collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }

}
