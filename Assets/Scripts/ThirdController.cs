using System;
using Cinemachine;
using InputSystem;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class ThirdController : MonoBehaviour
{
    private const int PlayerLayer = 6;

    private const float _threshold = 0.01f;

    [Header("Player")] public float MoveSpeed = 5f;

    public float SprintSpeed = 7f;

    [Range(0.0f, 0.3f)] public float RotationSmoothTime = 0.12f;

    public float SpeedChangeRate = 10.0f;

    [Space(10)] public float JumpHeight = 1.2f;

    public float Gravity = -15.0f;

    public float JumpTimeout = 0.50f;

    public float FallTimeout = 0.15f;

    [Header("Player Grounded")] public bool Grounded = true;

    public float GroundedOffset = -0.14f;

    public float GroundedRadius = 0.8f;

    public LayerMask GroundLayers;

    [Header("Cinemachine")] public GameObject CinemachineCameraTarget;

    public float TopClamp = 70.0f;

    public float BottomClamp = -30.0f;

    public float CameraAngleOverride;

    public bool LockCameraPosition;

    [Header("UI")] public GameObject EKeyUI;

    public float PickUpDistance = 3;

    [Header("Animation")] public Transform LeftHandIKTarget;
    public Transform RightHandIKTarget;
    
    private float _animationBlend;
    private Animator _animator;
    // animation IDs
    private int _animIDForward;
    private int _animIDStrafe;
    private int _animIDIsGrounded;
    private int _animIDIsJumping;
    private int _animIDSprint;

    //cinemachine
    private float _cinemachineTargetPitch;
    private float _cinemachineTargetYaw;
    private CharacterController _controller;
    private float _fallTimeoutDelta;

    private bool _hasAnimator;
    private ThirdInputs _input;

    //timeout deltatime
    private float _jumpTimeoutDelta;
    private Camera _mainCamera;
    private Transform _playerCubeModel;
    private Transform _aimTarget;

    private PlayerInput _playerInput;
    private float _rotationVelocity;

    //player
    private float _speed;
    private float _targetRotation;
    private readonly float _terminalVelocity = 53.0f;
    private float _verticalVelocity;

    private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";
    private readonly QueryParameters detectedItemQueryParameters = new(1 << 8);

    private void Awake()
    {
        
    }

    private void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _controller = GetComponent<CharacterController>();
        _input = GetComponent<ThirdInputs>();
        _playerInput = GetComponent<PlayerInput>();
        if (_mainCamera == null) _mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        _playerCubeModel = transform.Find("Cube");

        AssignNewAnimationIDs();
        
        // reset time out
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        _hasAnimator = TryGetComponent(out _animator);

        _aimTarget = GameObject.Find("AimTarget").transform;
    }

    private void Update()
    {
        OpenOrCloseMenu();

        if (GameManager.gameState != GameState.Run) return;

        JumpAndGravity();
        GroundCheck();
        Move();
        
        //--- 暂时关闭简单的物品检测 ---
        //DetectItem();
    }

    private void LateUpdate()
    {
        if (GameManager.gameState != GameState.Run) return;

        CameraRotation();

        _aimTarget.position = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 20f));
    }

    // Highly recommend NOT use Animation Rigging replaces OnAnimatorIK(), it's complex, and has bugs
    private void OnAnimatorIK(int layerIndex)
    {
        if (_animator)
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
        }

        if (LeftHandIKTarget != null)
        {
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandIKTarget.position);
            _animator.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandIKTarget.rotation);
        }

        if (RightHandIKTarget != null)
        {
            _animator.SetIKPosition(AvatarIKGoal.RightHand, RightHandIKTarget.position);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, RightHandIKTarget.rotation);
        }
    }

    private void OnDrawGizmosSelected()
    {
        var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = Grounded ? transparentGreen : transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }

    private void AssignNewAnimationIDs()
    {
        _animIDForward = Animator.StringToHash("Forward");
        _animIDStrafe = Animator.StringToHash("Strafe");
        _animIDIsGrounded = Animator.StringToHash("isGrounded");
        _animIDIsJumping = Animator.StringToHash("isJumping");
        _animIDSprint = Animator.StringToHash("Sprint");
    }

    private void GroundCheck()
    {
        var pos = transform.position;
        var spherePosition = new Vector3(pos.x, pos.y - GroundedOffset, pos.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        
        // update animator if using character
        if (_hasAnimator) _animator.SetBool(_animIDIsGrounded, Grounded);
    }

    private void CameraRotation()
    {
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            var deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
        // ---角色朝向根据摄像机改变，暂时开启---
        transform.rotation = Quaternion.Euler(0.0f, _cinemachineTargetYaw, 0.0f);
    }

    private void Move()
    {
        var targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        //空间换时间
        var controllerVelocity = _controller.velocity;
        var currentHorizontalSpeed = new Vector3(controllerVelocity.x, 0.0f, controllerVelocity.z).magnitude;

        const float speedOffset = 0.1f;
        var inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1.0f;

        //避免Float比较产生bug
        if (currentHorizontalSpeed < targetSpeed - speedOffset
            || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000.0f;
        }
        else
        {
            _speed = targetSpeed;
        }
        
        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        var inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            
            // ---在移动时改变角色朝向，暂时禁用---
            // var rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
            //     RotationSmoothTime);
            // transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        // --- 因为应用了Root Motion，暂时关闭对Controller的操控
        // var targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
        // _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
        //                  new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        
        // update animator if using character
        if (_hasAnimator)
        {
            if (!_input.sprint)
            {
                _animator.SetFloat(_animIDForward, _input.move.y * 0.5f);
                _animator.SetFloat(_animIDStrafe, _input.move.x * 0.5f);
            }
            else
            {
                _animator.SetFloat(_animIDForward, _input.move.y);
                _animator.SetFloat(_animIDStrafe, _input.move.x);
            }
        }
        
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset fall timeout timer
            _fallTimeoutDelta = FallTimeout;
            
            // update animator if using character
            if (_hasAnimator) _animator.SetBool(_animIDIsJumping, false);

            if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;

            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                
                // update animator if using character
                if (_hasAnimator) _animator.SetBool(_animIDIsJumping, true);
            }

            if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;

            // 通过下落延时来确定是否是下落状态有些草率，为什么不用速度
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                // todo: 播放下落动画
            }

            _input.jump = false;
        }

        if (_verticalVelocity < _terminalVelocity) _verticalVelocity += Gravity * Time.deltaTime;
    }

    private void DetectItem()
    {
        // 使用RelayCommand进行异步线程检测
        var results = new NativeArray<RaycastHit>(1, Allocator.TempJob);
        var commands = new NativeArray<RaycastCommand>(1, Allocator.TempJob);

        commands[0] = new RaycastCommand(_mainCamera.transform.position, _mainCamera.transform.forward,
            detectedItemQueryParameters);
        var handle = RaycastCommand.ScheduleBatch(commands, results, 1, 1, default(JobHandle));
        handle.Complete();

        if (results[0].collider == null)
        {
            if (EKeyUI.activeSelf) EKeyUI.SetActive(false);
        }
        else
        {
            if ((results[0].transform.position - CinemachineCameraTarget.transform.position).sqrMagnitude >
                PickUpDistance * PickUpDistance) return;
            EKeyUI.SetActive(true);

            if (_input.pickup)
            {
                results[0].transform.SetParent(_playerCubeModel);
                results[0].transform.position = new Vector3(0.4f, 0.8f, 0.0f);
                results[0].transform.gameObject.layer = PlayerLayer;
            }
        }

        // 忘记进行dispose会使编译器警告
        results.Dispose();
        commands.Dispose();
    }

    private void OpenOrCloseMenu()
    {
        // 使用InputSystem的按键状态后需恢复
        if (!_input.menuOpen) return;
        if (GameManager.gameState == GameState.Run)
            GameManager.gameState = GameState.Pause;
        //TODO: 打开菜单
        else
            GameManager.gameState = GameState.Run;
        _input.menuOpen = false;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}