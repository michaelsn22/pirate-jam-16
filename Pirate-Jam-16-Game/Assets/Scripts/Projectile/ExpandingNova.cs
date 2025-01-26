using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpandingNova : MonoBehaviour
{
    public float maxRadius = 10f;
    public float expansionDuration = 2f;
    public float damageRadius = 0.5f;
    public int damageAmount = 10;
    public LayerMask playerLayer;
    public Material waveMaterial;

    private float currentRadius = 0f;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private float hitboxHeight = 1f;

    //track targets hit once so we dont hit them again.
    private HashSet<Collider> damagedTargets = new HashSet<Collider>();

    void Start()
    {
        if (gameObject.name == "EnemyBoss3") //fix hitbox issues indepentant to 3rd boss.
        {
            //Debug.Log("setting hitbox height to 0.5f custom");
            hitboxHeight = 0.5f;
        }
    }
    private GameObject CreateWaveMesh()
    {
        GameObject waveObject = new GameObject("WaveMesh");
        waveObject.transform.SetParent(transform);
        waveObject.transform.localPosition = Vector3.zero;

        MeshFilter meshFilter = waveObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = waveObject.AddComponent<MeshRenderer>();
        meshRenderer.enabled = true;
        meshRenderer.material = waveMaterial;

        UpdateWaveMesh(meshFilter, 0f);

        return waveObject;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            // Draw the potential max range when not in play mode
            DrawWaveGizmo(maxRadius, Color.yellow);
        }
        else if (currentRadius > 0)
        {
            // Draw the current wave in play mode
            DrawWaveGizmo(currentRadius, Color.red);
        }
    }

    private void UpdateWaveMesh(MeshFilter meshFilter, float radius)
    {
        int segments = 72;
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[segments * 2];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2;
            float nextAngle = (float)(i + 1) / segments * Mathf.PI * 2;

            // Create a wavy, star-like pattern
            float waveOffset = Mathf.Sin(angle * 8) * 0.1f * radius;
            float starOffset = i % 2 == 0 ? 0 : -0.2f * radius;
            float adjustedRadius = radius + waveOffset + starOffset;

            float x = Mathf.Sin(angle) * adjustedRadius;
            float z = Mathf.Cos(angle) * adjustedRadius;

            float nextX = Mathf.Sin(nextAngle) * adjustedRadius;
            float nextZ = Mathf.Cos(nextAngle) * adjustedRadius;

            vertices[i * 2] = new Vector3(x, 0, z);
            vertices[i * 2 + 1] = new Vector3(nextX, 0, nextZ);

            int startIndex = i * 6;
            triangles[startIndex] = i * 2;
            triangles[startIndex + 1] = (i * 2 + 1) % (segments * 2);
            triangles[startIndex + 2] = (i * 2 + 2) % (segments * 2);
            triangles[startIndex + 3] = i * 2;
            triangles[startIndex + 4] = (i * 2 + 2) % (segments * 2);
            triangles[startIndex + 5] = (i * 2 + 3) % (segments * 2);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
    public void TriggerWaveAttack()
    {
        StartCoroutine(ExpandingWave());
    }

    private System.Collections.IEnumerator ExpandingWave()
    {
        float currentRadius = 0f;
        float expansionSpeed = maxRadius / expansionDuration;

        // Determine spawn position
        Vector3 spawnPosition = Vector3.zero;
        /*
        if (UnityEngine.Random.value > 0.5f)
        {
            hitboxHeight = 4f;
            spawnPosition = new Vector3(0f, 0.5f, 0f);
        }
        else{
            hitboxHeight = 1f;
        }
        */

        hitboxHeight = 1f;

        // Create a new mesh for this wave
        GameObject waveObject = new GameObject("WaveMesh");
        waveObject.transform.SetParent(transform);
        waveObject.transform.localPosition = spawnPosition;
        MeshFilter meshFilter = waveObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = waveObject.AddComponent<MeshRenderer>();
        meshRenderer.material = waveMaterial;

        //clear the list of hit colliders since a new wave has been spawned.
        damagedTargets.Clear();

        while (currentRadius < maxRadius)
        {
            currentRadius += expansionSpeed * Time.deltaTime;
            UpdateWaveMesh(meshFilter, currentRadius);
            CheckForPlayerCollision(currentRadius, spawnPosition);
            yield return null;
        }

        // Destroy the wave object after it's done
        Destroy(waveObject);
    }

    private void CheckForPlayerCollision(float currentRadius, Vector3 spawnPosition)
    {
        Vector3 center = transform.position + spawnPosition;
        float effectiveRadius = currentRadius + damageRadius;

        Collider[] hitColliders;
        if (hitboxHeight > 0)
        {
            Vector3 top = center + Vector3.up * hitboxHeight;
            Vector3 bottom = center;
            hitColliders = Physics.OverlapCapsule(bottom, top, effectiveRadius, playerLayer);
        }
        else
        {
            hitColliders = Physics.OverlapSphere(center, effectiveRadius, playerLayer);
        }

        foreach (var hitCollider in hitColliders)
        {
            if (damagedTargets.Contains(hitCollider))
            {
                continue; //skip only this target if it has been damaged.
            }

            Vector3 closestPoint = hitCollider.ClosestPoint(center);
            Vector3 directionToCollider = closestPoint - center;
            
            // Check if the collision point is within the vertical range of the attack
            if (hitboxHeight > 0 && (closestPoint.y < center.y || closestPoint.y > center.y + hitboxHeight))
            {
                continue; // Skip this collider if it's outside the vertical range
            }

            // Ignore height for radius calculation
            directionToCollider.y = 0;
            float distanceToCollider = directionToCollider.magnitude;

            if (distanceToCollider >= currentRadius - damageRadius && distanceToCollider <= currentRadius + damageRadius)
            {
                HealthScript damageable = hitCollider.GetComponent<HealthScript>();
                if (damageable != null)
                {
                    damageable.TakeDamage(30);
                    damagedTargets.Add(hitCollider);
                }
            }
        }
    }

    private void DrawWaveGizmo(float radius, Color color)
    {
        Gizmos.color = color;

        // Draw the bottom circle
        DrawCircle(transform.position, radius - damageRadius);
        DrawCircle(transform.position, radius + damageRadius);

        if (hitboxHeight > 0)
        {
            // Draw the top circle
            Vector3 topCenter = transform.position + Vector3.up * hitboxHeight;
            DrawCircle(topCenter, radius - damageRadius);
            DrawCircle(topCenter, radius + damageRadius);

            // Draw vertical lines to connect top and bottom circles
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90 * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                Gizmos.DrawLine(
                    transform.position + dir * (radius - damageRadius),
                    topCenter + dir * (radius - damageRadius)
                );
                Gizmos.DrawLine(
                    transform.position + dir * (radius + damageRadius),
                    topCenter + dir * (radius + damageRadius)
                );
            }
        }
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        int segments = 32;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2;
            Vector3 point = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}
