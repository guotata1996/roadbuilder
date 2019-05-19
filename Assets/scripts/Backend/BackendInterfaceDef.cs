using UnityEngine;

interface ILinearTravelable{
    float MoveAlongReal(float start_t, float real_length);
}

interface ITwodPosAvailable{
    Vector2 GetTwodPos(float t);
}

interface IHeightAvailable{
    float GetHeight(Vector2 twodPos);
}