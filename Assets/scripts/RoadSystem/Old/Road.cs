using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Old{
    public class Road
    {
        public Road(Curve _curve, List<string> _lane, bool _noEntity = false)
        {
            curve = _curve;
            laneconfigure = new List<string>();
            if (_lane != null)
            {
                foreach (string l in _lane)
                {
                    laneconfigure.Add(l);
                }
            }
            forwardVehicleController = new VehicleController(directionalLaneCount(true));
            backwardVehicleController = new VehicleController(directionalLaneCount(false));

            _margin0LLength = _margin0RLength = _margin1LLength = _margin1RLength = 0f;
            calculateParamMargins();
            noEntity = _noEntity;
        }

        private Road() { }
        public Curve curve;
        public List<string> laneconfigure { get; private set; }

        public GameObject roadObject;

        internal bool virtualRoad
        {
            get
            {
                return laneconfigure.Count == 0;
            }
        }

        public bool noEntity;

        public float width
        {
            get
            {
                return RoadRenderer.getConfigureWidth(laneconfigure);
            }
        }

        float[] forwardLaneCenterOffsetBackend;
        public float getLaneCenterOffset(int laneNum, bool direction){
            if (laneNum < 0 || laneNum >= directionalLaneCount(direction))
            {
                Debug.Assert(false);
                return 0;
            }

            if (forwardLaneCenterOffsetBackend == null)
            {
                updateLaneCenterOffsetBackend();
            }

            if (!direction){
                return forwardLaneCenterOffsetBackend[laneNum];
            }
            else{
                return forwardLaneCenterOffsetBackend[totalLaneCount - laneNum - 1];
            }
        }

        void updateLaneCenterOffsetBackend(){
            forwardLaneCenterOffsetBackend = new float[totalLaneCount];

            for (int foundLanes = 0, j = 0; foundLanes != totalLaneCount; ++j){
                if (laneconfigure[j] == "lane"){
                    forwardLaneCenterOffsetBackend[foundLanes] = RoadRenderer.getConfigureWidth(laneconfigure.GetRange(0, j)) +
                                    0.5f * RoadRenderer.getConfigureWidth(laneconfigure.GetRange(j, 1)) -
                                    0.5f * width;
                    foundLanes++;
                }
            }
        }

        int _forwardLaneCount = -1, _backwardLaneCount = -1;

        public int directionalLaneCount(bool direction){
            if (_forwardLaneCount == -1)
            {
                int mainSeparatorIndex = laneconfigure.FindIndex((obj) => obj.EndsWith("yellow"));
                if (mainSeparatorIndex == -1)
                {
                    _forwardLaneCount = totalLaneCount;
                    _backwardLaneCount = 0;
                }
                else
                {
                    _forwardLaneCount = laneconfigure.GetRange(mainSeparatorIndex + 1, laneconfigure.Count - mainSeparatorIndex - 1).Count(config => config == "lane");
                    _backwardLaneCount = laneconfigure.GetRange(0, mainSeparatorIndex).Count(config => config == "lane");

                }
            }
            return direction ? _forwardLaneCount : _backwardLaneCount;
        }

        int _totalLaneCount = -1;
        int totalLaneCount{
            get{
                if (_totalLaneCount == -1){
                    _totalLaneCount = laneconfigure.Count(config => config == "lane");
                }
                return _totalLaneCount;
            }
        }

        public float SPWeight{
            get
            {
                return curve.length;
            }
        }

        /*actual render info for vehicle*/

        float _margin0LLength, _margin0RLength, _margin1LLength, _margin1RLength;

        public float margin0LLength{
            get{
                return _margin0LLength;
            }
            set{
                _margin0LLength = value;
                calculateParamMargins();
            }
        }

        public float margin0RLength
        {
            get
            {
                return _margin0RLength;
            }
            set
            {
                _margin0RLength = value;
                calculateParamMargins();
            }
        }

        public float margin1LLength
        {
            get
            {
                return _margin1LLength;
            }
            set
            {
                _margin1LLength = value;
                calculateParamMargins();
            }
        }

        public float margin1RLength
        {
            get
            {
                return _margin1RLength;
            }
            set
            {
                _margin1RLength = value;
                calculateParamMargins();
            }
        }

        public float margin0Param { get; private set; }

        public float margin1Param { get; private set; }

        Curve[] renderingFragements;

        void calculateParamMargins(){
            float indicatorMargin0Bound = Mathf.Max(_margin0LLength, _margin0RLength);
            float indicatorMargin1Bound = Mathf.Max(_margin1LLength, _margin1RLength);
            renderingFragements = RoadRenderer.splitByMargin(curve, indicatorMargin0Bound, indicatorMargin1Bound);
            if (renderingFragements[0] != null){
                margin0Param = curve.paramOf(renderingFragements[0].At(1f)) ?? 0f;
            }
            else{
                margin0Param = 0;
            }
            if (renderingFragements[2] != null){
                margin1Param = curve.paramOf(renderingFragements[2].At(0f)) ?? 0f;
            }
            else{
                margin1Param = 1;
            }

            if (!virtualRoad){
                for (int i = 0; i != 3; ++i){
                    if (renderingFragements[i] != null)
                    {
                        renderingFragements[i].InitAllBuffers();
                    }
                    curve.InitAllBuffers();
                }
            }
        }

        delegate Vector3 curveValueFinder(int id, float p, bool usebuff);

        Vector3 renderingCurveSolver(float param, curveValueFinder finder, bool usebuff){
            Debug.Assert(renderingFragements != null);
            if (param < margin0Param && renderingFragements[0] != null)
            {
                return finder(0, param / margin0Param, usebuff);
            }
            else
            {
                if (param > margin1Param && renderingFragements[2] != null)
                {
                    return finder(2, (param - margin1Param) / (1f - margin1Param), usebuff);
                }
                else
                {
                    return finder(1, (param - margin0Param) / (margin1Param - margin0Param), usebuff);
                }
            }
        }

        public Curve marginedOutCurve{
            get{
                return renderingFragements[1];
            }
        }

        public Vector3 at(float param, bool usebuff = false){
            return renderingCurveSolver(param, at_finder, usebuff);
        }

        public Vector3 frontNormal(float param, bool usebuff = false){
            return renderingCurveSolver(param, frontNormal_finder, usebuff);
        }

        public Vector3 upNormal(float param, bool usebuff = false){
            return renderingCurveSolver(param, upNormal_finder, usebuff);
        }

        public Vector3 rightNormal(float param, bool usebuff = false){
            return renderingCurveSolver(param, rightNormal_finder, usebuff);
        }

        Vector3 at_finder(int id, float p, bool usebuff){
            return renderingFragements[id].At(p, usebuff);
        }

        Vector3 frontNormal_finder(int id, float p, bool usebuff){
            return renderingFragements[id].FrontNormal(p, usebuff);
        }

        Vector3 upNormal_finder(int id, float p, bool usebuff){
            return renderingFragements[id].UpNormal(p, usebuff);
        }

        Vector3 rightNormal_finder(int id, float p, bool usebuff){
            return renderingFragements[id].RightNormal(p, usebuff);
        }

        public VehicleController forwardVehicleController, backwardVehicleController;

        public override string ToString()
        {
            return curve.ToString();
        }

        public float length{
            get{
                return curve.length;
            }
        }

    }

}