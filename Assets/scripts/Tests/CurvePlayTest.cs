using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class CurvePlayTest
    {
        // A Test behaves as an ordinary method
        Dictionary<Vector2, Color> POI;

        Vector2[] roadPoints;
        Curve c, b, l;
        GameObject lightGameObject;

        [SetUp]
        public void CreateTests()
        {
            roadPoints = new Vector2[]{
                new Vector2(0f, 0f),
                new Vector2(0f, 20f),
                new Vector2(-30f, 0f)
            };
            POI = new Dictionary<Vector2, Color>();

            c = Arc.TryInit(roadPoints[1], roadPoints[0], -Mathf.PI / 2);
            b = Bezeir.TryInit(roadPoints[2], roadPoints[0], roadPoints[1]);
            l = Line.TryInit(roadPoints[2], roadPoints[1]);


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
        public void ParamOfTest()
        {
            Debug.Log(b.ParamOf(b.GetTwodPos(1.0f)));
        }

        [Test]
        public void SegmentationTest()
        {
            Debug.Log("Bezeir Test...");
            for (float i = 0; i < 0.9f; i += 0.1f)
            {
                Curve bi = b.DeepCopy();
                bi.Crop(i, i + 0.1f);
                Debug.Log(bi + "\n" + bi.GetTwodPos(i));
                POI.Add(bi.GetTwodPos(0.5f), Color.white);
            }
            Debug.Log("Arc Test...");
            for (float i = 0; i < 0.9f; i += 0.1f)
            {
                Curve ci = c.DeepCopy();
                ci.Crop(i, i + 0.1f);
                Debug.Log(ci + "\n" + ci.GetTwodPos(i));
                POI.Add(ci.GetTwodPos(0.5f), Color.white);
            }
        }

        [Test]
        public void IntersectionTest()
        {
            Debug.Log("c & b...");
            var inter = c._IntersectWith(b);
            foreach (var i in inter)
            {
                POI.Add(i, Color.yellow);
            }

            Debug.Log("l & b...");
            inter = l._IntersectWith(b);
            foreach (var i in inter)
            {
                POI.Add(i, Color.yellow);
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
        public IEnumerator Z_CurvePlayTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForSeconds(10);
        }
    }
}
