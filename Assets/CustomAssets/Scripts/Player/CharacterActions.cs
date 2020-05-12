using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This movement script is heavily based on unity's FPS kit movement script
public class CharacterActions : MonoBehaviour
{
    public static CharacterActions instance { get; protected set; }

    #region InspectorVariables
    public Camera fpsCamera;
    //public Camera topDownCamera;

    public Transform fpsTransform;
    //public Transform topDownPosition;

    [Header("Control Settings")]
    [Range(0f, 5f)]
    public float interactRange = 1f;
    [Range(1f, 10f)]
    public float mouseSensitivity = 5f;
    [Range(0f, 50f)]
    public float playerSpeed = 5.0f;
    [Range(1f, 2f)]
    public float runningSpeedMultiplier = 1.25f;
    [Range(0f, 1f)]
    public float crouchSpeedMultiplier = 0.75f;
    [Range(0f, 100f)]
    public float groundedMomentumValue = 50f;
    [Range(0f, 100f)]
    public float airborneMomentumValue = 75f;
    [Range(1f, 10f)]
    public float rageMovementSpeedMultiplier = 1f;
    [Range(0f, 2f)]
    public float crouchCameraOffset = 0.5f;
    [Range(1f, 10f)]
    public float jumpSpeed = 5.0f;
    [Range(1f, 5f)]
    public float rageJumpHeightMultiplier = 1f;
    [Range(5f, 20f)]
    public float gravityStrength = 10f;
    [Range(30f, 110f)]
    public float fieldOfView = 90f;
    [Range(0f, 20f)]
    public float fieldOfViewModifier = 10f;
    [Range(0f, 0.5f)]
    public float bobIntensity = 0.1f;
    [Range(0f, 5f)]
    public float bobSpeed = 2f;
    [Range(0f, 1f)]
    public float bobThreshold = 0.01f;
    [Range(0f, 1f)]
    public float knockbackResistance = 0.1f;
    #endregion

    #region OtherVariables
    CharacterController controller;

    bool isPaused = false;

    bool jumpNextFixedFrame = false;
    float groundedTimer;
    float speedAtJump = 0f;
    float verticalSpeed = 0f;
    float defaultControllerHeight;

    float currentBobCycle = 0f;
    bool playWalkSound = true;

    [HideInInspector] public float verticalAngle, horizontalAngle;
    [HideInInspector] public Vector2 recoilOffset = Vector2.zero;
    [HideInInspector] public float currentSpread = 0f;
    [HideInInspector] public bool canShoot = true;

    public float speed { get; private set; } = 0f;
    public bool lockControl { get; set; }
    public bool canPause { get; set; } = true;
    public bool crouched { get; private set; } = false;
    public bool running { get; private set; } = false;
    public bool isGrounded { get; private set; } = true;
    private float startFPSPosHeight;

    [HideInInspector] public Vector3 knockbackOffset = Vector3.zero;
    private Vector2 previousLateralMovement = Vector2.zero;
    #endregion

    #region Methods
    private void Awake()
    {
        instance = this;

    }
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isPaused = false;
        isGrounded = true;

        fpsCamera.transform.SetParent(fpsTransform, false);
        fpsCamera.transform.localPosition = Vector3.zero;
        fpsCamera.transform.localRotation = Quaternion.identity;
        controller = GetComponent<CharacterController>();

        verticalAngle = 0f;
        horizontalAngle = transform.localEulerAngles.y;
        defaultControllerHeight = controller.height;
        startFPSPosHeight = fpsTransform.localPosition.y;
    }

    /*************************************
    INPUT TO PLACE IN UPDATE OR TURN INTO EVENT */
    private void Update()
    {

        if (!isPaused && !lockControl)
        {
            if (Input.GetButtonDown("ToggleMinimap"))
            {
                PlayerManager.instance.ToggleMinimap();
            }

            #region Interact

            if (Input.GetButtonDown("Interact"))
            {
                LayerMask mask = LayerMask.GetMask("Interactable");
                RaycastHit hit;
                if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, interactRange, mask))
                {
                    hit.collider.gameObject.GetComponentInParent<IInteractableObject>().Action();
                }

            }

            #endregion

            #region Rage
            if (PlayerManager.instance.RageFull() && Input.GetButtonDown("Rage"))
            {
                StartCoroutine(PlayerManager.instance.Rage());
            }
            #endregion

            #region Weapon Switching
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                PlayerManager.instance.SwapGun(false);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                PlayerManager.instance.SwapGun(true);
            }
            else if (Input.GetButtonDown("Swap Weapon 1"))
            {
                PlayerManager.instance.SwapGun(0);
            }
            else if (Input.GetButtonDown("Swap Weapon 2"))
            {
                PlayerManager.instance.SwapGun(1);
            }
            else if (Input.GetButtonDown("Swap Weapon 3"))
            {
                PlayerManager.instance.SwapGun(2);
            }
            else if (Input.GetButtonDown("Swap Weapon 4"))
            {
                PlayerManager.instance.SwapGun(3);
            }
            else if (Input.GetButtonDown("Swap Weapon 5"))
            {
                PlayerManager.instance.SwapGun(4);
            }
            else if (Input.GetButtonDown("Swap Weapon 6"))
            {
                PlayerManager.instance.SwapGun(5);
            }
            else if (Input.GetButtonDown("Swap Weapon 7"))
            {
                PlayerManager.instance.SwapGun(6);
            }
            else if (Input.GetButtonDown("Swap Weapon 8"))
            {
                PlayerManager.instance.SwapGun(7);
            }
            else if (Input.GetButtonDown("Swap Weapon 9"))
            {
                PlayerManager.instance.SwapGun(8);
            }
            else if (Input.GetButtonDown("Swap Weapon 10"))
            {
                PlayerManager.instance.SwapGun(9);
            }
            #endregion


            #region Shooting
            if (canShoot)
            {
                if (Input.GetButtonDown("Fire2"))
                {
                    canShoot = false;

                    PlayerManager.instance.smooch.Shoot(fpsCamera.transform);
                    StartCoroutine(PlayerManager.instance.SmoochCooldown());
                }
                else if (PlayerManager.instance.CurrentGun().automatic)
                {
                    if (Input.GetButton("Fire1"))
                    {
                        canShoot = false;

                        PlayerManager.instance.CurrentGun().Shoot(fpsCamera.transform);
                        StartCoroutine(PlayerManager.instance.ShootCooldown());
                    }
                    else
                    {

                        currentSpread = Mathf.Lerp(currentSpread, 0f, PlayerManager.instance.CurrentGun().spreadRate / 2);
                    }
                }
                else
                {
                    if (Input.GetButtonDown("Fire1"))
                    {
                        canShoot = false;
                        PlayerManager.instance.CurrentGun().Shoot(fpsCamera.transform);
                        StartCoroutine(PlayerManager.instance.ShootCooldown());

                    }
                    else
                    {
                        currentSpread = Mathf.Lerp(currentSpread, 0f, PlayerManager.instance.CurrentGun().spreadRate / 2);
                    }
                }
            }
            #endregion

            //Jump
            if (isGrounded && Input.GetButtonDown("Jump"))
            {
                jumpNextFixedFrame = true;
            }

            //Crouch
            if (Input.GetButton("Crouch"))
            {
                crouched = true;
                Vector3 targetCameraPosition = new Vector3(0, -crouchCameraOffset, 0);
                fpsCamera.transform.localPosition = Vector3.Lerp(fpsCamera.transform.localPosition, targetCameraPosition, 0.3f); //
                controller.height = Mathf.Lerp(controller.height, defaultControllerHeight - crouchCameraOffset, 0.3f);
                Vector3 targetControllerPosition = new Vector3(0, -crouchCameraOffset / 2f, 0);
                controller.center = Vector3.Lerp(controller.center, targetControllerPosition, 0.3f);
            }
            else
            {
                crouched = false;
                Vector3 targetCameraPosition = new Vector3(0, 0, 0);
                fpsCamera.transform.localPosition = Vector3.Lerp(fpsCamera.transform.localPosition, targetCameraPosition, 0.3f);
                controller.height = Mathf.Lerp(controller.height, defaultControllerHeight, 0.3f);
                Vector3 targetControllerPosition = new Vector3(0, 0, 0);
                controller.center = Vector3.Lerp(controller.center, targetControllerPosition, 0.3f);
            }

            running = Input.GetButton("Run") && !crouched; //TODO: Also check if weapons are being fired

            //Trun the player
            float turnPlayer = Input.GetAxis("Mouse X") * mouseSensitivity; //Use new input system!!!
            horizontalAngle += turnPlayer;

            if (horizontalAngle > 360) horizontalAngle -= 360f;
            if (horizontalAngle < 0) horizontalAngle += 360f;

            Vector3 currentAngles = transform.localEulerAngles;
            currentAngles.y = horizontalAngle;
            transform.localEulerAngles = currentAngles;

            //Look up/down
            var turnCam = -Input.GetAxis("Mouse Y");
            turnCam *= mouseSensitivity;
            verticalAngle = Mathf.Clamp(turnCam + verticalAngle, -89.5f, 89.5f);
            currentAngles = fpsCamera.transform.localEulerAngles;
            currentAngles.x = Mathf.Clamp(verticalAngle - recoilOffset.y, -90f, 90f);
            currentAngles.y = recoilOffset.x;
            fpsCamera.transform.localEulerAngles = currentAngles;
        }
        else if (lockControl)
        {
            verticalAngle = fpsCamera.transform.localEulerAngles.x;
            horizontalAngle = transform.localEulerAngles.y;

            fpsCamera.fieldOfView = fieldOfView;
        }
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (jumpNextFixedFrame)
        {
            verticalSpeed = PlayerManager.instance.isRaged ? jumpSpeed * rageJumpHeightMultiplier : jumpSpeed;

            isGrounded = false;
            PlayerManager.instance.GetComponent<AudioSource>().PlayOneShot(PlayerManager.instance.jumpSound);
        }
        bool wasGrounded = isGrounded;

        //we define our own grounded and not use the Character controller one as the character controller can flicker
        //between grounded/not grounded on small step and the like. So we actually make the controller "not grounded" only
        //if the character controller reported not being grounded for at least .5 second;
        if (!controller.isGrounded)
        {
            if (isGrounded)
            {
                groundedTimer += Time.deltaTime;
                if (groundedTimer >= 0.25f)
                {
                    jumpNextFixedFrame = true;
                    isGrounded = false;
                }
            }
        }
        else
        {
            groundedTimer = 0f;
            isGrounded = true;
        }

        speed = 0;
        Vector3 move = Vector3.zero;

        if (!isPaused && !lockControl)
        {
            #region Movement
            //Calculate top speed

            float actualSpeed = crouched ? playerSpeed * crouchSpeedMultiplier : running ? playerSpeed * runningSpeedMultiplier : playerSpeed;

            if (jumpNextFixedFrame)
            {
                speedAtJump = actualSpeed;
            }

            //Calculate Target Speed
            move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")); // @@@@@@@@@@@@@@@@@@@ Use new input system equivilant here!!!!!
            if (move.sqrMagnitude > 1f) move.Normalize();

            float usedSpeed = isGrounded ? actualSpeed : speedAtJump;

            if (PlayerManager.instance.isRaged) usedSpeed *= rageMovementSpeedMultiplier;

            move = move * usedSpeed * Time.deltaTime;

            bool bobHead = move.magnitude > bobThreshold && isGrounded;

            move = transform.TransformDirection(move);

            //Calculate speed with momentum
            float transformedMomentum = (100f - (isGrounded ? groundedMomentumValue : airborneMomentumValue)) / 100f;
            move.x = Mathf.Lerp(previousLateralMovement.x, move.x, transformedMomentum);
            move.z = Mathf.Lerp(previousLateralMovement.y, move.z, transformedMomentum);

            //Move Character
            controller.Move(move + (knockbackOffset * Time.deltaTime));

            knockbackOffset = Vector3.Lerp(knockbackOffset, Vector3.zero, knockbackResistance);

            previousLateralMovement.x = move.x;
            previousLateralMovement.y = move.z;

            #endregion

            #region Camera Control


            //Head Bobbing
            if (bobHead)
            {
                currentBobCycle += move.magnitude * 2;
                currentBobCycle %= 2 * Mathf.PI;

                if (Mathf.Abs(Mathf.PI - currentBobCycle) < 0.25 && playWalkSound)
                {
                    PlayerManager.instance.GetComponent<AudioSource>().PlayOneShot(PlayerManager.instance.walkSound);
                    playWalkSound = false;
                }
                else if (Mathf.Abs(Mathf.PI - currentBobCycle) > 0.25)
                {
                    playWalkSound = true;
                }

                fpsTransform.localPosition = new Vector3
                {
                    x = fpsTransform.localPosition.x,
                    y = (bobIntensity * (Mathf.Cos(currentBobCycle)) + (startFPSPosHeight - bobIntensity)),
                    z = fpsTransform.localPosition.z
                };
            }
            else
            {
                currentBobCycle = 0f;
                fpsTransform.localPosition = new Vector3
                {
                    x = fpsTransform.localPosition.x,
                    y = Mathf.Lerp(fpsTransform.localPosition.y, startFPSPosHeight, 0.3f),
                    z = fpsTransform.localPosition.z
                };
            }



            recoilOffset = Vector2.Lerp(recoilOffset, Vector2.zero, PlayerManager.instance.CurrentGun().recoilResistance);
        }



        #endregion

        //Gravity!

        verticalSpeed = verticalSpeed - gravityStrength * Time.deltaTime;
        if (verticalSpeed < -gravityStrength) verticalSpeed = -gravityStrength;

        var verticalMove = new Vector3(0, verticalSpeed * Time.deltaTime, 0);
        var flag = controller.Move(verticalMove);

        if ((flag & CollisionFlags.Below) != 0) verticalSpeed = 0;

        if (!wasGrounded && isGrounded && !jumpNextFixedFrame)
        {
            PlayerManager.instance.GetComponent<AudioSource>().PlayOneShot(PlayerManager.instance.landingSound);
        }

        jumpNextFixedFrame = false;
    }



    #endregion
}
