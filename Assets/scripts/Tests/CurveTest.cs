namespace Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;

    public class CurveTest : MonoBehaviour
    {
        Vector3[] roadPoints;

        [SetUp]
        public void InitTest(){
            roadPoints = new Vector3[]{
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 0f, 20f),
                new Vector3(-30f, 8f, 0f)
            };
        }


        [Test]
        public void NewTestScriptSimplePasses()
        {
            Curve c = Arc.TryInit(roadPoints[0], roadPoints[1], Mathf.PI/2);
            Curve cr = c.reversed();

            Assert.True(Algebra.isclose(c.at(0f), cr.at(1f)));

            float a1 = c.angle_2d(1f);
            float a2 = cr.angle_2d(0f);
            //float angleDiff = (a2 - a1) / Mathf.PI;
            Debug.Log(a1);
            Debug.Log(a2);
            Assert.True(Algebra.isclose(a2, Mathf.PI/2));
        }


    }
}