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
using Oculus.Interaction.Body.Input;
using UnityEngine.Assertions;

namespace Oculus.Interaction.Body
{
    /// <summary>
    /// Debugs an <cref"IBody" /> by drawing its axises and joints.
    /// </summary>
    public class BodyDebugGizmos : SkeletonDebugGizmos
    {
        public enum CoordSpace
        {
            World,
            Local,
        }

        /// <summary>
        /// The IBody that will drive the visuals.
        /// </summary>
        [SerializeField, Interface(typeof(IBody))]
        private UnityEngine.Object _body;
        private IBody Body;

        /// <summary>
        /// The coordinate space in which to draw the skeleton. World space draws the skeleton at the world Body location.
        /// Local draws the skeleton relative to this transform's position, and can be placed, scaled, or mirrored as desired.
        /// </summary>
        [Tooltip("The coordinate space in which to draw the skeleton. " +
            "World space draws the skeleton at the world Body location. " +
            "Local draws the skeleton relative to this transform's position, and can be placed, scaled, or mirrored as desired.")]
        [SerializeField]
        private CoordSpace _space = CoordSpace.World;

        public CoordSpace Space
        {
            get => _space;
            set => _space = value;
        }

        protected bool _started = false;

        protected virtual void Awake()
        {
            Body = _body as IBody;
        }

        protected virtual void Start()
        {
            this.BeginStart(ref _started);
            Assert.IsNotNull(Body);
            this.EndStart(ref _started);
        }

        protected virtual void OnEnable()
        {
            if (_started)
            {
                Body.WhenBodyUpdated += HandleBodyUpdated;
            }
        }

        protected virtual void OnDisable()
        {
            if (_started)
            {
                Body.WhenBodyUpdated -= HandleBodyUpdated;
            }
        }

        protected override bool TryGetWorldJointPose(int jointId, out Pose pose)
        {
            bool result;
            switch (_space)
            {
                default:
                case CoordSpace.World:
                    result = Body.GetJointPose((BodyJointId)jointId, out pose);
                    break;
                case CoordSpace.Local:
                    result = Body.GetJointPoseFromRoot((BodyJointId)jointId, out pose);
                    pose.position = transform.TransformPoint(pose.position);
                    pose.rotation = transform.rotation * pose.rotation;
                    break;
            }
            return result;
        }

        protected override bool TryGetParentJointId(int jointId, out int parent)
        {
            if (Body.SkeletonMapping.TryGetParentJointId(
                (BodyJointId)jointId, out BodyJointId parentJointId))
            {
                parent = (int)parentJointId;
                return true;
            }
            parent = default;
            return false;
        }

        private VisibilityFlags GetModifiedDrawFlags()
        {
            VisibilityFlags modifiedFlags = Visibility;
            if (HasNegativeScale && Space == CoordSpace.Local)
            {
                modifiedFlags &= ~VisibilityFlags.Axes;
            }
            return modifiedFlags;
        }

        private void HandleBodyUpdated()
        {
            foreach (BodyJointId joint in Body.SkeletonMapping.Joints)
            {
                Draw((int)joint, GetModifiedDrawFlags());
            }
        }

        #region Inject

        public void InjectAllBodyJointDebugGizmos(IBody body)
        {
            InjectBody(body);
        }

        public void InjectBody(IBody body)
        {
            _body = body as UnityEngine.Object;
            Body = body;
        }

        #endregion
    }
}
