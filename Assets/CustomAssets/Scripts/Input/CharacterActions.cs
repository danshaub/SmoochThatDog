using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This movement script is heavily based on unity's FPS kit movement script
public class CharacterActions : MonoBehaviour
{
    public static CharacterActions instance { get; protected set; }

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
    bool playWalkSound = true;

    [HideInInspector] public float verticalAngle, horizontalAngle;
    public float speed { get; private set; } = 0f;
    public bool lockControl { get; set; }
    public bool canPause { get; set; } = true;
    public bool crouched { get; private set; } = false;

    public bool isGrounded { get; private set; } = true;

    bool canShoot = true;
    [HideInInspector] public float currentSpread = 0f;

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
    void Update()
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
            #region Weapon Switching
            if(Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                PlayerManager.instance.SwapGun(true);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                PlayerManager.instance.SwapGun(false);
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
            
            if (canShoot)
            {
                if (PlayerManager.instance.CurrentGun().automatic)
                {
                    if (Input.GetButton("Fire1"))
                    {
                        Shoot();
                    }
                    else
                    {
                        currentSpread = Mathf.Lerp(currentSpread, 0f, PlayerManager.instance.CurrentGun().spreadRate/2);
                    }
                }
                else
                {
                    if (Input.GetButtonDown("Fire1"))
                    {
                        Shoot();
                    }
                    else
                    {
                        currentSpread = Mathf.Lerp(currentSpread, 0f, PlayerManager.instance.CurrentGun().spreadRate/2);
                    }
                }
            }

            //Jump
            if(isGrounded && Input.GetButtonDown("Jump"))
            {
                verticalSpeed = jumpSpeed;
                isGrounded = false;
                loosedGrounding = true;
                PlayerManager.instance.audio.PlayOneShot(PlayerManager.instance.jumpSound);
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

                if(Mathf.Abs(Mathf.PI - currentBobCycle) < 0.25 && playWalkSound)
                {
                    PlayerManager.instance.audio.PlayOneShot(PlayerManager.instance.walkSound);
                    playWalkSound = false;
                }
                else if(Mathf.Abs(Mathf.PI - currentBobCycle) > 0.25)
                {
                    playWalkSound = true;
                }

                fpsPosition.localPosition = new Vector3
                { 
                    x = fpsPosition.localPosition.x,
                    y = (bobIntensity * (Mathf.Cos(currentBobCycle)) + (1 - bobIntensity)),
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
            fpsCamera.transform.localEulerAngles = currentAngles;
        }

        //Gravity!

        verticalSpeed = verticalSpeed - gravityStrength * Time.deltaTime;
        if (verticalSpeed < -gravityStrength) verticalSpeed = -gravityStrength;

        var verticalMove = new Vector3(0, verticalSpeed * Time.deltaTime, 0);
        var flag = controller.Move(verticalMove);

        if ((flag & CollisionFlags.Below) != 0) verticalSpeed = 0;

        if(!wasGrounded && isGrounded)
        {
            PlayerManager.instance.audio.PlayOneShot(PlayerManager.instance.landingSound);
        }
    }
    
    void Shoot()
    {
        Gun currentGun = PlayerManager.instance.CurrentGun();

        canShoot = false;
        StartCoroutine(ShootCooldown());

        if (currentGun.AmmoRemaining())
        {
            if (!currentGun.spreadOverTime)
            {
                currentSpread = currentGun.maxBulletSpread;
            }
            else
            {
                currentSpread = Mathf.Lerp(currentSpread, currentGun.maxBulletSpread, currentGun.spreadRate);
            }

            currentGun.UseAmmo();
            PlayerManager.instance.UpdateAmmoText();

            PlayerManager.instance.audio.PlayOneShot(currentGun.shootSound);

            PlayerManager.instance.playerAnimation.SetTrigger("Shoot");
            for(int i = 0; i < currentGun.bulletsPerShot; i++)
            {
                Vector3 raycastDirection = fpsCamera.transform.forward;

                Vector3 spreadOffset = new Vector3
                {
                    x = Random.Range(-1f, 1f),
                    y = Random.Range(-1f, 1f),
                    z = Random.Range(-1f, 1f)
                };

                spreadOffset = spreadOffset.normalized * Random.Range(0f, currentSpread/100);

                raycastDirection = (spreadOffset + raycastDirection).normalized;
                
                RaycastHit hit;
                if (Physics.Raycast(fpsCamera.transform.position, raycastDirection, out hit, currentGun.range))
                {
                    if(hit.transform.GetComponent<Target>() != null)
                    {
                        currentGun.Hit(hit);
                    }
                    else
                    {
                        GameObject particles = Instantiate(currentGun.hitParticlePrefab, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(particles, 1f);
                    }
                }

                //Debug.DrawRay(fpsCamera.transform.position, raycastDirection * currentGun.range, Color.black, 1f);

                Vector3 currentAngles = transform.localEulerAngles;
                currentAngles.y = horizontalAngle;
                transform.localEulerAngles = currentAngles;

                //Look up/down
                var turnCam = -currentGun.recoilStrength * Time.deltaTime;
                verticalAngle = Mathf.Clamp(turnCam + verticalAngle, -90f, 90f);
                currentAngles = fpsPosition.transform.localEulerAngles;
                currentAngles.x = verticalAngle;
                fpsCamera.transform.localEulerAngles = currentAngles;
            }
            
        }
        else
        {
            PlayerManager.instance.audio.PlayOneShot(currentGun.emptyClipSound);
        }
    }

    IEnumerator ShootCooldown()
    {
        if(PlayerManager.instance.CurrentGun().fireRate == 0f)
        {
            yield return new WaitForEndOfFrame();
        }
        else
        {
            yield return new WaitForSeconds(1f / PlayerManager.instance.CurrentGun().fireRate);
        }

        canShoot = true;

    }
}
