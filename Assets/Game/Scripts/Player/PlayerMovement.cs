using NUnit.Framework;
using UnityEngine;
 
public class PlayerMovement : MonoBehaviour
{

    private float currentAngle;
    [SerializeField]
    private float _walkSpeed;
    [SerializeField]
    private InputManager _input;
    private Rigidbody _rigidbody;

    [SerializeField]
    private float _rotationSmoothTime = 0.1f;
    private float _rotationSmoothVelocity;

    [SerializeField]
    private float _sprintSpeed;    
    [SerializeField]
    private float _walkSprintTransition;

    private float _speed;
    private float _standardClimbSpeed;

    [SerializeField]
    private float _jumpForce;

    [SerializeField]
    private Transform _groundDetector;
    
    [SerializeField]
    private float _detectorRadius;
    
    [SerializeField]
    private LayerMask _groundLayer;
    private bool _isGrounded;

    [SerializeField]
    private Vector3 _upperStepOffset;
    
    [SerializeField]
    private float _stepCheckerDistance;
    
    [SerializeField]
    private float _stepForce;


    [SerializeField]
    private Transform _climbDetector;

    [SerializeField]
    private float _climbCheckDistance;

    [SerializeField]
    private LayerMask _climbableLayer;

    [SerializeField]
    private Vector3 _climbOffset;

    private PlayerStance _playerStance;

    [SerializeField]
    private float _climbSpeed;
    [SerializeField]
    private float _climbSprintSpeed;
        [SerializeField]
    private float _climbSprintTransition;


	private void Start()
    {
        _input.OnMoveInput += Move;
        _input.OnSprintInput += Sprint;
         _input.OnClimbInput += StartClimb;
         _input.OnCancelClimb += CancelClimb;
    }

    private void Update()
    {
        CheckIsGrounded();
        CheckStep();
    }
    
    private void OnDestroy()
    {
        _input.OnMoveInput -= Move;
        _input.OnSprintInput -= Sprint;
        _input.OnJumpInput -= Jump;
        _input.OnClimbInput -= StartClimb;
        _input.OnCancelClimb -= CancelClimb;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _speed = _walkSpeed;
        _standardClimbSpeed = _climbSpeed;
        _input.OnJumpInput += Jump;
        _playerStance = PlayerStance.Stand;
    }

    private void Move(Vector2 axisDirection)
    {
        Vector3 movementDirection = Vector3.zero;
        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;
    
        if (isPlayerStanding)
        {
            if (axisDirection.magnitude >= 0.1)
            {
                float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                _rigidbody.AddForce(movementDirection * Time.deltaTime * _speed);
            }
        }
        else if (isPlayerClimbing)
        {
            if(!IsInFrontOfClimbingWall()){
                CancelClimb();
            }else{
                Vector3 horizontal = axisDirection.x * transform.right;
                Vector3 vertical = axisDirection.y * transform.up;
                movementDirection = horizontal + vertical;
                _rigidbody.AddForce(movementDirection * Time.deltaTime * _climbSpeed);
            }
            
        }
    }



    private void Sprint(bool isSprint)
    {

        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;

        if (isPlayerStanding)
        {
            if (isSprint)
            {
                Debug.Log("Sprinting");
                if (_speed < _sprintSpeed)
                {
                    _speed = _speed + _walkSprintTransition * Time.deltaTime;
                }
            }
            else
            {
                Debug.Log("Not Sprinting");
                if (_speed > _walkSpeed)
                {
                    _speed = _speed - _walkSprintTransition * Time.deltaTime;
                }
            }
        }
        else if (isPlayerClimbing)
        {
            Debug.Log(_climbSpeed);
            if (isSprint)
            {
                Debug.Log("Sprinting on Climb");
                if (_climbSpeed < _climbSprintSpeed)
                {
                    _climbSpeed = _climbSpeed + _climbSprintTransition * Time.deltaTime;
                }
            }
            else
            {
                Debug.Log("Not Sprinting on Climb");
                if (_climbSpeed > _standardClimbSpeed)
                {
                    _climbSpeed = _climbSpeed - _climbSprintTransition * Time.deltaTime;
                }
            }
        }
        
    }


    
    private void Jump()
    {
        Vector3 jumpValue;
        if (_isGrounded)
        {
            Vector3 jumpDirection = Vector3.up;
            // Face upward jump direction horizontally (no yaw change)
            Vector3 lookDir = new Vector3(jumpDirection.x, 0f, jumpDirection.z);
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }
            jumpValue = jumpDirection * _jumpForce * Time.deltaTime;
            Debug.Log(jumpValue);
            _rigidbody.AddForce(jumpValue);
        }else if (_playerStance == PlayerStance.Climb)
        {
            CancelClimb();
            
            Vector3 jumpDirection = (Vector3.up - transform.forward).normalized;
            // Rotate to face the horizontal component of the jump direction
            Vector3 horizontalJump = new Vector3(jumpDirection.x, 0f, jumpDirection.z);
            if (horizontalJump.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(horizontalJump);
            }

            jumpValue = jumpDirection * _jumpForce * Time.deltaTime;
            Debug.Log(jumpValue);
            _rigidbody.AddForce(jumpValue);
        }
    }

 
    private void CheckIsGrounded()
    {
        _isGrounded = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _groundLayer);
    }


    private void CheckStep()
    {
        bool isHitLowerStep = Physics.Raycast(_groundDetector.position,
                                                transform.forward,
                                                _stepCheckerDistance);
        bool isHitUpperStep = Physics.Raycast(_groundDetector.position +
                                                _upperStepOffset,
                                                transform.forward,
                                                _stepCheckerDistance);
        if (isHitLowerStep && !isHitUpperStep)
        {
            Debug.Log("Step Up");
            _rigidbody.AddForce(0, _stepForce, 0);
        }
    }

    private bool IsInFrontOfClimbingWall()
    {
       return Physics.Raycast(_climbDetector.position,
                            transform.forward,
                            out RaycastHit hit,
                            _climbCheckDistance,
                            _climbableLayer);
    }

    private void StartClimb()
    {
        Physics.Raycast(_climbDetector.position,
                        transform.forward,
                        out RaycastHit hit,
                        _climbCheckDistance,
                        _climbableLayer);
        bool isNotClimbing = _playerStance != PlayerStance.Climb;
        
        if (IsInFrontOfClimbingWall() && _isGrounded && isNotClimbing)
        {
            Vector3 offset = (transform.forward * _climbOffset.z) + (Vector3.up * _climbOffset.y);
            transform.position = hit.point - offset;
            _playerStance = PlayerStance.Climb;
            _rigidbody.useGravity = false;
        }
    }   

    private void CancelClimb()
    {
        if (_playerStance == PlayerStance.Climb)
        {
            _playerStance = PlayerStance.Stand;
            _rigidbody.useGravity = true;
            transform.position -= transform.forward * 0.5f; // Adjusted from 1f to 0.5f
            _climbSpeed = _standardClimbSpeed;
        }
    }

}