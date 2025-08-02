using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerTest
{
    private GameObject playerObj;
    private Player player;

    [SetUp]
    public void SetUp()
    {
        playerObj = new GameObject("Player");
        player = playerObj.AddComponent<Player>();

        var dummyVFX = GameObject.CreatePrimitive(PrimitiveType.Cube); 
        dummyVFX.name = "DestructionFX";
        player.destructionFX = dummyVFX;
    }

    [UnityTest]
    public IEnumerator Player_AssignsItselfAsInstance()
    {
        yield return null;

        Assert.IsNotNull(Player.instance);
        Assert.AreEqual(player, Player.instance);
    }

    [UnityTest]
    public IEnumerator GetDamage_TriggersDestruction()
    {
        yield return null;

        player.GetDamage(1);

        yield return null;

        Assert.IsTrue(playerObj == null || playerObj.Equals(null)); 
    }

    [UnityTest]
    public IEnumerator DestructionFX_IsInstantiated()
    {
        yield return null;

        var originalVFXCount = GameObject.FindObjectsOfType<MeshRenderer>().Length;

        player.GetDamage(5);

        yield return null;

        var newVFXCount = GameObject.FindObjectsOfType<MeshRenderer>().Length;

        Assert.Greater(newVFXCount, originalVFXCount); 
    }
}