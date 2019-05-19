using System;
using UnityEngine;

namespace Old{
    public class Linear2DObject
    {
        public Material material;
        public bool dashed;
        public float offset = 0f;
        public float margin_0 = 0f, margin_1 = 0f;
        public float width = RoadRenderer.separatorWidth;

        static Material yellowSepMaterial, whiteSepMaterial, blueIndicatorMaterial, removalMaterial;
        static Linear2DObject(){
            yellowSepMaterial = Resources.Load<Material>("Materials/yellow");
            whiteSepMaterial = Resources.Load<Material>("Materials/white");
            blueIndicatorMaterial = Resources.Load<Material>("Materials/blueindi");
            removalMaterial = Resources.Load<Material>("Materials/orange");
        }

        public Linear2DObject(string color){
            switch(color){
                case "yellow":
                    material = yellowSepMaterial;
                    break;
                case "white":
                    material = whiteSepMaterial;
                    break;
                case "blueindi":
                    material = blueIndicatorMaterial;
                    break;
                case "removal":
                    material = removalMaterial;
                    break;
            }  
        }
    }
}
