using System;
using System.Collections.Generic;
using Cinemachine;
using Game;
using Game.Shared;
using InputSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace GamePlay.Managers
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerWeaponsManager : MonoBehaviour
    {
        public enum WeaponSwitchState
        {
            Up,
            Down,
            PutDownPrevious,
            PutUpNew
        }

        [Tooltip("List of weapon the player will start with")]
        public List<WeaponController> StartingWeapons = new();

        [Header("References")] [Tooltip("Secondary camera used to avoid seeing weapon go throw geometries")]
        public CinemachineVirtualCamera WeaponCamera;

        [Tooltip("Parent transform where all weapons will be added in the hierarchy")]
        public Transform WeaponParentSocket;

        [Tooltip("Position for weapons when active but not actively aiming")]
        public Transform DefaultWeaponPosition;

        [Tooltip("Position for weapons when aiming")]
        public Transform AimingWeaponPosition;

        [Tooltip("Position for inactive weapons")]
        public Transform DownWeaponPosition;

        [Header("Weapon Bob")]
        [Tooltip("Frequency at which the weapon will move around in the screen when the player is in movement")]
        public float BobFrequency = 10f;

        [Tooltip("How fast the weapon bob is applied, the bigger value the fastest")]
        public float BobSharpness = 10f;

        [Tooltip("Distance the weapon bobs when not aiming")]
        public float DefaultBobAmount = 0.05f;

        [Tooltip("Distance the weapon bobs when aiming")]
        public float AimingBobAmount = 0.02f;

        [Header("Weapon Recoil")]
        [Tooltip("This will affect how fast the recoil moves the weapon, the bigger the value, the fastest")]
        public float RecoilSharpness = 50f;

        [Tooltip("Maximum distance the recoil can affect the weapon")]
        public float MaxRecoilDistance = 0.5f;

        [Tooltip("How fast the weapon goes back to it's original position after the recoil finished")]
        public float RecoilRestitutionSharpness = 10f;

        [Header("Misc")] [Tooltip("Speed at which the aiming animation is played")]
        public float AimingAnimationSpeed = 10f;

        [Tooltip("Filed of view when not aiming")]
        public float DefaultFov = 60f;

        [Tooltip("Portion of the regular FOV to apply to the weapon camera")]
        public float WeaponFovMultiplier = 1f;

        [Tooltip("Delay before switching weapon a second time, to avoid receiving multiple inputs from mouse wheel")]
        public float WeaponSwitchDelay = 1f;

        [Tooltip("Layer to set FPS weapon gameObjects to")]
        public LayerMask FpsWeaponLayer;

        // --- Animatinons
        private Animator _animator;
        private int _animIDWeaponType;
        private int _animAiming;
        private Transform _ainTarget;

        private Vector3 m_AccumulatedRecoil;
        private ThirdInputs m_InputHandler;
        private Vector3 m_LastCharacterPosition;
        private ThirdController m_PlayerCharacterController;
        private float m_TimeStartedWeaponSwitch;
        private float m_WeaponBobFactor; // 武器摆动因数
        private Vector3 m_WeaponBobLocalPosition;
        private Vector3 m_WeaponMainLocalPosition;
        private Quaternion m_WeaponMainLocalRotation;
        private Vector3 m_WeaponRecoilLocalPosition;

        private readonly WeaponController[] m_WeaponSlots = new WeaponController[9];
        private int m_WeaponSwitchNewWeaponIndex;
        private WeaponSwitchState m_WeaponSwitchState;
        public UnityAction<WeaponController, int> OnAddedWeapon;
        public UnityAction<WeaponController, int> OnRemoveWeapon;

        public UnityAction<WeaponController> OnSwitchedToWeapon;

        public bool IsAiming { get; private set; }
        public bool IsPointingAtEnemy { get; private set; }
        public int ActiveWeaponIndex { get; private set; }

        private void Start()
        {
            ActiveWeaponIndex = -1;
            m_WeaponSwitchState = WeaponSwitchState.Down;

            _animator = GetComponent<Animator>();

            m_InputHandler = GetComponent<ThirdInputs>();
            DebugUtility.HandleErrorIfNullGetComponent<ThirdInputs, PlayerWeaponsManager>(m_InputHandler, this,
                gameObject);

            m_PlayerCharacterController = GetComponent<ThirdController>();
            DebugUtility.HandleErrorIfNullGetComponent<ThirdController, PlayerWeaponsManager>(
                m_PlayerCharacterController, this, gameObject);

            _ainTarget = GameObject.Find("AimTarget").transform;

            // todo: is FOV should be set here?

            // --- 通过Animator.StringToHash将动画参数转为INT，提高比较效率 ---
            AssignAnimationIDs();

            OnSwitchedToWeapon += OnWeaponSwitched;

            // Add starting weapons
            foreach (var weapon in StartingWeapons) AddWeapon(weapon);

            SwitchWeapon(true);
        }

        private void Update()
        {
            // shoot handling
            var activeWeapon = GetActiveWeapon();

            if (activeWeapon != null && activeWeapon.IsReloading)
                return;

            if (activeWeapon != null && m_WeaponSwitchState == WeaponSwitchState.Up)
            {
                if (!activeWeapon.AutomaticReload && m_InputHandler.reload && activeWeapon.CurrentAmmoRatio < 1.0f)
                {
                    IsAiming = false;
                    activeWeapon.StartReloadAnimation();
                    m_InputHandler.reload = false;
                    return;
                }

                // Handle aiming down sights
                IsAiming = m_InputHandler.aiming;
                m_InputHandler.aiming = false;

                if (m_InputHandler.tapFire || m_InputHandler.holdFire)
                {
                    _animator.SetBool(_animAiming, true);
                }

                // todo: handle shooting, charge weapon unfinished
                var hasFired = activeWeapon.HandleShootInputs(
                    m_InputHandler.tapFire,
                    m_InputHandler.holdFire,
                    false);

                m_InputHandler.tapFire = false;
                //m_InputHandler.holdFire = false;

                //Handle accumulating recoil
                if (hasFired)
                {
                    m_AccumulatedRecoil += Vector3.back * activeWeapon.RecoilForce;
                    m_AccumulatedRecoil = Vector3.ClampMagnitude(m_AccumulatedRecoil, MaxRecoilDistance);
                }
            }

            if (!IsAiming &&
                (activeWeapon == null || !activeWeapon.IsCharging) &&
                m_WeaponSwitchState is WeaponSwitchState.Up or WeaponSwitchState.Down)
            {
                var switchWeaponInput = m_InputHandler.GetSwitchWeaponInput();
                if (switchWeaponInput == -1)
                {
                    Debug.Log("Switch Weapon Unfinished");
                }
                else if (switchWeaponInput != 0)
                {
                    var switchUp = switchWeaponInput > 0;
                    SwitchWeapon(switchUp);
                }
                else
                {
                    switchWeaponInput = m_InputHandler.GetSelectWeaponInput();
                    if (switchWeaponInput != -1)
                    {
                        if (GetWeaponAtSlotIndex(switchWeaponInput - 1) != null)
                            SwitchToWeaponIndex(switchWeaponInput - 1);
                    }
                    else
                    {
                        Debug.Log("Switch to weapon index unfinished");
                    }
                }
            }

            switch (m_WeaponSwitchState)
            {
                // 冲刺时放下武器/Put down weapon when sprint
                case WeaponSwitchState.Up when m_InputHandler.sprint:
                    m_WeaponSwitchState = WeaponSwitchState.PutDownPrevious;
                    m_TimeStartedWeaponSwitch = Time.time;
                    break;
                case WeaponSwitchState.Down when !m_InputHandler.sprint:
                    m_WeaponSwitchState = WeaponSwitchState.PutUpNew;
                    m_TimeStartedWeaponSwitch = Time.time;
                    break;
            }

            // Pointing at enemy handling
            IsPointingAtEnemy = false;
            if (!activeWeapon) return;
            if (Physics.Raycast(WeaponCamera.transform.position, WeaponCamera.transform.forward, out var hit,
                    1000, -1, QueryTriggerInteraction.Ignore))
                if (hit.collider.GetComponentInParent<Health>() != null)
                    IsPointingAtEnemy = true;
        }

        private void LateUpdate()
        {
            UpdateWeaponAiming();
            UpdateWeaponBob();
            //UpdateWeaponRecoil();
            UpdateWeaponSwitching();

            // Set final weapon socket position based on  all the animation influences
            WeaponParentSocket.localPosition =
                m_WeaponMainLocalPosition + m_WeaponBobLocalPosition + m_WeaponRecoilLocalPosition;
            WeaponParentSocket.LookAt(_ainTarget);
        }

        private void AssignAnimationIDs()
        {
            _animIDWeaponType = Animator.StringToHash("WeaponType");
            _animAiming = Animator.StringToHash("aiming");
        }

        private void UpdateWeaponSwitching()
        {
            // Calculate the time ratio (0 to 1) since weapon switch was triggered
            var switchingTimeFactor = WeaponSwitchDelay == 0f
                ? 1f
                : Mathf.Clamp01((Time.time - m_TimeStartedWeaponSwitch) / WeaponSwitchDelay);

            // Handle transiting to new switch state
            if (switchingTimeFactor >= 1f)
            {
                switch (m_WeaponSwitchState)
                {
                    case WeaponSwitchState.PutDownPrevious:
                    {
                        if (ActiveWeaponIndex != m_WeaponSwitchNewWeaponIndex)
                        {
                            // Deactivate old weapon
                            var oldWeapon = GetWeaponAtSlotIndex(ActiveWeaponIndex);
                            if (oldWeapon != null) oldWeapon.ShowWeapon(false);

                            ActiveWeaponIndex = m_WeaponSwitchNewWeaponIndex;
                            switchingTimeFactor = 0f;

                            // Activate new weapon
                            var newWeapon = GetWeaponAtSlotIndex(ActiveWeaponIndex);
                            OnSwitchedToWeapon?.Invoke(newWeapon);

                            if (newWeapon)
                            {
                                m_TimeStartedWeaponSwitch = Time.time;
                                m_WeaponSwitchState = WeaponSwitchState.PutUpNew;
                                break;
                            }
                        }

                        m_WeaponSwitchState = WeaponSwitchState.Down;
                        break;
                    }
                    case WeaponSwitchState.PutUpNew:
                        m_WeaponSwitchState = WeaponSwitchState.Up;
                        break;
                }
            }

            m_WeaponMainLocalPosition = m_WeaponSwitchState switch
            {
                WeaponSwitchState.PutDownPrevious => Vector3.Lerp(DefaultWeaponPosition.localPosition,
                    DownWeaponPosition.localPosition, switchingTimeFactor),
                // ReSharper disable once Unity.InefficientPropertyAccess : only access once
                WeaponSwitchState.PutUpNew => Vector3.Lerp(DownWeaponPosition.localPosition,
                    // ReSharper disable once Unity.InefficientPropertyAccess
                    DefaultWeaponPosition.localPosition, switchingTimeFactor),
                _ => m_WeaponMainLocalPosition
            };

            m_WeaponMainLocalRotation = m_WeaponSwitchState switch
            {
                WeaponSwitchState.PutDownPrevious => Quaternion.Lerp(DefaultWeaponPosition.localRotation,
                    DownWeaponPosition.localRotation, switchingTimeFactor),
                // ReSharper disable once Unity.InefficientPropertyAccess
                WeaponSwitchState.PutUpNew => Quaternion.Lerp(DownWeaponPosition.localRotation,
                    // ReSharper disable once Unity.InefficientPropertyAccess
                    DefaultWeaponPosition.localRotation, switchingTimeFactor),
                _ => m_WeaponMainLocalRotation
            };
        }

        private void UpdateWeaponRecoil()
        {
            // if the accumulation recoil is further away from the current position, make the current position move towards the recoil target
            if (m_WeaponRecoilLocalPosition.z >= m_AccumulatedRecoil.z * 0.99f)
            {
                m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, m_AccumulatedRecoil,
                    RecoilSharpness * Time.deltaTime);
            }
            // otherwise, move recoil position to make it recover towards its resting pose
            else
            {
                m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, Vector3.zero,
                    RecoilRestitutionSharpness * Time.deltaTime);
                m_AccumulatedRecoil = m_WeaponRecoilLocalPosition;
            }
        }

        private void UpdateWeaponBob()
        {
            if (Time.deltaTime <= 0f) return;

            var playerCharacterVelocity =
                (m_PlayerCharacterController.transform.position - m_LastCharacterPosition) / Time.deltaTime;

            // calculate a smoothed weapon bob based on current speed
            var characterMovementFactor = 0f;
            if (m_PlayerCharacterController.Grounded)
                characterMovementFactor =
                    Mathf.Clamp01(playerCharacterVelocity.magnitude /
                                  (m_PlayerCharacterController.MoveSpeed *
                                   m_PlayerCharacterController.SpeedChangeRate));

            m_WeaponBobFactor =
                Mathf.Lerp(m_WeaponBobFactor, characterMovementFactor, BobSharpness * Time.deltaTime);

            var bobAmount = IsAiming ? AimingBobAmount : DefaultBobAmount;
            var frequency = BobFrequency;
            var hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * m_WeaponBobFactor;
            var vBobValue = (Mathf.Sin(Time.time * frequency * 2f) * 0.5f + 0.5f) * bobAmount *
                            m_WeaponBobFactor;

            m_WeaponBobLocalPosition.x = hBobValue;
            m_WeaponBobLocalPosition.y = Mathf.Abs(vBobValue);

            m_LastCharacterPosition = m_PlayerCharacterController.transform.position;
        }

        private void UpdateWeaponAiming()
        {
            if (m_WeaponSwitchState != WeaponSwitchState.Up) return;

            var activeWeapon = GetActiveWeapon();
            if (IsAiming && activeWeapon)
                m_WeaponMainLocalPosition = Vector3.Lerp(m_WeaponMainLocalPosition,
                    AimingWeaponPosition.localPosition + activeWeapon.AimOffset,
                    AimingAnimationSpeed * Time.deltaTime);
            // todo: Set aim Fov?
            else
                m_WeaponMainLocalPosition = Vector3.Lerp(m_WeaponMainLocalPosition,
                    DefaultWeaponPosition.localPosition, AimingAnimationSpeed * Time.deltaTime);
            // todo: set fov
        }

        // Iterate on all weapon slots to find the next valid weapon to switch to
        public void SwitchWeapon(bool ascendingOrder)
        {
            var newWeaponIndex = -1;
            var closestSlotDistance = m_WeaponSlots.Length;
            for (var i = 0; i < m_WeaponSlots.Length; ++i)
                // If the weapon at this slot is valid, calculate its "distance" from the active slot index (either in ascending or descending order)
                // and select it if it's the closest distance yet
                if (i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)
                {
                    var distanceToActiveIndex = GetDistanceBetweenWeaponSlots(ActiveWeaponIndex, i, ascendingOrder);

                    if (distanceToActiveIndex >= closestSlotDistance) continue;

                    closestSlotDistance = distanceToActiveIndex;
                    newWeaponIndex = i;
                }

            // Handle switching to the new weapon index
            SwitchToWeaponIndex(newWeaponIndex);
        }

        // Switches to the given weapon index in weapon slots if the index is a valid weapon that is different from our current on
        public void SwitchToWeaponIndex(int newWeaponIndex, bool force = false)
        {
            if (force || (newWeaponIndex != ActiveWeaponIndex && newWeaponIndex >= 0))
            {
                // Store data related to weapon switching animation
                m_WeaponSwitchNewWeaponIndex = newWeaponIndex;
                m_TimeStartedWeaponSwitch = Time.time;

                // Handle case of switching to a valid weapon for the first time (simply put it up without putting anything down first)
                if (GetActiveWeapon() == null)
                {
                    m_WeaponMainLocalPosition = DownWeaponPosition.localPosition;
                    m_WeaponSwitchState = WeaponSwitchState.PutUpNew;
                    ActiveWeaponIndex = m_WeaponSwitchNewWeaponIndex;

                    var newWeapon = GetWeaponAtSlotIndex(m_WeaponSwitchNewWeaponIndex);
                    OnSwitchedToWeapon?.Invoke(newWeapon);
                }
                // Otherwise, remember we are putting down our current weapon for switching to the next on
                else
                {
                    m_WeaponSwitchState = WeaponSwitchState.PutDownPrevious;
                }
            }
        }

        public WeaponController GetActiveWeapon()
        {
            return GetWeaponAtSlotIndex(ActiveWeaponIndex);
        }

        private int GetDistanceBetweenWeaponSlots(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
        {
            var distanceBetweenSlots = 0;

            if (ascendingOrder)
                distanceBetweenSlots = toSlotIndex - fromSlotIndex;
            else
                distanceBetweenSlots = -1 * (toSlotIndex - fromSlotIndex);

            return distanceBetweenSlots;
        }

        public WeaponController GetWeaponAtSlotIndex(int index)
        {
            // find the active weapon in out weapon slots base on our active weapon index
            if (index >= 0 && index < m_WeaponSlots.Length) return m_WeaponSlots[index];

            return null;
        }

        public bool AddWeapon(WeaponController weaponPrefab)
        {
            // if we already hold this weapon type, don't add the weapon
            if (HasWeapon(weaponPrefab)) return false;

            // search our weapon slots for the first free one, assign the weapon to it, and return true or false when found or not
            for (var i = 0; i < m_WeaponSlots.Length; i++)
            {
                // only add the weapon if the slot is free
                if (m_WeaponSlots[i] != null) continue;

                // spawn the weapon prefab as child of the weapon socket
                var weaponInstance = Instantiate(weaponPrefab, WeaponParentSocket);
                var weaponInstanceTransform = weaponInstance.transform;
                weaponInstanceTransform.localPosition = new Vector3(0, 0, 0.4f);
                weaponInstanceTransform.localRotation = Quaternion.identity;

                // Set owner to this gameObject so the weapon can alter projectile/damage accordingly
                weaponInstance.Owner = gameObject;
                weaponInstance.SourcePrefab = weaponPrefab.gameObject;
                weaponInstance.ShowWeapon(false);

                // Assign the first person layer to the weapon
                var layerIndex = Mathf.RoundToInt(Mathf.Log(FpsWeaponLayer.value, 2)); // convert a layer mask to index
                foreach (var t in weaponInstance.gameObject.GetComponentsInChildren<Transform>())
                    t.gameObject.layer = layerIndex;

                m_WeaponSlots[i] = weaponInstance;

                OnAddedWeapon?.Invoke(weaponInstance, i);

                return true;
            }

            if (GetActiveWeapon() == null) SwitchWeapon(true);

            return false;
        }

        public WeaponController HasWeapon(WeaponController weaponPrefab)
        {
            // Checks if we already have a weapon coming from the specified prefab
            foreach (var w in m_WeaponSlots)
                if (w != null && w.SourcePrefab == weaponPrefab.gameObject)
                    return w;
            return null;
        }

        private void OnWeaponSwitched(WeaponController newWeapon)
        {
            if (newWeapon == null) return;

            newWeapon.ShowWeapon(true);
            if (newWeapon.ShootType == WeaponShootType.Automatic)
            {
                _animator.SetInteger(_animIDWeaponType, 2);
            }
        }
    }
}