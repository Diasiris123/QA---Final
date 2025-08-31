using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BonusTests
{
    private GameObject bonusGO;
    private GameObject playerGO;
    private PlayerShooting shooting;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        playerGO = new GameObject("Player");
        //playerGO.tag = "Player";
        playerGO.AddComponent<BoxCollider2D>();
        
        shooting = playerGO.AddComponent<PlayerShooting>();
        PlayerShooting.instance = shooting;
        shooting.weaponPower = 2;
        shooting.maxweaponPower = 4;
        shooting.fireRate = 10f;
        shooting.projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere); // dummy projectile
        
        shooting.guns = new Guns();
        shooting.guns.leftGun = new GameObject("LeftGun");
        shooting.guns.rightGun = new GameObject("RightGun");
        shooting.guns.centralGun = new GameObject("CentralGun");

        shooting.guns.leftGunVFX = shooting.guns.leftGun.AddComponent<ParticleSystem>();
        shooting.guns.rightGunVFX = shooting.guns.rightGun.AddComponent<ParticleSystem>();
        shooting.guns.centralGunVFX = shooting.guns.centralGun.AddComponent<ParticleSystem>();
        
        shooting.guns.leftGun.transform.parent = playerGO.transform;
        shooting.guns.rightGun.transform.parent = playerGO.transform;
        shooting.guns.centralGun.transform.parent = playerGO.transform;
        
        bonusGO = new GameObject("Bonus");
        var collider = bonusGO.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        bonusGO.AddComponent<Bonus>();

        yield return null;
    }

    [UnityTest]
    public IEnumerator Bonus_IncreasesWeaponPower_WhenBelowMax()
    {
        int before = PlayerShooting.instance.weaponPower;

        bonusGO.GetComponent<Bonus>().SendMessage("OnTriggerEnter2D", playerGO.GetComponent<Collider2D>());

        yield return null;

        Assert.AreEqual(before + 1, PlayerShooting.instance.weaponPower);
    }

    [UnityTest]
    public IEnumerator Bonus_DoesNotIncrease_WhenAtMax()
    {
        PlayerShooting.instance.weaponPower = PlayerShooting.instance.maxweaponPower;

        bonusGO.GetComponent<Bonus>().SendMessage("OnTriggerEnter2D", playerGO.GetComponent<Collider2D>());

        yield return null;

        Assert.AreEqual(PlayerShooting.instance.maxweaponPower, PlayerShooting.instance.weaponPower);
    }

    [UnityTest]
    public IEnumerator Bonus_DestroysItself_AfterTrigger()
    {
        bonusGO.GetComponent<Bonus>().SendMessage("OnTriggerEnter2D", playerGO.GetComponent<Collider2D>());

        yield return null;

        Assert.IsTrue(bonusGO == null || bonusGO.Equals(null));
    }

    [UnityTest]
    public IEnumerator Bonus_IgnoresNonPlayerCollision()
    {
        var enemyGO = new GameObject("Enemy");
        //enemyGO.tag = "Enemy";
        enemyGO.AddComponent<BoxCollider2D>();

        int before = PlayerShooting.instance.weaponPower;

        bonusGO.GetComponent<Bonus>().SendMessage("OnTriggerEnter2D", enemyGO.GetComponent<Collider2D>());

        yield return null;

        Assert.AreEqual(before, PlayerShooting.instance.weaponPower);
        Assert.IsNotNull(bonusGO); // Bonus should not be destroyed

        Object.DestroyImmediate(enemyGO);
    }
}
