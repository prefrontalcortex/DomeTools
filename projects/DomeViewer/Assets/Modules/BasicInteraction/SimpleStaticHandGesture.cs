using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;

namespace pfc.Fulldome
{
    public class SimpleStaticHandGesture : MonoBehaviour
    {
        [SerializeField]
            [Tooltip("The hand tracking events component to subscribe to receive updated joint data to be used for gesture detection.")]
            XRHandTrackingEvents m_HandTrackingEvents;

            [SerializeField]
            [Tooltip("The hand shape or pose that must be detected for the gesture to be performed.")]
            ScriptableObject m_HandShapeOrPose;

            [SerializeField]
            [Tooltip("The target Transform to user for target conditions in the hand shape or pose.")]
            Transform m_TargetTransform;

            [SerializeField]
            [Tooltip("The event fired when the gesture is performed.")]
            UnityEvent m_GesturePerformed;

            [SerializeField]
            [Tooltip("The event fired when the gesture is ended.")]
            UnityEvent m_GestureEnded;

            [SerializeField]
            [Tooltip("The minimum amount of time the hand must be held in the required shape and orientation for the gesture to be performed.")]
            float m_MinimumHoldTime = 0.2f;

            [SerializeField]
            [Tooltip("The interval at which the gesture detection is performed.")]
            float m_GestureDetectionInterval = 0.1f;

            [SerializeField]
            [Tooltip("The static gestures associated with this gestures handedness.")]
            SimpleStaticHandGesture[] m_SimpleStaticHandGesture;

            XRHandShape m_HandShape;
            XRHandPose m_HandPose;
            bool m_WasDetected;
            bool m_PerformedTriggered;
            float m_TimeOfLastConditionCheck;
            float m_HoldStartTime;
            Color m_BackgroundDefaultColor;
            Color m_BackgroundHiglightColor = new Color(0f, 0.627451f, 1f);

            /// <summary>
            /// The hand tracking events component to subscribe to receive updated joint data to be used for gesture detection.
            /// </summary>
            public XRHandTrackingEvents handTrackingEvents
            {
                get => m_HandTrackingEvents;
                set => m_HandTrackingEvents = value;
            }

            /// <summary>
            /// The hand shape or pose that must be detected for the gesture to be performed.
            /// </summary>
            public ScriptableObject handShapeOrPose
            {
                get => m_HandShapeOrPose;
                set => m_HandShapeOrPose = value;
            }

            /// <summary>
            /// The target Transform to user for target conditions in the hand shape or pose.
            /// </summary>
            public Transform targetTransform
            {
                get => m_TargetTransform;
                set => m_TargetTransform = value;
            }

            /// <summary>
            /// The event fired when the gesture is performed.
            /// </summary>
            public UnityEvent gesturePerformed
            {
                get => m_GesturePerformed;
                set => m_GesturePerformed = value;
            }

            /// <summary>
            /// The event fired when the gesture is ended.
            /// </summary>
            public UnityEvent gestureEnded
            {
                get => m_GestureEnded;
                set => m_GestureEnded = value;
            }

            /// <summary>
            /// The minimum amount of time the hand must be held in the required shape and orientation for the gesture to be performed.
            /// </summary>
            public float minimumHoldTime
            {
                get => m_MinimumHoldTime;
                set => m_MinimumHoldTime = value;
            }

            /// <summary>
            /// The interval at which the gesture detection is performed.
            /// </summary>
            public float gestureDetectionInterval
            {
                get => m_GestureDetectionInterval;
                set => m_GestureDetectionInterval = value;
            }

            void OnEnable()
            {
                m_HandTrackingEvents.jointsUpdated.AddListener(OnJointsUpdated);

                m_HandShape = m_HandShapeOrPose as XRHandShape;
                m_HandPose = m_HandShapeOrPose as XRHandPose;
                if (m_HandPose != null && m_HandPose.relativeOrientation != null)
                    m_HandPose.relativeOrientation.targetTransform = m_TargetTransform;
            }

            void OnDisable() => m_HandTrackingEvents.jointsUpdated.RemoveListener(OnJointsUpdated);

            void OnJointsUpdated(XRHandJointsUpdatedEventArgs eventArgs)
            {
                if (!isActiveAndEnabled || Time.timeSinceLevelLoad < m_TimeOfLastConditionCheck + m_GestureDetectionInterval)
                    return;

                var detected =
                    m_HandTrackingEvents.handIsTracked &&
                    m_HandShape != null && m_HandShape.CheckConditions(eventArgs) ||
                    m_HandPose != null && m_HandPose.CheckConditions(eventArgs);

                if (!m_WasDetected && detected)
                {
                    m_HoldStartTime = Time.timeSinceLevelLoad;
                }
                else if (m_WasDetected && !detected)
                {
                    m_PerformedTriggered = false;
                    m_GestureEnded?.Invoke();
                }

                m_WasDetected = detected;

                if (!m_PerformedTriggered && detected)
                {
                    var holdTimer = Time.timeSinceLevelLoad - m_HoldStartTime;
                    if (holdTimer > m_MinimumHoldTime)
                    {
                        m_GesturePerformed?.Invoke();
                        m_PerformedTriggered = true;
                    }
                }

                m_TimeOfLastConditionCheck = Time.timeSinceLevelLoad;
            }
        }
}
