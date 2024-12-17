using System;
using UnityEngine;


namespace Whilefun.FPEKit
{

    [Serializable]
    public class FPECurveControlledBob
    {

        [Header("Standing")]
        [SerializeField]
        private float HorizontalBobRangeStanding = 0.33f;
        [SerializeField]
        private float VerticalBobRangeStanding = 0.33f;
        [SerializeField]
        private float VerticalToHorizontalRatioStanding = 1.0f;

        [Header("Crouching")]
        [SerializeField]
        private float HorizontalBobRangeCrouching = 0.33f;
        [SerializeField]
        private float VerticalBobRangeCrouching = 0.33f;
        [SerializeField]
        private float VerticalToHorizontalRatioCrouching = 1.0f;

        [Header("General")]
        // sin curve for head bob
        [SerializeField]
        private AnimationCurve Bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                            new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                            new Keyframe(2f, 0f));

        private float m_CyclePositionX;
        private float m_CyclePositionY;
        private float m_BobBaseInterval;
        //private Vector3 m_OriginalCameraPosition;
        private float m_Time;

        //private Vector3 cameraPositionCrouching = Vector3.zero;
        //private Vector3 cameraPositionStanding = Vector3.zero;

        public void Setup(Camera camera, float bobBaseInterval)
        {

            m_BobBaseInterval = bobBaseInterval;
            //m_OriginalCameraPosition = camera.transform.localPosition;

            // get the length of the curve in time
            m_Time = Bobcurve[Bobcurve.length - 1].time;

        }

        public Vector3 DoHeadBob(float speed, bool isCrouching, Vector3 basePosition)
        {

            float xPos = 0.0f;
            float yPos = 0.0f;

            if (isCrouching)
            {
                //xPos = m_OriginalCameraPosition.x + (Bobcurve.Evaluate(m_CyclePositionX) * HorizontalBobRangeCrouching);
                //yPos = m_OriginalCameraPosition.y + (Bobcurve.Evaluate(m_CyclePositionY) * VerticalBobRangeCrouching);
                xPos = basePosition.x + (Bobcurve.Evaluate(m_CyclePositionX) * HorizontalBobRangeCrouching);
                yPos = basePosition.y + (Bobcurve.Evaluate(m_CyclePositionY) * VerticalBobRangeCrouching);
            }
            else
            {
                //xPos = m_OriginalCameraPosition.x + (Bobcurve.Evaluate(m_CyclePositionX) * HorizontalBobRangeStanding);
                //yPos = m_OriginalCameraPosition.y + (Bobcurve.Evaluate(m_CyclePositionY) * VerticalBobRangeStanding);
                xPos = basePosition.x + (Bobcurve.Evaluate(m_CyclePositionX) * HorizontalBobRangeStanding);
                yPos = basePosition.y + (Bobcurve.Evaluate(m_CyclePositionY) * VerticalBobRangeStanding);
            }

            m_CyclePositionX += (speed * Time.deltaTime) / m_BobBaseInterval;

            if (isCrouching)
            {
                m_CyclePositionY += ((speed * Time.deltaTime) / m_BobBaseInterval) * VerticalToHorizontalRatioCrouching;
            }
            else
            {
                m_CyclePositionY += ((speed * Time.deltaTime) / m_BobBaseInterval) * VerticalToHorizontalRatioStanding;
            }


            if (m_CyclePositionX > m_Time)
            {
                m_CyclePositionX = m_CyclePositionX - m_Time;
            }
            if (m_CyclePositionY > m_Time)
            {
                m_CyclePositionY = m_CyclePositionY - m_Time;
            }

            return new Vector3(xPos, yPos, 0.0f);

        }

    }

}
