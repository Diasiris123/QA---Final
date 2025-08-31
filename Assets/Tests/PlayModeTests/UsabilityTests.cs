using System;
using System.Collections;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class UsabilityTests
{
    private GameObject camGO;
    private GameObject playerGO;
    private PlayerShooting shooter;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        Telemetry.Clear();

        camGO = new GameObject("MainCamera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.transform.position = new Vector3(0, 0, -10);

        playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        playerGO.AddComponent<BoxCollider2D>();
        var player = playerGO.AddComponent<Player>();
        Player.instance = player;
        player.destructionFX = new GameObject("DummyPlayerVFX");

        shooter = playerGO.AddComponent<PlayerShooting>();
        PlayerShooting.instance = shooter;

        // guns with ParticleSystem ON them + assign *VFX fields too (covers both code paths)
        shooter.guns = new Guns
        {
            centralGun = new GameObject("CentralGun"),
            leftGun = new GameObject("LeftGun"),
            rightGun = new GameObject("RightGun"),
        };
        shooter.guns.centralGun.transform.parent = playerGO.transform;
        shooter.guns.leftGun.transform.parent = playerGO.transform;
        shooter.guns.rightGun.transform.parent = playerGO.transform;

        shooter.guns.centralGunVFX = shooter.guns.centralGun.AddComponent<ParticleSystem>();
        shooter.guns.leftGunVFX    = shooter.guns.leftGun.AddComponent<ParticleSystem>();
        shooter.guns.rightGunVFX   = shooter.guns.rightGun.AddComponent<ParticleSystem>();

        shooter.projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shooter.fireRate = 10f;
        shooter.weaponPower = 1;

        shooter.enabled = false; // avoid auto Update; we'll call MakeAShot manually

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (playerGO) Object.DestroyImmediate(playerGO);
        if (camGO) Object.DestroyImmediate(camGO);
        Telemetry.Clear();
        yield return null;
    }

    

    [UnityTest, Performance]
    public IEnumerator PlayerClamp_Smoke_NoSpikes()
    {
        var mover = playerGO.AddComponent<PlayerMoving>();
        mover.borders = new Borders
        {
            minXOffset = 1.5f, maxXOffset = 1.5f,
            minYOffset = 1.5f, maxYOffset = 1.5f
        };

        // IMPORTANT: let PlayerMoving.Start() run BEFORE we move the player
        yield return null;

        playerGO.transform.position = new Vector3(100, 100, 0); // outside
        yield return null; // allow one Update to clamp

        using (Measure.Frames().WarmupCount(1).MeasurementCount(1).Scope())
        {
            // no-op; just record a frame to show no spike on clamp
            yield return null;
            Telemetry.Log("ux:clamp_applied");
        }

        var p = playerGO.transform.position;
        Assert.That(p.x, Is.LessThan(10f));
        Assert.That(p.y, Is.LessThan(10f));
        Assert.That(p.x, Is.GreaterThan(-10f));
        Assert.That(p.y, Is.GreaterThan(-10f));

        try { Telemetry.SaveToCSV(); } catch { /* non-fatal */ }
    }
}