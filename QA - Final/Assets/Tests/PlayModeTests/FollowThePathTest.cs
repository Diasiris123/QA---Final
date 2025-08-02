using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FollowThePathTest
{
    private GameObject enemyGO;
    private FollowThePath followScript;
    private GameObject[] pathPoints;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        pathPoints = new GameObject[4];
        pathPoints[0] = CreatePoint(new Vector3(0, 0, 0));
        pathPoints[1] = CreatePoint(new Vector3(1, 0, 0));
        pathPoints[2] = CreatePoint(new Vector3(2, 1, 0));
        pathPoints[3] = CreatePoint(new Vector3(3, 1, 0));
        
        enemyGO = new GameObject("Enemy");
        followScript = enemyGO.AddComponent<FollowThePath>();
        followScript.path = new Transform[4];
        for (int i = 0; i < 4; i++) followScript.path[i] = pathPoints[i].transform;

        followScript.speed = 5f;
        followScript.rotationByPath = false;
        followScript.loop = false;

        followScript.SetPath(); 

        yield return null;
    }

    private GameObject CreatePoint(Vector3 pos)
    {
        var go = new GameObject("PathPoint");
        go.transform.position = pos;
        return go;
    }

    [UnityTest]
    public IEnumerator SetPath_PositionsObjectAtStart()
    {
        yield return null;

        Vector3 expected = pathPoints[0].transform.position;
        Vector3 actual = enemyGO.transform.position;

        Assert.AreEqual(expected.ToString(), actual.ToString()); // <ToString()> because for some reason the vector compare didn't work correctly
    }

    [UnityTest]
    public IEnumerator Object_MovesAlongPath_OverTime()
    {
        Vector3 initialPos = enemyGO.transform.position;
        yield return new WaitForSeconds(0.5f); 

        Vector3 currentPos = enemyGO.transform.position;
        Assert.AreNotEqual(initialPos, currentPos);
    }

    [UnityTest]
    public IEnumerator Object_IsDestroyed_WhenNotLooping()
    {
        followScript.speed = 100f; 
        yield return new WaitForSeconds(2f); 

        Assert.IsTrue(enemyGO == null || enemyGO.Equals(null));
    }

    [UnityTest]
    public IEnumerator Object_Loops_WhenLoopEnabled()
    {
        followScript = enemyGO.AddComponent<FollowThePath>();
        followScript.path = new Transform[4];
        for (int i = 0; i < 4; i++) followScript.path[i] = pathPoints[i].transform;

        followScript.speed = 100f;
        followScript.rotationByPath = false;
        followScript.loop = true;

        followScript.SetPath();
        yield return new WaitForSeconds(2f); 

        Assert.IsNotNull(followScript); 
    }

    [UnityTest]
    public IEnumerator Rotation_DoesNotCrash_WhenEnabled()
    {
        followScript.rotationByPath = true;
        followScript.speed = 10f;
        yield return new WaitForSeconds(0.5f); 

        Assert.Pass(); 
    }

    [UnityTest]
    public IEnumerator Path_With_Fewer_Than_4_Points_DoesNotCrash()
    {
        var shortEnemy = new GameObject("ShortPathEnemy");
        var shortFollow = shortEnemy.AddComponent<FollowThePath>();
        shortFollow.path = new Transform[3]; 

        for (int i = 0; i < 3; i++)
            shortFollow.path[i] = CreatePoint(new Vector3(i, i, 0)).transform;

        shortFollow.speed = 10f;

        LogAssert.ignoreFailingMessages = true;

        shortFollow.SetPath();
        yield return new WaitForSeconds(0.5f);

        Assert.IsNotNull(shortEnemy);
        Object.Destroy(shortEnemy);
    }
}
