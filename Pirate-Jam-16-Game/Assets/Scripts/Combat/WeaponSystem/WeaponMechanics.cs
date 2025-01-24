using Hertzole.ScriptableValues;
using Kitbashery.Gameplay;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponMechanics : MonoBehaviour
{
    [SerializeField] private WeaponData m_WeaponData;
    [SerializeField] private Transform[] WeaponMuzzles;
    //[SerializeField] private ScriptableStringEvent onAmmoChanged;
    //[SerializeField] private ScriptableIntEvent onAmmoGrabbed;
    [SerializeField] private Camera m_PlayerCam;

    private float timeBetweenShots;
    private Quaternion initialRotation;

    Vector3 recoilSmoothDampVelocity;
    float recoilRotSmoothDampVelocity;
    float recoilAngle;


    float nextShotTime;
    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;
    int projectilesRemainingInMag;
    int currentAmmoTotal;

    public bool isShooting;

    AudioSource source;
    private bool isReloading;
    private bool isCooldown;

    private void OnEnable()
    {
        WeaponControls.ShootingHeld += OnTriggerHold;
        WeaponControls.ShootingReleased += OnTriggerReleased;
        WeaponControls.Reload += OnReload;

        //onAmmoChanged.Invoke(this, $"{projectilesRemainingInMag} | {m_WeaponData.AmmoCapacity}");
        //onAmmoGrabbed.OnInvoked += OnAmmoPickup;
        
    }



    private void OnDisable()
    {
        WeaponControls.ShootingHeld -= OnTriggerHold;
        WeaponControls.ShootingReleased -= OnTriggerReleased;
        WeaponControls.Reload -= OnReload;

        //onAmmoGrabbed.OnInvoked -= OnAmmoPickup;
        
    }


    private void Start()
    {
        currentAmmoTotal = m_WeaponData.AmmoCapacity;
        nextShotTime = Time.time;
        shotsRemainingInBurst = m_WeaponData.burstCount;
        projectilesRemainingInMag = m_WeaponData.ProjectilesPerMag;
        source = GetComponentInParent<AudioSource>();
        timeBetweenShots = 1.0f / m_WeaponData.RateOfFire;
    }

    private bool CanShoot() => (!isReloading && Time.time >= nextShotTime && projectilesRemainingInMag > 0);

    public void Shoot()
    {
        if (CanShoot())
        {
            // Firemodes
            if (m_WeaponData.fireMode == FireMode.Burst)
            { 
                if (shotsRemainingInBurst == 0)
                {
                    internalCooldown();
                    return;
                }
                shotsRemainingInBurst--;
            }
            else if (m_WeaponData.fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                {
                    internalCooldown();
                    return;
                }
            }


            nextShotTime = Time.time + timeBetweenShots;
            // Spawn projectiles
            ChooseFireType();



            int j = Random.Range(0, m_WeaponData.ShootAudio.Length);
            //source.PlayOneShot(m_WeaponData.ShootAudio?[j]);

            projectilesRemainingInMag--;

            //onAmmoChanged.Invoke(this, $"{projectilesRemainingInMag} | {currentAmmoTotal}");
        }

    }



    private void ChooseFireType()
    {
        switch (m_WeaponData.bulletType)
        {
            case BulletType.projectile:
                SpawnProjectile();
                break;
            case BulletType.hitscan:
                SpawnHitScan();
                break;
        }
    }


    private void SpawnHitScan()
    {
        RaycastHit hit;
        foreach (Transform muzzle in WeaponMuzzles)
        {
            GameObject flash = ObjectPools.Instance.GetPooledObject(m_WeaponData.FlashPrefab.name);
            flash.transform.SetParent(muzzle.transform); 
            flash.transform.position = muzzle.position;
            flash.transform.forward = muzzle.forward;
            if (flash?.GetComponent<ParticleSystem>() != null)
            {
                StartCoroutine(StartFlash(flash));
            }
            if (Physics.Raycast(m_PlayerCam.transform.position, m_PlayerCam.transform.forward, out hit, m_WeaponData.WeaponRange))
            {
                // Handle hit, apply damage, effects, etc.
                if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    hit.collider.gameObject?.GetComponent<Health>().ModifyHealth(HealthModifiers.damage, m_WeaponData.DamageAmount);
                }
                Debug.Log(hit.collider.gameObject.name);
                GameObject impact =  ObjectPools.Instance.GetPooledObject("Stones hit");
                impact.transform.position = hit.point;
                StartCoroutine(StartFlash(impact));
            }
        }
    }

    private void SpawnProjectile()
    {

        foreach (Transform muzzle in WeaponMuzzles)
        {
            GameObject flash = ObjectPools.Instance.GetPooledObject(m_WeaponData.FlashPrefab.name);
            flash.transform.SetParent(muzzle.transform);
            flash.transform.position = muzzle.position;
            flash.transform.forward = muzzle.forward;
            if (flash?.GetComponent<ParticleSystem>() != null)
            {
                StartCoroutine(StartFlash(flash));
            }

            GameObject bullet = ObjectPools.Instance.GetPooledObject(m_WeaponData.BulletPrefab.name);
            if (bullet.activeSelf != true)
            {
                bullet.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);

                bullet.SetActive(true);

                bullet.transform.forward = muzzle.forward;
            }
        }

    }

    private IEnumerator StartFlash(GameObject effect)
    {
        effect.SetActive(true);
        effect.GetComponent<ParticleSystem>().Play();
        yield return new WaitForSeconds(0.5f);
        effect.SetActive(false);
    }

    public void OnReload()
    {
        if (!isReloading && projectilesRemainingInMag != m_WeaponData.ProjectilesPerMag)
        {
            StartCoroutine(AnimateReload());
        }
    }


    IEnumerator AnimateReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(.2f);

        source.PlayOneShot(m_WeaponData.ReloadAudio, 1);

        float reloadSpeed = 1 / m_WeaponData.reloadTime;
        float percent = 0;

        // Store the initial rotation of the weapon
        initialRotation = transform.rotation;

        while (percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;

            // Perform a full 360-degree rotation
            float rotationAngle = Mathf.Lerp(0, 360, percent);
            transform.rotation = initialRotation * Quaternion.Euler(Vector3.right * rotationAngle);

            yield return null;
        }

        isReloading = false;

        // Reset the rotation to the initial state
        transform.rotation = initialRotation;

        currentAmmoTotal -= (m_WeaponData.ProjectilesPerMag - projectilesRemainingInMag);
        projectilesRemainingInMag = m_WeaponData.ProjectilesPerMag;
        //onAmmoChanged.Invoke(this, $"{projectilesRemainingInMag} | {currentAmmoTotal}");
        
    }

    private IEnumerator internalCooldown()
    {
        WeaponControls.ShootingHeld -= OnTriggerHold;
        yield return new WaitForSeconds(nextShotTime);
        WeaponControls.ShootingHeld += OnTriggerHold;

    }

    private void OnAmmoPickup(object sender, int e)
    {

        if ((currentAmmoTotal += e) > m_WeaponData.AmmoCapacity)
        {
            currentAmmoTotal = m_WeaponData.AmmoCapacity;
        }
        else
        {
            currentAmmoTotal += e;
        }
            
       
    }

    public void OnTriggerHold()
    { 
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerReleased()
    {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = m_WeaponData.burstCount;
        
    }
}

