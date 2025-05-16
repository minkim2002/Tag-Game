using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class Move : NetworkBehaviour
{
    public LayerMask groundLayers; // To detect if the ball is on the ground
    public Transform groundCheck; // A point on the ball to check if it's grounded
    public Rigidbody rb;
    private float groundCheckRadius = 0.1f; // The radius of the ground check
    private bool isGrounded;
    private Animator animator;

    private Transform targetBall;
    private List<Move> teamMembersMulti = new List<Move>();
    private bool isTeamLeader = true;
    private Transform playerCamera;

    private NetworkVariable<bool> canMove = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> canCatch = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float speed = 4f; 
    private float jumpForce = 7f;
    private float catchDistance = 1.5f; 
   
    [SerializeField] private PlayerVisual playerVisual;
    public NetworkVariable<int> playerPlacement = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<NetworkObjectReference> TargetBallReference = new NetworkVariable<NetworkObjectReference>();

    private CameraFollow cameraFollow;

    private bool canSlowTarget = true;
    private float slowEffectDuration = 3f;
    private float slowCooldownTimer = 0f;

    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    private bool canRun = true;
    private bool canRoll = true;
    private float runCooldownTimer = 0f;
    private float rollCooldownTimer = 0f;
    private float boostDuration = 5f;
    private float originalSpeed;
    private float boostTimer = 0f;
    private bool isBoosted = false;



    [SerializeField] private TargetUIManager targetUIManager;
    public static Move LocalInstance { get; private set; }

    void Start()
    {
        originalSpeed = speed;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        animator = GetComponentInChildren<Animator>();
        Debug.Log("Animator found: " + animator);
        if (SceneManager.GetActiveScene().name != "GameScene")
        {
            enabled = false;
            return;
        }

        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(GameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
 
        if (!IsOwner)
        {
            // Disable the camera for non-local players
            playerCamera = transform.Find("PlayerCamera");
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(false);
            }
            return;
        }

        // Assign the camera to the CameraFollow script
        cameraFollow = Camera.main.GetComponent<CameraFollow>();   
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(transform);
        }
    }
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        if (canMove.Value)
        {
            HandleMovement();
        }
        
    }

    private void HandleMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        //// Get the forward direction relative to the camera
        Vector3 cameraForward = cameraFollow.transform.forward;
        Vector3 cameraRight = cameraFollow.transform.right;

        //// Remove the y component to keep movement horizontal
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        //// Calculate the movement direction based on camera orientation
        Vector3 movement = (cameraForward * moveVertical + cameraRight * moveHorizontal) * speed * Time.deltaTime;

        //// Use MovePosition to move the ball without causing it to roll
        rb.MovePosition(rb.position + movement);

        //// Check if the ball is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers);

        // Rotate the pigeon to face the direction of movement (only if there's movement)
        Vector3 lookDirection = cameraForward * moveVertical + cameraRight * moveHorizontal;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Debug.Log("walking");
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        if (animator != null && isGrounded)
        {
            SendAnimationTriggerServerRpc("isWalking", movement.magnitude > 0.01f);
        }

        

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            //animator.SetTrigger("isJumping");
            SendAnimationTriggerServerRpc("isJumping", true);
        }
        if (!isGrounded)
        {
            //animator.SetTrigger("isJumping");
            SendAnimationTriggerServerRpc("isJumping", true);
        }
        if (isGrounded)
        {
            //animator.ResetTrigger("isJumping");
            SendAnimationTriggerServerRpc("isJumping", false);
        }


        if (rb.velocity.y < 0)
        {
            // Falling
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            // Shorter jump when key is released early
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }


        //Allow the player to catch the target ball when pressing 'E'
        if (canCatch.Value && isTeamLeader && targetBall != null && Vector3.Distance(transform.position, targetBall.position) <= catchDistance && Input.GetKeyDown(KeyCode.E))
        {
            CatchTarget();
        }

        if (!canSlowTarget)
        {
            slowCooldownTimer -= Time.deltaTime;
            if (slowCooldownTimer <= 0f)
            {
                canSlowTarget = true;
            }
        }

        if (!isTeamLeader && targetBall != null && Vector3.Distance(transform.position, targetBall.position) <= catchDistance && Input.GetKeyDown(KeyCode.E) && canSlowTarget)
        {
            SlowTargetServerRpc(targetBall.GetComponent<NetworkObject>().NetworkObjectId);
            canSlowTarget = false;
            slowCooldownTimer = 5f;
        }

        // Trigger run
        if (Input.GetKeyDown(KeyCode.T) && canRun)
        {
            speed *= 1.5f;
            isBoosted = true;
            boostTimer = boostDuration;
            canRun = false;
            runCooldownTimer = 5f;
            SendAnimationTriggerServerRpc("isRunning", true);
        }

        // Trigger roll
        if (Input.GetKeyDown(KeyCode.Y) && canRoll)
        {
            speed *= 1.5f;
            isBoosted = true;
            boostTimer = boostDuration;
            canRoll = false;
            rollCooldownTimer = 5f;
            SendAnimationTriggerServerRpc("isRolling", true);
        }

        // Handle boost timer
        if (isBoosted)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f)
            {
                speed = originalSpeed;
                isBoosted = false;

                // Reset animation bools
                SendAnimationTriggerServerRpc("isRunning", false);
                SendAnimationTriggerServerRpc("isRolling", false);
            }
        }

        // Handle cooldowns
        if (!canRun)
        {
            runCooldownTimer -= Time.deltaTime;
            if (runCooldownTimer <= 0f)
                canRun = true;
        }

        if (!canRoll)
        {
            rollCooldownTimer -= Time.deltaTime;
            if (rollCooldownTimer <= 0f)
                canRoll = true;
        }

    }

    [ServerRpc]
    void SendAnimationTriggerServerRpc(string paramName, bool state)
    {
        BroadcastAnimationTriggerClientRpc(paramName, state);
    }

    [ClientRpc]
    void BroadcastAnimationTriggerClientRpc(string paramName, bool state)
    {
        
        animator.SetBool(paramName, state);
        
    }

  

    public void SetCanMove(bool value)
    {
        if (IsServer)
        {
            canMove.Value = value; // Only the server modifies this
        }
    }

    public void SetPlacement(int placement)
    {
        if (IsServer)
        {
            playerPlacement.Value = placement; // Server updates the placement
        }
    }

    public void SetCanCatch(bool value)
    {
        if (IsServer)
        {
            canCatch.Value = value; // Only the server can modify this
        }
    }

    void CatchTarget()
    {
        if (!IsOwner)
        {
            return;
        }

        if (targetBall == null)
        {
            return;
        }

        Move caughtBall = targetBall.GetComponent<Move>();
        if (caughtBall != null)
        {
            CatchTargetServerRpc(caughtBall.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }
 
    [ServerRpc(RequireOwnership = false)]
    private void CatchTargetServerRpc(ulong caughtBallNetworkId)
    {
        Debug.Log($"[Move] ServerRpc called. CaughtBallNetworkId: {caughtBallNetworkId}");

        NotifyCatchClientRpc(caughtBallNetworkId);  
    }

    [ClientRpc]
    private void NotifyCatchClientRpc(ulong caughtBallNetworkId)
    {
        NetworkObject caughtBallObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[caughtBallNetworkId];
        if (caughtBallObject != null)
        {
            Move caughtBall = caughtBallObject.GetComponent<Move>();
            if (caughtBall != null)
            {
                GameManager.Instance.HandlePlayerCaught(this, caughtBall);
                
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SlowTargetServerRpc(ulong targetBallNetworkId)
    {
        Debug.Log($"[Move] ServerRpc called to slow down target ball: {targetBallNetworkId}");
        NotifySlowTargetClientRpc(targetBallNetworkId);
    }

    [ClientRpc]
    private void NotifySlowTargetClientRpc(ulong targetBallNetworkId)
    {
        NetworkObject targetBallObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetBallNetworkId];
        if (targetBallObject != null)
        {
            Move targetMove = targetBallObject.GetComponent<Move>();
            if (targetMove != null)
            {
                StartCoroutine(SlowDownTarget(targetMove));
            }
        }
    }
    private IEnumerator SlowDownTarget(Move targetMove)
    {
        float originalSpeed = targetMove.speed;
        targetMove.speed *= 0.5f;

        yield return new WaitForSeconds(slowEffectDuration);

        targetMove.speed = originalSpeed;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[OnNetworkSpawn] Player {OwnerClientId} | IsOwner: {IsOwner} | IsHost: {IsHost}");

        if (IsOwner)
        {
            LocalInstance = this;
        }
       
    }

    void OnDrawGizmos()
    {
        if (targetBall != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetBall.position);
        }
    }

    public Transform getTargetBall() { return targetBall; }
    public void setTargetBall(Transform transform)
    {
        if (!IsServer)
        {
            return;
        }

        if (transform.TryGetComponent(out NetworkObject targetNetworkObject))
        {
            if (!targetNetworkObject.IsSpawned)
            {
                Debug.LogWarning($"[Move] Tried to assign an unspawned NetworkObject to TargetBallReference for {OwnerClientId}");
                return;
            }
            TargetBallReference.Value = targetNetworkObject;
            Debug.Log($"[Move] Server updated TargetBallReference for Player {OwnerClientId} -> Target {targetNetworkObject.name}");
        }
    }

    private void OnTargetBallChanged(NetworkObjectReference previousValue, NetworkObjectReference newValue)
    {
        if (newValue.TryGet(out NetworkObject targetNetworkObject))
        {
            targetBall = targetNetworkObject.transform;
            Debug.Log($"TargetBall updated for {OwnerClientId}: {targetBall.name}");
            StartCoroutine(WaitForTargetColor()); 
        }
    }

    private void OnEnable()
    {
        TargetBallReference.OnValueChanged += OnTargetBallChanged;
    }

    private void OnDisable()
    {
        TargetBallReference.OnValueChanged -= OnTargetBallChanged;
    }

    public void UpdateTargetUI()
    {
        if (targetBall != null)
        {
            PlayerVisual targetVis = LocalInstance.targetBall.GetComponentInChildren<PlayerVisual>();
            if (targetVis != null)
            {
                Color targetColor = targetVis.GetPlayerColor();
                LocalInstance.targetUIManager.UpdateTargetColor(targetColor);
                Debug.Log($"Updated target UI color to: {targetColor}");
            }
        }
    }

    private IEnumerator WaitForTargetColor()
    {
        yield return new WaitForSeconds(0.1f);
        UpdateTargetUI();
    }

    public List<Move> getTeamMembersMulti() { return teamMembersMulti; }

    public void SetIsTeamleader(bool temp) { isTeamLeader = temp; }

    public void AddToTeamMultiPlayer(Move newMember)
    {
        newMember.SetIsTeamleader(false);
        teamMembersMulti.Add(newMember);
        newMember.GetComponentInChildren<PlayerVisual>().UpdatePlayerColorClientRpc(GetComponentInChildren<PlayerVisual>().GetPlayerColor());
    }
}

