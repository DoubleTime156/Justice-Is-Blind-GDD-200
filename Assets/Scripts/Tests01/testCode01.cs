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
        destination.y += 2.1f;
        Assert.That(player.transform.position.y, Is.EqualTo(destination.y).Within(0.25), "up issue");

        //moving down
        Debug.Log("down Start: " + player.transform.position.y);
        Press(keyboard.sKey);
        yield return new WaitForSeconds(0.25f);
        Release(keyboard.sKey);
        yield return new WaitForSeconds(1);
        //checking movement distance
        Debug.Log("down done: " + player.transform.position.y);
        destination.y -= 2.1f;
        Assert.That(player.transform.position.y, Is.EqualTo(destination.y).Within(0.25), "down issue");

        //moving right
        Press(keyboard.dKey);
        yield return new WaitForSeconds(0.25f);
        Release(keyboard.dKey);
        yield return new WaitForSeconds(1);
        //checking movement distance
        destination.x += 2.1f;
        Assert.That(player.transform.position.x, Is.EqualTo(destination.x).Within(0.25), "right issue");

        //moving left
        Press(keyboard.aKey);
        yield return new WaitForSeconds(0.25f);
        Release(keyboard.aKey);
        yield return new WaitForSeconds(1);
        //checking movement distance
        destination.x -= 2.1f;
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
        destination.y += 4.2f;
        Debug.Log("Sprint Start: " + player.transform.position.y);
        Assert.That(player.transform.position.y, Is.EqualTo(destination.y).Within(0.25), "sprint issue");

      
        

        yield return null;
    }

}
