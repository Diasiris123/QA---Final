using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerShootingTests
{
    private GameObject playerGO;
    private PlayerShooting shooter;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        playerGO = new GameObject("PlayerShooting");

        shooter = playerGO.AddComponent<PlayerShooting>();
        PlayerShooting.instance = shooter;
        
        shooter.projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        
        shooter.guns = new Guns();
        shooter.guns.centralGun = new GameObject("CentralGun");
        shooter.guns.rightGun = new GameObject("RightGun");
        shooter.guns.leftGun = new GameObject("LeftGun");

        shooter.guns.centralGunVFX = shooter.guns.centralGun.AddComponent<ParticleSystem>();
        shooter.guns.leftGunVFX = shooter.guns.leftGun.AddComponent<ParticleSystem>();
        shooter.guns.rightGunVFX = shooter.guns.rightGun.AddComponent<ParticleSystem>();

        shooter.fireRate = 10f;
        shooter.weaponPower = 1;
        
        shooter.guns.centralGun.transform.parent = playerGO.transform;
        shooter.guns.leftGun.transform.parent = playerGO.transform;
        shooter.guns.rightGun.transform.parent = playerGO.transform;

        yield return null;
    }

    [UnityTest]
    public IEnumerator MakeAShot_CreatesCorrectAmountOfProjectiles()
    {
        int[] expectedShots = { 1, 2, 3, 5 };

        for (int power = 1; power <= 4; power++)
        {
            shooter.weaponPower = power;
            
            int beforeCount = GameObject.FindObjectsOfType<SphereCollider>().Length;
            
            typeof(PlayerShooting).GetMethod("MakeAShot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(shooter, null);

            yield return null;

            int afterCount = GameObject.FindObjectsOfType<SphereCollider>().Length;

            int shotsFired = afterCount - beforeCount;
            Assert.AreEqual(expectedShots[power - 1], shotsFired, $"Wrong number of shots at weaponPower {power}");
        }
    }

    [UnityTest]
    public IEnumerator SingletonInstance_IsAssigned()
    {
        yield return null;
        Assert.IsNotNull(PlayerShooting.instance); 
        Assert.AreEqual(shooter, PlayerShooting.instance);
    }
}
