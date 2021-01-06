using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class MovementTests
    {

        [SetUp]
        public void TestSetup()
        {
            SceneManager.LoadScene("PredatorsScene");
        }

        [UnityTest]
        public IEnumerator ObjectsSpawnedSuccessfullyTest()
        {
            yield return new WaitForSeconds(0.25f);

            var prey = GameObject.FindGameObjectWithTag("Prey");
            var predator = GameObject.FindGameObjectWithTag("Predator");
            var warden = GameObject.FindGameObjectWithTag("Warden");

            Assert.NotNull(prey);
            Assert.NotNull(predator);
            Assert.NotNull(warden);
        }

        [UnityTest]
        public IEnumerator MultipleInstancesSpawnedSuccessfullyTest()
        {
            yield return new WaitForSeconds(0.25f);

            var prey = GameObject.FindGameObjectsWithTag("Prey");
            var predator = GameObject.FindGameObjectsWithTag("Predator");
            var warden = GameObject.FindGameObjectsWithTag("Warden");

            Assert.GreaterOrEqual(prey.Length, 6);
            Assert.Less(prey.Length, 12);
            Assert.GreaterOrEqual(predator.Length, 2);
            Assert.Less(predator.Length, 5);
            Assert.AreEqual(warden.Length, 1);
        }

        [UnityTest]
        public IEnumerator PredatorPositionChangedTest()
        {
            yield return new WaitForSeconds(0.25f);
            
            var predator = GameObject.FindGameObjectWithTag("Predator");
            var positionBefore = predator.gameObject.transform.position;
            
            yield return new WaitForSeconds(1.5f);

            var positionAfter = predator.gameObject.transform.position;
            
            Assert.AreNotEqual(positionBefore, positionAfter);
        }

        [UnityTest]
        public IEnumerator ObjectDestroyedTest()
        {
            yield return new WaitForSeconds(1.25f);

            var prey = GameObject.FindGameObjectWithTag("Prey");
            GameObject.Destroy(prey);

            yield return new WaitForSeconds(0.25f);

            Assert.IsTrue(prey == null || prey.Equals(null));
        }
    }
}
