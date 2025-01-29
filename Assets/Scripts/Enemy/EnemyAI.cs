using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public EnemyData enemyData;
    private Transform player;
    public Transform firePoint;
    public GameObject projectilePrefab;
    private NavMeshAgent agent;
    private bool canAttack = true;
    private Vector3 patrolTarget;

    private bool playerInSightRange;
    private bool playerInAttackRange;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public AudioSource gunAudioSource;
    public AudioClip fireSound;

    [Header("Flash Effect")]
    public Light muzzleFlashLight;
    public float flashDuration = 0.05f;

    public Animator enemyAnimator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent.speed = enemyData.movementSpeed;
        SetNewPatrolTarget();
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerInSightRange = distanceToPlayer <= enemyData.sightRange;
        playerInAttackRange = distanceToPlayer <= enemyData.attackRange;

        if (playerInAttackRange)
        {
            agent.SetDestination(transform.position);
            Vector3 direction = player.position - transform.position;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
            if (canAttack)
            {
                StartCoroutine(Attack());
            }
        }
        else if (playerInSightRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            SetNewPatrolTarget();
        }
    }

    private void SetNewPatrolTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
        {
            patrolTarget = hit.position;
            agent.SetDestination(patrolTarget);
        }
    }

    private IEnumerator Attack()
    {
        canAttack = false;
        ShootProjectile();
        yield return new WaitForSeconds(enemyData.attackCooldown);
        canAttack = true;
    }

    private void ShootProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody projRb = projectile.GetComponent<Rigidbody>();
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            projRb.transform.rotation = Quaternion.LookRotation(direction);
        }
        projRb.velocity = firePoint.forward * enemyData.projectileSpeed;

        if (muzzleFlash != null) muzzleFlash.Play();

        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = true;
            StartCoroutine(DisableFlashAfterTime());
        }

        //enemyAnimator.SetTrigger("Shoot");

        if (gunAudioSource != null && fireSound != null)
            gunAudioSource.PlayOneShot(fireSound);
    }
    private IEnumerator DisableFlashAfterTime()
    {
        yield return new WaitForSeconds(flashDuration);
        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = false;
        }
    }
}

