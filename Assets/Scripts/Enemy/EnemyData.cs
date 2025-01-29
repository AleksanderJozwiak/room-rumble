using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Enemy base data", menuName = "ScriptableObjects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public EnemyType enemyType;
    public float movementSpeed;
    public float attackRange;
    public float sightRange;
    public float attackCooldown;
    public float projectileSpeed;
}

public enum EnemyType
{
    FlyingShooter,
    FastShooter,
    StaticShooter
}

