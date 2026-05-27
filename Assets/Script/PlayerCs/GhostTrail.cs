using System.Collections;
using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    public float spawnInterval = 0.03f;
    public float fadeDuration  = 0.2f;
    public Color ghostColor    = new Color(0.4f, 0.8f, 1f, 0.6f);

    private MeshFilter   meshFilter;
    private MeshRenderer meshRenderer;
    private Coroutine    spawnRoutine;

    void Start()
    {
        meshFilter   = GetComponentInChildren<MeshFilter>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public void StartTrail()
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnGhosts());
    }

    public void StopTrail()
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    IEnumerator SpawnGhosts()
    {
        while (true)
        {
            SpawnGhost();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnGhost()
    {
        if (meshFilter == null || meshRenderer == null) return;

        var ghost = new GameObject("Ghost");
        ghost.transform.SetPositionAndRotation(
            meshFilter.transform.position,
            meshFilter.transform.rotation);
        ghost.transform.localScale = meshFilter.transform.lossyScale;

        var mf = ghost.AddComponent<MeshFilter>();
        var mr = ghost.AddComponent<MeshRenderer>();
        mf.mesh       = meshFilter.sharedMesh;
        mr.material   = new Material(meshRenderer.sharedMaterial);
        mr.material.color = ghostColor;

        StartCoroutine(FadeGhost(mr, ghost));
    }

    IEnumerator FadeGhost(MeshRenderer mr, GameObject ghost)
    {
        float elapsed = 0f;
        Color start   = ghostColor;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / fadeDuration;
            mr.material.color = new Color(start.r, start.g, start.b, Mathf.Lerp(start.a, 0f, t));
            yield return null;
        }

        Destroy(ghost);
    }
}
