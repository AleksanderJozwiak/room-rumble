using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Required for UI elements

public class PlayerShooting : MonoBehaviour
{
    private Camera mainCamera;
    public PlayerStats playerStats;
    public PlayerUIManager uiManager;
    public Animator weaponAnimator;

    [Header("Hitscan")]
    public float weaponRange = 100f;
    public LayerMask hitLayers;
    public GameObject impactEffect;

    [Header("Bullet Tracer")]
    public GameObject bulletTracerPrefab;
    public Transform startPoint;
    public float tracerSpeed = 100f;
    public float tracerLifetime = 0.5f;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public AudioSource gunAudioSource;
    public AudioClip fireSound;
    public AudioClip reloadSound;

    [Header("Flash Effect")]
    public Light muzzleFlashLight;
    public float flashDuration = 0.05f;

    [Header("Hit Marker UI & Sound")]
    public Image hitMarkerUI;
    public float hitMarkerDuration = 0.1f;
    public AudioClip hitMarkerSound;
    public GameObject floatingDamagePrefab;


    private int currentAmmo;
    private bool canShoot = true;
    private bool isReloading = false;

    private void Start()
    {
        mainCamera = Camera.main;
        currentAmmo = playerStats.maxAmmo;
        uiManager.UpdateAmmoText(currentAmmo);

        if (hitMarkerUI != null)
            hitMarkerUI.enabled = false;
    }

    private void Update()
    {
        if (Cursor.lockState == CursorLockMode.None) return;

        if (Input.GetButton("Fire1") && canShoot && currentAmmo > 0 && !isReloading)
        {
            StartCoroutine(Shoot());
        }

        if ((Input.GetKeyDown(KeyCode.R) || currentAmmo <= 0) && currentAmmo < playerStats.maxAmmo && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Shoot()
    {
        if (currentAmmo <= 0 || isReloading) yield break;

        canShoot = false;
        currentAmmo--;
        uiManager.UpdateAmmoText(currentAmmo);

        if (muzzleFlash != null) muzzleFlash.Play();

        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = true;
            StartCoroutine(DisableFlashAfterTime());
        }

        weaponAnimator.SetBool("isShooting", true);
        weaponAnimator.SetTrigger("Shoot");

        if (gunAudioSource != null && fireSound != null)
            gunAudioSource.PlayOneShot(fireSound);

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, weaponRange, hitLayers))
        {
            if (hit.collider.TryGetComponent<Health>(out var target))
            {
                float finalDamage = playerStats.damage;
                if (Random.Range(0f, 100f) < playerStats.criticalChance)
                {
                    finalDamage *= 2; 
                }
                target.TakeDamage(finalDamage);

                if (hitMarkerUI != null)
                {
                    hitMarkerUI.enabled = true;
                    StartCoroutine(DisableHitMarker());
                }
                if (gunAudioSource != null && hitMarkerSound != null)
                {
                    gunAudioSource.PlayOneShot(hitMarkerSound);
                }

                if (floatingDamagePrefab != null)
                {
                    GameObject dmgText = Instantiate(floatingDamagePrefab, hit.point, Quaternion.identity);
                    dmgText.GetComponent<FloatingDamageText>().Initialize(finalDamage, hit.point);
                }
            }

            StartCoroutine(SpawnBulletTracer(startPoint.transform.position, hit.point));

            if (impactEffect != null)
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }

        yield return new WaitForSeconds(playerStats.fireRate);
        weaponAnimator.SetBool("isShooting", false);
        canShoot = true;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator DisableHitMarker()
    {
        yield return new WaitForSeconds(hitMarkerDuration);
        if (hitMarkerUI != null)
        {
            hitMarkerUI.enabled = false;
        }
    }

    private IEnumerator DisableFlashAfterTime()
    {
        yield return new WaitForSeconds(flashDuration);
        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = false;
        }
    }

    private IEnumerator Reload()
    {
        if (isReloading) yield break;

        isReloading = true;
        canShoot = false;

        if (weaponAnimator != null)
        {
            weaponAnimator.SetFloat("ReloadSpeed", 2f / playerStats.reloadSpeed);
            weaponAnimator.SetTrigger("Reload");
        }

        if (gunAudioSource != null && reloadSound != null)
        {
            gunAudioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(playerStats.reloadSpeed);

        currentAmmo = playerStats.maxAmmo;
        uiManager.UpdateAmmoText(currentAmmo);

        isReloading = false;
        canShoot = true;
    }

    private IEnumerator SpawnBulletTracer(Vector3 start, Vector3 end)
    {
        GameObject tracer = Instantiate(bulletTracerPrefab, start, Quaternion.identity);
        if (tracer == null)
        {
            yield break;
        }

        LineRenderer lineRenderer = tracer.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            yield break;
        }

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        yield return new WaitForSeconds(tracerLifetime);
        Destroy(tracer);
    }
}
