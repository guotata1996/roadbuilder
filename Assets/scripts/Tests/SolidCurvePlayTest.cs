using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SolidCurvePlayTest
    {
        Dictionary<Vector2, Color> POI;


        Vector2[] roadPoints;
        Curve c, b, l, l2;
        Function f;
        GameObject lightGameObject;

        [SetUp]
        public void CreateTests()
        {
            roadPoints = new Vector2[]{
                new Vector2(0f, 0f),
                new Vector2(0f, 20f),
                new Vector2(-30f, 0f),
                new Vector2(-20f, 20f)
            };
            POI = new Dictionary<Vector2, Color>();

            c = Arc.TryInit(roadPoints[1], roadPoints[0], -Mathf.PI / 2);
            b = Bezeir.TryInit(roadPoints[2], roadPoints[0], roadPoints[1]);
            l = Line.TryInit(roadPoints[0], roadPoints[3]);
            f = new LinearFunction(0.0f, 4.0f);

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
        public void RenderTest()
        {
            Curve3DSampler curve3d = new Curve3DSampler(b, f, 0.5f);
            var sprite = Resources.Load<Sprite>("Tests/1");
            SpriteVerticeSampler sb = new SpriteVerticeSampler(sprite, 1.0f, 0.1f);

            GameObject obj = SolidCurve.Generate(curve3d, sb);
            GameObject.Instantiate(obj);
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
        public IEnumerator Z_PlayTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForSeconds(10);
        }
    }
}
