using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float maxHealth;
    public float damage;
    public float chaseSpeed;
    public float roamingSpeed;
    public float rotateSpeed;
}
