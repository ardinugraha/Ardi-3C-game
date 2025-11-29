
using System.Collections;
using NUnit.Framework;
using UnityEngine;
 
public class PlayerMovement : MonoBehaviour
{


    [SerializeField]
    private CameraManager _cameraManager;
    [SerializeField]
    private Transform _cameraTransform;
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
    [SerializeField]
    private float _crouchSpeed;

    private Animator _animator;

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
    private CapsuleCollider _collider;

    [SerializeField]
    private float _glideSpeed;
    
    [SerializeField]
    private float _airDrag;

    [SerializeField]
    private Vector3 _glideRotationSpeed;
    
    [SerializeField]
    private float _minGlideRotationX;
    
    [SerializeField]
    private float _maxGlideRotationX;

    [SerializeField]
    private CheckingStance _checkingStance;
    private bool _isPunching;
    private int _combo = 0;
    [SerializeField]
    private float _resetComboInterval;
    private Coroutine _resetCombo;

    [SerializeField]
    private Transform _hitDetector;
    
    [SerializeField]
    private float _hitDetectorRadius;
    
    [SerializeField]
    private LayerMask _hitLayer;

    [SerializeField]
    private float _getUpDistance;

    [SerializeField]
    private Transform _topDetector;


	private void Start()
    {
        _input.OnMoveInput += Move;
        _input.OnSprintInput += Sprint;
         _input.OnClimbInput += StartClimb;
         _input.OnCancelClimb += CancelClimb;
         _input.OnCrouchInput += Crouch;
         _cameraManager.OnChangePerspective += ChangePerspective;
         _input.OnGlideInput += StartGlide;
        _input.OnCancelGlide += CancelGlide;
        _input.OnPunchInput += Punch;
    }

    private void Update()
    {
        CheckIsGrounded();
        CheckStep();
        Glide();
    }
    
    private void OnDestroy()
    {
        _input.OnMoveInput -= Move;
        _input.OnSprintInput -= Sprint;
        _input.OnJumpInput -= Jump;
        _input.OnClimbInput -= StartClimb;
        _input.OnCancelClimb -= CancelClimb;
        _cameraManager.OnChangePerspective -= ChangePerspective;
        _input.OnCrouchInput -= Crouch;
        _input.OnGlideInput -= StartGlide;
        _input.OnCancelGlide -= CancelGlide;
        _input.OnPunchInput -= Punch;
        if (_resetCombo != null)
        {
            StopCoroutine(_resetCombo);
            _resetCombo = null;
        }
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _speed = _walkSpeed;
        _standardClimbSpeed = _climbSpeed;
        _input.OnJumpInput += Jump;
        _playerStance = PlayerStance.Stand;
        _checkingStance = CheckingStance.Enable;
        _animator = GetComponent<Animator>();
        _collider = GetComponent<CapsuleCollider>();
        HideAndLockCursor();
    }

    private void Move(Vector2 axisDirection)
    {
        Vector3 movementDirection = Vector3.zero;
        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;
        bool isPlayerCrouching = _playerStance == PlayerStance.Crouch;
        bool isPlayerGliding = _playerStance == PlayerStance.Glide;
    
        if (isPlayerStanding && !_isPunching)
        {
            Vector3 velocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
            _animator.SetFloat("Velocity", axisDirection.magnitude * velocity.magnitude);
            _animator.SetFloat("VelocityZ", velocity.magnitude * axisDirection.y);
            _animator.SetFloat("VelocityX", velocity.magnitude * axisDirection.x);
            switch (_cameraManager.CameraState)
            {
                case CameraState.ThirdPerson:
                    if (axisDirection.magnitude >= 0.1)
                    {
                        float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
                        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                        movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                        _rigidbody.AddForce(movementDirection * Time.deltaTime * _speed);
                    }
                    break;
                case CameraState.FirstPerson:
                    transform.rotation = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f);
                    Vector3 verticalDirection = axisDirection.y * transform.forward;
                    Vector3 horizontalDirection = axisDirection.x * transform.right;
                    movementDirection = verticalDirection + horizontalDirection;
                    _rigidbody.AddForce(movementDirection * Time.deltaTime * _speed);
                    break;
                default:
                    break;
            }
        }
        else if (isPlayerClimbing)
        {
            if(_checkingStance == CheckingStance.Enable)
            {
                if(!IsInFrontOfClimbingWall()){
                    CancelClimb();
                }else{
                    Vector3 horizontal = axisDirection.x * transform.right;
                    Vector3 vertical = axisDirection.y * transform.up;
                    movementDirection = horizontal + vertical;
                    _rigidbody.AddForce(movementDirection * Time.deltaTime * _climbSpeed);
                    Vector3 velocity = new Vector3(_rigidbody.linearVelocity.x, _rigidbody.linearVelocity.y, 0);
                    _animator.SetFloat("ClimbVelocityY", velocity.magnitude * axisDirection.y);
                    _animator.SetFloat("ClimbVelocityX", velocity.magnitude * axisDirection.x);
                }
            }
            
        }
        else if (isPlayerCrouching && !_isPunching)
        {
            Vector3 velocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
            _animator.SetFloat("Velocity", axisDirection.magnitude * velocity.magnitude);
            _animator.SetFloat("VelocityZ", velocity.magnitude * axisDirection.y);
            _animator.SetFloat("VelocityX", velocity.magnitude * axisDirection.x);
            switch (_cameraManager.CameraState)
            {
                case CameraState.ThirdPerson:
                    if (axisDirection.magnitude >= 0.1)
                    {
                        float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
                        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                        movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                        _rigidbody.AddForce(movementDirection * Time.deltaTime * _crouchSpeed);
                    }
                    break;
                case CameraState.FirstPerson:
                    transform.rotation = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f);
                    Vector3 verticalDirection = axisDirection.y * transform.forward;
                    Vector3 horizontalDirection = axisDirection.x * transform.right;
                    movementDirection = verticalDirection + horizontalDirection;
                    _rigidbody.AddForce(movementDirection * Time.deltaTime * _crouchSpeed);
                    break;
                default:
                    break;
            }
        }
        else if (isPlayerGliding)
        {
            //Debug.Log("Gliding Movement");
            Vector3 rotationDegree = transform.rotation.eulerAngles;
            rotationDegree.x += _glideRotationSpeed.x * axisDirection.y;
            rotationDegree.x = Mathf.Clamp(rotationDegree.x, _minGlideRotationX, _maxGlideRotationX);
            rotationDegree.z += _glideRotationSpeed.z * axisDirection.x;
            rotationDegree.y += _glideRotationSpeed.y * axisDirection.x;
            //Debug.Log(rotationDegree);
            Vector3 position = transform.position;
            //Debug.Log("Glide Position: " + position);
            transform.rotation = Quaternion.Euler(rotationDegree);
        }
    }
private void Crouch()
{
    //Debug.Log("Crouch Toggled");
    if (_playerStance == PlayerStance.Stand)
    {
        _collider.height = 1.3f;
        _collider.center = Vector3.up * 0.66f;
        _playerStance = PlayerStance.Crouch;
        _animator.SetBool("IsCrouch", true);
        _speed = _crouchSpeed;
    }
    else if (_playerStance == PlayerStance.Crouch)
    {
        if(!IsTopCollide()){
            _collider.height = 1.8f;
            _collider.center = Vector3.up * 0.9f;
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("IsCrouch", false);
            _speed = _walkSpeed;
        }else{
            Debug.Log("Cant Stand");
        }
        
    }
}

	private void ChangePerspective()
    {
        _animator.SetTrigger("ChangePerspective");
    }


    private void Sprint(bool isSprint)
    {

        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;

        if (isPlayerStanding)
        {
            if (isSprint)
            {
                //Debug.Log("Sprinting");
                if (_speed < _sprintSpeed)
                {
                    _speed = _speed + _walkSprintTransition * Time.deltaTime;
                }
            }
            else
            {
                //Debug.Log("Not Sprinting");
                if (_speed > _walkSpeed)
                {
                    _speed = _speed - _walkSprintTransition * Time.deltaTime;
                }
            }
        }
        else if (isPlayerClimbing)
        {
            
            //Debug.Log(_climbSpeed);
            if (isSprint)
            {
                //Debug.Log("Sprinting on Climb");
                if (_climbSpeed < _climbSprintSpeed)
                {
                    _climbSpeed = _climbSpeed + _climbSprintTransition * Time.deltaTime;
                }
            }
            else
            {
                //Debug.Log("Not Sprinting on Climb");
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
            
            _animator.SetTrigger("Jump");
            
            Vector3 jumpDirection = Vector3.up;
            // Face upward jump direction horizontally (no yaw change)
            Vector3 lookDir = new Vector3(jumpDirection.x, 0f, jumpDirection.z);
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }
            jumpValue = jumpDirection * _jumpForce ;
            //Debug.Log(jumpValue);
            _rigidbody.AddForce(jumpValue);
        }else if (_playerStance == PlayerStance.Climb)
        {

            _animator.SetTrigger("Jump");
            
            CancelClimb();
            
            Vector3 jumpDirection = (Vector3.up - transform.forward).normalized;
            // Rotate to face the horizontal component of the jump direction
            Vector3 horizontalJump = new Vector3(jumpDirection.x, 0f, jumpDirection.z);
            if (horizontalJump.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(horizontalJump);
            }

            jumpValue = jumpDirection * _jumpForce ;
            //Debug.Log(jumpValue);
            _rigidbody.AddForce(jumpValue);
        }
    }

 
    private void CheckIsGrounded()
    {
        _isGrounded = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _groundLayer);
        _animator.SetBool("IsGrounded", _isGrounded);
        if (_isGrounded)
        {
            CancelGlide();
        }else{
            _animator.ResetTrigger("Jump");
            //Debug.Log("Not Grounded");
        }   
        
    }

    private void HideAndLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
            //Debug.Log("Step Up");
            _rigidbody.AddForce(0, _stepForce, 0);
        }
    }

    private bool IsInFrontOfClimbingWall()
    {
        bool IsInFrontOfClimbingWall = Physics.Raycast(_climbDetector.position,
                                        transform.forward,
                                        out RaycastHit hit,
                                        _climbCheckDistance,
                                        _climbableLayer);
        
       return IsInFrontOfClimbingWall;
    }

    private bool IsTopCollide()
    {
        bool IsTopCollide = Physics.Raycast(_topDetector.position,
                                        transform.up,
                                        out RaycastHit hit,
                                        _getUpDistance,
                                        _groundLayer);
       return IsTopCollide;
    }


    public void OnDrawGizmos()
    {
        DrawGizmosInFrontOfClimbingWall();
        DrawGizmosIsGrounded();
        DrawGizmosIsTopCollide();
                        
    }

    public void DrawGizmosInFrontOfClimbingWall()
    {
        //bool paused = false;
        bool IsInFrontOfClimbingWall = Physics.Raycast(_climbDetector.position,
                                        transform.forward,
                                        out RaycastHit hit,
                                        _climbCheckDistance,
                                        _climbableLayer);
        //Gizmos.color = Color.red;
        
        if (IsInFrontOfClimbingWall)
        {
            //Debug.Log("In front of climbing wall");
            Gizmos.color = Color.white;
        }else{
            Gizmos.color = Color.red;
            //paused = true;
        }
        Gizmos.DrawLine(_climbDetector.position,
                        _climbDetector.position + transform.forward * _climbCheckDistance);
        // if(paused){
        //     Debug.Break(); 
        // }
    }

    public void DrawGizmosIsGrounded()
    {
        bool isGrounded = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _groundLayer);
        if (isGrounded)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.blue;
        }
        Gizmos.DrawSphere(_groundDetector.position,_detectorRadius);
    }

    public void DrawGizmosIsTopCollide()
    {
        bool IsTopCollide = Physics.Raycast(_topDetector.position,
                                        transform.up,
                                        out RaycastHit hit,
                                        _getUpDistance,
                                        _groundLayer);
        if (IsTopCollide)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.blue;
        }
         Gizmos.DrawLine(_topDetector.position,
                        _topDetector.position + transform.up * _getUpDistance);
    }




    private void StartClimb()
    {
        Physics.Raycast(_climbDetector.position,
                        transform.forward,
                        out RaycastHit hit,
                        _climbCheckDistance,
                        _climbableLayer);
        bool isNotClimbing = _playerStance != PlayerStance.Climb;

        _cameraManager.SetTPSFieldOfView(70);
        
        if (IsInFrontOfClimbingWall() && _isGrounded && isNotClimbing)
        {
            _checkingStance = CheckingStance.Disable;
            Vector3 offset = (transform.forward * _climbOffset.z) + (Vector3.up * _climbOffset.y);
            
            transform.position = hit.point - offset;
            
            _playerStance = PlayerStance.Climb;
            _rigidbody.useGravity = false;
            _animator.SetBool("IsClimbing", true);
            _collider.center = Vector3.up * 1.3f;
            _checkingStance = CheckingStance.Enable;
        }
    }   

    private void CancelClimb()
    {
        _cameraManager.SetTPSFieldOfView(60);
        if (_playerStance == PlayerStance.Climb)
        {
            _playerStance = PlayerStance.Stand;
            _rigidbody.useGravity = true;
            transform.position -= transform.forward * 0.5f; // Adjusted from 1f to 0.5f
            _climbSpeed = _standardClimbSpeed;
            _animator.SetBool("IsClimbing", false);
            _collider.center = Vector3.up * 0.9f;
        }
    }

    private void StartGlide()
    {
        if (_playerStance != PlayerStance.Glide && !_isGrounded)
        {
            //Debug.Log("Start Gliding");
            _cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            _playerStance = PlayerStance.Glide;
            _animator.SetBool("IsGliding", true);
        }
    }
 
    private void CancelGlide()
    {
        if (_playerStance == PlayerStance.Glide)
        {
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("IsGliding", false);
        }
    }

    private void Glide()
    {
        if (_playerStance == PlayerStance.Glide)
        {
            Vector3 playerRotation = transform.rotation.eulerAngles;
            float lift = playerRotation.x;
            Vector3 upForce = transform.up * (lift + _airDrag);
            Vector3 forwardForce = transform.forward * _glideSpeed;
            Vector3 totalForce = upForce + forwardForce;
            _rigidbody.AddForce(totalForce * Time.deltaTime);
        }
    }

    private void Punch()
    {
        if (!_isPunching && _playerStance == PlayerStance.Stand)
        {
            _isPunching = true;
            if (_combo < 3)
            {
                _combo = _combo + 1;
            }
            else
            {
                _combo = 1;
            }
            _animator.SetInteger("Combo", _combo);
            _animator.SetTrigger("Punch");
        }
    }

    private void EndPunch()
    {
        _isPunching = false;
        if (_resetCombo != null)
        {
            StopCoroutine(_resetCombo);
        }
        _resetCombo = StartCoroutine(ResetCombo());
    }

    private IEnumerator ResetCombo()
    {
        yield return new WaitForSeconds(_resetComboInterval);
        _combo = 0;
        _resetCombo = null;
    }

    private void Hit()
    {
        Collider[] hitObjects = Physics.OverlapSphere(_hitDetector.position,
        _hitDetectorRadius,
        _hitLayer);
        for (int i = 0; i < hitObjects.Length; i++)
        {
            if (hitObjects[i].gameObject != null)
            {
                Destroy(hitObjects[i].gameObject);
            }
        }
    }

}