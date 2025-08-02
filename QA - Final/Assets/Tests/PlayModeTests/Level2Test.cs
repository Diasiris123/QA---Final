using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Level2Test
{
    private GameObject level2GO;
    private LevelController level2;
    private GameObject bossPrefab;
    private GameObject shieldedEnemyPrefab;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        var playerGO = new GameObject("Player");
        var player = playerGO.AddComponent<Player>();
        Player.instance = player;

        var playerMovingGO = new GameObject("PlayerMoving");
        var playerMoving = playerMovingGO.AddComponent<PlayerMoving>();
        playerMoving.borders = new Borders
        {
            minX = -5,
            maxX = 5,
            minY = -5,
            maxY = 5
        };
        PlayerMoving.instance = playerMoving;
        
        var camGO = new GameObject("MainCamera");
        camGO.tag = "MainCamera";
        camGO.AddComponent<Camera>();

        yield return null;
        
        bossPrefab = new GameObject("Boss");
        var boss = bossPrefab.AddComponent<Enemy>();
        boss.health = 200;
        boss.shield = 100;
        boss.destructionVFX = new GameObject("BossVFX");
        boss.hitEffect = new GameObject("BossHit");

        shieldedEnemyPrefab = new GameObject("ShieldedEnemy");
        var shielded = shieldedEnemyPrefab.AddComponent<Enemy>();
        shielded.health = 5;
        shielded.shield = 3;
        shielded.destructionVFX = new GameObject("ShieldedVFX");
        shielded.hitEffect = new GameObject("ShieldedHit");
        
        level2GO = new GameObject("Level2Controller");
        level2 = level2GO.AddComponent<LevelController>();
        level2.enemyWaves = new EnemyWaves[]
        {
            new EnemyWaves { timeToStart = 0.1f, wave = bossPrefab },
            new EnemyWaves { timeToStart = 0.2f, wave = shieldedEnemyPrefab }
        };

        level2.powerUp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        level2.timeForNewPowerup = 1f;
        
        var dummyPlanet = new GameObject("DummyPlanet");
        dummyPlanet.AddComponent<DirectMoving>();
        level2.planets = new GameObject[] { dummyPlanet };
        level2.planetsSpeed = 0.5f;
        level2.timeBetweenPlanets = 5f;

        yield return null;
    }

    [UnityTest]
    public IEnumerator Spawns_BossEnemy()
    {
        yield return new WaitForSeconds(0.3f);
        var boss = GameObject.Find("Boss(Clone)");
        Assert.IsNotNull(boss, "Boss enemy should have been instantiated.");
        var enemy = boss.GetComponent<Enemy>();
        Assert.AreEqual(200, enemy.health);
        Assert.AreEqual(100, enemy.shield);
    }

    [UnityTest]
    public IEnumerator Spawns_ShieldedEnemy()
    {
        yield return new WaitForSeconds(0.4f);
        var shielded = GameObject.Find("ShieldedEnemy(Clone)");
        Assert.IsNotNull(shielded, "Shielded enemy should have been instantiated.");
        var enemy = shielded.GetComponent<Enemy>();
        Assert.AreEqual(5, enemy.health);
        Assert.AreEqual(3, enemy.shield);
    }

    [UnityTest]
    public IEnumerator ShieldAbsorbs_DamageBeforeHealth()
    {
        var testGO = new GameObject("TestEnemy");
        var enemy = testGO.AddComponent<Enemy>();
        enemy.health = 5;
        enemy.shield = 3;
        enemy.destructionVFX = new GameObject();
        enemy.hitEffect = new GameObject();

        enemy.GetDamage(2);
        yield return null;
        Assert.AreEqual(1, enemy.shield);
        Assert.AreEqual(5, enemy.health);

        enemy.GetDamage(3);
        yield return null;
        Assert.AreEqual(0, enemy.shield);
        Assert.AreEqual(3, enemy.health);
    }

    [UnityTest]
    public IEnumerator BossEnemy_HasHigherHealthAndShield()
    {
        var bossGO = new GameObject("BossEnemy");
        var enemy = bossGO.AddComponent<Enemy>();
        enemy.health = 200;
        enemy.shield = 100;

        enemy.GetDamage(50);
        yield return null;

        Assert.AreEqual(50, enemy.shield);
        Assert.AreEqual(200, enemy.health);
    }
}
