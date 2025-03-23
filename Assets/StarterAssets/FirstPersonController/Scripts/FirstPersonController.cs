using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		[Header("Footstep Sounds")]
		[Tooltip("Array of walking footstep sounds")]
		public AudioClip[] walkingFootstepSounds; // Звуки шагов при ходьбе
		[Tooltip("Array of running footstep sounds")]
		public AudioClip[] runningFootstepSounds; // Звуки шагов при беге
		[Header("Footstep Sounds by Surface")]
		public AudioClip[] stoneWalkingFootstepSounds; // Звуки шагов по камню
		[Tooltip("Array of running footstep sounds on stone")]
		public AudioClip[] stoneRunningFootstepSounds; // Звуки бега по камню

		[Tooltip("Array of walking footstep sounds on wood")]
		public AudioClip[] woodWalkingFootstepSounds; // Звуки шагов по дереву
		[Tooltip("Array of running footstep sounds on wood")]
		public AudioClip[] woodRunningFootstepSounds; // Звуки бега по дереву

		[Tooltip("Delay between footsteps when walking")]
		public float walkingFootstepDelay = 0.5f; // Задержка между шагами при ходьбе
		[Tooltip("Delay between footsteps when running")]
		public float runningFootstepDelay = 0.3f; // Задержка между шагами при беге

		private AudioSource _audioSource; // Источник звука
		private float _footstepTimer; // Таймер для отслеживания задержки между шагами

		[Header("Stamina Settings")]
		[Tooltip("Maximum stamina of the player")]
		public float maxStamina = 100f; // Максимальная стамина
		[Tooltip("Current stamina of the player")]
		public float currentStamina; // Текущая стамина
		[Tooltip("Stamina depletion rate per second while running")]
		public float staminaDepletionRate = 10f; // Скорость уменьшения стамины при беге
		[Tooltip("Stamina regeneration rate per second while not running")]
		public float staminaRegenerationRate = 5f; // Скорость восстановления стамины
		[Tooltip("Minimum stamina required to sprint")]
		public float minStaminaToSprint = 10f; // Минимальная стамина для бега

		[Header("Breathing Sounds")]
		[Tooltip("Sound to play when stamina is fully depleted")]
		public AudioClip outOfBreathSound; // Звук одышки
		[Tooltip("Volume of the out of breath sound")]
		[Range(0, 1)] public float outOfBreathVolume = 1f; // Громкость звука
		private bool _hasPlayedOutOfBreathSound = false; // Флаг, чтобы звук не воспроизводился повторн

			
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
				return _playerInput.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

		// Инициализация аудиоисточника
		_audioSource = gameObject.AddComponent<AudioSource>();
		_audioSource.spatialBlend = 1.0f; // 3D-звук
		_audioSource.playOnAwake = false;

		// Инициализация стамины
    	currentStamina = maxStamina;
		// Сброс таймеров
		_jumpTimeoutDelta = JumpTimeout;
		_fallTimeoutDelta = FallTimeout;
		_footstepTimer = 0f;
		}

		private void Update()
		{
			JumpAndGravity();
			GroundedCheck();
			Move();
			HandleStamina();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private string GetSurfaceType()
		{
			Collider[] colliders = Physics.OverlapSphere(transform.position, GroundedOffset, GroundLayers);
			Debug.Log(Physics.OverlapSphere(transform.position, GroundedOffset, GroundLayers));
			if (colliders.Length > 0)
			{
				Debug.Log("Detected object: " + colliders[0].name);
				return colliders[0].tag;
			}
			return "Default";
		}
		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			// Установка целевой скорости
			float targetSpeed = MoveSpeed; // По умолчанию скорость ходьбы

			// Если игрок пытается бежать и стамина выше 0, и она не была истощена
			if (_input.sprint && currentStamina > 0 && !_isExhausted)
			{
				targetSpeed = SprintSpeed; // Установка скорости бега
			}

			// Если стамина была истощена, игрок не может бежать
			if (_isExhausted)
			{
				_input.sprint = false; // Принудительно отключаем бег
			}

			// Если нет ввода, установите целевую скорость на 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// Вычисление текущей горизонтальной скорости
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			// Ускорение или замедление до целевой скорости
			if (currentHorizontalSpeed < targetSpeed - 0.1f || currentHorizontalSpeed > targetSpeed + 0.1f)
			{
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// Нормализация направления ввода
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// Если есть ввод, поверните игрока
			if (_input.move != Vector2.zero)
			{
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// Перемещение игрока
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

			// Воспроизведение звуков шагов
			if (Grounded && _speed > 0.1f)
			{
				_footstepTimer -= Time.deltaTime;
				if (_footstepTimer <= 0f)
				{
					if (_input.sprint && currentStamina > 0 && !_isExhausted) // Если игрок бежит
					{
						PlayFootstepSound(runningFootstepDelay);
					}
					else // Если игрок идет
					{
						PlayFootstepSound(walkingFootstepDelay);
					}
				}
			}
		}
		private void PlayFootstepSound(float delay)
		{
			string surfaceType = GetSurfaceType();
			Debug.Log("" + surfaceType);
			AudioClip[] footstepSounds = null;

			if (_input.sprint && currentStamina > 0 && !_isExhausted) // Если игрок бежит
			{
				switch (surfaceType)
				{
					case "Stone":
						footstepSounds = stoneRunningFootstepSounds;
						break;
					case "Wood":
						footstepSounds = woodRunningFootstepSounds;
						break;
					default:
						footstepSounds = runningFootstepSounds; // По умолчанию
						break;
				}
			}
			else // Если игрок идет
			{
				switch (surfaceType)
				{
					case "Stone":
						footstepSounds = stoneWalkingFootstepSounds;
						break;
					case "Wood":
						footstepSounds = woodWalkingFootstepSounds;
						break;
					default:
						footstepSounds = walkingFootstepSounds; // По умолчанию
						break;
				}
			}

			if (footstepSounds != null && footstepSounds.Length > 0 && _audioSource != null)
			{
				int index = Random.Range(0, footstepSounds.Length); // Случайный выбор звука
				_audioSource.PlayOneShot(footstepSounds[index]); // Воспроизведение звука
				_footstepTimer = delay; // Установка задержки
			}
		}

		private bool _isExhausted = false; // Флаг, указывающий, что стамина была полностью истощена

		private void HandleStamina()
		{
			if (_input.sprint && _speed > 0.1f && currentStamina > 0) // Если игрок бежит
			{
				currentStamina -= staminaDepletionRate * Time.deltaTime; // Уменьшение стамины
				currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina); // Ограничение стамины

				// Сброс флага, если стамина восстановилась выше 70
				if (currentStamina > 70)
				{
					_hasPlayedOutOfBreathSound = false;
				}
			}
			else if (currentStamina < maxStamina) // Если игрок не бежит
			{
				currentStamina += staminaRegenerationRate * Time.deltaTime; // Восстановление стамины
				currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina); // Ограничение стамины

				// Сброс флага, если стамина восстановилась выше 70
				if (currentStamina > 70)
				{
					_hasPlayedOutOfBreathSound = false;
				}
			}

			// Если стамина достигла нуля, включаем флаг истощения
			if (currentStamina <= 0)
			{
				_isExhausted = true;
				_input.sprint = false; // Отключение бега

				// Воспроизведение звука одышки, если он ещё не был воспроизведён
				if (!_hasPlayedOutOfBreathSound && outOfBreathSound != null)
				{
					_audioSource.PlayOneShot(outOfBreathSound, outOfBreathVolume);
					_hasPlayedOutOfBreathSound = true; // Установка флага, чтобы звук не воспроизводился повторно
				}
			}

			// Если стамина восстановилась до 70%, сбрасываем флаг истощения
			if (_isExhausted && currentStamina >= 70)
			{
				_isExhausted = false;
			}
		}
		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}