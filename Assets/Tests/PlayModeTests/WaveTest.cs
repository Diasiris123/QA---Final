using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WaveTests
{
    private GameObject waveGO;
    private GameObject enemyPrefab;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        waveGO = new GameObject("Wave");
        var wave = waveGO.AddComponent<Wave>();

        enemyPrefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemyPrefab.AddComponent<Enemy>();
        enemyPrefab.AddComponent<FollowThePath>();

        wave.enemy = enemyPrefab;
        wave.count = 3;
        wave.speed = 1f;
        wave.timeBetween = 0.1f;
        wave.rotationByPath = false;
        wave.Loop = false;
        wave.shooting = new Shooting { shotChance = 0, shotTimeMin = 0f, shotTimeMax = 0f };

        var point1 = new GameObject("Path1").transform;
        var point2 = new GameObject("Path2").transform;
        point1.position = Vector3.zero;
        point2.position = Vector3.up;
        wave.pathPoints = new Transform[] { point1, point2 };

        yield return null;
    }

    [UnityTest]
    public IEnumerator Wave_CreatesExpectedEnemyCount()
    {
        yield return new WaitForSeconds(0.5f);
        var enemies = GameObject.FindObjectsOfType<Enemy>();
        Assert.AreEqual(4, enemies.Length);
    }
    
}