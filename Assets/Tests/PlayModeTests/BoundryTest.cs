using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BoundaryTests
{
    private GameObject boundaryGO;
    private GameObject cameraGO;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        cameraGO = new GameObject("Main Camera");
        var cam = cameraGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cameraGO.tag = "MainCamera";
        cam.transform.position = new Vector3(0, 0, -10);

        yield return null;
        
        boundaryGO = new GameObject("Boundary");
        var collider = boundaryGO.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        boundaryGO.AddComponent<Boundary>();

        yield return null;
    }

    [UnityTest]
    public IEnumerator ResizeCollider_SetsExpectedSize()
    {
        yield return null;

        var boxCollider = boundaryGO.GetComponent<BoxCollider2D>();
        var size = boxCollider.size;
        
        Assert.Greater(size.y, 10f);
        Assert.Less(size.y, 20f);
    }

    [UnityTest]
    public IEnumerator OnTriggerExit2D_DestroysProjectile()
    {
        var projectile = new GameObject("Projectile");
        projectile.tag = "Projectile";
        projectile.AddComponent<BoxCollider2D>();

        boundaryGO.GetComponent<Boundary>().SendMessage("OnTriggerExit2D", projectile.GetComponent<Collider2D>());

        yield return null;

        Assert.IsTrue(projectile == null || projectile.Equals(null));
    }

    [UnityTest]
    public IEnumerator OnTriggerExit2D_DestroysBonus()
    {
        var bonus = new GameObject("Bonus");
        bonus.tag = "Bonus";
        bonus.AddComponent<BoxCollider2D>();

        boundaryGO.GetComponent<Boundary>().SendMessage("OnTriggerExit2D", bonus.GetComponent<Collider2D>());

        yield return null;

        Assert.IsTrue(bonus == null || bonus.Equals(null));
    }

    [UnityTest]
    public IEnumerator OnTriggerExit2D_IgnoresUnrelatedTags()
    {
        var enemy = new GameObject("Enemy");
        enemy.tag = "Enemy";
        enemy.AddComponent<BoxCollider2D>();

        boundaryGO.GetComponent<Boundary>().SendMessage("OnTriggerExit2D", enemy.GetComponent<Collider2D>());

        yield return null;

        Assert.IsNotNull(enemy);
        Object.Destroy(enemy);
    }
}
