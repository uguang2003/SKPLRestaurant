using UnityEngine;

namespace Whilefun.FPEKit {

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(FPEMouseLook))]

    public class FPEFirstPersonController : MonoBehaviour
    {

        [Header("Movement Speeds")]
        [SerializeField]
        private float m_WalkSpeedCrouching = 2.0f;
        [SerializeField]
        private float m_RunSpeedCrouching = 2.0f;
        [SerializeField]
        private float m_WalkSpeedStanding = 4.0f;
        [SerializeField]
        private float m_RunSpeedStanding = 10.0f;

        private bool isWalking = true;

        // These impact the rate at which the cumulativeStepCycleCount is increased with steps of respective style.
        [Header("Movement Stride Sizes (For Audio)")]
        [SerializeField]
        private float walkStepLengthCrouching = 3.5f;
        [SerializeField]
        private float runStepLengthCrouching = 3.0f;
        [SerializeField]
        private float walkStepLengthStanding = 1.0f;
        [SerializeField]
        private float runStepLengthStanding = 0.7f;

        [Header("Jumping")]
        [SerializeField]
        private float m_JumpSpeed = 8.0f;
        [SerializeField]
        private float m_StickToGroundForce = 10.0f;
        [SerializeField]
        private float m_GravityMultiplier = 2.0f;
        [SerializeField, Tooltip("Minimum downward speed required before player is considered to have landed from a jump or fall. Default is -1;")]
        private float minimumFallSpeed = -1.0f;

        // Walking things
        private float cumulativeStepCycleCount = 0.0f;
        private float nextStepInCycle = 0.0f;
        [SerializeField, Tooltip("Approximate unit length of complete stride (left and right foot) of player walk. Influenced by current speed and movement type (e.g. walking vs. running)")]
        private float stepInterval = 5.0f;

        [Header("View Bob")]
        [SerializeField]
        private bool cameraBobEnabled = true;
        [SerializeField]
        private FPELerpControlledBob m_JumpBob = new FPELerpControlledBob();
        [SerializeField]
        private AnimationCurve CameraBobCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f), new Keyframe(1.5f, -1f), new Keyframe(2f, 0f));
        [SerializeField]
        private Vector2 bobRangeStanding = new Vector2(0.05f, 0.1f);
        [SerializeField]
        private Vector2 bobRangeCrouching = new Vector2(0.2f, 0.2f);

        [Header("Audio")]
        [SerializeField]
        private FPESoundBank footstepSounds = null;
        [SerializeField]
        private FPESoundBank jumpSounds = null;
        [SerializeField]
        private FPESoundBank landingSounds = null;

        private Camera m_Camera = null;
        private bool m_Jump = false;
        //private float m_YRotation = 0.0f;
        private Vector2 m_Input = Vector2.zero;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController = null;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded = true;
        private bool m_Jumping = false;
        private AudioSource m_AudioSource = null;

        // Crouching Stuff //
        private bool isCrouching = false;
        private float standingHeight = 0.0f;
        private float crouchingHeight = 0.0f;
        private float previousCharacterHeight = 0.0f;
        //The denominator with which we divide "standing" height to determine crouching height
        private float characterCrouchDivisor = 2.0f;
        private CharacterController controller;
        // Crouching camera stuff
        private Vector3 cameraOffsetStanding = Vector3.zero;
        private Vector3 cameraOffsetCrouching = Vector3.zero;

        private float bobCurveTime = 0.0f;
        private float bobCycleX = 0.0f;
        private float bobCycleY = 0.0f;

        private bool movementEnabled = true;
        private float currentSpeed = 0.0f;
        private bool movementStarted = false;

        // Custom stuff for my own version of this stanard script
        [Header("Custom Flags for Movement Options")]
        [Tooltip("Toggle movement sounds on and off")]
        public bool enableMovementSounds = false;
        [SerializeField, Tooltip("Toggle jump ability on and off")]
        private bool jumpEnabled = false;
        [SerializeField, Tooltip("Toggle run ability on and off")]
        private bool runEnabled = true;
        [SerializeField, Tooltip("Toggle crouch ability on and off")]
        private bool crouchEnabled = true;
        [SerializeField, Tooltip("If true, crouch will be a toggle")]
        private bool crouchAsToggle = true;

        // Player docking stuff
        [SerializeField, Tooltip("Distance at which the player will stop moving and snap to dock and undock positions")]
        private float dockSnapDistance = 0.1f;
        [SerializeField, Tooltip("Rate at which the player will smoothly move to dock and undock positions (larger number means quicker movement)")]
        private float dockingLerpFactor = 5.0f;

        private bool playerDocked = false;
        public enum ePlayerDockingState
        {
            IDLE = 0,
            DOCKING,
            UNDOCKING
        }
        private ePlayerDockingState currentDockingState = ePlayerDockingState.IDLE;

        private Vector2 targetMaxAngles = Vector2.zero;
        private Vector3 targetFocalPoint = Vector3.zero;
        private Vector3 targetDockPosition = Vector3.zero;
        private Vector3 previousFocalPoint = Vector3.zero;
        private Vector3 previousWorldPosition = Vector3.zero;

        public bool playerFrozen = false;

        private FPEMouseLook myMouseLook = null;
        private FPEInputManager myInputManager = null;


        void Awake()
        {

            controller = gameObject.GetComponent<CharacterController>();

            standingHeight = controller.height;
            crouchingHeight = standingHeight / characterCrouchDivisor;
            previousCharacterHeight = standingHeight;

            cameraOffsetStanding = Camera.main.transform.localPosition;
            cameraOffsetCrouching = cameraOffsetStanding;
            cameraOffsetCrouching.y -= 0.6f;

            bobCurveTime = CameraBobCurve[CameraBobCurve.length - 1].time;

        }


        private void Start()
        {

            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;

            cumulativeStepCycleCount = 0.0f;
            nextStepInCycle = cumulativeStepCycleCount + stepInterval;

            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();

            myMouseLook = gameObject.GetComponent<FPEMouseLook>();

            if (!myMouseLook)
            {
                Debug.LogError("FPEFirstPersonController:: Cannot find FPEMouseLook component on the player! Did you break the prefab?");
            }

            myMouseLook.Init(transform, m_Camera.transform);

            myInputManager = FPEInputManager.Instance;

            if (!myInputManager)
            {
                Debug.LogError("FPEFirstPersonController:: Cannot find an instance of FPEInputManager in the scene. Player movement look will not function correctly!");
            }

        }


        private void Update()
        {

            if (playerFrozen)
            {
                // Nothing
            }
            else if (playerDocked)
            {

                if (currentDockingState == ePlayerDockingState.DOCKING)
                {

                    if(Vector3.Distance(transform.position, targetDockPosition) < dockSnapDistance)
                    {

                        transform.position = targetDockPosition;
                        myMouseLook.LookAtPosition(transform, m_Camera.transform, targetFocalPoint);
                        myMouseLook.enableLookRestriction(targetMaxAngles);
                        myMouseLook.enableMouseLook = true;
                        currentDockingState = ePlayerDockingState.IDLE;

                    }

                }
                else if (currentDockingState == ePlayerDockingState.UNDOCKING)
                {

                    if (Vector3.Distance(transform.position, targetDockPosition) < dockSnapDistance)
                    {

                        transform.position = targetDockPosition;
                        myMouseLook.LookAtPosition(transform, m_Camera.transform, targetFocalPoint);
                        myMouseLook.disableLookRestriction();
                        myMouseLook.enableMouseLook = true;
                        currentDockingState = ePlayerDockingState.IDLE;
                        playerDocked = false;

                    }

                }
                else
                {
                    RotateView();
                }

            }
            else
            {

                RotateView();

                // Only jump if we are allowed, not already jumping, and not crouched
                if (jumpEnabled && !m_Jump && !isCrouching)
                {

                    // Workaround for conflicting jump and menu buttons when using XBox controller. When assigned bumper is pressed when menu is open, player jumps when menu is closed.
                    if (Time.timeScale != 0.0f && m_CharacterController.isGrounded)
                    {
                        m_Jump = myInputManager.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_JUMP);
                    }

                }

                if (!m_PreviouslyGrounded && m_CharacterController.isGrounded && (m_MoveDir.y < minimumFallSpeed))
                {

                    StartCoroutine(m_JumpBob.DoBobCycle());
                    PlayLandingSound();
                    m_MoveDir.y = 0f;
                    m_Jumping = false;

                }

                if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
                {
                    m_MoveDir.y = 0f;
                }

                m_PreviouslyGrounded = m_CharacterController.isGrounded;

                // Crouch based on crouch method (toggle vs. hold down)
                if (crouchEnabled)
                {

                    if (movementEnabled)
                    {

                        if (crouchAsToggle)
                        {

                            if (myInputManager.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_CROUCH))
                            {
                                if (isCrouching)
                                {
                                    if (haveHeadRoomToStand())
                                    {
                                        isCrouching = false;
                                    }
                                }
                                else
                                {
                                    isCrouching = true;
                                }
                            }
                        }
                        else
                        {
                            if (myInputManager.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_CROUCH))
                            {
                                isCrouching = true;
                            }
                            else
                            {
                                if (isCrouching)
                                {
                                    if (haveHeadRoomToStand())
                                    {
                                        isCrouching = false;
                                    }
                                }
                            }
                        }

                    }

                }
                else
                {
                    // Set it to false here in case crouching is disabled during play, and player was mid-crouch
                    isCrouching = false;
                }

                // Crouching stuff
                previousCharacterHeight = controller.height;

                // Footstep audio special case: If player moves a little, but not a "full stride", there should still be a foot step sound. And if they just stopped walking, there should also be one
                if (myInputManager.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_HORIZONTAL) || myInputManager.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_VERTICAL) && !movementStarted)
                {
                    movementStarted = true;
                    cumulativeStepCycleCount = 0.0f;
                    nextStepInCycle = cumulativeStepCycleCount + stepInterval;
                    PlayFootStepAudio();
                }
                if (myInputManager.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_HORIZONTAL) || myInputManager.GetButtonDown(FPEInputManager.eFPEInput.FPE_INPUT_VERTICAL) && movementStarted)
                {
                    movementStarted = false;
                    PlayFootStepAudio();
                }

                ProgressStepCycle(currentSpeed);
                UpdateCameraPosition(currentSpeed);

            }

        }


        private void FixedUpdate()
        {

            currentSpeed = 0.0f;
            GetInput(out currentSpeed);

            if (playerFrozen)
            {
                // Nothing
            }
            else if (playerDocked)
            {

                if (currentDockingState == ePlayerDockingState.DOCKING)
                {

                    transform.position = Vector3.Lerp(transform.position, targetDockPosition, dockingLerpFactor * Time.fixedDeltaTime);
                    myMouseLook.LookAtPosition(transform, m_Camera.transform, targetFocalPoint);

                }
                else if (currentDockingState == ePlayerDockingState.UNDOCKING)
                {

                    transform.position = Vector3.Lerp(transform.position, targetDockPosition, dockingLerpFactor * Time.fixedDeltaTime);
                    myMouseLook.LookAtPosition(transform, m_Camera.transform, targetFocalPoint);

                }

            }
            else
            {

                if (isCrouching)
                {
                    gameObject.GetComponent<CharacterController>().height = Mathf.Lerp(gameObject.GetComponent<CharacterController>().height, crouchingHeight, 5 * Time.fixedDeltaTime);
                }
                else
                {
                    gameObject.GetComponent<CharacterController>().height = Mathf.Lerp(gameObject.GetComponent<CharacterController>().height, standingHeight, 5 * Time.fixedDeltaTime);
                }

                // We move the transform to be the x/z and exactly middle of Y relative to controller height change from crouch/stand
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + (controller.height - previousCharacterHeight) / 2, gameObject.transform.position.z);

                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = Vector3.zero;

                if (movementEnabled)
                {
                    desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;
                }

                // get a normal for the surface that is being touched to move along it
                RaycastHit hitInfo;
                Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo, m_CharacterController.height / 2f);
                desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

                m_MoveDir.x = desiredMove.x * currentSpeed;
                m_MoveDir.z = desiredMove.z * currentSpeed;

                if (m_CharacterController.isGrounded)
                {

                    m_MoveDir.y = -m_StickToGroundForce;

                    if (m_Jump)
                    {

                        m_MoveDir.y = m_JumpSpeed;
                        PlayJumpSound();
                        m_Jump = false;
                        m_Jumping = true;

                    }

                }
                else
                {
                    m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
                }

                m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

            }

        }


        private void UpdateCameraPosition(float speed)
        {

            if (cameraBobEnabled)
            {

                if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
                {

                    float xOffset = CameraBobCurve.Evaluate(bobCycleX);
                    float yOffset = CameraBobCurve.Evaluate(bobCycleY);

                    Vector3 newCameraPosition = cameraOffsetStanding;
                    Vector2 bobRange = bobRangeStanding;

                    if (isCrouching)
                    {
                        newCameraPosition = cameraOffsetCrouching;
                        bobRange = bobRangeCrouching;
                    }

                    newCameraPosition.y += (yOffset * bobRange.y) - m_JumpBob.Offset();
                    newCameraPosition.x += xOffset * bobRange.x;

                    // Update bob cycle
                    float VerticalToHorizontalRatioStanding = 2.0f;
                    bobCycleX += (speed * Time.deltaTime) / stepInterval;
                    bobCycleY += ((speed * Time.deltaTime) / stepInterval) * VerticalToHorizontalRatioStanding;

                    if (bobCycleX > bobCurveTime)
                    {
                        bobCycleX = bobCycleX - bobCurveTime;
                    }
                    if (bobCycleY > bobCurveTime)
                    {
                        bobCycleY = bobCycleY - bobCurveTime;
                    }

                    // Lastly, lerp toward our new target camera position
                    m_Camera.transform.localPosition = Vector3.Lerp(m_Camera.transform.localPosition, newCameraPosition, 0.1f);


                }
                else
                {

                    // If we aren't actively moving or bobbing, just lerp toward our appropriate camera position
                    if (isCrouching)
                    {
                        Vector3 newCameraPosition = cameraOffsetCrouching;
                        newCameraPosition.y -= m_JumpBob.Offset();
                        m_Camera.transform.localPosition = Vector3.Lerp(m_Camera.transform.localPosition, newCameraPosition, 0.1f);
                    }
                    else
                    {
                        Vector3 newCameraPosition = cameraOffsetStanding;
                        newCameraPosition.y -= m_JumpBob.Offset();
                        m_Camera.transform.localPosition = Vector3.Lerp(m_Camera.transform.localPosition, newCameraPosition, 0.1f);
                    }

                }

            }
            else
            {

                if (isCrouching)
                {
                    m_Camera.transform.localPosition = Vector3.Lerp(m_Camera.transform.localPosition, cameraOffsetCrouching, 0.1f);
                }
                else
                {
                    m_Camera.transform.localPosition = Vector3.Lerp(m_Camera.transform.localPosition, cameraOffsetStanding, 0.1f);
                }

            }


        }

        private void PlayJumpSound()
        {

            if (enableMovementSounds)
            {
                jumpSounds.Play(m_AudioSource);
            }

        }

        private void PlayLandingSound()
        {

            // We check timeSinceLevelLoad to prevent perpetual clunky land sound when every scene starts
            if (enableMovementSounds && Time.timeSinceLevelLoad > 1f)
            {

                landingSounds.Play(m_AudioSource);
                // TODO: Fix this
                nextStepInCycle = cumulativeStepCycleCount + 0.5f;

            }

        }

        private void ProgressStepCycle(float speed)
        {

            if (m_CharacterController.velocity.sqrMagnitude > 0.0f && (m_Input.x != 0.0f || m_Input.y != 0.0f))
            {

                if (isCrouching)
                {
                    cumulativeStepCycleCount += (m_CharacterController.velocity.magnitude + (speed * (isWalking ? walkStepLengthCrouching : runStepLengthCrouching))) * Time.deltaTime;
                }
                else
                {
                    cumulativeStepCycleCount += (m_CharacterController.velocity.magnitude + (speed * (isWalking ? walkStepLengthStanding : runStepLengthStanding))) * Time.deltaTime;
                }

            }

            if (cumulativeStepCycleCount > nextStepInCycle)
            {

                cumulativeStepCycleCount = 0.0f;
                nextStepInCycle = cumulativeStepCycleCount + stepInterval;

                PlayFootStepAudio();

            }

        }

        private void PlayFootStepAudio()
        {

            if (!m_CharacterController.isGrounded)
            {
                return;
            }

            if (m_CharacterController.isGrounded && enableMovementSounds && movementEnabled)
            {
                footstepSounds.Play(m_AudioSource);
            }

        }

        private void GetInput(out float speed)
        {

            float horizontal = myInputManager.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_HORIZONTAL);
            float vertical = myInputManager.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_VERTICAL);

            // Keep track of whether or not the character is walking or running
            isWalking = (runEnabled ? (!myInputManager.GetButton(FPEInputManager.eFPEInput.FPE_INPUT_RUN)) : true);

            // Set the desired speed to be walking or running
            if (isCrouching)
            {
                speed = isWalking ? m_WalkSpeedCrouching : m_RunSpeedCrouching;
            }
            else
            {
                speed = isWalking ? m_WalkSpeedStanding : m_RunSpeedStanding;
            }

            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1.0f)
            {
                m_Input.Normalize();
            }

        }

        private void RotateView()
        {
            myMouseLook.LookRotation(transform, m_Camera.transform);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {

            Rigidbody body = hit.collider.attachedRigidbody;

            // Don't move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }

            body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);

        }

        private bool haveHeadRoomToStand()
        {

            bool haveHeadRoom = true;
            //Debug.DrawRay(gameObject.transform.position, gameObject.transform.up * (standingHeight - (controller.height/2.0f)), Color.red);

            RaycastHit headRoomHit;
            if (Physics.Raycast(gameObject.transform.position, gameObject.transform.up, out headRoomHit, (standingHeight - (controller.height / 2.0f))))
            {
                //Debug.Log("Headroom hit " + headRoomHit.collider.transform.name);
                haveHeadRoom = false;
            }

            return haveHeadRoom;

        }

        #region PUBLIC_INTERFACE

        public void dockThePlayer(Transform dockTransform, Vector2 maxAngleFromFocalPoint, Vector3 focalPoint, bool smoothDock = false)
        {

            playerDocked = true;

            // Make a "fake" focal point to restore to, just an invisible point in front of where the player was looking prior to docking
            previousFocalPoint = m_Camera.transform.position + (m_Camera.transform.forward * 5.0f);
            previousWorldPosition = transform.position;

            if (smoothDock)
            {

                targetDockPosition = dockTransform.position;
                targetMaxAngles = maxAngleFromFocalPoint;
                targetFocalPoint = focalPoint;
                currentDockingState = ePlayerDockingState.DOCKING;

            }
            else
            {
                
                transform.position = dockTransform.position;
                myMouseLook.LookAtPosition(transform, m_Camera.transform, focalPoint);
                myMouseLook.enableLookRestriction(maxAngleFromFocalPoint);

            }

        }

        public void unDockThePlayer(bool smoothUndock = false)
        {

            if (playerDocked)
            {

                if (smoothUndock)
                {

                    targetDockPosition = previousWorldPosition;
                    targetFocalPoint = previousFocalPoint;
                    myMouseLook.enableMouseLook = false;
                    currentDockingState = ePlayerDockingState.UNDOCKING;

                }
                else
                {

                    transform.position = previousWorldPosition;
                    myMouseLook.disableLookRestriction();
                    myMouseLook.LookAtPosition(transform, m_Camera.transform, previousFocalPoint);
                    playerDocked = false;

                }

            }

        }

        public Vector3 GetCurrentPlayerFocalPoint()
        {
            return (m_Camera.transform.position + (m_Camera.transform.forward * 5.0f));
        }

        public void enableMovement()
        {
            movementEnabled = true;
        }

        public void disableMovement()
        {
            movementEnabled = false;
        }

        public void enableRun()
        {
            runEnabled = true;
        }

        public void disableRun()
        {
            runEnabled = false;
        }

        public void enableJump()
        {
            jumpEnabled = true;
        }

        public void disableJump()
        {
            jumpEnabled = false;
        }

        public void enableCrouch()
        {
            crouchEnabled = true;
        }

        public void disableCrouch()
        {
            crouchEnabled = false;
        }

        public bool dockInProgress()
        {
            return (currentDockingState != ePlayerDockingState.IDLE);
        }

        /// <summary>
        /// Forces player's camera to look at a position
        /// </summary>
        /// <param name="position">The position to look at</param>
        public void forcePlayerLookToPosition(Vector3 position)
        {
            gameObject.GetComponent<FPEMouseLook>().LookAtPosition(transform, m_Camera.transform, position);
        }

        #endregion

        #region SAVING_AND_LOADING

        //
        // Assumptions:
        // -----------
        // 1. Player can never access save menu when they are in the middle of a dock movement (e.g. currentDockingState is not IDLE)
        // 2. When game is loaded, player is kicked to gameplay (e.g. Movement and MouseLook state is always ENABLED upon restoration of state)
        //
        // Note: In these functions, we use gameObject.GetComponent<FPEMouseLook>() rather than myMouseLook because these functions are subject 
        // to weird execution orders at times, since they happen during game saving and loading, which happens on or before or just after 
        // OnSceneLoaded, etc. Using a just-in-time fetch here is fine because it's not happening every frame.
        //

        public FPEPlayerStateSaveData getPlayerStateDataForSavedGame()
        {

            GameObject tempDock = FPEInteractionManagerScript.Instance.getCurrentDockForSaveGame();
            string currentDockName = (tempDock == null ? "" : tempDock.name);
            FPEPlayerStateSaveData playerData = new FPEPlayerStateSaveData(transform, GetCurrentPlayerFocalPoint(), isCrouching, playerDocked, currentDockName, targetMaxAngles, targetFocalPoint, targetDockPosition, previousFocalPoint, previousWorldPosition);

            return playerData;

        }

        public void restorePlayerStateFromSavedGame(FPEPlayerStateSaveData data)
        {

            // Player position and look focus
            transform.position = data.playerPosition();
            transform.rotation = data.playerRotation();
            gameObject.GetComponent<FPEMouseLook>().LookAtPosition(transform, m_Camera.transform, data.playerLookAt());
            isCrouching = data.Crouching;
            playerDocked = data.Docked;
            targetMaxAngles = data.MaxAngles;
            targetFocalPoint = data.TargetFocalPos;
            targetDockPosition = data.TargetDockPos;
            previousFocalPoint = data.PreviousFocalPos;
            previousWorldPosition = data.PreviousWorldPos;

            // Look for dock, if required
            if (data.Docked && data.DockName != "")
            {

                FPEInteractableDockScript[] allDocks = GameObject.FindObjectsOfType<FPEInteractableDockScript>();
                GameObject foundDock = null;

                for (int i = 0; i < allDocks.Length; i++)
                {

                    if(allDocks[i].gameObject.name == data.DockName)
                    {
                        foundDock = allDocks[i].gameObject;
                        break;
                    }

                }

                if(foundDock != null)
                {

                    gameObject.GetComponent<FPEMouseLook>().enableLookRestriction(targetMaxAngles);
                    FPEInteractionManagerScript.Instance.restoreCurrentDockFromSavedGame(foundDock);
                    isCrouching = false;

                }
                // If there was supposed to be a dock per the save file, but it cannot be found, try to restore the player state in a friendly way
                else
                {

                    Debug.LogError("FPEFirstPersonController.restorePlayerStateFromSavedGame():: Saved dock named '"+ data.DockName + "' could not be found in the scene. Restoring player to undocked state in last known good world position instead. Your scene must have changed, or this is an old saved game that is no longer compatible with the most recent version of your game or scene.");
                    transform.position = data.PreviousWorldPos;
                    transform.rotation = Quaternion.identity;
                    gameObject.GetComponent<FPEMouseLook>().LookAtPosition(transform, m_Camera.transform, data.PreviousFocalPos);
                    playerDocked = false;
                    isCrouching = false;

                }

            }

            // We ALWAYS assume the player can move their mouse once a game is loaded we restore player state
            gameObject.GetComponent<FPEMouseLook>().enableMouseLook = true;

        }

        public void setPlayerLookToNeutralLevelLoadedPosition()
        {


            // Create a neutral "look forward" position based on player's transform position and rotation
            Vector3 neutralLookAt = transform.position + m_Camera.transform.localPosition + (transform.forward * 5.0f);
            gameObject.GetComponent<FPEMouseLook>().LookAtPosition(transform, m_Camera.transform, neutralLookAt);

        }

        /// <summary>
        /// A special function used by FPESaveLoadManager via FPEInteractionManager to ensure player states are reset when returning 
        /// to the main menu. Made for special New Game -> Dock/Crouch/Etc. -> Exit to Menu -> New Game edge case.
        /// </summary>
        public void resetPlayerForMainMenu()
        {

            gameObject.GetComponent<FPEMouseLook>().disableLookRestriction();
            playerDocked = false;
            isCrouching = false;

        }

        #endregion

    }

}
