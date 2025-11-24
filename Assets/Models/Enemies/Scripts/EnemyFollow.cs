using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public float moveSpeed = 1.2f;
    public float stopDistance = 2f;
    
    [Header("Randomization")]
    [SerializeField] private float wanderStrength = 0.5f;
    [SerializeField] private float wanderChangeInterval = 1f;
    
    [Header("Detection")]
    [SerializeField] private float detectionRange = 10f;
    
    private Transform player;
    private CharacterController characterController;
    private Vector3 randomOffset;
    private float nextWanderTime;
    private bool isActivated = false;

    private void Start()
    {
        // Busca al jugador por su tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("EnemyFollow: no se encontr� ning�n objeto con el tag 'Player'.");

        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("EnemyFollow: el enemigo necesita un CharacterController para detectar colisiones con otros enemigos.");
        }

        moveSpeed += Random.Range(-0.3f, 0.3f);
        nextWanderTime = Time.time + Random.Range(0f, wanderChangeInterval);
        GenerateRandomOffset();
    }

    private void Update()
    {
        if (player == null || characterController == null) return;

        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        float dist = dir.magnitude;

        if (!isActivated)
        {
            if (dist <= detectionRange)
            {
                isActivated = true;
            }
            else
            {
                return;
            }
        }

        if (Time.time >= nextWanderTime)
        {
            GenerateRandomOffset();
            nextWanderTime = Time.time + wanderChangeInterval;
        }

        Vector3 targetPosition = player.position + randomOffset;
        dir = (targetPosition - transform.position);
        dir.y = 0f;
        dist = dir.magnitude;

        if (dist > stopDistance)
        {
            dir.Normalize();
            Vector3 movement = dir * moveSpeed * Time.deltaTime;
            characterController.Move(movement);
        }

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }

    private void GenerateRandomOffset()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(0f, wanderStrength);
        randomOffset = new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
    }
}
