using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerBehaviourScript : MonoBehaviour
{
    public GameObject player;
    public GameObject spear;
    Animator animator;
    public float speed = 5f; // Adjusted speed for smoother movement
    public float xMin = -370f;
    public float xMax = 355f;
    public float yMin = -196f;
    public float yMax = 175f;
    private Vector2 movement;
    private SpriteRenderer spriteRenderer;
    public float health = 100f;
    public float maxHealth = 100f;
    public Slider healthSlider;
    public Slider powerSlider;
    private Rigidbody2D rb;
    public GameObject endCanvas;
    public EndGameManagar EndGameManagar;
    int numOfStars = 3;
    public ParticleSystem SpeedParticle;
    bool isCurse = false;
    public DamageTextBehaviour damageTextBehaviour;
    public bool isGameOver = false;


    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        EndGameManagar = endCanvas.GetComponent<EndGameManagar>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }
    }

    void Update()
    {
        if (isGameOver)
        {
            movement = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            return; // לא לגעת באנימציה אם כבר מת
        }

        else
        {
            // Get player input
            float moveX = Input.GetAxis("Horizontal");
            float moveY = Input.GetAxis("Vertical");

            if (isCurse)
            {
                moveX *= (-1);
                moveY *= (-1);
            }

            // Calculate movement
            movement = new Vector2(moveX, moveY).normalized * speed; // Normalized to prevent diagonal speed boost

            // Animation and sprite flipping
            if (movement.magnitude > 0)
            {
                animator.SetInteger("State", 1);

                if (moveX < 0)
                {
                    spriteRenderer.flipX = true; // Face left
                    if (spear != null)
                        spear.transform.rotation = Quaternion.Euler(0, 180, 39);

                    // Set SpeedParticle position and rotation for left
                    if (SpeedParticle != null)
                    {
                        SpeedParticle.transform.localPosition = new Vector3(0.6f, -0.4f, 0);
                        SpeedParticle.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    }
                }
                else
                {
                    spriteRenderer.flipX = false; // Face right
                    if (spear != null)
                        spear.transform.rotation = Quaternion.Euler(0, 0, 39);

                    // Set SpeedParticle position and rotation for right
                    if (SpeedParticle != null)
                    {
                        SpeedParticle.transform.localPosition = new Vector3(-0.6f, -0.4f, 0);
                        SpeedParticle.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                }

                if (spear != null)
                {
                    spear.transform.localPosition = new Vector3(0.2f, -0.3f, 0); // Reset to parent's position
                }
            }
            else
            {
                animator.SetInteger("State", 0);
            }

            // Clamp the player's position within the boundaries
            float clampedX = Mathf.Clamp(transform.position.x, xMin, xMax);
            float clampedY = Mathf.Clamp(transform.position.y, yMin, yMax);
            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }
    }

    void FixedUpdate()
    {
        if (health <= 0 || isGameOver) return; // לא להזיז את השחקן
        rb.linearVelocity = movement;
    }

    /*
     private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Rock"))
        {
            // Stop the player's movement when colliding with a rock
            rb.velocity = Vector2.zero;
        }
    }
    */



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Rock"))
        {
            Vector2 pushDirection = (transform.position - collision.transform.position).normalized;
            rb.AddForce(pushDirection * 10f, ForceMode2D.Impulse); // Apply recoil
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (health <= 0 || isGameOver) return;
        if (health > 0)
        {
            if (collision.CompareTag("Bubble") || collision.CompareTag("Magenta"))
            {
                animator.SetInteger("State", 2);
                BubbleMovement bubbleBehavior = collision.GetComponent<BubbleMovement>();
                int damage = bubbleBehavior.damage;
                damageTextBehaviour.ShowDamage(damage);
                TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (healthSlider != null)
        {
            healthSlider.value = health;
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isGameOver = true;

        // הפעל את האנימציה של המוות
        animator.SetInteger("State", 3);
        if (BuffManagarBehaviour.Instance != null)
        {
            BuffManagarBehaviour.Instance.ResetAllBuffs();
        }
        // נטרל באפים אקטיביים
        isCurse = false;
        speed = 5f; // או הערך הבסיסי שלך
        StopAllCoroutines(); // <---- מבטל כל באף פעיל מיידית

        // שאר הקוד שלך:
        CalculateStars();
        healthSlider.gameObject.SetActive(false);
        powerSlider.gameObject.SetActive(false);
        rb.linearVelocity = Vector2.zero;
        if (spear != null)
            spear.gameObject.SetActive(false);

        endCanvas.gameObject.SetActive(true);
        EndGameManagar.SetEndText("Defeat");
        EndGameManagar.SetMedls(numOfStars);
        EndGameManagar.SetStars(numOfStars);

        if (PersistentObjectManager.Instance != null)
        {
            int stageIndex = SceneManager.GetActiveScene().buildIndex - 1;
            PersistentObjectManager.Instance.SaveStarRating(stageIndex, numOfStars);
        }
    }


    public void OnWin()
    {
        isGameOver = true;
        Debug.Log("Player has won the game!");
        CalculateStars();

        if (endCanvas != null)
        {
            endCanvas.gameObject.SetActive(true);
        }
        if (EndGameManagar != null)
        {
            EndGameManagar.SetEndText("Victory");
            EndGameManagar.SetMedls(numOfStars);
            EndGameManagar.SetStars(numOfStars);
        }

        // Save the highest stars earned for this stage
        if (PersistentObjectManager.Instance != null)
        {
            int stageIndex = SceneManager.GetActiveScene().buildIndex - 1; // Since Scene 1 is the level selection
            PersistentObjectManager.Instance.SaveStarRating(stageIndex, numOfStars);
        }
    }

    void CalculateStars()
    {
        float healthPercentage = (health / maxHealth) * 100;

        if (healthPercentage >= 75)
        {
            numOfStars = 3;
        }
        else if (healthPercentage >= 50)
        {
            numOfStars = 2;
        }
        else if (healthPercentage >= 25)
        {
            numOfStars = 1;
        }
        else
        {
            numOfStars = 0;
        }
    }


    public void IncreasingHealth(float healthIncrease, float duration)
    {
        StartCoroutine(GraduallyIncreaseHealth(healthIncrease, duration));
    }

    private IEnumerator GraduallyIncreaseHealth(float totalHealthIncrease, float duration)
    {
        float healthPerSecond = totalHealthIncrease / duration;
        float elapsed = 0f;

        while (elapsed < duration && health < maxHealth)
        {
            health += healthPerSecond * Time.deltaTime;
            if (health > maxHealth)
            {
                health = maxHealth; // Ensure it doesn't exceed max health
            }

            if (healthSlider != null)
            {
                healthSlider.value = health;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void SetSpeed(float deltaSpeed)
    {
        speed += deltaSpeed;
    }
    public void setIsCurse(bool curse)
    {
        isCurse = curse;
    }
}