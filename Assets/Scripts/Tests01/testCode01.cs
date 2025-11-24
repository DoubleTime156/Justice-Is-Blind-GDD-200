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
    
    
    float MoE = 1.0f;//margin of error
    public override void Setup()
    {
        SceneManager.LoadScene("Scenes/Test_Level");
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
        Assert.That(player.transform.position.y, Is.EqualTo(destination.y).Within(MoE), "up issue");

        //moving down
        Debug.Log("down Start: " + player.transform.position.y);
        Press(keyboard.sKey);
        yield return new WaitForSeconds(0.25f);
        Release(keyboard.sKey);
        yield return new WaitForSeconds(1);
        //checking movement distance
        Debug.Log("down done: " + player.transform.position.y);
        destination.y -= 2.1f;
        Assert.That(player.transform.position.y, Is.EqualTo(destination.y).Within(MoE), "down issue");

        //moving right
        Press(keyboard.dKey);
        yield return new WaitForSeconds(0.25f);
        Release(keyboard.dKey);
        yield return new WaitForSeconds(1);
        //checking movement distance
        destination.x += 2.1f;
        Assert.That(player.transform.position.x, Is.EqualTo(destination.x).Within(MoE), "right issue");

        //moving left
        Press(keyboard.aKey);
        yield return new WaitForSeconds(0.25f);
        Release(keyboard.aKey);
        yield return new WaitForSeconds(1);
        //checking movement distance
        destination.x -= 2.1f;
        Assert.That(player.transform.position.x, Is.EqualTo(destination.x).Within(MoE), "left issue");

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
        Assert.That(player.transform.position.y, Is.EqualTo(destination.y).Within(MoE), "sprint issue");

        //testing cane tap, use visual assertion
        //Press(keyboard.spaceKey);
        //yield return new WaitForSeconds(0.25f);
        //Release(keyboard.spaceKey);
        //yield return new WaitForSeconds(1.0f);        

        yield return null;
    }

}
