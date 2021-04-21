using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TimeManagerTests : MonoBehaviour{
    TimeManager time;
    GameManager game;
    EventManager events;

    [SetUp]
    public void Setup(){
        GameObject tmp = new GameObject();
        time = tmp.AddComponent<TimeManager>();
        game = tmp.AddComponent<GameManager>();
        events = tmp.AddComponent<EventManager>();
        game.testMode = true;
    }

    [TearDown]
    public void TearDown(){
        Destroy(time);
        Destroy(game);
        Destroy(events);
    }

    [UnityTest]
    public IEnumerator TicksPerSecond(){
        Assert.AreEqual(0, time.Tick);
        yield return new WaitForSeconds(1f);
        Assert.AreEqual(10, time.Tick);
        yield return new WaitForSeconds(1f);
        Assert.AreEqual(20, time.Tick);
        yield return new WaitForSeconds(0.1f);
        Assert.AreEqual(21, time.Tick);
    }

    [UnityTest]
    public IEnumerator SpeedChange(){
        Assert.AreEqual(0, time.Tick);
        yield return new WaitForSeconds(1f);
        Assert.AreEqual(10, time.Tick);

        time.GameSpeed = 20;
        yield return new WaitForSeconds(1f);
        Assert.AreEqual(30, time.Tick);

        time.GameSpeed = 30;
        yield return new WaitForSeconds(1f);
        Assert.AreEqual(60, time.Tick);

        time.GameSpeed = 10;
        yield return new WaitForSeconds(1f);
        Assert.AreEqual(70, time.Tick);
    }

    [UnityTest]
    public IEnumerator PauseAndResume(){
        Assert.AreEqual(0, time.Tick);

        Assert.AreEqual(0, time.Tick);
        yield return new WaitForSeconds(0.5f);
        Assert.AreEqual(5, time.Tick);


        time.Paused = true;
        
        yield return new WaitForSeconds(0.8f);
        Assert.AreEqual(5, time.Tick);

        time.Paused = false;
        time.GameSpeed = 10;
        yield return new WaitForSeconds(1.5f);
        Assert.AreEqual(20, time.Tick);
    }
}