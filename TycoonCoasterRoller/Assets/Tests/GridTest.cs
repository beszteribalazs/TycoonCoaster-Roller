using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class GridTest
    {
        GridXZ grid;
        int gridWidth;
        int gridHeight;
        float cellSize;
        Vector3 vector;
        BuildingTypeSO roadSO;
        private GameObject testObject;
        private Building baseRoad;
        

        [SetUp]
        public void Setup()
        {
            gridWidth = 5;
            gridHeight = 5;
            cellSize = 3f;
            vector = Vector3.zero;
            grid = new GridXZ(gridWidth, gridHeight, cellSize, vector);
            roadSO = ScriptableObject.CreateInstance<BuildingTypeSO>();
            roadSO.buildingName = "roadX";
            roadSO.type = BuildingTypeSO.Type.Road;
            testObject = new GameObject();
            baseRoad = testObject.AddComponent<Road>();
            baseRoad.Type = roadSO;
        }
        
        [TearDown]
        public void Teardown(){
            Object.Destroy(testObject);
        }
        
        [UnityTest]
        public IEnumerator RoadNoNeighbours()
        {
            grid.GetCell(2,2).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 2).AdjacentRoads;
            Assert.AreEqual(0,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        
        [UnityTest]
        public IEnumerator RoadUp()
        {
            grid.GetCell(0,0).SetBuilding(baseRoad);
            grid.GetCell(0,1).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(0, 0).AdjacentRoads;
            Assert.AreEqual(1,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        
        [UnityTest]
        public IEnumerator RoadDown()
        {
            grid.GetCell(0,0).SetBuilding(baseRoad);
            grid.GetCell(0,1).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(0, 1).AdjacentRoads;
            Assert.AreEqual(3,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadLeft()
        {
            grid.GetCell(1,0).SetBuilding(baseRoad);
            grid.GetCell(0,0).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(1, 0).AdjacentRoads;
            Assert.AreEqual(4,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadRight()
        {
            grid.GetCell(0,0).SetBuilding(baseRoad);
            grid.GetCell(1,0).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(0, 0).AdjacentRoads;
            Assert.AreEqual(2,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadUpDown()
        {
            grid.GetCell(2,2).SetBuilding(baseRoad);
            grid.GetCell(2,3).SetBuilding(baseRoad);
            grid.GetCell(2,1).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 2).AdjacentRoads;
            Assert.AreEqual(10,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadUpLeft()
        {
            grid.GetCell(2,2).SetBuilding(baseRoad);
            grid.GetCell(2,3).SetBuilding(baseRoad);
            grid.GetCell(1,2).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 2).AdjacentRoads;
            Assert.AreEqual(8,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadUpRight()
        {
            grid.GetCell(2,2).SetBuilding(baseRoad);
            grid.GetCell(2,3).SetBuilding(baseRoad);
            grid.GetCell(3,2).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 2).AdjacentRoads;
            Assert.AreEqual(5,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadDownLeft()
        {
            grid.GetCell(2,2).SetBuilding(baseRoad);
            grid.GetCell(2,1).SetBuilding(baseRoad);
            grid.GetCell(1,2).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 2).AdjacentRoads;
            Assert.AreEqual(7,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadDownRight()
        {
            grid.GetCell(2,2).SetBuilding(baseRoad);
            grid.GetCell(2,1).SetBuilding(baseRoad);
            grid.GetCell(3,2).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 2).AdjacentRoads;
            Assert.AreEqual(6,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadLeftRight()
        {
            grid.GetCell(2,2).SetBuilding(baseRoad);
            grid.GetCell(1,2).SetBuilding(baseRoad);
            grid.GetCell(3,2).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 2).AdjacentRoads;
            Assert.AreEqual(9,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadUpDownLeft()
        {
            grid.GetCell(2,2).SetBuilding(baseRoad);
            grid.GetCell(2,3).SetBuilding(baseRoad);
            grid.GetCell(2,1).SetBuilding(baseRoad);
            grid.GetCell(1,2).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 2).AdjacentRoads;
            Assert.AreEqual(14,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadUpDownRight()
        {
            grid.GetCell(2,2).SetBuilding(baseRoad);
            grid.GetCell(2,3).SetBuilding(baseRoad);
            grid.GetCell(2,1).SetBuilding(baseRoad);
            grid.GetCell(3,2).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 2).AdjacentRoads;
            Assert.AreEqual(12,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadUpLeftRight()
        {
            grid.GetCell(2,2).SetBuilding(baseRoad);
            grid.GetCell(2,3).SetBuilding(baseRoad);
            grid.GetCell(1,2).SetBuilding(baseRoad);
            grid.GetCell(3,2).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 2).AdjacentRoads;
            Assert.AreEqual(11,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadDownLeftRight()
        {
            grid.GetCell(2,2).SetBuilding(baseRoad);
            grid.GetCell(2,1).SetBuilding(baseRoad);
            grid.GetCell(1,2).SetBuilding(baseRoad);
            grid.GetCell(3,2).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 2).AdjacentRoads;
            Assert.AreEqual(13,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadUpDownLeftRight()
        {
            grid.GetCell(2,2).SetBuilding(baseRoad);
            grid.GetCell(2,3).SetBuilding(baseRoad);
            grid.GetCell(2,1).SetBuilding(baseRoad);
            grid.GetCell(1,2).SetBuilding(baseRoad);
            grid.GetCell(3,2).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 2).AdjacentRoads;
            Assert.AreEqual(15,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadEntranceDown()
        {
            grid.GetCell(2,0).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 0).AdjacentRoads;
            Assert.AreEqual(16,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadEntranceUpDownLeftRight()
        {
            grid.GetCell(2,0).SetBuilding(baseRoad);
            grid.GetCell(2,1).SetBuilding(baseRoad);
            grid.GetCell(1,0).SetBuilding(baseRoad);
            grid.GetCell(3,0).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 0).AdjacentRoads;
            Assert.AreEqual(17,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadEntranceDownRight()
        {
            grid.GetCell(2,0).SetBuilding(baseRoad);
            grid.GetCell(3,0).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 0).AdjacentRoads;
            Assert.AreEqual(18,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadEntranceDownLeft()
        {
            grid.GetCell(2,0).SetBuilding(baseRoad);
            grid.GetCell(1,0).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 0).AdjacentRoads;
            Assert.AreEqual(19,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadEntranceUpDownRight()
        {
            grid.GetCell(2,0).SetBuilding(baseRoad);
            grid.GetCell(2,1).SetBuilding(baseRoad);
            grid.GetCell(3,0).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 0).AdjacentRoads;
            Assert.AreEqual(20,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadEntranceUpDownLeft()
        {
            grid.GetCell(2,0).SetBuilding(baseRoad);
            grid.GetCell(2,1).SetBuilding(baseRoad);
            grid.GetCell(1,0).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 0).AdjacentRoads;
            Assert.AreEqual(21,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadEntranceLeftRight()
        {
            grid.GetCell(2,0).SetBuilding(baseRoad);
            grid.GetCell(1,0).SetBuilding(baseRoad);
            grid.GetCell(3,0).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 0).AdjacentRoads;
            Assert.AreEqual(22,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
        
        [UnityTest]
        public IEnumerator RoadEntranceUpDown()
        {
            grid.GetCell(2,0).SetBuilding(baseRoad);
            grid.GetCell(2,1).SetBuilding(baseRoad);
            int adjacentRoadsCase = grid.GetCell(2, 0).AdjacentRoads;
            Assert.AreEqual(23,adjacentRoadsCase);
            yield return new WaitForSeconds(0.1f);
        }
    }
}