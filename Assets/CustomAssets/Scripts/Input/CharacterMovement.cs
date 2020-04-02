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
    public float mouseSensitivity = 100.0f;
    public float playerSpeed = 5.0f;
    public float runningSpeed = 7.0f;
    public float crouchSpeedMultiplier = 0.75f;
    public float crouchCameraOffset = 0.5f;
    public float jumpSpeed = 5.0f;
    public float gravityStrength = 10f;
    public float fieldOfView = 90f;
    public float fieldOfViewModifier = 10f;

    float verticalSpeed = 0f;
    bool isPaused = false;

    public float verticalAngle, horizontalAngle;
    public float speed { get; private set; } = 0f;
    public bool lockControl { get; set; }
    public bool canPause { get; set; } = true;
    public bool crouched { get; private set; } = false;

    public bool grounded => isGrounded;

    CharacterController controller;

    bool isGrounded;
    float groundedTimer;
    float speedAtJump = 0f;

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
            }
            else
            {
                crouched = false;
                Vector3 targetCameraPosition = new Vector3(0, 0, 0);
                fpsCamera.transform.localPosition = Vector3.Lerp(fpsCamera.transform.localPosition, targetCameraPosition, 0.3f);
            }

            //Calculate top speed
            bool running = Input.GetButton("Run") && !crouched; //TODO: Also check if weapons are being fired
            float actualSpeed = running ? runningSpeed : playerSpeed;
            actualSpeed = crouched ? actualSpeed * crouchSpeedMultiplier : actualSpeed;

            if (loosedGrounding)
            {
                speedAtJump = actualSpeed;
            }

            //Move player character
            move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            if (move.sqrMagnitude > 1f) move.Normalize();

            float usedSpeed = isGrounded ? actualSpeed : speedAtJump;

            move = move * usedSpeed * Time.deltaTime;

            move = transform.TransformDirection(move);
            controller.Move(move);

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
