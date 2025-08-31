using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ProjectileTest
{
    private GameObject projectileGO;

    public class TestPlayer : Player // only for test purposes
    {
        public int damageTaken = 0;
        public override void GetDamage(int dmg) => damageTaken = dmg;
    }

    public class TestEnemy : Enemy // only for tests purposes
    {
        public int damageTaken = 0;
        public override void GetDamage(int dmg) => damageTaken = dmg;
    }

    [UnityTest]
    public IEnumerator EnemyBullet_HitsPlayer_TakesDamage_AndIsDestroyed()
    {
        var playerGO = new GameObject("Player");
        //playerGO.tag = "Player";
        var testPlayer = playerGO.AddComponent<TestPlayer>();
        Player.instance = testPlayer;
        playerGO.AddComponent<BoxCollider2D>();

        projectileGO = new GameObject("Projectile");
        var proj = projectileGO.AddComponent<Projectile>();
        proj.damage = 5;
        proj.enemyBullet = true;
        proj.destroyedByCollision = true;
        projectileGO.AddComponent<BoxCollider2D>().isTrigger = true;
        
        proj.SendMessage("OnTriggerEnter2D", playerGO.GetComponent<Collider2D>());

        yield return null;
        
        Assert.AreEqual(5, testPlayer.damageTaken);
        Assert.IsTrue(projectileGO == null || projectileGO.Equals(null));
    }

    [UnityTest]
    public IEnumerator PlayerBullet_HitsEnemy_TakesDamage_AndIsDestroyed()
    {
        var enemyGO = new GameObject("Enemy");
        //enemyGO.tag = "Enemy";
        var testEnemy = enemyGO.AddComponent<TestEnemy>();
        enemyGO.AddComponent<BoxCollider2D>();

        projectileGO = new GameObject("Projectile");
        var proj = projectileGO.AddComponent<Projectile>();
        proj.damage = 4;
        proj.enemyBullet = false;
        proj.destroyedByCollision = true;
        projectileGO.AddComponent<BoxCollider2D>().isTrigger = true;

        proj.SendMessage("OnTriggerEnter2D", enemyGO.GetComponent<Collider2D>());
        yield return null;

        Assert.AreEqual(4, testEnemy.damageTaken);
        Assert.IsTrue(projectileGO == null || projectileGO.Equals(null));

        Object.Destroy(enemyGO);
    }

    [UnityTest]
    public IEnumerator Bullet_IsNotDestroyed_WhenNotSetToDestroy()
    {
        var enemyGO = new GameObject("Enemy");
        //enemyGO.tag = "Enemy";
        enemyGO.AddComponent<TestEnemy>();
        enemyGO.AddComponent<BoxCollider2D>();

        projectileGO = new GameObject("Projectile");
        var proj = projectileGO.AddComponent<Projectile>();
        proj.damage = 1;
        proj.enemyBullet = false;
        proj.destroyedByCollision = false;
        projectileGO.AddComponent<BoxCollider2D>().isTrigger = true;

        proj.SendMessage("OnTriggerEnter2D", enemyGO.GetComponent<Collider2D>());
        yield return null;

        Assert.IsNotNull(projectileGO);
        Object.Destroy(enemyGO);
    }

    [UnityTest]
    public IEnumerator Bullet_Ignores_OtherTags()
    {
        var wall = new GameObject("Wall");
        //wall.tag = "Untagged"; 
        wall.AddComponent<BoxCollider2D>();

        projectileGO = new GameObject("Projectile");
        var proj = projectileGO.AddComponent<Projectile>();
        proj.damage = 1;
        proj.enemyBullet = false;
        proj.destroyedByCollision = true;
        projectileGO.AddComponent<BoxCollider2D>().isTrigger = true;

        proj.SendMessage("OnTriggerEnter2D", wall.GetComponent<Collider2D>());
        yield return null;

        Assert.IsNotNull(projectileGO);
        Object.Destroy(wall);
    }
}
