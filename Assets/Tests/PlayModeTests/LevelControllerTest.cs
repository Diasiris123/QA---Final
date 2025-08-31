using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class LevelControllerTests
{
    private GameObject levelControllerGO;
    private LevelController levelController;
    private GameObject wavePrefab;
    private GameObject powerup;
    private GameObject planet1;
    private GameObject planet2;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        var playerGO = new GameObject("Player");
        playerGO.AddComponent<Player>();
        Player.instance = playerGO.GetComponent<Player>();
        Object.DontDestroyOnLoad(playerGO);
        
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
        Object.DontDestroyOnLoad(playerMovingGO);
        
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.AddComponent<Camera>();
        camGO.transform.position = new Vector3(0, 0, -10);
        Object.DontDestroyOnLoad(camGO);

        yield return null;
        
        wavePrefab = new GameObject("Wave");
        wavePrefab.AddComponent<Wave>();
        Object.DontDestroyOnLoad(wavePrefab);

        powerup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        powerup.name = "PowerUp";
        Object.DontDestroyOnLoad(powerup);
        
        planet1 = new GameObject("Planet1");
        planet1.AddComponent<DirectMoving>();
        planet2 = new GameObject("Planet2");
        planet2.AddComponent<DirectMoving>();
        Object.DontDestroyOnLoad(planet1);
        Object.DontDestroyOnLoad(planet2);
        
        levelControllerGO = new GameObject("LevelController");
        levelController = levelControllerGO.AddComponent<LevelController>();

        levelController.enemyWaves = new EnemyWaves[]
        {
            new EnemyWaves { timeToStart = 0.1f, wave = wavePrefab }
        };

        levelController.powerUp = powerup;
        levelController.timeForNewPowerup = 0.1f;
        levelController.planets = new GameObject[] { planet1, planet2 };
        levelController.timeBetweenPlanets = 0.1f;
        levelController.planetsSpeed = 1f;

        Object.DontDestroyOnLoad(levelControllerGO);

        yield return null;
    }

    [UnityTest]
    public IEnumerator PowerUp_IsInstantiated()
    {
        yield return new WaitForSeconds(0.3f);
        Assert.IsNotNull(GameObject.Find("PowerUp(Clone)"));
    }

    [UnityTest]
    public IEnumerator Planet_IsInstantiated()
    {
        yield return new WaitForSeconds(10.3f); 
        var found = GameObject.Find("Planet1(Clone)") ?? GameObject.Find("Planet2(Clone)");
        Assert.IsNotNull(found);
    }
}
