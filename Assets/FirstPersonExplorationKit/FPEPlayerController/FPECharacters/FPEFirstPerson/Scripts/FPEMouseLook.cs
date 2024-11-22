using System;
using UnityEngine;

namespace Whilefun.FPEKit {

    public class FPEMouseLook : MonoBehaviour
    {

        public bool clampVerticalRotation = true;
        public float MinimumX = -80.0f;
        public float MaximumX = 80.0f;

        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;

        [Header("Custom Flags")]
        [Tooltip("Toggle mouse look on and off")]
        public bool enableMouseLook = true;

        // View limiting stuff (for "docked" type interactions)
        private bool restrictLook = false;
        private Vector2 lookRestrictionAngles = Vector2.zero;
        private Vector2 rotationSum = Vector2.zero;
        private Vector2 lastRotationChanges = Vector2.zero;

        private FPEInputManager inputManager = null;


        public void Init(Transform character, Transform camera)
        {

            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;

        }

        void Start()
        {

            inputManager = FPEInputManager.Instance;

            if (!inputManager)
            {
                Debug.LogError("FPEMouseLook:: Cannot find an instance of FPEInputManager in the scene. Mouse look will not function correctly!");
            }

        }

        
        public void LookRotation(Transform character, Transform camera)
        {

            if (enableMouseLook)
            {

                lastRotationChanges.x = inputManager.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_MOUSELOOKY);
                lastRotationChanges.y = inputManager.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_MOUSELOOKX);

                // If there was no mouse input use gamepad instead
                if (lastRotationChanges.x == 0 & lastRotationChanges.y == 0)
                {

                    // Note: We scale our gamepad by delta time because it's not a "change since last frame" like mouse 
                    // input, so we need to simulate that ourselves.
                    lastRotationChanges.x = inputManager.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_LOOKY) * Time.deltaTime;
                    lastRotationChanges.y = inputManager.GetAxis(FPEInputManager.eFPEInput.FPE_INPUT_LOOKX) * Time.deltaTime;

                }

                if (restrictLook)
                {
                    
                    if ((rotationSum.y + lastRotationChanges.y) > lookRestrictionAngles.y)
                    {
                        lastRotationChanges.y = (lookRestrictionAngles.y - rotationSum.y);
                    }
                    else if ((rotationSum.y + lastRotationChanges.y) < -lookRestrictionAngles.y)
                    {
                        lastRotationChanges.y = (-lookRestrictionAngles.y - rotationSum.y);
                    }

                    rotationSum.y += lastRotationChanges.y;

                    if ((rotationSum.x + lastRotationChanges.x) > lookRestrictionAngles.x)
                    {
                        lastRotationChanges.x = (lookRestrictionAngles.x - rotationSum.x);
                    }
                    else if ((rotationSum.x + lastRotationChanges.x) < -lookRestrictionAngles.x)
                    {
                        lastRotationChanges.x = (-lookRestrictionAngles.x - rotationSum.x);
                    }

                    rotationSum.x += lastRotationChanges.x;

                }

                m_CharacterTargetRot *= Quaternion.Euler(0.0f, lastRotationChanges.y, 0.0f);
                m_CameraTargetRot *= Quaternion.Euler(-lastRotationChanges.x, 0.0f, 0.0f);

                // Only clamp when not restricting look
                if (!restrictLook && clampVerticalRotation)
                {
                    m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
                }

                if (inputManager.LookSmoothing)
                {
                    character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, inputManager.LookSmoothFactor * Time.deltaTime);
                    camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot, inputManager.LookSmoothFactor * Time.deltaTime);
                }
                else
                {
                    character.localRotation = m_CharacterTargetRot;
                    camera.localRotation = m_CameraTargetRot;
                }

            }

        }


        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {

            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;

        }

        /// <summary>
        /// Makes player camera look at a position
        /// </summary>
        /// <param name="character">Character's transform</param>
        /// <param name="camera">Character's 'Main' camera transform</param>
        /// <param name="focalPoint">The world position of the focal point to make player look at</param>
        public void LookAtPosition(Transform character, Transform camera, Vector3 focalPoint)
        {

            // Make character face target //
            Vector3 relativeCharPosition = focalPoint - character.position;
            Quaternion rotation = Quaternion.LookRotation(relativeCharPosition);
            Vector3 flatCharRotation = rotation.eulerAngles;
            flatCharRotation.x = 0.0f;
            flatCharRotation.z = 0.0f;
            character.localRotation = Quaternion.Euler(flatCharRotation);
            // Key: make target rotation our current rotation :)
            m_CharacterTargetRot = character.localRotation;

            // Make Camera face target //
            Vector3 relativeCamPosition = focalPoint - camera.position;
            Quaternion camRotation = Quaternion.LookRotation(relativeCamPosition);
            Vector3 flatCamRotation = camRotation.eulerAngles;
            flatCamRotation.y = 0.0f;
            flatCamRotation.z = 0.0f;
            camera.localRotation = Quaternion.Euler(flatCamRotation);
            // Key: Make cam target rotation our current cam rotation
            m_CameraTargetRot = camera.localRotation;

        }
        
        public void enableLookRestriction(Vector2 maxAnglesFromOrigin)
        {

            // Note: we flip these because 'x' rotation means 'horizontal' to the user, but it really translates to 'tilt' which is vertical
            lookRestrictionAngles.x = maxAnglesFromOrigin.y;
            lookRestrictionAngles.y = maxAnglesFromOrigin.x;
            rotationSum = Vector2.zero;
            restrictLook = true;

        }

        public void disableLookRestriction()
        {
            restrictLook = false;
        }

    }

}
