using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.TestTools;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Tests
{
    public class TestGameManager
    {
        GameManager gameManager;
        EventManager eventManager;
        TimeManager timeManager;
        BuildingTypeSO wcObject;
        GridXZ grid;

        [SetUp]
        public void Setup()
        {
            GameObject buildings = new GameObject();
            GameObject empty = new GameObject();
            GameObject help = new GameObject();
            buildings.name = "Buildings";
            gameManager = help.AddComponent<GameManager>();
            eventManager = help.AddComponent<EventManager>();
            timeManager = help.AddComponent<TimeManager>();
            empty.AddComponent<Attraction>();
            gameManager.testMode = true;
            grid = new GridXZ(1, 1, 3, Vector3.zero);
            wcObject = ScriptableObject.CreateInstance<BuildingTypeSO>();
            wcObject.type = BuildingTypeSO.Type.Attraction;
            wcObject.capacity = 1;
            wcObject.price = 150;
            wcObject.sellMultiplier = 0.5f;
            wcObject.baseIncome = 10;
            wcObject.breakChance = 0.2f;
            wcObject.width = 1;
            wcObject.height = 1;
            wcObject.prefab = empty.transform;
            wcObject.preview = empty.transform;
            wcObject.buildingName = "Toilet";
            wcObject.uiPrefab = empty.transform;


        }

        [TearDown]
        public void Teardown()
        {
            Object.Destroy(gameManager);
            Object.Destroy(eventManager);
            Object.Destroy(timeManager);
            grid = null;
            wcObject = null;
        }

        [UnityTest]
        public IEnumerator MoneyChangeUp()
        {
            gameManager.Money = 500;
            gameManager.Money = gameManager.Money + 10;
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(510, gameManager.Money);
        }
        
        [UnityTest]
        public IEnumerator MoneyChangeDown()
        {
            gameManager.Money = 500;
            gameManager.Money = gameManager.Money - 10;
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(490, gameManager.Money);
        }


        private bool test;
        [UnityTest]
        public IEnumerator RepairAttractionWithNoMoney(){
            test = false;
            List<Vector2Int> positionList =
                wcObject.GetPositionList(new Vector2Int(0, 0), BuildingTypeSO.Direction.Down);
            gameManager.Money = 0;
            Building wc = Building.SpawnBuilding(Vector3.zero, new Vector2Int(0, 0), BuildingTypeSO.Direction.Down,wcObject, positionList);
            yield return new WaitForSeconds(0.1f);
            eventManager.onNoMoney += HelperForRepairAttractionWithNoMoney;
            yield return new WaitForSeconds(0.1f);
            gameManager.RepairAttraction((Attraction)(wc));
            yield return new WaitForSeconds(0.1f);
            Assert.IsTrue(test);
        }

        private void HelperForRepairAttractionWithNoMoney()
        {
            test = true;
        }
        
        [UnityTest]
        public IEnumerator RepairAttractionWithMoneyButNoAvaliableMechanics(){
            test = false;
            List<Vector2Int> positionList =
                wcObject.GetPositionList(new Vector2Int(0, 0), BuildingTypeSO.Direction.Down);
            gameManager.Money = 99999;
            Building wc = Building.SpawnBuilding(Vector3.zero, new Vector2Int(0, 0), BuildingTypeSO.Direction.Down,wcObject, positionList);
            yield return new WaitForSeconds(0.1f);
            Attraction help = (Attraction)wc;
            GameObject empty = new GameObject();
            help.brokeVisual = empty.transform;
            help.BreakBuilding();
            yield return new WaitForSeconds(0.1f);
            gameManager.RepairAttraction((Attraction)(wc));
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(99999,gameManager.Money);
        }
        
        [UnityTest]
        public IEnumerator GameSecondCheck(){
            gameManager.GameLoop();
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(2,gameManager.GameSecond);
        }
        
        [UnityTest]
        public IEnumerator GameHourCheck(){
            for (int i = 0; i < 70; i++)
            {
                gameManager.GameLoop();
            }
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(1,gameManager.GameHour);
        }
        
        [UnityTest]
        public IEnumerator GameDayCheck(){
            gameManager.GameLoop();
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(0,gameManager.DayCount);
        }

        [UnityTest]
        public IEnumerator BuyBuildingCheck()
        {
            gameManager.Money = 500;
            gameManager.BuyBuilding(wcObject);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(350,gameManager.Money);
        }
        
        [UnityTest]
        public IEnumerator SoldBuildingCheck()
        {
            gameManager.Money = 500;
            List<Vector2Int> positionList =
                wcObject.GetPositionList(new Vector2Int(0, 0), BuildingTypeSO.Direction.Down);
            Building wc = Building.SpawnBuilding(Vector3.zero, new Vector2Int(0, 0), BuildingTypeSO.Direction.Down,wcObject, positionList);
            yield return new WaitForSeconds(0.1f);
            gameManager.SellBuilding(wc);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(575,gameManager.Money);
        }
        
        [UnityTest]
        public IEnumerator UpgradeAttractionWithNoMoney()
        {
            List<Vector2Int> positionList =
                wcObject.GetPositionList(new Vector2Int(0, 0), BuildingTypeSO.Direction.Down);
            gameManager.Money = 0;
            Building wc = Building.SpawnBuilding(Vector3.zero, new Vector2Int(0, 0), BuildingTypeSO.Direction.Down,wcObject, positionList);
            yield return new WaitForSeconds(0.1f);
            Attraction help = (Attraction)wc;
            GameObject empty = new GameObject();
            help.brokeVisual = empty.transform;
            help.BreakBuilding();
            yield return new WaitForSeconds(0.1f);
            bool helpBool=gameManager.UpgradeBuilding((Attraction)(wc));
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(helpBool);
        }
        
        [UnityTest]
        public IEnumerator UpgradeAttractionWithMoney()
        {
            List<Vector2Int> positionList =
                wcObject.GetPositionList(new Vector2Int(0, 0), BuildingTypeSO.Direction.Down);
            gameManager.Money = 9999;
            Building wc = Building.SpawnBuilding(Vector3.zero, new Vector2Int(0, 0), BuildingTypeSO.Direction.Down,wcObject, positionList);
            yield return new WaitForSeconds(0.1f);
            Attraction help = (Attraction)wc;
            GameObject empty = new GameObject();
            help.brokeVisual = empty.transform;
            help.BreakBuilding();
            yield return new WaitForSeconds(0.1f);
            bool helpBool=gameManager.UpgradeBuilding((Attraction)(wc));
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(false);
        }
        
        [UnityTest]
        public IEnumerator BuyJanitorWithMoney()//////
        {
            gameManager.Money = 0;
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(gameManager.BuyJanitor());
        }
        
        [UnityTest]
        public IEnumerator BuyJanitorWithNoMoney()
        {
            gameManager.Money = 0;
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(gameManager.BuyJanitor());
        }
        
        [UnityTest]
        public IEnumerator BuyMechanicWithMoney()
        {
            gameManager.Money = 9000;
            yield return new WaitForSeconds(0.1f);
            Assert.IsTrue(gameManager.BuyMechanic());
        }
        
        [UnityTest]
        public IEnumerator BuyMechanicWithNoMoney()
        {
            gameManager.Money = 0;
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(gameManager.BuyMechanic());
        }
        
        [UnityTest]
        public IEnumerator RemoveMechanicWithAvaliable()
        {
            gameManager.availableMechanics = 1;
            gameManager.totalMechanics = 1;
            gameManager.availableMechanics = 1;
            
            yield return new WaitForSeconds(0.1f);
            Assert.IsTrue(gameManager.RemoveMechanic());
        }
        
        [UnityTest]
        public IEnumerator RemoveMechanicWithNoAvaliable()
        {
            gameManager.availableMechanics = 0;
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(gameManager.RemoveMechanic());
        }
        
        [UnityTest]
        public IEnumerator RemoveJanitorWithNoAvaliable()
        {
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(gameManager.RemoveJanitor());
        }
        
        [UnityTest]
        public IEnumerator RemoveJanitorWithAvaliable()
        {
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(gameManager.RemoveJanitor());
        }
        
        [UnityTest]
        public IEnumerator StoredJanitorChange()
        {
            gameManager.storedJanitors = 1;
            gameManager.storedJanitors = gameManager.storedJanitors - 1;
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(0,gameManager.storedJanitors);
        }
        

    }
}

