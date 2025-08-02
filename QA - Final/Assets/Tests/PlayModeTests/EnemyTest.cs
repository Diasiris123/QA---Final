using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyTests
{
    private GameObject enemyGO;
    private Enemy enemy;

    public class TestPlayer : Player // only for test
    {
        public int receivedDamage = 0;

        public override void GetDamage(int damage)
        {
            receivedDamage = damage;
        }
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        enemyGO = new GameObject("Enemy");
        enemy = enemyGO.AddComponent<Enemy>();
        enemy.health = 2;
        
        enemy.Projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        enemy.destructionVFX = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemy.hitEffect = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        yield return null;
    }

    [UnityTest]
    public IEnumerator GetDamage_ReducesHealth()
    {
        int startHealth = enemy.health;
        enemy.GetDamage(1);
        yield return null;
        Assert.AreEqual(startHealth - 1, enemy.health);
    }

    [UnityTest]
    public IEnumerator GetDamage_TriggersDestruction_WhenHealthBelowZero()
    {
        enemy.health = 1;
        enemy.GetDamage(2);
        yield return null;
        Assert.IsTrue(enemyGO == null || enemyGO.Equals(null));
    }

    [UnityTest]
    public IEnumerator GetDamage_InstantiatesHitEffect_WhenStillAlive()
    {
        enemy.health = 2;
        enemy.shield = 0;
        int before = Object.FindObjectsOfType<MeshRenderer>().Length;

        enemy.GetDamage(1);
        yield return null;

        int after = Object.FindObjectsOfType<MeshRenderer>().Length;
        Assert.Greater(after, before); 
    }

    [UnityTest]
    public IEnumerator Destruction_InstantiatesDestructionVFX()
    {
        enemy.health = 1;

        int before = Object.FindObjectsOfType<MeshRenderer>().Length;
        enemy.GetDamage(2);
        yield return null;

        int after = Object.FindObjectsOfType<MeshRenderer>().Length;
        Assert.Greater(after, before);
    }

    [UnityTest]
    public IEnumerator OnTriggerEnter2D_DamagesPlayer()
    {
        var playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        var testPlayer = playerGO.AddComponent<TestPlayer>();
        Player.instance = testPlayer;

        playerGO.AddComponent<BoxCollider2D>();
        enemyGO.AddComponent<BoxCollider2D>();
        
        var projScript = enemy.Projectile.AddComponent<Projectile>();
        projScript.damage = 5;
        
        enemy.OnTriggerEnter2D(playerGO.GetComponent<Collider2D>());
        yield return null;

        Assert.AreEqual(5, testPlayer.receivedDamage);

        Object.Destroy(playerGO);
    }
}
