using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BenelliM4Controller : MonoBehaviour
{
    [Header("Required References")]
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private Camera armsCamera;

    [Header("UI References")]
    [SerializeField] private TMP_Text ammoText;

    [Header("Weapon Sounds")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;

    [Header("Weapon Settings")]
    public int maxAmmo = 6;
    public int totalAmmo = 24;
    public float reloadTime = 2.7f;
    public float fireRate = 0.6f;

    [Header("Combat Settings")]
    public float damage = 25f;
    public float range = 100f;
    public LayerMask enemyLayer;
    public GameObject hitEffectPrefab;

    private AudioSource audioSource;
    private int currentAmmo;
    private bool isReloading;
    private bool isShooting;
    private float nextFireTime;
    private bool wasFiringBeforePause;
    private bool inputBlocked;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    void Update()
    {
        if (PauseMenuController.GameIsPaused)
        {
            wasFiringBeforePause = Input.GetButton("Fire1");
            inputBlocked = true;
            return;
        }

        if (inputBlocked)
        {
            // Блокируем ввод на один кадр после паузы
            inputBlocked = false;
            wasFiringBeforePause = false;
            return;
        }

        if (isReloading || isShooting) return;

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                if (!audioSource.isPlaying)
                    audioSource.PlayOneShot(emptySound);
                TryReload();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && totalAmmo > 0)
        {
            TryReload();
        }
    }

    void Shoot()
    {
        currentAmmo--;
        nextFireTime = Time.time + fireRate;
        isShooting = true;

        weaponAnimator.SetTrigger("Shoot");
        audioSource.PlayOneShot(shootSound);

        if (Physics.Raycast(armsCamera.transform.position, armsCamera.transform.forward, out RaycastHit hit, range, enemyLayer))
        {
            if (hit.transform.TryGetComponent(out WolfAI wolf))
            {
                wolf.TakeDamage((int)damage, hit.point);
            }

            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

        UpdateAmmoUI();
        Invoke(nameof(ResetShooting), 0.1f);
    }

    void TryReload()
    {
        if (PauseMenuController.GameIsPaused || currentAmmo >= maxAmmo || totalAmmo <= 0)
            return;

        isReloading = true;
        weaponAnimator.SetTrigger("Reload");
        audioSource.PlayOneShot(reloadSound);
        Invoke(nameof(FinishReload), reloadTime);
    }

    void FinishReload()
    {
        int ammoNeeded = maxAmmo - currentAmmo;
        int ammoToAdd = Mathf.Min(ammoNeeded, totalAmmo);

        currentAmmo += ammoToAdd;
        totalAmmo -= ammoToAdd;

        isReloading = false;
        UpdateAmmoUI();
    }

    void ResetShooting()
    {
        isShooting = false;
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo}/{totalAmmo}";
        }
    }

    public void AddAmmo(int amount)
    {
        totalAmmo = Mathf.Max(0, totalAmmo + amount);
        UpdateAmmoUI();
    }
}