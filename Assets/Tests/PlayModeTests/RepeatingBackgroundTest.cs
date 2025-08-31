using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections;

public class RepeatingBackgroundTests
{
    private GameObject backgroundGO;
    private RepeatingBackground repeatingBackground;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        backgroundGO = new GameObject("Background");
        repeatingBackground = backgroundGO.AddComponent<RepeatingBackground>();
        repeatingBackground.verticalSize = 5f;

        yield return null;
    }

    [UnityTest]
    public IEnumerator Reposition_WhenBelowThreshold()
    {
        backgroundGO.transform.position = new Vector2(0, -6f);
        
        yield return null;

        float expectedY = -6f + (5f * 2f);
        Assert.AreEqual(expectedY, backgroundGO.transform.position.y, 0.01f);
    }

    [UnityTest]
    public IEnumerator DoesNotReposition_WhenAboveThreshold()
    {
        backgroundGO.transform.position = new Vector2(0, -4f);
        
        yield return null;

        Assert.AreEqual(-4f, backgroundGO.transform.position.y, 0.01f);
    }
}