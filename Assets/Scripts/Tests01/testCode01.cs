using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

public class testCode01 : InputTestFixture
{

    //settup for input testing
    Keyboard keyboard;
    Mouse mouse;


    public override void Setup()
    {
        SceneManager.LoadScene("Scenes/Level_Tutorial");
        base.Setup();
        //input devices
        keyboard = InputSystem.AddDevice<Keyboard>("keyboard");
        mouse = InputSystem.AddDevice<Mouse>("mouse");
    }

    [UnityTest]
    public IEnumerator BasicMovementTests()
    {
        //find starting position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 StartPosition = player.transform.position;
        Vector2 destination = StartPosition;

        //moving up
        Debug.Log("up Start: " + player.transform.position.y);
        Press(keyboard.wKey);
        yield return new WaitForSeconds(0.25f);
        Release(keyboard.wKey);
        yield return new WaitForSeconds(1);
        //checking movement distance
        Debug.Log("up done: " + player.transform.position.y);
        destination.y += 1.9f;
       // Assert.That(player.transform.position.y, Is.EqualTo(destination.y).Within(0.25), "up issue");

        //moving down
        Press(keyboard.sKey);
        yield return new WaitForSeconds(0.25f);
        Release(keyboard.sKey);
        yield return new WaitForSeconds(1);
        //checking movement distance
        destination.y -= 1.9f;
        Assert.That(player.transform.position.y, Is.EqualTo(destination.y).Within(0.25), "down issue");

        //moving right
        Press(keyboard.dKey);
        yield return new WaitForSeconds(0.25f);
        Release(keyboard.dKey);
        yield return new WaitForSeconds(1);
        //checking movement distance
        destination.x += 1.9f;
        Assert.That(player.transform.position.x, Is.EqualTo(destination.x).Within(0.25), "right issue");

        //moving left
        Press(keyboard.aKey);
        yield return new WaitForSeconds(0.25f);
        Release(keyboard.aKey);
        yield return new WaitForSeconds(1);
        //checking movement distance
        destination.x -= 1.9f;
        Assert.That(player.transform.position.x, Is.EqualTo(destination.x).Within(0.25), "left issue");

        // //testing sprint
        Debug.Log("Sprint Start: " + player.transform.position.y);
        Press(keyboard.shiftKey);
        yield return new WaitForSeconds(0.5f);
        Press(keyboard.wKey);
        yield return new WaitForSeconds(0.25f);
        Release(keyboard.wKey);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(0.5f);
        Release(keyboard.shiftKey);
        yield return new WaitForSeconds(1);
        //checking movement distance
        destination.y += 3.8f;
        Debug.Log("Sprint Start: " + player.transform.position.y);
        Assert.That(player.transform.position.y, Is.EqualTo(destination.y).Within(0.25), "sprint issue");

        ////testing sneak
        //Press(keyboard.cKey);
        //yield return new WaitForSeconds(0.5f);
        //Press(keyboard.sKey);
        //yield return new WaitForSeconds(0.25f);
        //Release(keyboard.sKey);
        //yield return new WaitForSeconds(0.5f);
        //Release(keyboard.cKey);
        //yield return new WaitForSeconds(1);
        ////checking movement distance
        //destination.y -= 0.9f;
        //Assert.That(player.transform.position.y, Is.EqualTo(destination.y).Within(0.25), "sneak issue");

        

        yield return null;
    }

    //// A Test behaves as an ordinary method
    //[Test]
    //public void testCode01SimplePasses()
    //{
    //    // Use the Assert class to test conditions
    //}

    //// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    //// `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator testCode01WithEnumeratorPasses()
    //{
    //    // Use the Assert class to test conditions.
    //    // Use yield to skip a frame.
    //    yield return null;
    //}
}
