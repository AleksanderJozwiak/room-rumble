using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public EnemyData enemyData;
    private Transform player;
    public Transform center;
    public Transform firePoint;
    public GameObject projectilePrefab;
    private NavMeshAgent agent;
    private bool canAttack = true;
    private Vector3 patrolTarget;

    private bool playerInSightRange;
    private bool playerInAttackRange;

    [Header("Enemy Behavior")]
    public bool isHeavy = false;

    private bool hasLineOfSight;
    private Coroutine attackLoopCoroutine;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public AudioClip fireSoundClip;
    public float fireSoundVolume = 1f;
    public float maxDisctance;

    [Header("Flash Effect")]
    public Light muzzleFlashLight;
    public float flashDuration = 0.05f;

    public Animator enemyAnimator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = GetComponentInParent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent.speed = enemyData.movementSpeed;
        SetNewPatrolTarget();
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(center.transform.position, player.position);

        hasLineOfSight = false;
        Vector3 directionToPlayer = (player.position - center.transform.position).normalized;
        Vector3 rayOrigin = center.transform.position + new Vector3(0, 0.5f ,0);

        //Debug.DrawRay(rayOrigin, directionToPlayer * enemyData.sightRange, Color.red);

        if (Physics.Raycast(rayOrigin, directionToPlayer, out RaycastHit hit, enemyData.sightRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                hasLineOfSight = true;
            }
        }

        playerInSightRange = distanceToPlayer <= enemyData.sightRange && hasLineOfSight;
        playerInAttackRange = distanceToPlayer <= enemyData.attackRange && hasLineOfSight;

        if (playerInAttackRange)
        {
            
            Vector3 direction = player.position - transform.position;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            if (isHeavy)
            {
                if (canAttack)
                    StartCoroutine(Attack());
            }
            else
            {
                if (attackLoopCoroutine == null)
                    attackLoopCoroutine = StartCoroutine(AttackLoop());
            }
        }
        else if (playerInSightRange)
        {
            StopAttackLoop();
            ChasePlayer();
        }
        else
        {
            StopAttackLoop();
            Patrol();
        }
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            agent.isStopped = true;

            float attackDuration = Random.Range(0.7f, 2f);
            float elapsed = 0f;

            while (elapsed < attackDuration && playerInAttackRange && hasLineOfSight)
            {
                if (canAttack)
                {
                    StartCoroutine(Attack());
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            float distanceRange = Random.Range(2f, 5f);
            Vector3 randomDirection = Random.insideUnitSphere * distanceRange;
            randomDirection += transform.position;
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, distanceRange, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
                //Debug.DrawLine(transform.position, hit.position, Color.green, 1f);
                yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < 0.5f);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void StopAttackLoop()
    {
        if (attackLoopCoroutine != null)
        {
            StopCoroutine(attackLoopCoroutine);
            attackLoopCoroutine = null;
        }
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void Patrol()
    {
        agent.isStopped = false;
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            SetNewPatrolTarget();
        }
    }

    private void SetNewPatrolTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 10f, NavMesh.AllAreas))
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
        firePoint.transform.rotation = Quaternion.LookRotation(player.transform.position - firePoint.transform.position);
        
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

        if (fireSoundClip != null)
            SoundFXManager.instance.PlaySoundFXClip3D(fireSoundClip, transform, fireSoundVolume, 0f, maxDisctance);
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

