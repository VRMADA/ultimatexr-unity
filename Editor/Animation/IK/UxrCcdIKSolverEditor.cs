// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCcdIKSolverEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation.IK;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Animation.IK
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrCcdIKSolver" />. Also draws handles in the scene window.
    /// </summary>
    [CustomEditor(typeof(UxrCcdIKSolver))]
    public class UxrCcdIKSolverEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Initializes the link count variable and hooks the Update() method to monitor changes on the component's link count.
        /// </summary>
        private void OnEnable()
        {
            _linkCount               =  ((UxrCcdIKSolver)serializedObject.targetObject).Links.Count;
            EditorApplication.update += EditorApplication_Updated;
        }

        /// <summary>
        ///     Removes the Update hook.
        /// </summary>
        private void OnDisable()
        {
            EditorApplication.update -= EditorApplication_Updated;
        }

        /// <summary>
        ///     Draws the IK scene handles.
        /// </summary>
        private void OnSceneGUI()
        {
            UxrCcdIKSolver solverCcd = target as UxrCcdIKSolver;

            if (solverCcd == null || solverCcd.Links == null)
            {
                return;
            }

            if (Application.isPlaying == false)
            {
                solverCcd.ComputeLinkData();
            }

            int index = 0;

            foreach (UxrCcdLink link in solverCcd.Links)
            {
                if (link.Bone == null)
                {
                    continue;
                }

                Vector3 normal = link.Bone.TransformDirection(link.RotationAxis1);
                Handles.color = new Color(Mathf.Abs(link.RotationAxis1.x), Mathf.Abs(link.RotationAxis1.y), Mathf.Abs(link.RotationAxis1.z), 0.3f);

                float angle1Min = link.Axis1HasLimits ? link.Axis1AngleMin : -180.0f;
                float angle1Max = link.Axis1HasLimits ? link.Axis1AngleMax : 180.0f;

                Handles.DrawSolidArc(link.Bone.position,
                                     normal,
                                     Quaternion.AngleAxis(angle1Min - link.Angle1, normal) * link.Bone.TransformDirection(link.LocalSpaceAxis1ZeroAngleVector),
                                     angle1Max - angle1Min,
                                     link.LinkLength * 0.5f);
                Handles.color = new Color(Mathf.Abs(link.RotationAxis1.x), Mathf.Abs(link.RotationAxis1.y), Mathf.Abs(link.RotationAxis1.z), 1.0f);
                Handles.DrawLine(link.Bone.position, link.Bone.position + 0.6f * link.LinkLength * link.Bone.TransformDirection(link.LocalSpaceAxis1ZeroAngleVector));

                if (link.Constraint == UxrCcdConstraintType.TwoAxes)
                {
                    float angle2Min = link.Axis2HasLimits ? link.Axis2AngleMin : -180.0f;
                    float angle2Max = link.Axis2HasLimits ? link.Axis2AngleMax : 180.0f;

                    normal        = link.Bone.TransformDirection(link.RotationAxis2);
                    Handles.color = new Color(Mathf.Abs(link.RotationAxis2.x), Mathf.Abs(link.RotationAxis2.y), Mathf.Abs(link.RotationAxis2.z), 0.3f);
                    Handles.DrawSolidArc(link.Bone.position,
                                         normal,
                                         Quaternion.AngleAxis(angle2Min + link.Angle2, normal) * link.Bone.TransformDirection(link.LocalSpaceAxis2ZeroAngleVector),
                                         angle2Max - angle2Min,
                                         link.LinkLength * 0.5f);
                    Handles.color = new Color(Mathf.Abs(link.RotationAxis2.x), Mathf.Abs(link.RotationAxis2.y), Mathf.Abs(link.RotationAxis2.z), 1.0f);
                    Handles.DrawLine(link.Bone.position, link.Bone.position + 0.6f * link.LinkLength * link.Bone.TransformDirection(link.LocalSpaceAxis2ZeroAngleVector));
                }

                if (index == 0 && link.Bone != null && solverCcd.EndEffector != null && solverCcd.Goal != null)
                {
                    Handles.color = Color.magenta;
                    Handles.DrawLine(link.Bone.position,             solverCcd.EndEffector.position);
                    Handles.DrawLine(solverCcd.EndEffector.position, solverCcd.Goal.position);
                }

                index++;
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Monitors for changes in the link count so that if new ones are added they are assigned default parameters.
        ///     Unity does not support assigning default values to new elements added. This is the reason we need to do this.
        /// </summary>
        private void EditorApplication_Updated()
        {
            UxrCcdIKSolver solverCcd = target as UxrCcdIKSolver;

            if (solverCcd != null && solverCcd.Links.Count != _linkCount)
            {
                if (solverCcd.Links.Count > _linkCount)
                {
                    for (int i = _linkCount; i < solverCcd.Links.Count; ++i)
                    {
                        solverCcd.SetLinkDefaultValues(i);
                    }
                }

                _linkCount = solverCcd.Links.Count;
            }
        }

        #endregion

        #region Private Types & Data

        private int _linkCount;

        #endregion
    }
}