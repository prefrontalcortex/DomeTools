using System.Collections.Generic;
using UnityEngine.XR.Hands.Gestures;

namespace UnityEngine.XR.Hands.Samples.Gestures.DebugTools
{
    /// <summary>
    /// Controls the debug UI for <see cref="XRHandShape"/> that shows the target and tolerances on the UI controlled
    /// by a <see cref="XRAllFingerStatesDebugUI"/>.
    /// </summary>
    public class XRHandShapeDebugUI : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The debug UI that will be used to display the finger states.")]
        XRAllFingerShapesDebugUI m_XRAllFingerShapesDebugUI;

        XRHandShape m_HandShape;

        bool m_HandShapeDetected;

        readonly List<XRFingerShapeDebugBar> k_ReusableBarsToHide = new List<XRFingerShapeDebugBar>();

        readonly List<XRFingerShapeDebugBar> k_Bars = new List<XRFingerShapeDebugBar>();

        /// <summary>
        /// The hand shape that will be displayed in the debug UI.
        /// </summary>
        public ScriptableObject handShapeOrPose
        {
            get => m_HandShape;
            set
            {
                var handPose = value as XRHandPose;

                m_HandShape = handPose != null ? handPose.handShape : value as XRHandShape;

                m_HandShapeDetected = m_HandShape != null;
                foreach (var bar in k_Bars)
                    bar.handShapeDetected = m_HandShapeDetected;

                if (m_HandShapeDetected)
                {
                    // Hide previously enabled bars
                    foreach (var bar in k_Bars)
                        bar.HideTargetAndTolerance();
                }
            }
        }

        void Awake()
        {
            if (k_Bars.Count == 0)
            {
                foreach (var graph in m_XRAllFingerShapesDebugUI.xrFingerShapeDebugGraphs)
                {
                    foreach (var bar in graph.bars)
                        k_Bars.Add(bar);
                }
            }
        }

        void Update()
        {
            // Track all the bars that have no target and tolerance so they can be hidden
            k_ReusableBarsToHide.Clear();
            foreach (var graph in m_XRAllFingerShapesDebugUI.xrFingerShapeDebugGraphs)
                k_ReusableBarsToHide.AddRange(graph.bars);

            if (m_HandShapeDetected)
            {
                foreach (var condition in m_HandShape.fingerShapeConditions)
                {
                    foreach (var shapeCondition in condition.targets)
                    {
                        var xrFingerShapeDebugGraph = m_XRAllFingerShapesDebugUI.xrFingerShapeDebugGraphs[(int)condition.fingerID];
                        var bar = xrFingerShapeDebugGraph.bars[(int)shapeCondition.shapeType];
                        bar.SetTargetAndTolerances(shapeCondition.desired, shapeCondition.upperTolerance, shapeCondition.lowerTolerance);
                        k_ReusableBarsToHide.Remove(bar);
                    }
                }
            }
        }

        /// <summary>
        /// Clear the detected handshape reference inorder to stop displaying any corresponding UI
        /// </summary>
        public void ClearDetectedHandShape()
        {
            handShapeOrPose = null;
        }
    }
}
