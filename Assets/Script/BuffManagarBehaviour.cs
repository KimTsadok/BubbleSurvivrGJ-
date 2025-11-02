using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BuffManagarBehaviour : MonoBehaviour
{
    public static BuffManagarBehaviour Instance;

    [Header("Buff Prefabs")]
    public GameObject BuffGem3Prefab;
    public GameObject BuffGem4Prefab;
    public GameObject BuffGem7Prefab;
    public GameObject BuffGem10Prefab;

    [Header("Buff Timer Prefabs")]
    public GameObject HealBuffTimerPrefab;
    public GameObject RocksBuffTimerPrefab;
    public GameObject SlowdownBuffTimerPrefab;
    public GameObject SpeedBuffTimerPrefab;
    public GameObject BlessBuffTimerPrefab;
    public GameObject CurseBuffTimerPrefab;

    [Header("Particles")]
    public ParticleSystem HealParticle;
    public ParticleSystem SlowDownParticle;
    public ParticleSystem SpeedParticle;
    public ParticleSystem CurseParticle;

    [Header("References")]
    public GameObject RocksBuff;
    public PlayerBehaviourScript PlayerBehaviour;
    public WeaponThrower WeaponBehaviour;
    public Transform contentTransform;

    [Header("Buff Timing Control")]
    [Tooltip("שלוט על מהירות הופעת הבאפים (1 = רגיל, 0.5 = כפול מהירות, 2 = חצי מהירות)")]
    public float BuffSpawnSpeed = 1f;

    private BuffObject currentBuffObject;
    private BuffType currentBuffType;
    private BuffType previousBuffType;

    float minX = -330f;
    float maxX = 350f;
    float minY = -200f;
    float maxY = 195f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (HealParticle != null) HealParticle.gameObject.SetActive(true); HealParticle.Stop();
        if (SlowDownParticle != null) SlowDownParticle.gameObject.SetActive(true); SlowDownParticle.Stop();
        if (SpeedParticle != null) SpeedParticle.gameObject.SetActive(true); SpeedParticle.Stop();
        if (CurseParticle != null) CurseParticle.gameObject.SetActive(true); CurseParticle.Stop();

        previousBuffType = (BuffType)(-1);
    }

    void Start()
    {
        // זמן ההמתנה ההתחלתי תלוי במהירות
        float initialDelay = 20f / BuffSpawnSpeed;
        Invoke("StartRandomBuffs", initialDelay);
    }

    void StartRandomBuffs()
    {
        // טווח הזמן בין הופעות הבאפים תלוי במהירות
        float minInterval = 8f / BuffSpawnSpeed;
        float maxInterval = 18f / BuffSpawnSpeed;
        float randomInterval = Random.Range(minInterval, maxInterval);

        InvokeRepeating("SpawnRandomBuff", 0f, randomInterval);
    }

    void SpawnRandomBuff()
    {
        if (PlayerBehaviour != null && PlayerBehaviour.isGameOver) return;

        currentBuffObject = GetRandomBuffObject();
        currentBuffType = GetRandomBuffType();

        Vector3 randomPosition = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            0f
        );

        GameObject buffObject = Instantiate(GetBuffPrefab(currentBuffObject), randomPosition, Quaternion.identity);
        BuffsBehaviour buffObjectTrigger = buffObject.GetComponent<BuffsBehaviour>();
        buffObjectTrigger.buffType = currentBuffType;

        Destroy(buffObject, 5f);
    }

    GameObject GetBuffPrefab(BuffObject buffObject)
    {
        switch (buffObject)
        {
            case BuffObject.Gem3: return BuffGem3Prefab;
            case BuffObject.Gem4: return BuffGem4Prefab;
            case BuffObject.Gem7: return BuffGem7Prefab;
            case BuffObject.Gem10: return BuffGem10Prefab;
            default: return null;
        }
    }

    BuffObject GetRandomBuffObject()
    {
        return (BuffObject)Random.Range(0, System.Enum.GetValues(typeof(BuffObject)).Length);
    }

    BuffType GetRandomBuffType()
    {
        int attempts = 0;
        BuffType randomBuffType;

        do
        {
            randomBuffType = (BuffType)Random.Range(0, System.Enum.GetValues(typeof(BuffType)).Length);
            attempts++;
            if (attempts > 20)
                break;
        } while (randomBuffType == previousBuffType);

        previousBuffType = randomBuffType;
        return randomBuffType;
    }

    public void ActivateBuff(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.DecreaseSpeed:
                PlayerBehaviour.SetSpeed(-30f);
                CreateBuffTimer(SlowdownBuffTimerPrefab);
                StartParticleEffect(SlowDownParticle, 10f);
                StartCoroutine(RevertSpeedAfterDelay(10f, -30f));
                break;

            case BuffType.IncreaseSpeed:
                PlayerBehaviour.SetSpeed(30f);
                CreateBuffTimer(SpeedBuffTimerPrefab);
                StartParticleEffect(SpeedParticle, 10f);
                StartCoroutine(RevertSpeedAfterDelay(10f, 30f));
                break;

            case BuffType.IncreaseHealth:
                PlayerBehaviour.IncreasingHealth(30f, 10f);
                CreateBuffTimer(HealBuffTimerPrefab);
                StartParticleEffect(HealParticle, 10f);
                break;

            case BuffType.ActivateRocks:
                RocksBuff.SetActive(true);
                CreateBuffTimer(RocksBuffTimerPrefab);
                StartCoroutine(DeactivateRocksAfterDelay(10f));
                break;

            case BuffType.Bless:
                WeaponBehaviour.SetIsBless(true);
                CreateBuffTimer(BlessBuffTimerPrefab);
                StartCoroutine(RemoveBlessAfterDelay(10f));
                break;

            case BuffType.Curse:
                PlayerBehaviour.setIsCurse(true);
                CreateBuffTimer(CurseBuffTimerPrefab);
                StartParticleEffect(CurseParticle, 10f);
                StartCoroutine(RemoveCurseAfterDelay(10f, false));
                break;
        }
    }

    void StartParticleEffect(ParticleSystem particleSystem, float duration)
    {
        if (particleSystem != null)
        {
            particleSystem.Play();
            StartCoroutine(StopParticleEffectAfterDelay(particleSystem, duration));
        }
    }

    IEnumerator StopParticleEffectAfterDelay(ParticleSystem particleSystem, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (particleSystem != null)
            particleSystem.Stop();
    }

    IEnumerator RemoveBlessAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        WeaponBehaviour.SetIsBless(false);
        WeaponBehaviour.SetWeaponCount(6);
    }

    IEnumerator RemoveCurseAfterDelay(float delay, bool curse)
    {
        yield return new WaitForSeconds(delay);
        PlayerBehaviour.setIsCurse(curse);
    }

    IEnumerator RevertSpeedAfterDelay(float delay, float speedChange)
    {
        yield return new WaitForSeconds(delay);
        PlayerBehaviour.SetSpeed(-speedChange);
    }

    IEnumerator DeactivateRocksAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RocksBuff.SetActive(false);
    }

    void CreateBuffTimer(GameObject buffTimerPrefab)
    {
        if (buffTimerPrefab == null || contentTransform == null)
            return;

        GameObject newBuffTimer = Instantiate(buffTimerPrefab, contentTransform);
        newBuffTimer.transform.localScale = Vector3.one;

        Image bubbleTimerImage = newBuffTimer.transform.Find("BubbleTimer_IM")?.GetComponent<Image>();
        if (bubbleTimerImage != null && bubbleTimerImage.type == Image.Type.Filled)
        {
            bubbleTimerImage.fillAmount = 0f;
            StartCoroutine(FillImageOverTime(bubbleTimerImage, 10f));
        }

        Destroy(newBuffTimer, 10f);
    }

    IEnumerator FillImageOverTime(Image image, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            image.fillAmount = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
    }

    public void ResetAllBuffs()
    {
        // 1️⃣ עצור את כל הקורוטינות (כמו טיימרים של באפים)
        StopAllCoroutines();

        // 2️⃣ כבה את כל חלקיקי ה־Buffs
        if (HealParticle != null) HealParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (SlowDownParticle != null) SlowDownParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (SpeedParticle != null) SpeedParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (CurseParticle != null) CurseParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // 3️⃣ הסר את כל ה־BuffTimerPrefab מה־UI
        if (contentTransform != null)
        {
            foreach (Transform child in contentTransform)
            {
                Destroy(child.gameObject);
            }
        }

        // 4️⃣ נטרל מצבים פעילים
        if (RocksBuff != null) RocksBuff.SetActive(false);
        if (PlayerBehaviour != null)
        {
            PlayerBehaviour.SetSpeed(0); // החזר למהירות המקורית אם אתה רוצה
            PlayerBehaviour.setIsCurse(false);
        }
        if (WeaponBehaviour != null)
        {
            WeaponBehaviour.SetIsBless(false);
        }

        Debug.Log("✅ כל הבאפים אופסו, כל הפארטיקלס הופסקו והטיימרים הוסרו.");
    }
}
public enum BuffObject { Gem3, Gem4, Gem7, Gem10 }
public enum BuffType { ActivateRocks, Bless, Curse, IncreaseHealth, DecreaseSpeed, IncreaseSpeed }
