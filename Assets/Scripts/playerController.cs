using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {
    [Header ("Animations")]
    private TrailRenderer trailRenderer;
    private Animator animator;
    private SpriteRenderer sprite;
    private Transform transformPlayer;

    [Header ("Physics")]
    private Rigidbody2D rb;

    [Header ("Movement")]
    public float movementSpeed = 8f;
    private float horizontalMovement;
    private float verticalMovement;

    [Header ("Dashing")]
    public float dashingVelocity = 30f;
    public float dashingTime = 0.2f;
    private bool isDashing = false;
    private Vector2 dashDir;
    private bool dashAvailable = true;
    private Coroutine dashCoroutine = null;

    [Header ("Jumping")]
    public float jumpPower = 10f;
    public int maxJumps = 2;
    private int remJumps = 1;

    [Header ("GroundChecker")]
    private Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.5f);
    private LayerMask groundLayer;
    private bool onGround = false;

    [Header ("WallChecker")]
    private Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.5f, 0.5f);
    private LayerMask wallLayer;
    private bool touchingWall = false;

    [Header ("WallMovement")]
    public float wallClimbSpeed = 3f;
    private bool isWallSliding = false;
    public float wallSlideSpeed = 0.2f;

    [Header ("Gravity")]
    public float baseGravity = 1.5f;
    public float maxFallSpeed = 12f;
    public float fallSpeedMultiplier = 2.5f;

    // [Header ("Player Input")]
    private PlayerInput playerInput;

    // [Header ("Effects")]
    private ParticleSystem dustFX;
    private ParticleSystem dashFX;
    private ParticleSystemRenderer dashFXRenderer;
    
    [Header ("Health")]
    public float deathTime = 2;
    public int maxHealth = 3;
    private HealthUI healthUI;
    private int currentHealth;
    private bool isDying = false;

    [Header ("Speed")]
    public float timeScale = 1.3f;

    // [Header ("Levels Spawn")]
    private int currentLevel = 0;
    private int numOfLevels;
    private List<Transform> checkpoints;

    // [Header ("Lights")]
    private Light2D lights;

    [Header ("Scene")]
    private bool gameIsEnd = false;
    public float endGameDelay = 3f;

    [Header ("Sound")]
    public float walkSoundDelay = 0.35f;
    public float grabSoundDelay = 0.5f;
    public float landSoundDelay = 0.5f;
    public float jumpSoundDelay = 0.5f;
    private float jumpSoundTimer = 0;
    private float landSoundTimer = 0;
    private float walkSoundTimer = 0f;
    private float grabSoundTimer = 0f;

    void Start() {
        // Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        trailRenderer = gameObject.GetComponent<TrailRenderer>();
        animator = gameObject.GetComponent<Animator>();
        sprite = gameObject.GetComponent<SpriteRenderer>();
        transformPlayer = gameObject.transform;
        rb = gameObject.GetComponent<Rigidbody2D>();
        playerInput = gameObject.GetComponent<PlayerInput>();
        lights = gameObject.GetComponent<Light2D>();
        groundLayer = wallLayer = LayerMask.GetMask("Surface");
        foreach (Transform child in transform) {
            if (child.CompareTag("WallChecker")) {
                wallCheckPos = child;
            } else if (child.CompareTag("GroundChecker")) {
                groundCheckPos = child;
            } else if (child.CompareTag("DustFX")) {
                dustFX = child.gameObject.GetComponent<ParticleSystem>();
            } else if (child.CompareTag("DashFX")) {
                dashFX = child.gameObject.GetComponent<ParticleSystem>();
                dashFXRenderer = dashFX.GetComponent<ParticleSystemRenderer>();
            }
        }

        checkpoints = new List<Transform>();
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        System.Array.Sort(taggedObjects, (a, b) => a.transform.parent.name.CompareTo(b.transform.parent.name));
        foreach (GameObject obj in taggedObjects) {
            checkpoints.Add(obj.transform);
            Debug.Log(obj.transform.parent.name);
        }
        numOfLevels = checkpoints.Count;

        Time.timeScale = timeScale;
        healthUI = GameObject.FindWithTag("HeartContainer").GetComponent<HealthUI>();
        healthUI.SetMaxHearts(maxHealth);
        currentHealth = maxHealth;
    }

    void Update() {
        ProcessGravity();
        GroundChecker();
        WallChecker();
        WallSlide();
        soundsDelay();
        MoveSound();
    }

    private void ProcessGravity() {
        if (isDying) return;
        if (!isDashing && !isWallSliding) {
            float velocityX = horizontalMovement * movementSpeed;
            rb.linearVelocity = new Vector2(velocityX, rb.linearVelocityY);
            FallGravity();
            SpriteDirection();
        }
        if (isWallSliding && verticalMovement != 0) {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, Mathf.Max(verticalMovement * wallClimbSpeed, rb.linearVelocityY));
        }
        animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
        animator.SetFloat("yVelocity", rb.linearVelocityY);
    }
    public void FallGravity() {
        if (isDashing || isDying) return;
        if (rb.linearVelocityY < 0 && !isWallSliding) {
            rb.gravityScale = baseGravity * fallSpeedMultiplier * (1 - Mathf.Min(verticalMovement, 0));
            rb.linearVelocity = new Vector2(rb.linearVelocityX,
            Mathf.Max(-maxFallSpeed, rb.linearVelocityY));
        } else {
            rb.gravityScale = baseGravity * (1 - Mathf.Min(verticalMovement, 0));
        }
    }

    public void Move(InputAction.CallbackContext context) {
        if (isDashing || isDying || gameIsEnd) return;
        horizontalMovement = context.ReadValue<Vector2>().x;
        verticalMovement = context.ReadValue<Vector2>().y;
    }

    private void soundsDelay() {
        jumpSoundTimer = Mathf.Max(jumpSoundTimer - Time.deltaTime, 0);
        landSoundTimer = Mathf.Max(landSoundTimer - Time.deltaTime, 0);
        grabSoundTimer = Mathf.Max(grabSoundTimer - Time.deltaTime, 0);
        walkSoundTimer = Mathf.Max(walkSoundTimer - Time.deltaTime, 0);
    }
    private void MoveSound() {
        if (onGround && rb.linearVelocity.magnitude > 0.2 && walkSoundTimer <= 0) {
            SoundEffectManager.Play("walk");
            walkSoundTimer = walkSoundDelay;
        }
    }
    public void Jump(InputAction.CallbackContext context) {
        if(isDashing || isDying || gameIsEnd) return;
        if (context.performed && isWallSliding) {
            remJumps = 0;
            animator.SetTrigger("jump");
            if (jumpSoundTimer <= 0) {
                SoundEffectManager.Play("jump");
                jumpSoundTimer = jumpSoundDelay;
            }
            rb.linearVelocity = new Vector2(horizontalMovement, verticalMovement * jumpPower).normalized * movementSpeed;
            dustFX.Play();
        } else if (context.performed && remJumps > 0) {
            remJumps--;
            animator.SetTrigger("jump");
            if (jumpSoundTimer <= 0) {
                SoundEffectManager.Play("jump");
                jumpSoundTimer = jumpSoundDelay;
            }
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpPower);
            dustFX.Play();
        } else if (context.canceled && rb.linearVelocityY > 0 && !isWallSliding) {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY / fallSpeedMultiplier);
        }
    }
    
    public void Dash(InputAction.CallbackContext context) {
        if (isDying || gameIsEnd) return;
        if (context.performed && dashAvailable && 
            (horizontalMovement != 0 || verticalMovement != 0)) {
            dashFX.Play();
            dustFX.Play();
            dashDir = new Vector2(horizontalMovement, verticalMovement);
            if (dashDir == Vector2.zero) return;
            animator.SetBool("isDashing", true);
            SoundEffectManager.Play("dash");
            remJumps = 0;
            isDashing = true;
            dashAvailable = false;
            trailRenderer.emitting = true;
            lights.overlapOperation = Light2D.OverlapOperation.Additive;
            lights.intensity = 2;
            dashCoroutine = StartCoroutine(StopDashing());
        }
        if (isDashing) {
            rb.linearVelocity = dashDir.normalized * dashingVelocity;
            return;
        }
    }

    public void InterruptDash() {
        if (isDashing && dashCoroutine != null) {
            StopCoroutine(dashCoroutine);
            PostDashing();
        }
    }

    private IEnumerator StopDashing() {
        yield return new WaitForSeconds(dashingTime);
        PostDashing();
    }

    private void PostDashing() {
        lights.overlapOperation = Light2D.OverlapOperation.AlphaBlend;
        lights.intensity = 1;
        rb.linearVelocity = new Vector2(horizontalMovement, verticalMovement);
        remJumps = 0;
        trailRenderer.emitting = false;
        isDashing = false;
        animator.SetBool("isDashing", false);
        dashCoroutine = null;
    }

    public void WallSlide() {
        if (isDying || gameIsEnd) return;
        bool wantToSlide = playerInput.actions["Grap"].IsPressed();
        if (wantToSlide && !onGround && touchingWall) {
            isWallSliding = true;
            if (verticalMovement == 0) {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, Mathf.Max(-wallSlideSpeed, rb.linearVelocityY));
            }
        } else {
            isWallSliding = false;
        }
        animator.SetBool("isWallSliding", isWallSliding);
    }
    
    private void SpriteDirection() {
        if (isDashing || isDying || gameIsEnd) return;
        Quaternion oldRotation = transformPlayer.rotation;
        if (rb.linearVelocityX < 0) {
            transformPlayer.rotation = Quaternion.Euler(0, 180, 0);
            dashFXRenderer.flip = new Vector3(1, 0, 0);
        } else if (rb.linearVelocityX > 0) {
            transformPlayer.rotation = Quaternion.Euler(0, 0, 0);
            dashFXRenderer.flip = new Vector3(0, 0, 0);
        }
        if (Quaternion.Angle(oldRotation, transformPlayer.rotation) > 0.1f) {
            dustFX.Play();
        }
    }

    private void GroundChecker() {
        bool wasOnGround = onGround;
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer)) {
            onGround = true;
            remJumps = maxJumps - 1;
            if (!isDashing) {
                dashAvailable = true;
            }
        } else {
            onGround = false;
        }
        if (!wasOnGround && onGround && landSoundTimer <= 0) {
            SoundEffectManager.Play("land");
            landSoundTimer = landSoundDelay;
        }
    }

    private void WallChecker() {
        bool wantToSlide = playerInput.actions["Grap"].IsPressed();
        bool wasTouchingWall = touchingWall;
        if (Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer)
            && !playerInput.actions["Jump"].IsPressed()) {
            touchingWall = true;
        } else {
            touchingWall = false;
        }
        if (!wasTouchingWall && wantToSlide && touchingWall && !onGround && grabSoundTimer <= 0) {
            SoundEffectManager.Play("grab");
            grabSoundTimer = grabSoundDelay;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("SpawnPoint")) {
            for (int i = 0; i < numOfLevels; i++) {
                Transform spawnLocation = other.gameObject.transform;
                if (spawnLocation == checkpoints[i].transform) {
                    currentLevel = Math.Max(currentLevel, i);
                    break;
                }
            }
            if (currentLevel == numOfLevels - 1) {
                gameIsEnd = true;
                StartCoroutine(EndGame());
            }
        }
        if (other.CompareTag("Spike") && !isDying) {
            currentHealth--;
            animator.SetBool("isDying", true);
            SoundEffectManager.Play("death");
            isDying = true;
            rb.bodyType = RigidbodyType2D.Static;
            InterruptDash();
            StartCoroutine(StopDying());
        }
    }

    private IEnumerator EndGame() {
        yield return new WaitForSeconds(endGameDelay);
        SceneManager.LoadScene("End Game Menu");
    }
    private IEnumerator StopDying() {
        yield return new WaitForSeconds(deathTime);
        AfterDeath();
    }

    private void AfterDeath() {
        animator.SetBool("isDying", false);
        if (currentHealth == -1) {
            SceneManager.LoadScene("Game Over Menu");
        }
        isDying = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        healthUI.UpdateHearts(currentHealth);
        transformPlayer.position = checkpoints[currentLevel].position;
        horizontalMovement = verticalMovement = 0;
        rb.linearVelocityY = rb.linearVelocityX = 0;
    }


    public void PauseGame(InputAction.CallbackContext context) {
        if (context.performed) {
            SceneManager.LoadScene("Start Menu");
        }
    }

    // private void OnDrawGizmosSelected() {
    //     Gizmos.color = Color.white;
    //     Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    // }
}