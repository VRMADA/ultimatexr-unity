// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TransformExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Core.Math;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.Unity.Math;
using UltimateXR.Extensions.Unity.Render;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UltimateXR.Extensions.Unity
{
    using UnityObject = Object;

    /// <summary>
    ///     <see cref="Transform" /> extensions.
    /// </summary>
    public static class TransformExt
    {
        #region Public Types & Data

        /// <summary>
        ///     Mirroring options for <see cref="TransformExt.ApplyMirroring" />.
        /// </summary>
        public enum MirrorType
        {
            /// <summary>
            ///     Mirror X and Y vectors. Z Vector will be computed the using cross product.
            /// </summary>
            MirrorXY,

            /// <summary>
            ///     Mirror X and Z vectors. Y Vector will be computed the using cross product.
            /// </summary>
            MirrorXZ,

            /// <summary>
            ///     Mirror Y and Z Vectors. X Vector will be computed using the cross product.
            /// </summary>
            MirrorYZ
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Destroys all children using Destroy().
        /// </summary>
        /// <param name="transform">Transform to destroy all children from</param>
        public static void DestroyAllChildren(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                UnityObject.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        ///     Destroys all children GameObjects using Destroy() that have a given component type.
        /// </summary>
        /// <param name="transform">Transform to destroy all children from</param>
        /// <param name="includeInactive">Also delete children with inactive components?</param>
        public static void DestroyAllChildren<T>(this Transform transform, bool includeInactive = true)
                    where T : Component
        {
            T[] children = transform.GetComponentsInChildren<T>(includeInactive);

            foreach (T t in children)
            {
                if (t != null)
                {
                    UnityObject.Destroy(t.gameObject);
                }
            }
        }

        /// <summary>
        ///     Destroys all children using DestroyImmediate().
        /// </summary>
        /// <param name="transform">Transform to destroy all children from</param>
        public static void DestroyImmediateAllChildren(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                UnityObject.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        ///     Destroys all children GameObjects using DestroyImmediate() that have a given component type.
        /// </summary>
        /// <param name="transform">Transform to destroy all children from</param>
        /// <param name="includeInactive">Also delete children with inactive components?</param>
        public static void DestroyImmediateAllChildren<T>(this Transform transform, bool includeInactive = true)
                    where T : Component
        {
            T[] children = transform.GetComponentsInChildren<T>(includeInactive);

            foreach (T t in children)
            {
                if (t != null)
                {
                    UnityObject.DestroyImmediate(t.gameObject);
                }
            }
        }

        /// <summary>
        ///     Assigns a transform the same position and rotation from another.
        /// </summary>
        /// <param name="self">Transform to change</param>
        /// <param name="target">Target to get the position and rotation from</param>
        public static void SetPositionAndRotation(this Transform self, Transform target)
        {
            self.SetPositionAndRotation(target.position, target.rotation);
        }
		
#if !(UNITY_2021_3_11 || UNITY_2022_2 || UNITY_2023_1)
		
		/// <summary>
        ///     Sets the local position and local rotation in one go.
        /// </summary>
        /// <param name="self">Transform to change</param>
        /// <param name="localPosition">New local position</param>
		/// <param name="localRotation">New local rotation</param>
        public static void SetLocalPositionAndRotation(this Transform self, Vector3 localPosition, Quaternion localRotation)
        {
            self.localPosition = localPosition;
			self.localRotation = localRotation;
        }
		
#endif

        /// <summary>
        ///     Sets the localPosition.x value of a given <see cref="Transform" />.
        /// </summary>
        /// <param name="self">Transform to set the localPosition.x of</param>
        /// <param name="x">New x value</param>
        public static void SetLocalPositionX(this Transform self, float x)
        {
            Vector3 localPosition = self.localPosition;
            localPosition.x    = x;
            self.localPosition = localPosition;
        }

        /// <summary>
        ///     Sets the localPosition.y value of a given <see cref="Transform" />.
        /// </summary>
        /// <param name="self">Transform to set the localPosition.y of</param>
        /// <param name="y">New y value</param>
        public static void SetLocalPositionY(this Transform self, float y)
        {
            Vector3 localPosition = self.localPosition;
            localPosition.y    = y;
            self.localPosition = localPosition;
        }

        /// <summary>
        ///     Sets the localPosition.z value of a given <see cref="Transform" />.
        /// </summary>
        /// <param name="self">Transform to set the localPosition.z of</param>
        /// <param name="x">New z value</param>
        public static void SetLocalPositionZ(this Transform self, float z)
        {
            Vector3 localPosition = self.localPosition;
            localPosition.z    = z;
            self.localPosition = localPosition;
        }

        /// <summary>
        ///     Adds x, y and z values to the localPosition of a given <see cref="Transform" />.
        /// </summary>
        /// <param name="self">Transform whose localPosition to add the values to</param>
        /// <param name="x">Value to add to localPosition.x</param>
        /// <param name="y">Value to add to localPosition.y</param>
        /// <param name="z">Value to add to localPosition.z</param>
        public static void IncreaseLocalPosition(this Transform self, float x, float y, float z)
        {
            Vector3 localPosition = self.localPosition;
            localPosition.x    += x;
            localPosition.y    += y;
            localPosition.z    += z;
            self.localPosition =  localPosition;
        }

        /// <summary>
        ///     Sets the position.x value of a <see cref="Transform" />.
        /// </summary>
        /// <param name="self">Transform whose position.x value to set</param>
        /// <param name="x">New x value</param>
        public static void SetPositionX(this Transform self, float x)
        {
            Vector3 position = self.position;
            position.x    = x;
            self.position = position;
        }

        /// <summary>
        ///     Sets the position.y value of a <see cref="Transform" />.
        /// </summary>
        /// <param name="self">Transform whose position.y value to set</param>
        /// <param name="x">New y value</param>
        public static void SetPositionY(this Transform self, float y)
        {
            Vector3 position = self.position;
            position.y    = y;
            self.position = position;
        }

        /// <summary>
        ///     Sets the position.z value of a <see cref="Transform" />.
        /// </summary>
        /// <param name="self">Transform whose position.z value to set</param>
        /// <param name="x">New z value</param>
        public static void SetPositionZ(this Transform self, float z)
        {
            Vector3 position = self.position;
            position.z    = z;
            self.position = position;
        }

        /// <summary>
        ///     Adds x, y and z values to the position of a given <see cref="Transform" />.
        /// </summary>
        /// <param name="self">Transform whose position to add the values to</param>
        /// <param name="x">Value to add to position.x</param>
        /// <param name="y">Value to add to position.y</param>
        /// <param name="z">Value to add to position.z</param>
        public static void IncreasePosition(this Transform self, float x, float y, float z)
        {
            Vector3 position = self.position;
            position.x    += x;
            position.y    += y;
            position.z    += z;
            self.position =  position;
        }

        /// <summary>
        ///     Gets a vector result of adding a Transform's right, up and forward vectors scaled by the x, y and z values of a
        ///     vector.
        /// </summary>
        /// <param name="self">Transform whose axes will be used</param>
        /// <param name="v">Vector with the x, y and z scale factors</param>
        /// <returns>Vector result of applying the scale factors on the Transform axes and adding them together</returns>
        public static Vector3 GetScaledVector(this Transform self, Vector3 v)
        {
            return GetScaledVector(self, v.x, v.y, v.z);
        }

        /// <summary>
        ///     Gets a vector result of adding a Transform's right, up and forward vectors scaled by x, y and z values
        ///     respectively.
        /// </summary>
        /// <param name="self">Transform whose axes will be used</param>
        /// <param name="x">Scale factor applied to the Transform's right vector</param>
        /// <param name="y">Scale factor applied to the Transform's up vector</param>
        /// <param name="z">Scale factor applied to the Transform's forward vector</param>
        /// <returns>Vector result of applying the scale values on the Transform axes and adding them together</returns>
        public static Vector3 GetScaledVector(this Transform self, float x, float y, float z)
        {
            return self.right * x + self.up * y + self.forward * z;
        }

        /// <summary>
        ///     Gets a vector representing the axis from a <see cref="Transform" /> that has the smallest angle to a given
        ///     world-space vector.
        ///     The returned vector may be any of the three positive or negative axes in local space of the
        ///     <see cref="Transform" />.
        /// </summary>
        /// <param name="self">Transform whose axes to compare to the given vector</param>
        /// <param name="vector">World-space vector to compare</param>
        /// <returns>A local-space vector representing the positive or negative axis with the smallest angle to the vector</returns>
        public static Vector3 GetClosestLocalAxis(this Transform self, Vector3 vector)
        {
            return self.InverseTransformDirection(vector).normalized.GetClosestAxis();
        }

        /// <summary>
        ///     Gets a vector representing the axis from a <see cref="Transform" /> that has the smallest angle to a given
        ///     world-space vector.
        ///     The returned vector may be any of the three positive or negative axes in world space of the
        ///     <see cref="Transform" />.
        /// </summary>
        /// <param name="self">Transform whose axes to compare to the given vector</param>
        /// <param name="vector">World-space vector to compare</param>
        /// <returns>A world-space vector representing the positive or negative axis with the smallest angle to the vector</returns>
        public static Vector3 GetClosestAxis(this Transform self, Vector3 vector)
        {
            return self.TransformDirection(self.InverseTransformDirection(vector).normalized.GetClosestAxis());
        }

        /// <summary>
        ///     Constraints the given transform position to the volume specified by a box collider.
        /// </summary>
        /// <param name="self">The transform to constrain</param>
        /// <param name="box">The volume that the position will be constrained to</param>
        public static void ClampPositionToBox(this Transform self, BoxCollider box)
        {
            self.position = self.position.ClampToBox(box);
        }

        /// <summary>
        ///     Aligns all children of a given component on an axis.
        /// </summary>
        /// <param name="transform">Transform whose children to align</param>
        /// <param name="padding">Pivot separation between the different children</param>
        /// <param name="axis">Axis where the children will be aligned</param>
        /// <param name="space">Space to use for the alignment axis</param>
        /// <exception cref="System.ArgumentNullException">Transform is null</exception>
        public static void AlignChildrenOnAxis(this Transform transform, float padding, Vector3 axis, Space space = Space.World)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            Vector3 paddingVector = padding * axis;

            Vector3 centerPos = space == Space.Self ? Vector3.zero : transform.position;
            Vector3 firstPos  = centerPos - 0.5f * (transform.childCount - 1) * paddingVector;

            for (int i = 0; i < transform.childCount; ++i)
            {
                Vector3 pos = firstPos + i * paddingVector;
                if (space == Space.Self)
                {
                    transform.GetChild(i).localPosition = pos;
                }
                else
                {
                    transform.GetChild(i).position = pos;
                }
            }
        }

        /// <summary>
        ///     Aligns a set of transforms on an axis.
        /// </summary>
        /// <param name="transforms">Transform whose children to align</param>
        /// <param name="padding">Pivot separation between the different children</param>
        /// <param name="axis">Axis where the children will be aligned</param>
        /// <param name="space">Space to use for the alignment axis</param>
        /// <exception cref="System.ArgumentNullException">Transform is null</exception>
        public static void AlignOnAxis(this IEnumerable<Transform> transforms, float padding, Vector3 axis, Space space = Space.World)
        {
            if (transforms == null)
            {
                throw new ArgumentNullException(nameof(transforms));
            }

            Transform[] tArray        = transforms.ToArray();
            Vector3     paddingVector = padding * axis;

            Vector3 pivotPos = space == Space.Self ? tArray[0].localPosition : tArray[0].position;
            Vector3 firstPos = pivotPos - 0.5f * (tArray.Length - 1) * paddingVector;

            for (var i = 0; i < tArray.Length; ++i)
            {
                Vector3 pos = firstPos + i * paddingVector;
                if (space == Space.Self)
                {
                    tArray[i].localPosition = pos;
                }
                else
                {
                    tArray[i].position = pos;
                }
            }
        }

        /// <summary>
        ///     Applies a mirroring to a transform.
        /// </summary>
        /// <param name="self">Transform to apply mirroring to</param>
        /// <param name="mirror">Mirror</param>
        /// <param name="mirrorAxis">Mirror axis</param>
        /// <param name="mirrorType">Which vectors to mirror. See <see cref="MirrorType" /></param>
        /// <param name="rotate">Whether to rotate the object</param>
        /// <param name="reposition">Whether to translate the object</param>
        public static void ApplyMirroring(this Transform self,
                                          Transform      mirror,
                                          UxrAxis        mirrorAxis,
                                          MirrorType     mirrorType,
                                          bool           rotate     = true,
                                          bool           reposition = true)
        {
            ApplyMirroring(self, mirror.position, mirror.TransformDirection(mirrorAxis), mirrorType, rotate, reposition);
        }

        /// <summary>
        ///     Applies a mirroring to a transform.
        /// </summary>
        /// <param name="self">Transform to apply mirroring to</param>
        /// <param name="mirrorPosition">Mirror position</param>
        /// <param name="mirrorNormal">Mirror normal</param>
        /// <param name="mirrorType">Which vectors to mirror. See <see cref="MirrorType" /></param>
        /// <param name="rotate">Whether to rotate the object</param>
        /// <param name="reposition">Whether to translate the object</param>
        public static void ApplyMirroring(this Transform self,
                                          Vector3        mirrorPosition,
                                          Vector3        mirrorNormal,
                                          MirrorType     mirrorType,
                                          bool           rotate     = true,
                                          bool           reposition = true)
        {
            if (rotate)
            {
                Vector3 right   = Vector3.Reflect(self.right,   mirrorNormal);
                Vector3 up      = Vector3.Reflect(self.up,      mirrorNormal);
                Vector3 forward = Vector3.Reflect(self.forward, mirrorNormal);

                if (mirrorType == MirrorType.MirrorXY)
                {
                    self.rotation = Quaternion.LookRotation(Vector3.Cross(right, up), up);
                }
                else if (mirrorType == MirrorType.MirrorXZ)
                {
                    self.rotation = Quaternion.LookRotation(forward, Vector3.Cross(forward, right));
                }
                else if (mirrorType == MirrorType.MirrorYZ)
                {
                    self.rotation = Quaternion.LookRotation(forward, up);
                }
            }

            if (reposition)
            {
                Vector3 projection = new Plane(mirrorNormal, mirrorPosition).ClosestPointOnPlane(self.position);
                self.position += (projection - self.position) * 2;
            }
        }

        /// <summary>
        ///     Applies the transformation required to make <paramref name="sourceAlign" /> align with
        ///     <paramref name="targetAlign" />.
        /// </summary>
        /// <param name="self">Transform to apply the alignment to</param>
        /// <param name="sourceAlign">Source reference that will try to match <paramref name="targetAlign" /></param>
        /// <param name="targetAlign">Target reference</param>
        /// <param name="rotate">Allows to control whether to rotate or not</param>
        /// <param name="translate">Allows to control whether to translate or not</param>
        /// <param name="t">Optional interpolation value</param>
        public static void ApplyAlignment(this Transform self,
                                          Transform      sourceAlign,
                                          Transform      targetAlign,
                                          bool           rotate    = true,
                                          bool           translate = true,
                                          float          t         = 1.0f)
        {
            if (rotate)
            {
                ApplyAlignment(self, sourceAlign.rotation, targetAlign.rotation, t);
            }

            if (translate)
            {
                self.position += (targetAlign.position - sourceAlign.position) * t;
            }
        }

        /// <summary>
        ///     Applies the transformation to make a transform defined by <paramref name="sourcePosition" /> and
        ///     <paramref name="sourceRotation" /> move and rotate to <paramref name="targetPosition" /> and
        ///     <paramref name="targetRotation" />.
        /// </summary>
        /// <param name="transform">Transform to apply the alignment to</param>
        /// <param name="sourcePosition">Source position that will try to match <paramref name="targetPosition" /></param>
        /// <param name="sourceRotation">Source rotation that will try to match <paramref name="targetRotation" /></param>
        /// <param name="targetPosition">Target position</param>
        /// <param name="targetRotation">Target rotation</param>
        /// <param name="rotate">Allows to control whether to rotate or not</param>
        /// <param name="translate">Allows to control whether to translate or not</param>
        /// <param name="t">Optional interpolation value [0.0, 1.0]</param>
        public static void ApplyAlignment(this Transform transform,
                                          Vector3        sourcePosition,
                                          Quaternion     sourceRotation,
                                          Vector3        targetPosition,
                                          Quaternion     targetRotation,
                                          bool           rotate,
                                          bool           translate,
                                          float          t = 1.0f)
        {
            if (rotate)
            {
                ApplyAlignment(transform, sourceRotation, targetRotation, t);
            }

            if (translate)
            {
                transform.position += (targetPosition - sourcePosition) * t;
            }
        }

        /// <summary>
        ///     Applies the transformation to make a rotation defined by <paramref name="sourceRotation" /> rotate towards
        ///     <paramref name="targetRotation" />.
        /// </summary>
        /// <param name="self">Transform to apply the alignment to</param>
        /// <param name="sourceRotation">Source rotation that will try to match <paramref name="targetRotation" /></param>
        /// <param name="targetRotation">Target rotation to match</param>
        /// <param name="t">Optional interpolation value [0.0, 1.0]</param>
        public static void ApplyAlignment(this Transform self, Quaternion sourceRotation, Quaternion targetRotation, float t = 1.0f)
        {
            Quaternion selfRotation = self.rotation;
            Quaternion rotation     = Quaternion.RotateTowards(sourceRotation, targetRotation, 180.0f);
            Quaternion relative     = Quaternion.Inverse(sourceRotation) * selfRotation;
            self.rotation = Quaternion.Slerp(selfRotation, rotation * relative, t);
        }

        /// <summary>
        ///     Moves <paramref name="rootPosition" /> and rotates <paramref name="rootRotation" /> so that a child
        ///     defined by <paramref name="childPosition" /> and <paramref name="childRotation" /> gets aligned to
        ///     <paramref name="targetPosition" /> and <paramref name="targetRotation" />.
        /// </summary>
        /// <param name="rootPosition">Position of the root object</param>
        /// <param name="rootRotation">Rotation of the root object</param>
        /// <param name="childPosition">
        ///     Position of the child object that will try to align to <paramref name="targetPosition" />
        /// </param>
        /// <param name="childRotation">
        ///     Rotation of the child object that will try to align to <paramref name="targetRotation" />
        /// </param>
        /// <param name="targetPosition">Target position</param>
        /// <param name="targetRotation">Target rotation</param>
        /// <param name="rotate">Allows to control whether to rotate or not</param>
        /// <param name="translate">Allows to control whether to translate or not</param>
        /// <param name="t">Optional interpolation value [0.0, 1.0]</param>
        public static void ApplyAlignment(ref Vector3    rootPosition,
                                          ref Quaternion rootRotation,
                                          Vector3        childPosition,
                                          Quaternion     childRotation,
                                          Vector3        targetPosition,
                                          Quaternion     targetRotation,
                                          bool           rotate,
                                          bool           translate,
                                          float          t = 1.0f)
        {
            Matrix4x4 matrix      = Matrix4x4.TRS(rootPosition, rootRotation, Vector3.one);
            Vector3   relativePos = matrix.inverse.MultiplyPoint(childPosition);

            if (rotate)
            {
                Quaternion rotationTowards = Quaternion.RotateTowards(childRotation, targetRotation, 180.0f);
                Quaternion relative        = Quaternion.Inverse(childRotation) * rootRotation;
                rootRotation = Quaternion.Slerp(rootRotation, rotationTowards * relative, t);

                Matrix4x4 newMatrix = Matrix4x4.TRS(rootPosition, rootRotation, Vector3.one);
                childPosition = newMatrix.MultiplyPoint(relativePos);
            }

            if (translate)
            {
                rootPosition += (targetPosition - childPosition) * t;
            }
        }

        /// <summary>
        ///     Applies an interpolation between two other transforms.
        /// </summary>
        /// <param name="source">Transform to apply the interpolation to</param>
        /// <param name="a">Start transform</param>
        /// <param name="b">Finish transform</param>
        /// <param name="t">Interpolation value</param>
        public static void ApplyInterpolation(Transform source, Transform a, Transform b, float t)
        {
            source.SetPositionAndRotation(Vector3.Lerp(a.position, b.position, t), Quaternion.Slerp(a.rotation, b.rotation, t));
        }

        /// <summary>
        ///     Checks if a given transform has a given parent in its upwards hierarchy, or if it is the transform itself.
        /// </summary>
        /// <param name="self">Caller</param>
        /// <param name="parent">Transform to check if it is present</param>
        /// <returns>True if present, false if not</returns>
        public static bool HasParent(this Transform self, Transform parent)
        {
            if (self == null)
            {
                return false;
            }
            
            if (self == parent)
            {
                return true;
            }

            if (self.parent != null)
            {
                return HasParent(self.parent, parent);
            }

            return false;
        }

        /// <summary>
        ///     Checks if a given transform has a given child in its hierarchy or if the transform is the child itself.
        /// </summary>
        /// <param name="current">Caller</param>
        /// <param name="child">Transform to check if it is present</param>
        /// <returns>True if present, false if not</returns>
        public static bool HasChild(this Transform current, Transform child)
        {
            if (current == null)
            {
                return false;
            }
            
            if (current == child)
            {
                return true;
            }

            for (int i = 0; i < current.transform.childCount; ++i)
            {
                if (current.GetChild(i) == child)
                {
                    return true;
                }

                if (HasChild(current.GetChild(i), child))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets all the children recursively under a given <see cref="Transform" />.
        ///     The list will not contain the source <see cref="transform" /> itself.
        /// </summary>
        /// <param name="transform">Transform to get all children of</param>
        /// <param name="transforms">A list with all the transforms found below the hierarchy</param>
        public static void GetAllChildren(this Transform transform, ref List<Transform> transforms)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform child = transform.GetChild(i);
                transforms.Add(child);
                child.GetAllChildren(ref transforms);
            }
        }

        /// <summary>
        ///     From a set of transforms, returns which one of them is a common root of all if any.
        ///     The transform must be in the list itself.
        /// </summary>
        /// <param name="transforms">Variable number of transforms to check</param>
        /// <returns>
        ///     Returns which transform from all the ones passed as parameters is a common root from all. If no one is a
        ///     common root it will return null.
        /// </returns>
        public static Transform GetCommonRootTransformFromSet(params Transform[] transforms)
        {
            Transform commonRoot = null;

            for (int i = 0; i < transforms.Length; i++)
            {
                if (i == 0)
                {
                    commonRoot = transforms[i];
                }
                else
                {
                    if (commonRoot == null || (transforms[i] != commonRoot && HasParent(transforms[i], commonRoot) == false))
                    {
                        bool found = true;

                        for (int j = 0; j < i - 1; j++)
                        {
                            if (transforms[i] != transforms[j] && HasParent(transforms[j], transforms[i]) == false)
                            {
                                found = false;
                            }
                        }

                        commonRoot = found ? transforms[i] : null;
                    }
                }
            }

            return commonRoot;
        }

        /// <summary>
        ///     Gets all the transforms that don't have any children.
        /// </summary>
        /// <param name="transform">
        ///     Transform where to start looking for. This method will only traverse the transform itself and its sub-hierarchy
        /// </param>
        /// <param name="transforms">A list where all the found transforms without children will be appended</param>
        public static void GetTransformsWithoutChildren(this Transform transform, ref List<Transform> transforms)
        {
            if (transform.childCount == 0)
            {
                transforms.Add(transform);
            }
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    GetTransformsWithoutChildren(transform.GetChild(i), ref transforms);
                }
            }
        }

        /// <summary>
        ///     Gets the first non-null transform from a set of transforms.
        /// </summary>
        /// <param name="transforms">Variable number of transforms to check</param>
        /// <returns>Returns the first non-null transform or null if none was found</returns>
        public static Transform GetFirstNonNullTransformFromSet(params Transform[] transforms)
        {
            foreach (Transform transform in transforms)
            {
                if (transform != null)
                {
                    return transform;
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the n-th non-null transform from a set of transforms.
        /// </summary>
        /// <param name="n">How many non-null elements to skip until the next non-null is returned</param>
        /// <param name="transforms">Variable number of transforms to check</param>
        /// <returns>Returns the i-th non-null transform or null if none was found</returns>
        public static Transform GetNthNonNullTransformFromSet(int n, params Transform[] transforms)
        {
            int found = 0;

            foreach (Transform transform in transforms)
            {
                if (transform != null)
                {
                    if (found == n)
                    {
                        return transform;
                    }

                    found++;
                }
            }

            return null;
        }

        /// <summary>
        ///     Tries to find an object by its name in the given <see cref="Transform" /> or any of its children recursively
        /// </summary>
        /// <param name="self">Transform whose hierarchy to search</param>
        /// <param name="name">Name of the transform to find</param>
        /// <param name="stringComparison">Comparison rules to use</param>
        /// <returns>First transform found with the given name, or null if not found</returns>
        public static Transform FindRecursive(this Transform self, string name, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (self == null)
            {
                return null;
            }

            if (string.Compare(self.name, name, stringComparison) == 0)
            {
                return self;
            }

            for (int i = 0; i < self.childCount; ++i)
            {
                Transform transform = self.GetChild(i).FindRecursive(name, stringComparison);

                if (transform != null)
                {
                    return transform;
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the full GameObject path of a Transform in the hierarchy.
        /// </summary>
        /// <param name="self">Transform to get the full path from</param>
        /// <returns>Full path of the GameObject</returns>
        public static string GetPathUnderScene(this Transform self)
        {
            self.ThrowIfNull(nameof(self));
            string path = self.name;

            while (self.parent is not null)
            {
                self = self.parent;
                path = $"{self.name}/{path}";
            }

            return path;
        }

        /// <summary>
        ///     Gets a unique path name of Transform in the hierarchy. Adds a sibling index to the path to assure that siblings
        ///     with the same name create different path names.
        /// </summary>
        /// <param name="self">Transform to get a unique path from</param>
        /// <returns>Unique full path that includes sibling index information to make it different from others</returns>
        public static string GetUniqueScenePath(this Transform self)
        {
            self.ThrowIfNull(nameof(self));
            string prePath = self.parent is not null ? self.parent.GetUniqueScenePath() : $"{self.gameObject.scene.name}:";
            return $"{prePath}/{self.GetSiblingIndex():00}.-{self.name}";
        }

        /// <summary>
        ///     Gets the parent local to world transform matrix.
        /// </summary>
        /// <param name="self">Transform to get the parent matrix of</param>
        /// <returns>Parent local-to-world matrix or Identity if it has no parent</returns>
        public static Matrix4x4 GetParentWorldMatrix(this Transform self)
        {
            return self.parent != null ? self.parent.localToWorldMatrix : Matrix4x4.identity;
        }

        /// <summary>
        ///     Gets the parent rotation or the identity Quaternion if it doesn't exist.
        /// </summary>
        /// <param name="self">Transform to get the parent rotation of</param>
        /// <returns>Parent rotation or Identity if it has no parent</returns>
        public static Quaternion GetParentRotation(this Transform self)
        {
            return self.parent != null ? self.parent.rotation : Quaternion.identity;
        }

        /// <summary>
        ///     Gets the given position in <paramref name="transform" /> local coordinates. If <paramref name="transform" /> is
        ///     null, <paramref name="position" /> will be returned.
        /// </summary>
        /// <param name="transform">Transform to get the local coordinates in</param>
        /// <param name="position">Position</param>
        /// <returns>Coordinates in local space or <paramref name="position" /> if <paramref name="transform" /> is null</returns>
        public static Vector3 GetLocalPosition(Transform transform, Vector3 position)
        {
            return transform != null ? transform.InverseTransformPoint(position) : position;
        }

        /// <summary>
        ///     Transforms a position to world space coordinates. If <paramref name="transform" /> is null,
        ///     <paramref name="localPosition" /> will be returned.
        /// </summary>
        /// <param name="transform">Transform with the space of the local coordinates</param>
        /// <param name="localPosition">Position in local coordinates</param>
        /// <returns>Coordinates in world space or <paramref name="localPosition" /> if <paramref name="transform" /> is null</returns>
        public static Vector3 GetWorldPosition(Transform transform, Vector3 localPosition)
        {
            return transform != null ? transform.TransformPoint(localPosition) : localPosition;
        }

        /// <summary>
        ///     Gets the given rotation in <paramref name="transform" /> local coordinates. If <paramref name="transform" /> is
        ///     null, <paramref name="rotation" /> will be returned.
        /// </summary>
        /// <param name="transform">Transform to get the local coordinates in</param>
        /// <param name="rotation">Rotation</param>
        /// <returns>Rotation in local space or <paramref name="rotation" /> if <paramref name="transform" /> is null</returns>
        public static Quaternion GetLocalRotation(Transform transform, Quaternion rotation)
        {
            return transform != null ? Quaternion.Inverse(transform.rotation) : rotation;
        }

        /// <summary>
        ///     Transforms a rotation to world space. If <paramref name="transform" /> is null, <paramref name="localRotation" />
        ///     will be returned.
        /// </summary>
        /// <param name="transform">Transform with the space of the local rotation</param>
        /// <param name="localRotation">Local rotation</param>
        /// <returns>Rotation in world space or <paramref name="localRotation" /> if <paramref name="transform" /> is null</returns>
        public static Quaternion GetWorldRotation(Transform transform, Quaternion localRotation)
        {
            return transform != null ? transform.rotation * localRotation : localRotation;
        }

        /// <summary>
        ///     Gets the given direction in <paramref name="transform" /> local coordinates. If <paramref name="transform" /> is
        ///     null, <paramref name="direction" /> will be returned.
        /// </summary>
        /// <param name="transform">Transform to get the local coordinates in</param>
        /// <param name="direction">Direction</param>
        /// <returns>Direction in local space or <paramref name="direction" /> if <paramref name="transform" /> is null</returns>
        public static Vector3 GetLocalDirection(Transform transform, Vector3 direction)
        {
            return transform != null ? Quaternion.Inverse(transform.rotation) * direction : direction;
        }

        /// <summary>
        ///     Transforms a direction to world space. If <paramref name="transform" /> is null, <paramref name="localDirection" />
        ///     will be returned.
        /// </summary>
        /// <param name="transform">Transform with the space of the local direction</param>
        /// <param name="localDirection">Direction</param>
        /// <returns>Rotation in world space or <paramref name="localDirection" /> if <paramref name="transform" /> is null</returns>
        public static Vector3 GetWorldDirection(Transform transform, Vector3 localDirection)
        {
            return transform != null ? transform.rotation * localDirection : localDirection;
        }

        /// <summary>
        ///     Computes the bounds of all MeshRenderers that hang from a parent transform.
        /// </summary>
        /// <param name="self">Parent Transform to get all the MeshRenderers of</param>
        /// <param name="space">Space in which to retrieve the bounds</param>
        /// <param name="includeInactive">Whether to include inactive MeshRenderers</param>
        /// <returns>Bounds containing all MeshRenderers</returns>
        /// <exception cref="ArgumentNullException">Transform is null</exception>
        public static Bounds CalculateBounds(this Transform self, Space space = Space.World, bool includeInactive = false)
        {
            self.ThrowIfNull(nameof(self));

            Bounds result = self.GetComponentsInChildren<MeshRenderer>(includeInactive).CalculateBounds();

            if (result != default && space == Space.Self)
            {
                result = self.InverseTransformBounds(result);
            }

            foreach (RectTransform r in self.GetComponentsInChildren<RectTransform>(includeInactive))
            {
                if (result == default)
                {
                    result = r.CalculateRectBounds(space);
                }
                else
                {
                    result.Encapsulate(r.CalculateRectBounds(space));
                }
            }

            return result;
        }

        /// <summary>
        ///     Computes the bounds containing a <see cref="RectTransform" />'s <see cref="RectTransform.rect" />.
        /// </summary>
        /// <param name="transform">RectTransform to process</param>
        /// <param name="space">Space in which to retrieve the bounds</param>
        /// <returns>Bounds of the RectTransform</returns>
        /// <exception cref="ArgumentNullException">Transform is null</exception>
        public static Bounds CalculateRectBounds(this RectTransform transform, Space space = Space.World)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            Rect   rect        = transform.rect;
            Bounds localBounds = new Bounds(rect.center, rect.size);
            return space == Space.Self ? localBounds : transform.TransformBounds(localBounds);
        }

        /// <summary>
        ///     Gets the bounds of a BoxCollider in a given space.
        /// </summary>
        /// <param name="boxCollider">BoxCollider to get the bounds of</param>
        /// <param name="space">Space in which to retrieve the bounds</param>
        /// <returns>BoxCollider bounds</returns>
        /// <exception cref="ArgumentNullException">BoxCollider is null</exception>
        public static Bounds CalculateBounds(this BoxCollider boxCollider, Space space = Space.World)
        {
            if (boxCollider == null)
            {
                throw new ArgumentNullException(nameof(boxCollider));
            }

            if (space == Space.Self)
            {
                return new Bounds(boxCollider.center, boxCollider.size);
            }

            if (boxCollider.enabled)
            {
                return boxCollider.bounds;
            }

            Bounds localBounds = new Bounds(boxCollider.center, boxCollider.size);
            return boxCollider.transform.TransformBounds(localBounds);
        }

        /// <summary>
        ///     Gets the bounds in a given space of all MeshRenderers in a set of Transforms.
        /// </summary>
        /// <param name="transforms">Transforms whose MeshRenderers to get the bounds of</param>
        /// <param name="space">Space in which to retrieve the bounds</param>
        /// <param name="includeInactive">Whether to include inactive MeshRenderers</param>
        /// <returns>Bounds containing all MeshRenderers</returns>
        /// <exception cref="ArgumentNullException">transforms is null</exception>
        public static Bounds CalculateBounds(this IEnumerable<Transform> transforms, Space space = Space.World, bool includeInactive = false)
        {
            if (transforms == null)
            {
                throw new ArgumentNullException(nameof(transforms));
            }

            Bounds    bounds         = default;
            Transform firstTransform = null;
            int       nBounds        = 0;

            foreach (Transform t in transforms)
            {
                Bounds b = t.CalculateBounds(Space.World, includeInactive);
                if (nBounds++ == 0)
                {
                    firstTransform = t;
                    bounds         = b;
                }
                else
                {
                    bounds.Encapsulate(b);
                }
            }

            if (bounds == default)
            {
                return default;
            }

            return space == Space.World ? bounds : firstTransform!.InverseTransformBounds(bounds);
        }

        /// <summary>
        ///     Gets the bounds in the given transform's children with the largest squared length of the bounds
        ///     <see cref="Bounds.size" /> vector.
        /// </summary>
        /// <param name="transform">Transform to process all children of</param>
        /// <param name="includeInactive">Whether to include inactive MeshRenderers</param>
        /// <returns>Bounds with the largest squared length size vector</returns>
        /// <exception cref="ArgumentNullException">transform is null</exception>
        public static Bounds GetWiderBoundsInChildren(this Transform transform, bool includeInactive = false)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            Bounds result     = default;
            float  sqrSizeMax = 0f;

            for (int i = 0; i < transform.childCount; ++i)
            {
                Bounds b       = transform.GetChild(i).CalculateBounds(Space.World, includeInactive);
                float  sqrSize = b.size.sqrMagnitude;

                if (b.size.sqrMagnitude > sqrSizeMax)
                {
                    result     = b;
                    sqrSizeMax = sqrSize;
                }
            }

            return result;
        }

        /// <summary>
        ///     Gets the bounds in the given transforms children with the largest squared length of the bounds
        ///     <see cref="Bounds.size" /> vector.
        /// </summary>
        /// <param name="transforms">Transforms to process all children of</param>
        /// <param name="includeInactive">Whether to include inactive MeshRenderers</param>
        /// <returns>Bounds with the largest squared length size vector</returns>
        /// <exception cref="ArgumentNullException">transforms is null</exception>
        public static Bounds GetWiderBounds(this IEnumerable<Transform> transforms, bool includeInactive = false)
        {
            if (transforms == null)
            {
                throw new ArgumentNullException(nameof(transforms));
            }

            Bounds result     = default;
            float  sqrSizeMax = 0f;

            foreach (Transform t in transforms)
            {
                Bounds b       = t.CalculateBounds(Space.World, includeInactive);
                float  sqrSize = b.size.sqrMagnitude;

                if (b.size.sqrMagnitude > sqrSizeMax)
                {
                    result     = b;
                    sqrSizeMax = sqrSize;
                }
            }

            return result;
        }

        /// <summary>
        ///     Transforms a given <see cref="Bounds" /> object in local space using a <see cref="Transform" /> component.
        /// </summary>
        /// <param name="transform">Transform applied to the bounds</param>
        /// <param name="localBounds">Bounds in local space</param>
        /// <returns>Transformed bounds</returns>
        /// <exception cref="ArgumentNullException">transform is null</exception>
        public static Bounds TransformBounds(this Transform transform, Bounds localBounds)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            var center = transform.TransformPoint(localBounds.center);

            // self the local extents' axes
            var extents = localBounds.extents;
            var axisX   = transform.TransformVector(extents.x, 0,         0);
            var axisY   = transform.TransformVector(0,         extents.y, 0);
            var axisZ   = transform.TransformVector(0,         0,         extents.z);

            // sum their absolute value to get the world extents
            extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
            extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
            extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

            return new Bounds { center = center, extents = extents };
        }

        /// <summary>
        ///     Transforms a given <see cref="Bounds" /> object in world space to the local space of a <see cref="Transform" />.
        /// </summary>
        /// <param name="transform">Transform defining the local space where to move the bounds to</param>
        /// <param name="worldBounds">Bounds in world space</param>
        /// <returns>Transformed bounds</returns>
        /// <exception cref="ArgumentNullException">transform is null</exception>
        public static Bounds InverseTransformBounds(this Transform transform, Bounds worldBounds)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            var center = transform.InverseTransformPoint(worldBounds.center);

            // self the local extents' axes
            var extents = worldBounds.extents;
            var axisX   = transform.InverseTransformVector(extents.x, 0,         0);
            var axisY   = transform.InverseTransformVector(0,         extents.y, 0);
            var axisZ   = transform.InverseTransformVector(0,         0,         extents.z);

            // sum their absolute value to get the world extents
            extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
            extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
            extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

            return new Bounds { center = center, extents = extents };
        }

        #endregion
    }
}