using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This movement script is heavily based on unity's FPS kit movement script
public class CharacterMovement : MonoBehaviour
{
    public static CharacterMovement instance { get; protected set; }

    public Camera fpsCamera;
    //public Camera topDownCamera;

    public Transform fpsPosition;
    //public Transform topDownPosition;

    [Header("Control Settings")]
    [Range(1f, 10f)]
    public float mouseSensitivity = 5f;
    [Range(0f, 50f)]
    public float playerSpeed = 5.0f;
    [Range(1f, 2f)]
    public float runningSpeedMultiplier = 1.25f;
    [Range(0f, 1f)]
    public float crouchSpeedMultiplier = 0.75f;
    [Range(0f, 100f)]
    public float momentumValue = 75f;
    [Range(0f, 2f)]
    public float crouchCameraOffset = 0.5f;
    [Range(1f, 10f)]
    public float jumpSpeed = 5.0f;
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

    float verticalSpeed = 0f;
    bool isPaused = false;
    float currentBobCycle = 0f;

    [HideInInspector] public float verticalAngle, horizontalAngle;
    public float speed { get; private set; } = 0f;
    public bool lockControl { get; set; }
    public bool canPause { get; set; } = true;
    public bool crouched { get; private set; } = false;

    public bool isGrounded { get; private set; } = true;

    CharacterController controller;

    float groundedTimer;
    float speedAtJump = 0f;
    float defaultControllerHeight;

    private Vector2 previousLateralMovement = Vector2.zero;

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

        fpsCamera.transform.SetParent(fpsPosition, false);
        fpsCamera.transform.localPosition = Vector3.zero;
        fpsCamera.transform.localRotation = Quaternion.identity;
        controller = GetComponent<CharacterController>();

        verticalAngle = 0f;
        horizontalAngle = transform.localEulerAngles.y;
        defaultControllerHeight = controller.height;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(canPause && Input.GetButtonDown("Menu"))
        {
            //PauseMenu.Instance.Display();
        }

        //FullscreenMap.Instance.gameObject.SetActive(Input.GetButton("Map"));

        bool wasGrounded = isGrounded;
        bool loosedGrounding = false;

        //we define our own grounded and not use the Character controller one as the character controller can flicker
        //between grounded/not grounded on small step and the like. So we actually make the controller "not grounded" only
        //if the character controller reported not being grounded for at least .5 second;
        if (!controller.isGrounded)
        {
            if (isGrounded)
            {
                groundedTimer += Time.deltaTime;
                if(groundedTimer >= 0.5f)
                {
                    loosedGrounding = true;
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

        if(!isPaused && !lockControl)
        {
            //Jump
            if(isGrounded && Input.GetButtonDown("Jump"))
            {
                verticalSpeed = jumpSpeed;
                isGrounded = false;
                loosedGrounding = true;
                //TODO: play jump audio clip
            }

            //Crouch
            if(isGrounded && Input.GetButton("Crouch"))
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

            //Calculate top speed
            bool running = Input.GetButton("Run") && !crouched; //TODO: Also check if weapons are being fired
            float actualSpeed = crouched ? playerSpeed * crouchSpeedMultiplier : running ? playerSpeed * runningSpeedMultiplier : playerSpeed;

            if (loosedGrounding)
            {
                speedAtJump = actualSpeed;
            }

            //Calculate Target Speed
            move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            if (move.sqrMagnitude > 1f) move.Normalize();

            float usedSpeed = isGrounded ? actualSpeed : speedAtJump;

            move = move * usedSpeed * Time.deltaTime;

            bool bobHead = move.magnitude > bobThreshold && isGrounded;

            move = transform.TransformDirection(move );

            //Calculate speed with momentum
            float transformedMomentum = (100f - momentumValue) / 100f;
            move.x = Mathf.Lerp(previousLateralMovement.x, move.x, transformedMomentum);
            move.z = Mathf.Lerp(previousLateralMovement.y, move.z, transformedMomentum);

            //Move Character
            controller.Move(move);

            previousLateralMovement.x = move.x;
            previousLateralMovement.y = move.z;

            //Set FOV
            if (crouched)
            {
                fpsCamera.fieldOfView = Mathf.Lerp(fpsCamera.fieldOfView, fieldOfView - (fieldOfViewModifier / 2), 0.3f);
            }
            else if (running)
            {
                fpsCamera.fieldOfView = Mathf.Lerp(fpsCamera.fieldOfView, fieldOfView + fieldOfViewModifier, 0.3f);
            }
            else
            {
                fpsCamera.fieldOfView = Mathf.Lerp(fpsCamera.fieldOfView, fieldOfView, 0.3f);
            }

            fpsCamera.fieldOfView = Mathf.Clamp(fpsCamera.fieldOfView, 30f, 120f);

            //Head Bobbing
            if (bobHead)
            {
                currentBobCycle += move.magnitude * 2;
                currentBobCycle %= 2 * Mathf.PI;
                fpsPosition.localPosition = new Vector3
                { 
                    x = fpsPosition.localPosition.x,
                    y = (bobIntensity * (Mathf.Cos(currentBobCycle)) + (1 - bobIntensity)), //TODO: FIX TO NOT EXCEED 1
                    z = fpsPosition.localPosition.z
                };
            }
            else
            {
                currentBobCycle = 0f;
                fpsPosition.localPosition = new Vector3
                {
                    x = fpsPosition.localPosition.x,
                    y = Mathf.Lerp(fpsPosition.localPosition.y, 1, 0.3f),
                    z = fpsPosition.localPosition.z
                };
            }

            //Trun the player
            float turnPlayer = Input.GetAxis("Mouse X") * mouseSensitivity;
            horizontalAngle += turnPlayer;

            if (horizontalAngle > 360) horizontalAngle -= 360f;
            if (horizontalAngle < 0) horizontalAngle += 360f;

            Vector3 currentAngles = transform.localEulerAngles;
            currentAngles.y = horizontalAngle;
            transform.localEulerAngles = currentAngles;

            //Look up/down
            var turnCam = -Input.GetAxis("Mouse Y");
            turnCam *= mouseSensitivity;
            verticalAngle = Mathf.Clamp(turnCam + verticalAngle, -90f, 90f);
            currentAngles = fpsPosition.transform.localEulerAngles;
            currentAngles.x = verticalAngle;
            fpsPosition.transform.localEulerAngles = currentAngles;

            //TODO: Reload, Change Weapon
        }

        //Gravity!

        verticalSpeed = verticalSpeed - gravityStrength * Time.deltaTime;
        if (verticalSpeed < -gravityStrength) verticalSpeed = -gravityStrength;

        var verticalMove = new Vector3(0, verticalSpeed * Time.deltaTime, 0);
        var flag = controller.Move(verticalMove);

        if ((flag & CollisionFlags.Below) != 0) verticalSpeed = 0;

        if(!wasGrounded && isGrounded)
        {
            //Play landing sound
        }
    }
}
