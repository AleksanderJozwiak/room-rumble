using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerMovement : MonoBehaviour
{
    public PlayerStats playerStats;
    public PlayerUIManager uiManager;
    public Animator weaponAnimator;

    public float sprintSpeedMultiplier = 1.5f;
    public float rotationSpeed = 250f;
    public float gravity = -9.81f;
    public float jumpHeight = 2.5f;

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isWalking;
    private bool isSprinting;
    private bool isRegeneratingStamina = false;
    private bool canSprintAgain = true;

    [Header("Footstep Settings")]
    public AudioSource footstepAudioSource;
    public List<AudioClip> footstepSounds;
    public float footstepInterval = 0.5f;
    private float footstepTimer;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (footstepAudioSource == null)
        {
            footstepAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveForward = Input.GetAxis("Vertical");
        float moveRight = Input.GetAxis("Horizontal");

        isSprinting = Input.GetKey(KeyCode.LeftShift) && playerStats.currentStamina > 0 && canSprintAgain;
        float currentSpeed = isSprinting ? playerStats.movementSpeed * sprintSpeedMultiplier : playerStats.movementSpeed;

        if (isSprinting)
        {
            weaponAnimator.SetBool("isWalking", true);
            playerStats.currentStamina -= Time.deltaTime * 20;
            if (playerStats.currentStamina < 0)
            {
                playerStats.currentStamina = 0;
                canSprintAgain = false;
            }
            uiManager.UpdateStaminaBar();

            StopCoroutine(StaminaRegen());
            isRegeneratingStamina = false;
        }
        else if (!isRegeneratingStamina && playerStats.currentStamina < playerStats.maxStamina)
        {
            StartCoroutine(StaminaRegen());
        }

        bool isMoving = (moveForward != 0 || moveRight != 0);

        if (isMoving && !isWalking)
        {
            weaponAnimator.SetBool("isWalking", true);
            isWalking = true;
        }
        else if (!isMoving && isWalking)
        {
            weaponAnimator.SetBool("isWalking", false);
            isWalking = false;
        }

        Vector3 move = (transform.forward * moveForward + transform.right * moveRight).normalized;
        characterController.Move(move * currentSpeed * Time.deltaTime);

        float rotation = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        transform.Rotate(0, rotation, 0);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        PlayFootstepSounds(isMoving);
    }

    private void PlayFootstepSounds(bool isMoving)
    {
        if (isMoving && isGrounded && footstepSounds.Count > 0)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                footstepTimer = isSprinting ? footstepInterval / 1.5f : footstepInterval; // Faster footsteps when sprinting
                footstepAudioSource.clip = footstepSounds[UnityEngine.Random.Range(0, footstepSounds.Count)];
                footstepAudioSource.Play();
            }
        }
    }

    private IEnumerator StaminaRegen()
    {
        isRegeneratingStamina = true;
        yield return new WaitForSeconds(1f);

        while (playerStats.currentStamina < playerStats.maxStamina)
        {
            playerStats.currentStamina += Time.deltaTime * 10;
            if (playerStats.currentStamina > playerStats.maxStamina) playerStats.currentStamina = playerStats.maxStamina;
            if (playerStats.currentStamina > playerStats.maxStamina * 0.25f) canSprintAgain = true;
            uiManager.UpdateStaminaBar();
            yield return null;
        }

        isRegeneratingStamina = false;
    }

    public void TakeDamage(int damage)
    {
        playerStats.currentHealth -= damage;
        uiManager.UpdateHealthBar();
        if (playerStats.currentHealth < 0) {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("Player died");
    }
}
