using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SectionPlayTest
    {
        Dictionary<Vector2, Color> POI;
        readonly Color semi = new Color(0f, 0f, 0f, 0.5f);
        GameObject lightGameObject;

        [SetUp]
        public void CreateTests()
        {
            POI = new Dictionary<Vector2, Color>();
            if (lightGameObject == null)
            {
                lightGameObject = new GameObject("The Light");

                Light lightComp = lightGameObject.AddComponent<Light>();
                lightComp.color = Color.white;
                lightComp.type = LightType.Directional;
                lightGameObject.transform.position = new Vector3(0, 50, 0);
                lightGameObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }


        [Test]
        public void BoundaryEnumeratorTest()
        {
            var sp = Resources.Load<Sprite>("Tests/1");
            SpriteBoundary sb = new SpriteBoundary(sp);

            int cnt = 0;
            foreach(Vector2Int bound in sb)
            {
                if (cnt == 0)
                {
                    POI[bound] = Color.yellow;
                }
                if (cnt == 1)
                {
                    POI[bound] = Color.green;
                }
                if (cnt > 1)
                {
                    POI[bound] = semi;
                }

                cnt++;
            }
        }


        [TearDown]
        public void EndTest()
        {
            foreach(var loc in POI.Keys)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                c.transform.position = Algebra.toVector3(loc);
                c.transform.localScale = Vector3.one * 0.5f;
                c.GetComponent<MeshRenderer>().material.color = POI[loc];
            }

        }

        [UnityTest]
        public IEnumerator Z_WaitForSeconds()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForSeconds(10);
        }
    }
}
