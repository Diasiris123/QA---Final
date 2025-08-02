using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerMovingClampTests
{
    private GameObject cameraGO;
    private GameObject playerGO;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        cameraGO = new GameObject("Main Camera");
        cameraGO.tag = "MainCamera";
        var cam = cameraGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.transform.position = new Vector3(0, 0, -10);

        yield return null; 
    }

    [UnityTest]
    public IEnumerator Player_IsClampedInsideBorders_WhenMovedOutside()
    {
        playerGO = new GameObject("Player");
        var playerMoving = playerGO.AddComponent<PlayerMoving>();
        playerMoving.borders = new Borders
        {
            minXOffset = 1.5f,
            maxXOffset = 1.5f,
            minYOffset = 1.5f,
            maxYOffset = 1.5f
        };

        yield return null; // For start
        
        playerGO.transform.position = new Vector3(100, 100, 0); // Goes to invalid position

        yield return null; // For Update
        
        var pos = playerGO.transform.position;
        Debug.Log($"Clamped Position: {pos}");
        
        Assert.Less(pos.x, 10f);
        Assert.Less(pos.y, 10f);
        Assert.Greater(pos.x, -10f);
        Assert.Greater(pos.y, -10f);
    }
}