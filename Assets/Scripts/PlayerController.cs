using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    private Rigidbody playerRb;
    private GameObject focalPoint;
    public bool hasPowerup = false;
    public float powerUpStrength;
    public GameObject powerupIndicator;

    public PowerUpType currentPowerUp = PowerUpType.None;
    public GameObject rocketPrefab;
    private GameObject tmpRocket;
    private Coroutine powerUpCountdown;

    public float smashSpeed;
    public float hangTime;
    public float explosionForce;
    public float explosionRadius;
    public bool smashing = false;
    private float floorY;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        focalPoint = GameObject.Find("Focal Point");
    }

    // Update is called once per frame
    void Update()
    {
        float forwardInput = Input.GetAxis("Vertical");

        playerRb.AddForce(focalPoint.transform.forward * speed * forwardInput);

        powerupIndicator.transform.position = transform.position + new Vector3(0, -0.5f, 0);

        if (currentPowerUp == PowerUpType.Rockets && Input.GetKeyDown(KeyCode.F))
        {
            LaunchRockets();
        }

        if (currentPowerUp == PowerUpType.Smash && Input.GetKeyDown(KeyCode.Space))
        {
            smashing = true;
            StartCoroutine(Smash());
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            hasPowerup = true;
            currentPowerUp = other.gameObject.GetComponent<PowerUp>().powerUpType;
            powerupIndicator.gameObject.SetActive(true);
            Destroy(other.gameObject);
            //StartCoroutine(PowerupCountdownRoutine());

            if (powerUpCountdown != null)
            {
                StopCoroutine(powerUpCountdown);
            }
            powerUpCountdown = StartCoroutine(PowerupCountdownRoutine());

        }
    }

    IEnumerator PowerupCountdownRoutine()
    {
        yield return new WaitForSeconds(7);
        hasPowerup = false;
        currentPowerUp = PowerUpType.None;
        powerupIndicator.gameObject.SetActive(false);
    }

    IEnumerator Smash()
    {
        var enemies = FindObjectsOfType<Enemy>();

        floorY = transform.position.y;

        float jumpTime = Time.time + hangTime;

        while (Time.time < jumpTime) // going up
        {
            playerRb.velocity = new Vector2(playerRb.velocity.x, smashSpeed);
            yield return null;
        }

        while (transform.position.y > floorY) // coming down
        {
            playerRb.velocity = new Vector2(playerRb.velocity.x, -smashSpeed * 2);
            yield return null;
        }

        for (int i = 0; i < enemies.Length ; i++)
        {
            if (enemies[i] != null)
            {
                enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.0f, ForceMode.Impulse);
            }
        }
        smashing = false;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && currentPowerUp == PowerUpType.Pushback)
        {
            Rigidbody enemyRb = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = collision.transform.position - transform.position;

            enemyRb.AddForce(awayFromPlayer * powerUpStrength, ForceMode.Impulse);

            Debug.Log("Player collided with: " + collision.gameObject.name + " with powerup set to " + currentPowerUp.ToString());
        }
    }

    void LaunchRockets()
    {
        foreach (var enemy in FindObjectsOfType<Enemy>())
        {
            tmpRocket = Instantiate(rocketPrefab, transform.position + Vector3.up, Quaternion.identity);
            tmpRocket.GetComponent<RocketBehaviour>().Fire(enemy.transform);
        }
    }
}
