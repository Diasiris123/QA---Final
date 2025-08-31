using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;

public class StressTests
{
    private GameObject enemyTemplate;           // never destroy this during a run
    private readonly List<GameObject> spawned = new();

    private GameObject playerGO;
    private GameObject camGO;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Camera
        camGO = new GameObject("MainCamera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 5;
        cam.transform.position = new Vector3(0, 0, -10);

        // Player (far away + dummy VFX)
        playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        playerGO.AddComponent<BoxCollider2D>();
        var player = playerGO.AddComponent<Player>();
        Player.instance = player;
        player.destructionFX = new GameObject("DummyPlayerVFX");
        playerGO.transform.position = new Vector3(9999, 9999, 0);

        // Enemy template
        enemyTemplate = new GameObject("EnemyTemplate");
        var e = enemyTemplate.AddComponent<Enemy>();
        e.health = 1; e.shield = 0;
        e.destructionVFX = new GameObject("VFX");
        e.hitEffect = new GameObject("Hit");

        // Projectile setup (avoid colliding path)
        e.Projectile = new GameObject("Projectile");
        var proj = e.Projectile.AddComponent<Projectile>();
        proj.damage = 1;
        proj.enemyBullet = true;
        proj.destroyedByCollision = false;
        e.Projectile.AddComponent<BoxCollider2D>().isTrigger = true;
        e.Projectile.AddComponent<Rigidbody2D>().isKinematic = true;

        enemyTemplate.AddComponent<FollowThePath>();

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (var go in spawned)
            if (go) Object.DestroyImmediate(go);
        spawned.Clear();

        if (enemyTemplate) Object.DestroyImmediate(enemyTemplate);
        if (playerGO) Object.DestroyImmediate(playerGO);
        if (camGO) Object.DestroyImmediate(camGO);
        yield return null;
    }

    [UnityTest, Performance]
    public IEnumerator Ramp_Enemies_Until_We_Exceed_60FPS()
    {
        const float frameBudgetMs = 16.67f;
        int n = 150;
        int step = 150;

        for (int iter = 0; iter < 8; iter++) // bounded
        {
            // spawn batch from template and track instances
            for (int i = 0; i < n; i++)
            {
                var go = Object.Instantiate(enemyTemplate);
                spawned.Add(go);

                go.transform.position = new Vector3(Random.Range(-7f, 7f), Random.Range(-3.5f, 3.5f), 0f);

                var p = go.GetComponent<FollowThePath>();
                if (p != null)
                {
                    p.path = new Transform[4];
                    p.path[0] = Point(go.transform.position);
                    p.path[1] = Point(go.transform.position + new Vector3(0.75f, 0.25f, 0f));
                    p.path[2] = Point(go.transform.position + new Vector3(-0.35f, 0.9f, 0f));
                    p.path[3] = Point(go.transform.position + new Vector3(-0.9f, -0.1f, 0f));
                    p.speed = 2.5f; p.loop = true; p.rotationByPath = false;
                    p.SetPath();
                    foreach (var t in p.path) t.parent = go.transform;
                }
            }

            yield return new WaitForSeconds(1.0f);

            var group = new SampleGroup($"Frame (N={n})", SampleUnit.Millisecond);
            using (Measure.Frames().SampleGroup(group).WarmupCount(10).MeasurementCount(120).Scope())
            {
                int f = 0; while (f++ < 130) yield return null;
            }

            Debug.Log($"[STRESS] Completed N={n}. Check median vs {frameBudgetMs} ms in the report.");

            // cleanup only spawned instances (keep template)
            foreach (var go in spawned)
                if (go) Object.Destroy(go);
            spawned.Clear();

            n += step;
            yield return null;
        }

        Assert.Pass("Stress ramp executed.");
    }

    [UnityTest]
    public IEnumerator Report_Enemy_Count_When_Exceeding_60FPS_Budget()
    {
        const float frameBudgetMs = 16.67f;
        int n = 1000;
        int step = 250;
        int maxBatches = 50;

        for (int batch = 0; batch < maxBatches; batch++)
        {
            // Spawn N enemies
            for (int i = 0; i < n; i++)
            {
                var go = Object.Instantiate(enemyTemplate);
                spawned.Add(go);

                go.transform.position = new Vector3(Random.Range(-7f, 7f), Random.Range(-3.5f, 3.5f), 0f);

                var p = go.GetComponent<FollowThePath>();
                if (p != null)
                {
                    p.path = new Transform[2];
                    p.path[0] = Point(go.transform.position);
                    p.path[1] = Point(go.transform.position + new Vector3(0.75f, 0.25f, 0f));
                    p.speed = 2.5f; p.loop = true; p.rotationByPath = false;
                    p.SetPath();
                    foreach (var t in p.path) t.parent = go.transform;
                }
            }

            // Let movement stabilize
            yield return new WaitForSeconds(0.5f);

            // Sample average frame time over a window
            const int sampleFrames = 60;
            float sumMs = 0f;
            for (int f = 0; f < sampleFrames; f++)
            {
                yield return null;
                sumMs += Time.deltaTime * 1000f;
            }
            float avgMs = sumMs / sampleFrames;

            if (avgMs > frameBudgetMs)
            {
                Debug.Log($"[BUDGET] Exceeded {frameBudgetMs:F2} ms/frame with enemies on screen: {spawned.Count}. Avg={avgMs:F2} ms over {sampleFrames} frames.");
                // Keep them on screen for any manual inspection then clean up
                foreach (var go in spawned)
                    if (go) Object.Destroy(go);
                spawned.Clear();
                yield return null;
                Assert.Pass($"Exceeded budget at enemy count: {n}");
                yield break;
            }

            // Cleanup and increase load
            foreach (var go in spawned)
                if (go) Object.Destroy(go);
            spawned.Clear();
            yield return null;

            n += step;
        }

        Assert.Inconclusive("Did not exceed 16.67 ms within the tested range.");
    }

    [UnityTest]
    public IEnumerator FPS_500_Frames_Spike_Scan()
    {
        const int framesToTest = 500;
        const float frameBudgetMs = 16.67f;
        const float severeSpikeMs = 2f * frameBudgetMs;

        int spikesOverBudget = 0;
        int severeSpikes = 0;
        float worstMs = 0f;
        int worstFrame = -1;

        // Optional short warmup
        for (int i = 0; i < 30; i++) yield return null;

        for (int f = 0; f < framesToTest; f++)
        {
            yield return null;
            float ms = Time.deltaTime * 1000f;

            if (ms > frameBudgetMs) spikesOverBudget++;
            if (ms > severeSpikeMs) severeSpikes++;

            if (ms > worstMs)
            {
                worstMs = ms;
                worstFrame = f;
            }
        }

        Debug.Log($"[FPS] 500-frame scan complete. Spikes>{frameBudgetMs:F2}ms: {spikesOverBudget}, Severe>{severeSpikeMs:F2}ms: {severeSpikes}, Worst: {worstMs:F2} ms at frame #{worstFrame}.");

        // Let the test succeed but report findings; optionally assert on severe spikes
        Assert.Pass($"Spike summary — over budget: {spikesOverBudget}, severe: {severeSpikes}, worst: {worstMs:F2} ms at frame {worstFrame}.");
    }

    private Transform Point(Vector3 p)
    {
        var t = new GameObject("pt").transform; t.position = p; return t;
    }
}