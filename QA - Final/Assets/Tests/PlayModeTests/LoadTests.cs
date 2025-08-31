using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;

public class LoadTests
{
    private readonly int[] counts = { 500, 1000, 2000, 3000, 4000, 5000 };

    private GameObject enemyTemplate;     // scene “prefab template” – never destroy this
    private readonly List<GameObject> spawned = new(); // track only what we spawn

    private GameObject playerGO;
    private GameObject camGO;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Camera
        camGO = new GameObject("MainCamera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.transform.position = new Vector3(0, 0, -10);

        // Player (far away + dummy VFX so Instantiate() in Player.Destruction() never nulls)
        playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        playerGO.AddComponent<BoxCollider2D>();
        var player = playerGO.AddComponent<Player>();
        Player.instance = player;
        player.destructionFX = new GameObject("DummyPlayerVFX");
        playerGO.transform.position = new Vector3(9999, 9999, 0);

        // Enemy template (do NOT add this to 'spawned')
        enemyTemplate = new GameObject("EnemyTemplate");
        var e = enemyTemplate.AddComponent<Enemy>();
        e.health = 2; e.shield = 0;
        e.destructionVFX = new GameObject("VFX");
        e.hitEffect = new GameObject("Hit");

        // Enemy projectile that won’t trigger player death paths
        e.Projectile = new GameObject("Projectile");
        var proj = e.Projectile.AddComponent<Projectile>();
        proj.damage = 1;
        proj.enemyBullet = true;
        proj.destroyedByCollision = false; // avoid hitting Player.GetDamage()
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
    public IEnumerator Enemies_Sustain_FrameTarget_At_LoadLevels()
    {
        const int warmup = 20;
        const int measure = 180;

        foreach (var n in counts)
        {
            // Spawn N from the template; track each instance so we only destroy instances
            for (int i = 0; i < n; i++)
            {
                var go = Object.Instantiate(enemyTemplate);
                spawned.Add(go);

                go.name = $"Enemy_{i}";
                go.transform.position = new Vector3(Random.Range(-7f, 7f), Random.Range(-3.5f, 3.5f), 0f);

                var p = go.GetComponent<FollowThePath>();
                if (p != null)
                {
                    p.path = new Transform[4];
                    p.path[0] = Point(go.transform.position);
                    p.path[1] = Point(go.transform.position + new Vector3(1.0f, 0.4f, 0f));
                    p.path[2] = Point(go.transform.position + new Vector3(0.2f, 0.9f, 0f));
                    p.path[3] = Point(go.transform.position + new Vector3(-0.8f, 0.2f, 0f));
                    p.speed = 2.0f;
                    p.loop = true;
                    p.rotationByPath = false;
                    p.SetPath();
                    foreach (var t in p.path) t.parent = go.transform; // auto-clean
                }
            }

            // settle
            yield return new WaitForSeconds(1.0f);

            // sample ~3s (and compute avg FPS ourselves for a clear console line)
            var group = new SampleGroup($"Frame (N={n})", SampleUnit.Millisecond);
            float totalDt = 0f;
            int measuredFrames = 0;

            using (Measure.Frames().SampleGroup(group).WarmupCount(warmup).MeasurementCount(measure).Scope())
            {
                for (int f = 0; f < warmup + measure; f++)
                {
                    yield return null;
                    if (f >= warmup) { totalDt += Time.deltaTime; measuredFrames++; }
                }
            }

            var avgDelta = (measuredFrames > 0) ? totalDt / measuredFrames : 0f;
            var fps = (avgDelta > 0f) ? (1f / avgDelta) : 0f;
            Debug.Log($"[LOAD] N={n} | AvgFPS ≈ {fps:0.0} over {measuredFrames} frames.");

            // destroy only spawned instances from this step
            foreach (var go in spawned)
                if (go) Object.Destroy(go);
            spawned.Clear();

            yield return null;
        }

        Assert.Pass("Load test ran. Review Performance Report and the FPS logs per N.");
    }

    private Transform Point(Vector3 p)
    {
        var t = new GameObject("pt").transform; t.position = p; return t;
    }
}