using System;
using UnityEngine.XR.Hands.Gestures;

namespace UnityEngine.XR.Hands.Samples.GestureSample
{
    public class HandShapeCompletenessCalculator : MonoBehaviour
    {
        bool TryGetFingerShapeValue(XRFingerShapeType fingerShapeType, XRFingerShape fingerShape, out float value)
        {
            value = 0f;
            switch (fingerShapeType)
            {
                case XRFingerShapeType.FullCurl:
                    return fingerShape.TryGetFullCurl(out value);

                case XRFingerShapeType.BaseCurl:
                    return fingerShape.TryGetBaseCurl(out value);

                case XRFingerShapeType.TipCurl:
                    return fingerShape.TryGetTipCurl(out value);

                case XRFingerShapeType.Pinch:
                    return fingerShape.TryGetPinch(out value);

                case XRFingerShapeType.Spread:
                    return fingerShape.TryGetSpread(out value);

                default:
                    Debug.Log($"Finger shape type {fingerShapeType} is invalid for finger shape target condition.");
                    return false;
            }
        }

        bool IsWithinToleranceRange(float value, XRFingerShapeCondition.Target target)
        {
            return (target.desired - target.lowerTolerance) <= value &&
                value <= (target.desired + target.upperTolerance);
        }

        /// <summary>
        /// Calculates a smoothed completeness score for a value relative to the target,
        /// using a linear interpolation approach outside the tolerance range to ensure smooth transitions.
        /// </summary>
        /// <param name="value">The value to evaluate against the target range</param>
        /// <param name="target">The target condition, including the desired value and its upper and lower tolerances.</param>
        /// <returns>
        /// A score between 0 and 1:
        /// - Returns 1.0 if the value is within the tolerance range.
        /// - Returns an interpolated score between 0 and 1 if the value is outside the tolerance range
        /// but inside the smoothing range.
        /// - Returns 0.0 if the value is outside the tolerance range and smoothing range.
        /// </returns>
        /// <remarks>
        /// This method ensures a gradual change in the completeness score near the tolerance range boundaries,
        /// avoiding abrupt jumps by applying linear interpolation within a smoothing range.
        /// </remarks>
        float CalculateScoreWithExternalBuffer(float value, in XRFingerShapeCondition.Target target)
        {
            // Size of the smoothing range (buffer) outside the tolerance range
            const float k_BufferSize = 0.1f;

            if (IsWithinToleranceRange(value, target))
                return 1.0f;

            // Calculate smoothing ranges (buffer) for lower bound
            float lowerBoundBufferStart = Mathf.Clamp01(target.desired - target.lowerTolerance - k_BufferSize);
            float lowerBoundBufferEnd = Mathf.Clamp01(target.desired - target.lowerTolerance);

            // Calculate smoothing ranges (buffer) for upper bound
            float upperBoundBufferStart = Mathf.Clamp01(target.desired + target.upperTolerance);
            float upperBoundBufferEnd = Mathf.Clamp01(target.desired + target.upperTolerance + k_BufferSize);

            var interpolatedValue = 0f;

            // Linearly interpolate for values within the lower smoothing range
            if (lowerBoundBufferStart < value && value < lowerBoundBufferEnd)
                interpolatedValue = (value - lowerBoundBufferStart) / k_BufferSize;

            // Linearly interpolate for values within the upper smoothing range
            else if (upperBoundBufferStart < value && value < upperBoundBufferEnd)
                interpolatedValue = (upperBoundBufferEnd - value) / k_BufferSize;

            return interpolatedValue;
        }

        /// <summary>
        /// Calculates a smoothed completeness score for a value relative to the target,
        /// considering only the tolerance range (without additional smoothing outside the range).
        /// </summary>
        /// <param name="value">The value to evaluate against the target range.</param>
        /// <param name="target">The target condition, including the desired value and its upper and lower tolerances.</param>
        /// <returns>
        /// A score between 0 and 1:
        /// - Returns 1.0 if the value is exactly equal to the target's desired value.
        /// - Returns a linearly interpolated score between 0 and 1 if the value lies within the tolerance range.
        /// - Returns 0.0 if the value is outside the tolerance range.
        /// </returns>
        /// <remarks>
        /// This method calculates the score using linear interpolation within the tolerance range,
        /// ensuring gradual changes in the score based on the distance from the lower or upper tolerance bounds.
        /// </remarks>
        float CalculateScoreWithInternalBuffer(float value, in XRFingerShapeCondition.Target target)
        {
            // Define the bounds for the lower tolerance range
            var lowerBoundBufferStart = Mathf.Clamp01(target.desired - target.lowerTolerance);
            var lowerBoundBufferEnd = target.desired;

            // Define the bounds for the upper tolerance range
            var upperBoundBufferStart = target.desired;
            var upperBoundBufferEnd = Mathf.Clamp01(target.desired + target.upperTolerance);

            var interpolatedValue = 0f;

            // Linearly interpolate for values within the lower tolerance range
            if (lowerBoundBufferStart < value && value < lowerBoundBufferEnd)
                interpolatedValue = (value - lowerBoundBufferStart) / target.lowerTolerance;

            // Linearly interpolate for values within the upper tolerance range
            else if (upperBoundBufferStart < value && value < upperBoundBufferEnd)
                interpolatedValue = (upperBoundBufferEnd - value) / target.upperTolerance;

            return interpolatedValue;
        }

        /// <summary>
        /// Calculates the completeness score of a finger shape based on the specified condition.
        /// </summary>
        /// <param name="fingerShape">The actual finger shape to evaluate against the target.</param>
        /// <param name="fingerShapeCondition">The condition that defines the target shapes and tolerances for the finger.</param>
        /// <param name="completenessScore">A completeness score between <c>0</c> to <c>1</c>, representing how closely the
        /// actual finger shape matches the desired target shapes defined in the condition.
        /// The score is the average of the interpolated scores calculated for each target shape in the condition.
        /// <returns>True if a valid completeness score can be calculated, otherwise false.</returns>
        bool TryCalculateFingerCompletenessScore(in XRFingerShape fingerShape, in XRFingerShapeCondition fingerShapeCondition,
            out float completenessScore)
        {
            completenessScore = 0f;

            try
            {
                if (fingerShapeCondition.targets == null || fingerShapeCondition.targets.Length == 0)
                    return false;

                float interpolatedCompletenessScoreSum = 0f;
                for (var targetIndex = 0; targetIndex < fingerShapeCondition.targets.Length; ++targetIndex)
                {
                    var target = fingerShapeCondition.targets[targetIndex];
                    bool hasValue = TryGetFingerShapeValue(target.shapeType, fingerShape, out var value);

                    // You can implement custom logic for calculating the completeness score.
                    // This file provides two examples: CalculateScoreWithExternalBuffer and CalculateScoreWithInternalBuffer.
                    interpolatedCompletenessScoreSum += CalculateScoreWithExternalBuffer(value, target);
                }

                completenessScore = interpolatedCompletenessScoreSum / (float)fingerShapeCondition.targets.Length;

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred while calculating finger completeness score: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Calculates the completeness score for a hand shape.
        /// </summary>
        /// <param name="hand">The hand performing the hand shape.</param>
        /// <param name="handShape">The target hand shape.</param>
        /// <param name="completenessScore">The hand shape completeness score, which indicates how closely the current hand matches the target shape.
        /// Value is normalized from <c>0</c> to <c>1</c>, where <c>1.0</c> indicates a match and <c>0.0</c> indicates no alignment.</param>
        /// <returns>True if a valid score can be successfully calculated, otherwise false.</returns>
        public bool TryCalculateHandShapeCompletenessScore(in XRHand hand, in XRHandShape handShape, out float completenessScore)
        {
            completenessScore = 0f;

            try
            {
                if (hand == null || handShape == null)
                    return false;

                var fingerShapeConditions = handShape.fingerShapeConditions;

                if (fingerShapeConditions == null || fingerShapeConditions.Count == 0)
                    return false;

                var fingerScoreSum = 0f;
                for (var index = 0; index < fingerShapeConditions.Count; ++index)
                {
                    var fingerShapeCondition = fingerShapeConditions[index];
                    var fingerShape = hand.CalculateFingerShape(fingerShapeCondition.fingerID, XRFingerShapeTypes.All);

                    TryCalculateFingerCompletenessScore(fingerShape, fingerShapeCondition, out var fingerCompletenessScore);

                    fingerScoreSum += fingerCompletenessScore;
                }

                completenessScore = fingerScoreSum / (float)fingerShapeConditions.Count;

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred while calculating hand shape completeness score: {ex.Message}");
                return false;
            }
        }
    }
}
