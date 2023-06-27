using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Game.Shared
{
    public enum WeaponShootType
    {
        Manual,
        Automatic = 2,
        Charge
    }

    [Serializable]
    public struct CrosshairData
    {
        [Tooltip("The image that will be used for this weapon's crosshair")]
        public Sprite CrosshairSprite;

        [Tooltip("The size of the crosshair image")]
        public int CrosshairSize;

        [Tooltip("The color of the crosshair image")]
        public Color CrosshairColor;
    }

    [RequireComponent(typeof(AudioSource))]
    public class WeaponController : MonoBehaviour
    {
        private const string k_AnimAttackParameter = "Attack";

        [Header("Information")] [Tooltip("The name that will be displayed in the UI for this weapon")]
        public string WeaponName;

        [Tooltip("The image that will be displayed in the UI for this weapon")]
        public Sprite WeaponIcon;

        [Tooltip("Default data for the crosshair")]
        public CrosshairData CrosshairDataDefault;

        [Tooltip("Data for the crosshair when targeting an enemy")]
        public CrosshairData CrosshairDataTargetInSight;

        [Header("Internal References")]
        [Tooltip("The root object for the weapon, this is what will be deactivated when the weapon isn't active")]
        public GameObject WeaponRoot;

        [Tooltip("Tip of the weapon, where the projectiles are shot")]
        public Transform WeaponMuzzle;

        public Transform LeftHandTarget;
        public Transform RightHandTarget;

        [Header("Shoot Parameters")] [Tooltip("The type of weapon wil affect how it shoots")]
        public WeaponShootType ShootType;

        [Tooltip("The projectile prefab")] public ProjectileBase ProjectilePrefab;

        [Tooltip("Minimum duration between two shots")]
        public float DelayBetweenShots = 0.5f;

        [Tooltip("Angle for the cone in which the bullets will be shot randomly")]
        public float BulletSpreadAngle;

        [Tooltip("Amount of bullets per shot")]
        public int BulletsPerShot = 1;

        [Tooltip("Force that will push back the weapon after each shot")] [Range(0f, 2f)]
        public float RecoilForce = 1;

        [Tooltip("Ratio of the default FOV that this weapon applies while aiming")] [Range(0f, 1f)]
        public float AimZoomRatio = 1f;

        [Tooltip("Translation to apply to weapon arm when aiming with this weapon")]
        public Vector3 AimOffset;

        [Header("Ammo Parameters")] [Tooltip("Should the player manually reload")]
        public bool AutomaticReload = true;

        [Tooltip("Has physical clip on the weapon and ammo shells are ejected when firing")]
        public bool HasPhysicalBullets;

        [Tooltip("Number of bullets in a clip")]
        public int ClipSize = 30;

        [Tooltip("Bullet Shell Casing")] public GameObject ShellCasing;

        [Tooltip("Weapon Ejection Port for physical ammo")]
        public Transform EjectionPort;

        [Tooltip("Force applied on the shell")] [Range(0.0f, 5.0f)]
        public float ShellCasingEjectionForce = 2.0f;

        [Tooltip("Maximum number of shell that can be spawned before reuse")] [Range(1, 30)]
        public int ShellPoolSize = 1;

        [Tooltip("Amount of ammo reloaded per second")]
        public float AmmoReloadRate = 1f;

        [Tooltip("Delay after the last shot before starting to reload")]
        public float AmmoReloadDelay = 2f;

        [Tooltip("Maximum amount fo ammo in the gun")]
        public int MaxAmmo = 14;

        [Header("Charging parameters")] [Tooltip("Trigger a shot when maximum charge is reached")]
        public bool AutomaticReleaseOnCharged;

        [Tooltip("Duration to reach maximum charge")]
        public float MaxChargeDuration = 2f;

        [Tooltip("Initial ammo used when starting to charge")]
        public float AmmoUsedOnStartCharge = 1f;

        [Tooltip("Additional ammo used when charge reaches its maximum")]
        public float AmmoUsageRateWhileCharging = 1f;

        [Header("Audio & Visual")] [Tooltip("Optional weapon animator for OnShoot animations")]
        public Animator WeaponAnimator;

        [Tooltip("Prefab of the muzzle flash")]
        public GameObject MuzzleFlashPrefab;

        [Tooltip("Unparent the muzzle flash instance on spawn")]
        public bool UnparentMuzzleFlash;

        [Tooltip("sound played when shooting")]
        public AudioClip ShootSfx;

        [Tooltip("Sound played when changing to this weapon")]
        public AudioClip ChangeWeaponSfx;

        [Tooltip("Continuous Shooting Sound")] public bool UseContinuousShootSound;

        public AudioClip ContinuousShootStartSfx;
        public AudioClip ContinuousShootLoopSfx;
        public AudioClip ContinuoutShootEndSfx;

        private int m_CarriedPhysicalBullets;

        private float m_CurrentAmmo;
        private Vector3 m_LastMuzzlePosition;
        private float m_LastTimeShot = Mathf.NegativeInfinity;

        private Queue<Rigidbody> m_PhysicalAmmoPool;

        private AudioSource m_ShootAudioSource;
        private bool m_WantsToShoot;

        public UnityAction OnShoot;
        public float LastChargeTriggerTimestamp { get; private set; }

        public GameObject Owner { get; set; }
        public GameObject SourcePrefab { get; set; }
        public bool IsCharging { get; private set; }
        public float CurrentAmmoRatio { get; private set; }
        public bool IsWeaponActive { get; private set; }
        public bool IsCooling { get; private set; }
        public float CurrentCharge { get; private set; }
        public Vector3 MuzzleWorldVelocity { get; private set; }

        public bool IsReloading { get; private set; }

        private void Awake()
        {
            m_CurrentAmmo = MaxAmmo;
            m_CarriedPhysicalBullets = HasPhysicalBullets ? ClipSize : 0;
            m_LastMuzzlePosition = WeaponMuzzle.position;

            // todo: Audio

            if (!HasPhysicalBullets) return;
            m_PhysicalAmmoPool = new Queue<Rigidbody>(ShellPoolSize);

            for (var i = 0; i < ShellPoolSize; i++)
            {
                var shell = Instantiate(ShellCasing, transform);
                shell.SetActive(false);
                m_PhysicalAmmoPool.Enqueue(shell.GetComponent<Rigidbody>());
            }
        }

        private void Update()
        {
            UpdateAmmo();
            UpdateCharge();
            // todo: Update sound

            if (Time.deltaTime <= 0) return;
            MuzzleWorldVelocity = (WeaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
            m_LastMuzzlePosition = WeaponMuzzle.position;
        }

        public event Action OnShootProcessed;

        public float GetAmmoNeedToShout()
        {
            return (ShootType != WeaponShootType.Charge ? 1f : Mathf.Max(1f, AmmoUsedOnStartCharge)) /
                   (MaxAmmo * BulletsPerShot);
        }

        public int GetCarriedPhysicalBullets()
        {
            return m_CarriedPhysicalBullets;
        }

        public int GetCurrentAmmo()
        {
            return Mathf.FloorToInt(m_CurrentAmmo);
        }

        public void AddCarriablePhysicalBullets(int count)
        {
            m_CarriedPhysicalBullets = Mathf.Max(m_CarriedPhysicalBullets + count, MaxAmmo);
        }

        private void ShootShell()
        {
            var nextShell = m_PhysicalAmmoPool.Dequeue();

            var nextShellTransform = nextShell.transform;
            nextShellTransform.position = EjectionPort.transform.position;
            nextShellTransform.rotation = EjectionPort.transform.rotation;
            nextShell.gameObject.SetActive(true);
            nextShell.transform.SetParent(null);
            nextShell.collisionDetectionMode = CollisionDetectionMode.Continuous;
            nextShell.AddForce(nextShellTransform.up * ShellCasingEjectionForce, ForceMode.Impulse);

            m_PhysicalAmmoPool.Enqueue(nextShell);
        }


        private void Reload()
        {
            if (m_CarriedPhysicalBullets > 0) m_CurrentAmmo = Mathf.Min(m_CarriedPhysicalBullets, ClipSize);

            IsReloading = false;
        }

        public void StartReloadAnimation()
        {
            if (m_CurrentAmmo >= m_CarriedPhysicalBullets) return;

            GetComponent<Animator>().SetTrigger("Reload");
            IsReloading = true;
        }

        private void UpdateAmmo()
        {
            if (AutomaticReload && m_LastTimeShot + AmmoReloadDelay < Time.time && m_CurrentAmmo < MaxAmmo &&
                !IsCharging)
            {
                m_CurrentAmmo += AmmoReloadRate * Time.deltaTime;

                // limits ammo to max value
                m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo, 0, MaxAmmo);

                IsCooling = true;
            }
            else
            {
                IsCooling = false;
            }

            if (MaxAmmo == Mathf.Infinity)
                CurrentAmmoRatio = 1f;
            else
                CurrentAmmoRatio = m_CurrentAmmo / MaxAmmo;
        }

        private void UpdateCharge()
        {
            if (!IsCharging) return;
            if (CurrentCharge >= 1f) return;

            var chargeLeft = 1f - CurrentCharge;

            // Calculate how much charge ratio to add this frame
            var chargeAdded = 0f;
            if (MaxChargeDuration <= 0f)
                chargeAdded = chargeLeft;
            else
                chargeAdded = 1f / MaxChargeDuration * Time.deltaTime;

            chargeAdded = Mathf.Clamp(chargeAdded, 0f, chargeLeft);

            var ammoThisChargeWouldRequire = chargeAdded * AmmoUsageRateWhileCharging;
            if (ammoThisChargeWouldRequire > m_CurrentAmmo) return;

            UseAmmo(ammoThisChargeWouldRequire);

            CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdded);
        }

        private void UpdateContinuousShootSound()
        {
        }

        public void UseAmmo(float amount)
        {
            m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo - amount, 0f, MaxAmmo);
            m_CarriedPhysicalBullets -= Mathf.RoundToInt(amount);
            m_CarriedPhysicalBullets = Mathf.Clamp(m_CarriedPhysicalBullets, 0, MaxAmmo);
            m_LastTimeShot = Time.time;
        }

        public void ShowWeapon(bool show)
        {
            WeaponRoot.SetActive(show);

            // todo: Sound
            // if (show && ChangeWeaponSfx)
            // {
            //     m_ShootAudioSource.PlayOneShot(ChangeWeaponSfx);
            // }

            IsWeaponActive = show;
        }

        public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            m_WantsToShoot = inputDown || inputHeld;
            switch (ShootType)
            {
                case WeaponShootType.Manual:
                    if (!inputDown) return false;
                    return TryShoot();

                case WeaponShootType.Automatic:
                    if (!inputHeld && !inputDown) return false;
                    return TryShoot();

                case WeaponShootType.Charge:
                    if (inputHeld) TryBeginCharge();

                    // Check if we released charge or if the weapon shoot automatically when it's fully charged
                    if (inputUp || (AutomaticReleaseOnCharged && CurrentCharge >= 1f)) return TryReleaseCharge();

                    return false;

                default:
                    return false;
            }
        }

        private bool TryShoot()
        {
            if (m_CurrentAmmo >= 1f
                && m_LastTimeShot + DelayBetweenShots < Time.time)
            {
                HandleShoot();
                m_CurrentAmmo -= 1f;

                return true;
            }

            return false;
        }

        private bool TryBeginCharge()
        {
            if (!IsCharging
                && m_CurrentAmmo >= AmmoUsedOnStartCharge
                && Mathf.FloorToInt(m_CurrentAmmo - AmmoUsedOnStartCharge + BulletsPerShot) > 0
                && m_LastTimeShot + DelayBetweenShots < Time.time)
            {
                UseAmmo(AmmoUsedOnStartCharge);

                LastChargeTriggerTimestamp = Time.time;
                IsCharging = true;

                return true;
            }

            return false;
        }

        private bool TryReleaseCharge()
        {
            if (!IsCharging) return false;

            HandleShoot();

            CurrentCharge = 0f;
            IsCharging = false;
            return true;
        }

        private void HandleShoot()
        {
            var bulletsPerShotFinal = ShootType == WeaponShootType.Charge
                ? Mathf.CeilToInt(CurrentCharge * BulletsPerShot)
                : BulletsPerShot;

            // spawn all bullets with random direciton
            for (var i = 0; i < bulletsPerShotFinal; i++)
            {
                var shotDirection = GetShotDirectionWithinSpread(WeaponMuzzle);
                var newProjectile = Instantiate(ProjectilePrefab, WeaponMuzzle.position,
                    Quaternion.LookRotation(shotDirection));
                
                newProjectile.Shoot(this);
            }

            // todo: muzzle flash

            if (HasPhysicalBullets)
            {
                ShootShell();
                m_CarriedPhysicalBullets--;
            }

            m_LastTimeShot = Time.time;

            // todo: sound

            // todo: anim

            OnShoot?.Invoke();
            OnShootProcessed?.Invoke();
        }

        public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
        {
            var spreadAngleRatio = BulletSpreadAngle / 100f;
            var spreadWorldDirection = Vector3.Slerp(shootTransform.forward, Random.insideUnitSphere,
                spreadAngleRatio);
            return spreadWorldDirection;
        }
    }
}