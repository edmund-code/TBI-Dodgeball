/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface
{
    internal class ProxyCameraRig
    {
        public Camera Camera { get; private set; }
        public Transform CameraTransform { get; private set; }
        public Transform LeftControllerTransform { get; private set; }
        public Transform RightControllerTransform { get; private set; }

        public bool Refresh()
        {
            if (Camera != null && Camera.isActiveAndEnabled) return true;

            SearchForCamera();

            return Camera;
        }

        public void SearchForCamera()
        {
            if (RuntimeSettings.Instance.UseCustomIntegrationConfig)
            {
                Camera = CustomIntegrationConfig.GetCamera();
                CameraTransform = Camera?.gameObject.transform;
                LeftControllerTransform = CustomIntegrationConfig.GetLeftControllerTransform();
                RightControllerTransform = CustomIntegrationConfig.GetRightControllerTransform();
            }
            else
            {
                var cameraRig = GameObject.FindAnyObjectByType<OVRCameraRig>();
                if (cameraRig)
                {
                    // OVR Path
                    Camera = cameraRig.leftEyeCamera;
                    CameraTransform = cameraRig.leftEyeAnchor;
                    LeftControllerTransform = cameraRig.leftControllerAnchor;
                    RightControllerTransform = cameraRig.rightControllerAnchor;
                }
            }
        }
    }
}

