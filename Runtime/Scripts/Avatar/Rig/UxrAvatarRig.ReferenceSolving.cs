// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarRig.ReferenceSolving.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Animation.IK;
using UltimateXR.Core;
using UltimateXR.Extensions.System;
using UltimateXR.Extensions.Unity;
using UltimateXR.Extensions.Unity.Render;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    partial class UxrAvatarRig
    {
        #region Public Methods

        /// <summary>
        ///     Tries to get the <see cref="SkinnedMeshRenderer" /> that represents the given hand.
        /// </summary>
        /// <param name="avatar">Avatar</param>
        /// <param name="handSide">Which hand side to retrieve</param>
        /// <returns>The renderer if found or null</returns>
        public static SkinnedMeshRenderer TryToGetHandRenderer(UxrAvatar avatar, UxrHandSide handSide)
        {
            if (avatar == null)
            {
                return null;
            }

            SkinnedMeshRenderer mostInfluentialSkin = null;
            int                 maxInfluenceCount   = 0;

            foreach (SkinnedMeshRenderer skin in avatar.GetAllAvatarRendererComponents())
            {
                if (!skin.gameObject.activeInHierarchy)
                {
                    continue;
                }

                int influenceCount = 0;

                foreach (Transform bone in avatar.GetHand(handSide).Transforms)
                {
                    influenceCount += MeshExt.GetBoneInfluenceVertexCount(skin, bone);
                }

                if (influenceCount > maxInfluenceCount)
                {
                    maxInfluenceCount   = influenceCount;
                    mostInfluentialSkin = skin;
                }
            }

            return mostInfluentialSkin;
        }

        /// <summary>
        ///     Tries to solve which bones from a <see cref="SkinnedMeshRenderer" /> are remaining parts of the arm that still have
        ///     no references.
        /// </summary>
        /// <param name="arm">Arm to solve</param>
        /// <param name="skin">Source skin to navigate the bones looking for missing elements that are not in the arm</param>
        public static void TryToResolveArm(UxrAvatarArm arm, SkinnedMeshRenderer skin)
        {
            // First top to bottom pass

            bool handResolveTried = false;

            if (arm.Clavicle != null)
            {
                if (handResolveTried == false)
                {
                    TryToResolveHand(arm.Hand, arm.Clavicle, arm.Clavicle, skin);
                    handResolveTried = true;
                }

                if (arm.UpperArm == null)
                {
                    arm.UpperArm = GetNextLimbBoneIfOnlyOne(arm.Clavicle, skin);
                }
            }

            if (arm.UpperArm != null)
            {
                if (handResolveTried == false)
                {
                    TryToResolveHand(arm.Hand, arm.UpperArm, arm.UpperArm, skin);
                    handResolveTried = true;
                }

                if (arm.Forearm == null)
                {
                    arm.Forearm = GetNextLimbBoneIfOnlyOne(arm.UpperArm, skin);
                }
            }
            else
            {
                arm.UpperArm = GetNextLimbBoneIfOnlyOne(arm.Clavicle, skin);
            }

            if (arm.Forearm != null)
            {
                if (handResolveTried == false)
                {
                    TryToResolveHand(arm.Hand, arm.Forearm, arm.Forearm, skin);
                    handResolveTried = true;
                }

                if (arm.Hand.Wrist == null)
                {
                    arm.Hand.Wrist = GetNextLimbBoneIfOnlyOne(arm.Forearm, skin);
                }
            }
            else
            {
                arm.Forearm = GetNextLimbBoneIfOnlyOne(arm.UpperArm, skin);
            }

            if (arm.Hand.Wrist != null)
            {
                TryToResolveHand(arm.Hand, arm.Hand.Wrist, arm.Hand.Wrist, skin);
            }

            // Bottom to top pass

            if (arm.Forearm == null && arm.Hand.Wrist != null)
            {
                arm.Forearm = GetPreviousLimbBoneIfOnlyChild(arm.Hand.Wrist, skin);
            }

            if (arm.UpperArm == null && arm.Forearm != null)
            {
                arm.UpperArm = GetPreviousLimbBoneIfOnlyChild(arm.Forearm, skin);
            }

            if (arm.Clavicle == null && arm.UpperArm != null)
            {
                arm.Clavicle = GetPreviousLimbBoneIfOnlyChild(arm.UpperArm, skin);
            }

            if (arm.UpperArm != null && arm.Hand.Wrist != null && arm.Hand.Wrist.parent != null && arm.Hand.Wrist.parent.parent == arm.UpperArm)
            {
                arm.Forearm = arm.Hand.Wrist.parent;
            }
        }

        /// <summary>
        ///     Tries to solve missing bone elements of a hand using a <see cref="SkinnedMeshRenderer" /> as source.
        /// </summary>
        /// <param name="hand">Hand to resolve</param>
        /// <param name="root">The wrist, root of the hand</param>
        /// <param name="current">
        ///     The current transform being processed. The original call is using the same as
        ///     <paramref name="root" />.
        /// </param>
        /// <param name="skin">Source skin to navigate the bones looking for missing elements that are not in the hand</param>
        /// <returns>Whether the hand was correctly solved</returns>
        public static bool TryToResolveHand(UxrAvatarHand hand, Transform root, Transform current, SkinnedMeshRenderer skin)
        {
            // Try to find 5 fingers hanging from current. If not found, try searching recursively

            if (current == null)
            {
                return false;
            }

            if (hand.HasFingerData())
            {
                return true;
            }

            if (IsBoneInList(skin, root) && IsBoneInList(skin, current))
            {
                List<List<Transform>> handFingerBones = new List<List<Transform>>();

                int fingersFound = 0;

                for (int i = 0; i < current.childCount; ++i)
                {
                    if (CanBeFinger(current.GetChild(i), skin, handFingerBones))
                    {
                        fingersFound++;
                    }
                    else
                    {
                        // Maybe metacarpals are not skinned? look in their children
                        for (int j = 0; j < current.GetChild(i).childCount; ++j)
                        {
                            if (CanBeFinger(current.GetChild(i).GetChild(j), skin, handFingerBones))
                            {
                                fingersFound++;
                            }
                        }
                    }
                }

                if (fingersFound == HandFingerCount)
                {
                    // Now resolve which finger is which. We use the closest finger root bone to the hand bone as the thumb.
                    // From there, we compute the distances from the thumb distal bone to the other finger roots to know index, middle, ring and little.

                    List<Transform> fingerRoots   = new List<Transform>();
                    List<Transform> fingerDistals = new List<Transform>();

                    for (int i = 0; i < fingersFound; ++i)
                    {
                        fingerRoots.Add(handFingerBones[i][0]);
                        fingerDistals.Add(handFingerBones[i].Count == 4 ? handFingerBones[i][3] : handFingerBones[i][2]);
                    }

                    int thumbFinger = ClosestTransformIndex(current, fingerDistals.ToArray());
                    SetupFinger(hand, UxrFingerType.Thumb, handFingerBones, fingerRoots[thumbFinger]);
                    fingerDistals.RemoveAt(thumbFinger);
                    fingerRoots.RemoveAt(thumbFinger);

                    int indexFinger = ClosestTransformIndex(handFingerBones[thumbFinger][2], fingerDistals.ToArray());
                    SetupFinger(hand, UxrFingerType.Index, handFingerBones, fingerRoots[indexFinger]);
                    fingerDistals.RemoveAt(indexFinger);
                    fingerRoots.RemoveAt(indexFinger);

                    int middleFinger = ClosestTransformIndex(handFingerBones[thumbFinger][2], fingerDistals.ToArray());
                    SetupFinger(hand, UxrFingerType.Middle, handFingerBones, fingerRoots[middleFinger]);
                    fingerDistals.RemoveAt(middleFinger);
                    fingerRoots.RemoveAt(middleFinger);

                    int ringFinger = ClosestTransformIndex(handFingerBones[thumbFinger][2], fingerDistals.ToArray());
                    SetupFinger(hand, UxrFingerType.Ring, handFingerBones, fingerRoots[ringFinger]);
                    fingerDistals.RemoveAt(ringFinger);
                    fingerRoots.RemoveAt(ringFinger);

                    SetupFinger(hand, UxrFingerType.Little, handFingerBones, fingerRoots[0]);

                    if (hand.Wrist == null)
                    {
                        hand.Wrist = current;
                    }

                    return true;
                }
            }

            for (int i = 0; i < current.childCount; ++i)
            {
                if (TryToResolveHand(hand, root, current.GetChild(i), skin))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Tries to infer rig elements by doing some checks on names and bone hierarchy.
        ///     This is useful when we have a rig that has no full humanoid avatar set up on its animator .
        /// </summary>
        public static void TryToInferMissingRigElements(UxrAvatarRig rig, IEnumerable<SkinnedMeshRenderer> skins)
        {
            if (rig == null)
            {
                return;
            }
            
            // Head

            if (rig.Head.Neck == null)
            {
                rig.Head.Neck = TryToResolveBoneUniqueOr(skins, "neck");
            }
            if (rig.Head.Head == null)
            {
                rig.Head.Head = TryToResolveBoneUniqueOr(skins, "head");
            }
            if (rig.Head.Jaw == null)
            {
                rig.Head.Jaw = TryToResolveBoneUniqueOr(skins, "jaw");
            }
            if (rig.Head.LeftEye == null)
            {
                rig.Head.LeftEye = TryToResolveBoneUniqueAnd(skins, "eye", "left");
            }
            if (rig.Head.LeftEye == null)
            {
                rig.Head.LeftEye = TryToResolveBoneUniqueAnd(skins, "eye", "l");
            }
            if (rig.Head.RightEye == null)
            {
                rig.Head.RightEye = TryToResolveBoneUniqueAnd(skins, "eye", "right");
            }
            if (rig.Head.RightEye == null)
            {
                rig.Head.RightEye = TryToResolveBoneUniqueAnd(skins, "eye", "r");
            }

            // Arms

            if (rig.LeftArm.Clavicle == null)
            {
                rig.LeftArm.Clavicle = TryToResolveBoneUniqueAnd(skins, "clavicle", "left");
            }
            if (rig.LeftArm.Clavicle == null)
            {
                rig.LeftArm.Clavicle = TryToResolveBoneUniqueAnd(skins, "collarbone", "left");
            }
            if (rig.LeftArm.Clavicle == null)
            {
                rig.LeftArm.Clavicle = TryToResolveBoneUniqueAnd(skins, "clavicle", "l");
            }
            if (rig.LeftArm.Clavicle == null)
            {
                rig.LeftArm.Clavicle = TryToResolveBoneUniqueAnd(skins, "collarbone", "l");
            }
            if (rig.LeftArm.UpperArm == null)
            {
                rig.LeftArm.UpperArm = TryToResolveBoneUniqueAnd(skins, "upper", "left", "arm");
            }
            if (rig.LeftArm.UpperArm == null)
            {
                rig.LeftArm.UpperArm = TryToResolveBoneUniqueAnd(skins, "upper", "l", "arm");
            }
            if (rig.LeftArm.Forearm == null)
            {
                rig.LeftArm.Forearm = TryToResolveBoneUniqueAnd(skins, "forearm", "left");
            }
            if (rig.LeftArm.Forearm == null)
            {
                rig.LeftArm.Forearm = TryToResolveBoneUniqueAnd(skins, "forearm", "l");
            }
            if (rig.LeftArm.Hand.Wrist == null)
            {
                rig.LeftArm.Hand.Wrist = TryToResolveBoneUniqueAnd(skins, "hand", "left");
            }
            if (rig.LeftArm.Hand.Wrist == null)
            {
                rig.LeftArm.Hand.Wrist = TryToResolveBoneUniqueAnd(skins, "wrist", "left");
            }
            if (rig.LeftArm.Hand.Wrist == null)
            {
                rig.LeftArm.Hand.Wrist = TryToResolveBoneUniqueAnd(skins, "hand", "l");
            }
            if (rig.LeftArm.Hand.Wrist == null)
            {
                rig.LeftArm.Hand.Wrist = TryToResolveBoneUniqueAnd(skins, "wrist", "l");
            }

            if (rig.RightArm.Clavicle == null)
            {
                rig.RightArm.Clavicle = TryToResolveBoneUniqueAnd(skins, "clavicle", "right");
            }
            if (rig.RightArm.Clavicle == null)
            {
                rig.RightArm.Clavicle = TryToResolveBoneUniqueAnd(skins, "collarbone", "right");
            }
            if (rig.RightArm.Clavicle == null)
            {
                rig.RightArm.Clavicle = TryToResolveBoneUniqueAnd(skins, "clavicle", "r");
            }
            if (rig.RightArm.Clavicle == null)
            {
                rig.RightArm.Clavicle = TryToResolveBoneUniqueAnd(skins, "collarbone", "r");
            }
            if (rig.RightArm.UpperArm == null)
            {
                rig.RightArm.UpperArm = TryToResolveBoneUniqueAnd(skins, "upper", "right", "arm");
            }
            if (rig.RightArm.UpperArm == null)
            {
                rig.RightArm.UpperArm = TryToResolveBoneUniqueAnd(skins, "upper", "r", "arm");
            }
            if (rig.RightArm.Forearm == null)
            {
                rig.RightArm.Forearm = TryToResolveBoneUniqueAnd(skins, "forearm", "right");
            }
            if (rig.RightArm.Forearm == null)
            {
                rig.RightArm.Forearm = TryToResolveBoneUniqueAnd(skins, "forearm", "r");
            }
            if (rig.RightArm.Hand.Wrist == null)
            {
                rig.RightArm.Hand.Wrist = TryToResolveBoneUniqueAnd(skins, "hand", "right");
            }
            if (rig.RightArm.Hand.Wrist == null)
            {
                rig.RightArm.Hand.Wrist = TryToResolveBoneUniqueAnd(skins, "wrist", "right");
            }
            if (rig.RightArm.Hand.Wrist == null)
            {
                rig.RightArm.Hand.Wrist = TryToResolveBoneUniqueAnd(skins, "hand", "r");
            }
            if (rig.RightArm.Hand.Wrist == null)
            {
                rig.RightArm.Hand.Wrist = TryToResolveBoneUniqueAnd(skins, "wrist", "r");
            }
                
            // Try to resolve arms using topology

            foreach (SkinnedMeshRenderer skin in skins)
            {
                TryToResolveArm(rig._leftArm,  skin);
                TryToResolveArm(rig._rightArm, skin);
            }

            // Legs

            if (rig.LeftLeg.UpperLeg == null)
            {
                rig.LeftLeg.UpperLeg = TryToResolveBoneUniqueAnd(skins, "upper", "left", "leg");
            }
            if (rig.LeftLeg.UpperLeg == null)
            {
                rig.LeftLeg.UpperLeg = TryToResolveBoneUniqueAnd(skins, "leg", "left");
            }
            if (rig.LeftLeg.UpperLeg == null)
            {
                rig.LeftLeg.UpperLeg = TryToResolveBoneUniqueAnd(skins, "thigh", "left");
            }
            if (rig.LeftLeg.UpperLeg == null)
            {
                rig.LeftLeg.UpperLeg = TryToResolveBoneUniqueAnd(skins, "upper", "l", "leg");
            }
            if (rig.LeftLeg.UpperLeg == null)
            {
                rig.LeftLeg.UpperLeg = TryToResolveBoneUniqueAnd(skins, "leg", "l");
            }
            if (rig.LeftLeg.UpperLeg == null)
            {
                rig.LeftLeg.UpperLeg = TryToResolveBoneUniqueAnd(skins, "thigh", "l");
            }
            if (rig.LeftLeg.LowerLeg == null)
            {
                rig.LeftLeg.LowerLeg = TryToResolveBoneUniqueAnd(skins, "lower", "left", "leg");
            }
            if (rig.LeftLeg.LowerLeg == null)
            {
                rig.LeftLeg.LowerLeg = TryToResolveBoneUniqueAnd(skins, "calf", "left");
            }
            if (rig.LeftLeg.LowerLeg == null)
            {
                rig.LeftLeg.LowerLeg = TryToResolveBoneUniqueAnd(skins, "lower", "l", "leg");
            }
            if (rig.LeftLeg.LowerLeg == null)
            {
                rig.LeftLeg.LowerLeg = TryToResolveBoneUniqueAnd(skins, "calf", "l");
            }
            if (rig.LeftLeg.Foot == null)
            {
                rig.LeftLeg.Foot = TryToResolveBoneUniqueAnd(skins, "foot", "left");
            }
            if (rig.LeftLeg.Foot == null)
            {
                rig.LeftLeg.Foot = TryToResolveBoneUniqueAnd(skins, "ankle", "left");
            }
            if (rig.LeftLeg.Foot == null)
            {
                rig.LeftLeg.Foot = TryToResolveBoneUniqueAnd(skins, "foot", "l");
            }
            if (rig.LeftLeg.Foot == null)
            {
                rig.LeftLeg.Foot = TryToResolveBoneUniqueAnd(skins, "ankle", "l");
            }
            if (rig.LeftLeg.Toes == null)
            {
                rig.LeftLeg.Toes = TryToResolveBoneUniqueAnd(skins, "toe", "left");
            }
            if (rig.LeftLeg.Toes == null)
            {
                rig.LeftLeg.Toes = TryToResolveBoneUniqueAnd(skins, "toe", "l");
            }

            if (rig.RightLeg.UpperLeg == null)
            {
                rig.RightLeg.UpperLeg = TryToResolveBoneUniqueAnd(skins, "upper", "right", "leg");
            }
            if (rig.RightLeg.UpperLeg == null)
            {
                rig.RightLeg.UpperLeg = TryToResolveBoneUniqueAnd(skins, "leg", "right");
            }
            if (rig.RightLeg.UpperLeg == null)
            {
                rig.RightLeg.UpperLeg = TryToResolveBoneUniqueAnd(skins, "thigh", "right");
            }
            if (rig.RightLeg.UpperLeg == null)
            {
                rig.RightLeg.UpperLeg = TryToResolveBoneUniqueAnd(skins, "upper", "r", "leg");
            }
            if (rig.RightLeg.UpperLeg == null)
            {
                rig.RightLeg.UpperLeg = TryToResolveBoneUniqueAnd(skins, "leg", "r");
            }
            if (rig.RightLeg.UpperLeg == null)
            {
                rig.RightLeg.UpperLeg = TryToResolveBoneUniqueAnd(skins, "thigh", "r");
            }
            if (rig.RightLeg.LowerLeg == null)
            {
                rig.RightLeg.LowerLeg = TryToResolveBoneUniqueAnd(skins, "lower", "right", "leg");
            }
            if (rig.RightLeg.LowerLeg == null)
            {
                rig.RightLeg.LowerLeg = TryToResolveBoneUniqueAnd(skins, "calf", "right");
            }
            if (rig.RightLeg.LowerLeg == null)
            {
                rig.RightLeg.LowerLeg = TryToResolveBoneUniqueAnd(skins, "lower", "r", "leg");
            }
            if (rig.RightLeg.LowerLeg == null)
            {
                rig.RightLeg.LowerLeg = TryToResolveBoneUniqueAnd(skins, "calf", "r");
            }
            if (rig.RightLeg.Foot == null)
            {
                rig.RightLeg.Foot = TryToResolveBoneUniqueAnd(skins, "foot", "right");
            }
            if (rig.RightLeg.Foot == null)
            {
                rig.RightLeg.Foot = TryToResolveBoneUniqueAnd(skins, "ankle", "right");
            }
            if (rig.RightLeg.Foot == null)
            {
                rig.RightLeg.Foot = TryToResolveBoneUniqueAnd(skins, "foot", "r");
            }
            if (rig.RightLeg.Foot == null)
            {
                rig.RightLeg.Foot = TryToResolveBoneUniqueAnd(skins, "ankle", "r");
            }
            if (rig.RightLeg.Toes == null)
            {
                rig.RightLeg.Toes = TryToResolveBoneUniqueAnd(skins, "toe", "right");
            }
            if (rig.RightLeg.Toes == null)
            {
                rig.RightLeg.Toes = TryToResolveBoneUniqueAnd(skins, "toe", "r");
            }
            
            // Hips/pelvis

            if (rig.Hips == null)
            {
                rig._hips = TryToResolveBoneUniqueOr(skins, "hips", "pelvis");
            }

            // Try to resolve spine-chest using topology

            Transform spineTop    = rig.Head.Neck != null ? rig.Head.Neck : rig.Head.Head;
            Transform spineBottom = rig.Hips;

            if (spineTop != null)
            {
                if (rig.UpperChest == null && spineTop.parent != spineBottom)
                {
                    rig._upperChest = spineTop.parent;
                }

                if (rig.Chest == null && rig.UpperChest != null && rig.UpperChest.parent != null && rig.UpperChest.parent != spineBottom)
                {
                    rig._chest = rig.UpperChest.parent;
                }

                if (rig.Spine == null && rig.Chest != null && rig.Chest.parent != null && rig.Chest.parent != spineBottom)
                {
                    rig._spine = rig.Chest.parent;
                }
            }

            // Try to resolve hips-spine-chest using name

            if (rig.UpperChest == null)
            {
                rig._upperChest = TryToResolveBoneUniqueOr(skins, "upperchest");
            }
            if (rig.Chest == null)
            {
                rig._chest = TryToResolveBoneUniqueOr(skins, "chest");
            }
            if (rig.Spine == null)
            {
                rig._spine = TryToResolveBoneUniqueOr(skins, "spine");
            }
            
            // Try to find wrist torsion elements

            foreach (UxrAvatarArm arm in rig.GetArms())
            {
                if (arm.Forearm != null && arm.Hand != null)
                {
                    UxrWristTorsionIKSolver[] wristTorsionSolvers = arm.Forearm.GetComponentsInChildren<UxrWristTorsionIKSolver>();

                    if (wristTorsionSolvers.Length == 0)
                    {
                        List<Transform> forearmChildren = new List<Transform>();
                        arm.Forearm.GetAllChildren(ref forearmChildren);
                        
                        // Find nodes that can potentially be for progressive forearm twist 

                        IEnumerable<Transform> torsionNodes = forearmChildren.Where(t => !t.HasParent(arm.Hand.Wrist) && t.name.ToLower().Contains("torsion"));

                        if (!torsionNodes.Any())
                        {
                            torsionNodes = forearmChildren.Where(t => !t.HasParent(arm.Hand.Wrist) && t.name.ToLower().Contains("twist"));
                        }
                        
                        // Create components and assign torsion amount to found nodes. Be careful to distinguish between nodes hanging from the same
                        // parent and nodes in a hierarchy because the torsion amount will be inherited.

                        foreach (Transform torsionNode in torsionNodes)
                        {
                            UxrWristTorsionIKSolver              newTorsionSolver = torsionNode.gameObject.AddComponent<UxrWristTorsionIKSolver>();
                            IEnumerable<UxrWristTorsionIKSolver> parentSolvers    = torsionNodes.Where(t => t != torsionNode && t.HasChild(torsionNode)).Select(t => t.GetComponent<UxrWristTorsionIKSolver>()).Where(s => s != null);

                            float totalAmount = Vector3.Distance(torsionNode.position, arm.Forearm.position) / Vector3.Distance(arm.Hand.Wrist.position, arm.Forearm.position);
                            float netAmount   = Mathf.Max(0.0f, totalAmount - parentSolvers.Sum(s => s.Amount));

                            newTorsionSolver.Amount = netAmount;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Tries to sets up all rig elements from the <see cref="Animator" /> of a humanoid model.
        /// </summary>
        /// <param name="rig">Rig to set</param>
        /// <param name="animator">Source to get the rig elements from</param>
        /// <returns>Whether the animator contained humanoid data</returns>
        public static bool SetupRigElementsFromAnimator(UxrAvatarRig rig, Animator animator)
        {
            if (animator == null || animator.isHuman == false)
            {
                return false;
            }

            // Head

            rig.Head.LeftEye  = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            rig.Head.RightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);
            rig.Head.Jaw      = animator.GetBoneTransform(HumanBodyBones.Jaw);
            rig.Head.Neck     = animator.GetBoneTransform(HumanBodyBones.Neck);
            rig.Head.Head     = animator.GetBoneTransform(HumanBodyBones.Head);

            // Body

            rig._upperChest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            rig._chest      = animator.GetBoneTransform(HumanBodyBones.Chest);
            rig._spine      = animator.GetBoneTransform(HumanBodyBones.Spine);
            rig._hips       = animator.GetBoneTransform(HumanBodyBones.Hips);

            // Left arm

            rig.LeftArm.Clavicle = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            rig.LeftArm.UpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rig.LeftArm.Forearm  = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);

            rig.LeftArm.Hand.Wrist = animator.GetBoneTransform(HumanBodyBones.LeftHand);

            rig.LeftArm.Hand.Thumb.Proximal     = animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
            rig.LeftArm.Hand.Thumb.Intermediate = animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
            rig.LeftArm.Hand.Thumb.Distal       = animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal);

            rig.LeftArm.Hand.Index.Proximal     = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
            rig.LeftArm.Hand.Index.Intermediate = animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
            rig.LeftArm.Hand.Index.Distal       = animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);

            rig.LeftArm.Hand.Middle.Proximal     = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
            rig.LeftArm.Hand.Middle.Intermediate = animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
            rig.LeftArm.Hand.Middle.Distal       = animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);

            rig.LeftArm.Hand.Ring.Proximal     = animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
            rig.LeftArm.Hand.Ring.Intermediate = animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
            rig.LeftArm.Hand.Ring.Distal       = animator.GetBoneTransform(HumanBodyBones.LeftRingDistal);

            rig.LeftArm.Hand.Little.Proximal     = animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
            rig.LeftArm.Hand.Little.Intermediate = animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
            rig.LeftArm.Hand.Little.Distal       = animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal);

            // Right arm

            rig.RightArm.Clavicle   = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            rig.RightArm.UpperArm   = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            rig.RightArm.Forearm    = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            rig.RightArm.Hand.Wrist = animator.GetBoneTransform(HumanBodyBones.RightHand);

            rig.RightArm.Hand.Thumb.Proximal     = animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
            rig.RightArm.Hand.Thumb.Intermediate = animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
            rig.RightArm.Hand.Thumb.Distal       = animator.GetBoneTransform(HumanBodyBones.RightThumbDistal);

            rig.RightArm.Hand.Index.Proximal     = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
            rig.RightArm.Hand.Index.Intermediate = animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
            rig.RightArm.Hand.Index.Distal       = animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);

            rig.RightArm.Hand.Middle.Proximal     = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
            rig.RightArm.Hand.Middle.Intermediate = animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
            rig.RightArm.Hand.Middle.Distal       = animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);

            rig.RightArm.Hand.Ring.Proximal     = animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
            rig.RightArm.Hand.Ring.Intermediate = animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
            rig.RightArm.Hand.Ring.Distal       = animator.GetBoneTransform(HumanBodyBones.RightRingDistal);

            rig.RightArm.Hand.Little.Proximal     = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);
            rig.RightArm.Hand.Little.Intermediate = animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
            rig.RightArm.Hand.Little.Distal       = animator.GetBoneTransform(HumanBodyBones.RightLittleDistal);

            // Left leg

            rig.LeftLeg.UpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            rig.LeftLeg.LowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            rig.LeftLeg.Foot     = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rig.LeftLeg.Toes     = animator.GetBoneTransform(HumanBodyBones.LeftToes);

            // Right leg

            rig.RightLeg.UpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            rig.RightLeg.LowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            rig.RightLeg.Foot     = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            rig.RightLeg.Toes     = animator.GetBoneTransform(HumanBodyBones.RightToes);

            return true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Tries to fine a bone with a unique name in the hierarchy.
        /// </summary>
        /// <param name="skins">Skins with the bones where to look for</param>
        /// <param name="name">
        ///     The name ignoring uppercase/lowercase. It will either look for a unique bone that is exactly the
        ///     name, ends with the name or contains the name but always uniquely.
        /// </param>
        /// <param name="alternatives">Different alternative names to use in case <paramref name="name" /> isn't found</param>
        /// <returns>The transform or null if it wasn't found or there were two or more candidates</returns>
        private static Transform TryToResolveBoneUniqueOr(IEnumerable<SkinnedMeshRenderer> skins, string name, params string[] alternatives)
        {
            TryToResolveNonControllerHandBoneUniqueOr(skins, out Transform candidate, out int candidateOccurrences, name, alternatives);

            return candidate != null && candidateOccurrences == 1 ? candidate : null;
        }

        /// <summary>
        ///     Tries to fine a bone with a matching or similar name in the hierarchy.
        /// </summary>
        /// <param name="skins">Skins with the bones where to look for</param>
        /// <param name="candidate">Returns the most significant candidate</param>
        /// <param name="candidateCount">Returns the total number of candidates with same value that were found</param>
        /// <param name="name">
        ///     The name ignoring uppercase/lowercase. It will either look for a unique bone that is exactly the
        ///     name, ends with the name or contains the name but always uniquely.
        /// </param>
        /// <param name="alternatives">Different alternative names to use in case <paramref name="name" /> isn't found</param>
        private static void TryToResolveNonControllerHandBoneUniqueOr(IEnumerable<SkinnedMeshRenderer> skins, out Transform candidate, out int candidateCount, string name, params string[] alternatives)
        {
            candidate      = null;
            candidateCount = 0;

            void UpdateCandidate(bool isFullMatch, Transform bone, ref Transform currentCandidate, ref int currentCount, ref int currentFullMatchCount)
            {
                if (bone == currentCandidate)
                {
                    return;
                }

                if (isFullMatch)
                {
                    // Full match

                    if (currentFullMatchCount > 0 && currentCandidate.HasParent(bone))
                    {
                        // Favor parent: use candidate and reset counter
                        currentFullMatchCount = 1;
                        currentCount          = 1;
                        currentCandidate      = bone;
                    }
                    else if (!(currentCandidate != null && currentCandidate.HasChild(bone)))
                    {
                        // No parent candidate present: use candidate but increment counter
                        currentFullMatchCount++;
                        currentCount     = currentFullMatchCount;
                        currentCandidate = bone;
                    }
                }
                else if (currentFullMatchCount == 0)
                {
                    // Semi match

                    if (currentCandidate != null && currentCandidate.HasParent(bone))
                    {
                        // Favor parent: use candidate and reset counter
                        currentFullMatchCount = 0;
                        currentCount          = 1;
                        currentCandidate      = bone;
                    }
                    else if (!(currentCandidate != null && currentCandidate.HasChild(bone)))
                    {
                        // No parent candidate present: use candidate but increment counter
                        currentCount++;
                        currentCandidate = bone;
                    }
                }
            }

            Dictionary<Transform, int> dictionaryProcessed     = new Dictionary<Transform, int>();
            int                        candidateFullMatchCount = 0; // Number of times it ends exactly with <name>. We will treat this differently than if it's somewhere in between

            foreach (SkinnedMeshRenderer skin in skins)
            {
                // We append also all the children to the bones. This is necessary because sometimes a node is not in the bone list
                // but still will drive the child bone due to being the parent.
                // A dictionary will make sure that we don't process the same node twice.

                List<Transform> allChildren = new List<Transform>();

                if (skin.rootBone != null)
                {
                    allChildren.Add(skin.rootBone);
                    skin.rootBone.GetAllChildren(ref allChildren);
                }

                foreach (Transform bone in allChildren)
                {
                    if (dictionaryProcessed.ContainsKey(bone))
                    {
                        continue;
                    }

                    dictionaryProcessed.Add(bone, 1);

                    // Look for name or alternatives

                    string nameToLower = bone.name.ToLower();

                    if (IsWordEnd(nameToLower, name.ToLower()))
                    {
                        UpdateCandidate(true, bone, ref candidate, ref candidateCount, ref candidateFullMatchCount);
                        continue;
                    }

                    if (nameToLower.Contains(name.ToLower()))
                    {
                        UpdateCandidate(false, bone, ref candidate, ref candidateCount, ref candidateFullMatchCount);
                        continue;
                    }

                    foreach (string altName in alternatives)
                    {
                        if (IsWordEnd(nameToLower, altName.ToLower()))
                        {
                            UpdateCandidate(true, bone, ref candidate, ref candidateCount, ref candidateFullMatchCount);
                        }
                        else if (nameToLower.Contains(altName.ToLower()))
                        {
                            UpdateCandidate(false, bone, ref candidate, ref candidateCount, ref candidateFullMatchCount);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Checks if the given 'name' contains 'part' as a word. We consider 'part' as a word in 'name' if:
        ///     <list type="bullet">
        ///         <item>'name' ends with 'part'.</item>
        ///         <item>
        ///             'name' contains 'part' and 'part' has a separator character next to it. We consider a separator any
        ///             character that is not a letter or digit.
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="name">String to process</param>
        /// <param name="part">String that should be a part</param>
        /// <returns>Whether <paramref name="name" /> meets the requirements</returns>
        private static bool IsWordEnd(string name, string part)
        {
            if (name.Contains(part))
            {
                if (name.EndsWith(part))
                {
                    return true;
                }

                int pos = name.IndexOf(part);

                return pos != -1 && !char.IsLetterOrDigit(name[pos + part.Length]);
            }

            return false;
        }

        /// <summary>
        ///     Tries to fine a bone with a unique name in the hierarchy.
        /// </summary>
        /// <param name="skins">Skins with the bones where to look for</param>
        /// <param name="name">
        ///     The name ignoring uppercase/lowercase. It will either look for a unique bone that is exactly the
        ///     name, ends with the name or contains the name but always uniquely.
        /// </param>
        /// <param name="additionalStrings">
        ///     Additional strings that also need to meet the same requirement as
        ///     <paramref name="name" />.
        /// </param>
        /// <returns>The transform or null if it wasn't found or there were two or more candidates</returns>
        private static Transform TryToResolveBoneUniqueAnd(IEnumerable<SkinnedMeshRenderer> skins, string name, params string[] additionalStrings)
        {
            TryToResolveNonControllerHandBoneUniqueAnd(skins, out Transform candidate, out int candidateCount, name, additionalStrings);

            return candidate != null && candidateCount == 1 ? candidate : null;
        }

        /// <summary>
        ///     Tries to fine a bone with a matching or similar name in the hierarchy and, optionally, additional strings that are
        ///     all also required to be part of the name.
        /// </summary>
        /// <param name="skins">Skins with the bones where to look for</param>
        /// <param name="candidate">Returns the most significant candidate</param>
        /// <param name="candidateCount">Returns the total number of candidates with same value that were found</param>
        /// <param name="name">
        ///     The name ignoring uppercase/lowercase. It will either look for a unique bone that is exactly the
        ///     name, ends with the name or contains the name.
        /// </param>
        /// <param name="additionalStrings">
        ///     Additional strings that also need to meet the same requirement as
        ///     <paramref name="name" />.
        /// </param>
        private static void TryToResolveNonControllerHandBoneUniqueAnd(IEnumerable<SkinnedMeshRenderer> skins, out Transform candidate, out int candidateCount, string name, params string[] additionalStrings)
        {
            candidate      = null;
            candidateCount = 0;

            int maxOccurrences = 0;

            Dictionary<Transform, int> dictionaryProcessed = new Dictionary<Transform, int>();

            bool CheckIfNameMeetsRequirements(string nodeName, out int foundOccurrences)
            {
                foundOccurrences = nodeName.GetOccurrenceCount(name, false);

                if (foundOccurrences == 0)
                {
                    return false;
                }

                // Look for additional strings that we need to look for and are mandatory

                foreach (string additionalString in additionalStrings)
                {
                    int additionalOccurrences = nodeName.GetOccurrenceCount(additionalString, false);

                    if (additionalOccurrences == 0)
                    {
                        return false;
                    }

                    foundOccurrences += additionalOccurrences;
                }

                return true;
            }

            foreach (SkinnedMeshRenderer skin in skins)
            {
                // We append also all the children to the bones. This is necessary because sometimes a node is not in the bone list
                // but still will drive the child bone due to being the parent.
                // A dictionary will make sure that we don't process the same node twice.

                List<Transform> allChildren = new List<Transform>();

                if (skin.rootBone != null)
                {
                    allChildren.Add(skin.rootBone);
                    skin.rootBone.GetAllChildren(ref allChildren);
                }

                foreach (Transform bone in allChildren)
                {
                    if (dictionaryProcessed.ContainsKey(bone))
                    {
                        continue;
                    }

                    dictionaryProcessed.Add(bone, 1);

                    // Find occurrences of the given name

                    if (!CheckIfNameMeetsRequirements(bone.name, out int occurrences))
                    {
                        continue;
                    }

                    // Update the candidate

                    if (candidate == null)
                    {
                        candidate      = bone;
                        candidateCount = 1;
                        maxOccurrences = occurrences;
                    }
                    else if (candidate.HasParent(bone))
                    {
                        candidate      = bone;
                        candidateCount = 1;
                        maxOccurrences = occurrences;
                    }
                    else if (bone.HasParent(candidate))
                    {
                        // Do nothing, we keep the parent.
                    }
                    else
                    {
                        if (occurrences > maxOccurrences)
                        {
                            candidate      = bone;
                            candidateCount = 1;
                            maxOccurrences = occurrences;
                        }
                        else if (occurrences == maxOccurrences)
                        {
                            candidateCount++;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Sets up a finger.
        /// </summary>
        /// <param name="hand">Hand the finger is part of</param>
        /// <param name="fingerType">The finger type</param>
        /// <param name="handFingerBones">The list of finger bones in the hand</param>
        /// <param name="fingerRootBone">The root bone of all fingers</param>
        private static void SetupFinger(UxrAvatarHand hand, UxrFingerType fingerType, List<List<Transform>> handFingerBones, Transform fingerRootBone)
        {
            foreach (List<Transform> fingerBones in handFingerBones)
            {
                if (fingerBones[0] == fingerRootBone)
                {
                    switch (fingerType)
                    {
                        case UxrFingerType.Thumb:
                            hand.Thumb.SetupFingerBones(fingerBones);
                            break;

                        case UxrFingerType.Index:
                            hand.Index.SetupFingerBones(fingerBones);
                            break;

                        case UxrFingerType.Middle:
                            hand.Middle.SetupFingerBones(fingerBones);
                            break;

                        case UxrFingerType.Ring:
                            hand.Ring.SetupFingerBones(fingerBones);
                            break;

                        case UxrFingerType.Little:
                            hand.Little.SetupFingerBones(fingerBones);
                            break;

                        case UxrFingerType.None: break;
                    }
                }
            }
        }

        /// <summary>
        ///     Checks whether the given bone can be the root bone of a finger.
        /// </summary>
        /// <param name="fingerRootCandidate">The root bone candidate</param>
        /// <param name="skin">The skin where the bones are</param>
        /// <param name="handFingerBones">
        ///     If the given bone can be the root bone of a finger, a list of finger bones are added to
        ///     the list
        /// </param>
        /// <returns>Whether the given bone can be the root bone of a finger</returns>
        private static bool CanBeFinger(Transform fingerRootCandidate, SkinnedMeshRenderer skin, List<List<Transform>> handFingerBones)
        {
            if (IsBoneInList(skin, fingerRootCandidate))
            {
                // fingerRootCandidate is a bone. Now we will enumerate all nodes without children starting from fingerRootCandidate and try
                // to find 3 consecutive parents going upwards ending at fingerRootCandidate or any of its sub-hierarchy nodes.
                // If found, we will consider this a finger. It may or may not end up having fingerRootCandidate as proximalBone.

                List<Transform> potentialFingerDistalBones = new List<Transform>();
                fingerRootCandidate.GetTransformsWithoutChildren(ref potentialFingerDistalBones);

                for (int candidate = 0; candidate < potentialFingerDistalBones.Count; ++candidate)
                {
                    Transform distalCandidate = potentialFingerDistalBones[candidate];

                    while ((!IsBoneInList(skin, distalCandidate) || MeshExt.GetBoneInfluenceVertexCount(skin, distalCandidate, 0.01f) == 0) && distalCandidate != fingerRootCandidate)
                    {
                        distalCandidate = distalCandidate.parent;
                    }
                    
                    if (distalCandidate != fingerRootCandidate)
                    {
                        if (distalCandidate.parent != fingerRootCandidate && distalCandidate.parent != null)
                        {
                            Transform       intermediateCandidate = distalCandidate.parent;
                            List<Transform> fingerBones           = new List<Transform>();

                            if (intermediateCandidate.parent != fingerRootCandidate && intermediateCandidate.parent != null)
                            {
                                // Metacarpal
                                fingerBones.Add(intermediateCandidate.parent.parent);
                            }

                            fingerBones.Add(intermediateCandidate.parent);
                            fingerBones.Add(intermediateCandidate);
                            fingerBones.Add(distalCandidate);

                            handFingerBones.Add(fingerBones);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets the next bone in a hierarchical chain of bones.
        /// </summary>
        /// <param name="bone">The bone where the search will start</param>
        /// <param name="skin">The SkinnedMeshRenderer the bones are for</param>
        /// <param name="lastInChain">
        ///     If true, the search will look for a bone next to the specified one that
        ///     has no other child bones part of the skin. If false, the search will look for a bone next to
        ///     the specified one that has another child bone -ond only one- part of the skin.
        /// </param>
        /// <returns>Bone if it meets the requirements or null if not</returns>
        private static Transform GetNextLimbBoneIfOnlyOne(Transform bone, SkinnedMeshRenderer skin, bool lastInChain = false)
        {
            if (bone != null)
            {
                int childBones     = 0;
                int childBoneIndex = -1;

                if (IsBoneInList(skin, bone))
                {
                    // Check childs in bone

                    for (int i = 0; i < bone.childCount; ++i)
                    {
                        Transform childBone       = bone.GetChild(i);
                        int       childChildBones = 0;

                        // Check childs in child

                        for (int j = 0; j < childBone.childCount; ++j)
                        {
                            if (IsBoneInList(skin, childBone.GetChild(j)))
                            {
                                childChildBones++;
                            }
                        }

                        // Does this child meet the conditions?

                        if (IsBoneInList(skin, childBone))
                        {
                            if ((lastInChain && childChildBones == 0) || (!lastInChain && childChildBones > 0))
                            {
                                childBones++;
                                childBoneIndex = i;
                            }
                        }
                    }
                }

                if (childBones == 1)
                {
                    return bone.GetChild(childBoneIndex);
                }
                
                // Try another approach. Look for non-skinned bones.

                if (bone.childCount == 1)
                {
                    if (lastInChain && bone.GetChild(0).childCount == 0)
                    {
                        return bone.GetChild(0);
                    }

                    if (!lastInChain && bone.GetChild(0).childCount > 0)
                    {
                        return bone.GetChild(0);
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the parent of a bone in the hierarchy if it is also part of the bones in a skin and if the bone is the
        ///     parent's only child bone.
        /// </summary>
        /// <param name="bone">Bone to get the parent from</param>
        /// <param name="skin">Skin where the bones are</param>
        /// <returns>Gets the parent of the bone if it meets the requirements or null if not</returns>
        private static Transform GetPreviousLimbBoneIfOnlyChild(Transform bone, SkinnedMeshRenderer skin)
        {
            if (bone != null)
            {
                if (bone.parent != null && IsBoneInList(skin, bone.parent))
                {
                    int childBones = 0;

                    for (int i = 0; i < bone.parent.childCount; ++i)
                    {
                        if (IsBoneInList(skin, bone.parent.GetChild(i)) || bone.parent.GetChild(i) == bone)
                        {
                            childBones++;
                        }
                    }

                    if (childBones == 1)
                    {
                        return bone.parent;
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the index of the closest bone in a list to a reference.
        /// </summary>
        /// <param name="bone">Reference bone</param>
        /// <param name="otherBones">List where to find the closest bone</param>
        /// <returns>Index of the closest bone in the list or -1 if the list is empty</returns>
        private static int ClosestTransformIndex(Transform bone, params Transform[] otherBones)
        {
            if (otherBones.Length == 0)
            {
                return -1;
            }

            int   closestIndex = 0;
            float minDistance  = Vector3.Distance(bone.transform.position, otherBones[0].transform.position);

            for (int i = 1; i < otherBones.Length; ++i)
            {
                float distance = Vector3.Distance(bone.transform.position, otherBones[i].transform.position);

                if (distance < minDistance)
                {
                    minDistance  = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        /// <summary>
        ///     Checks whether a transform is in the list of bones of a <see cref="SkinnedMeshRenderer" />.
        /// </summary>
        /// <param name="skin">Skin where to look</param>
        /// <param name="transformToCheck">Bone to check for</param>
        /// <returns>Whether the bone was found in the skin</returns>
        private static bool IsBoneInList(SkinnedMeshRenderer skin, Transform transformToCheck)
        {
            if (transformToCheck.name.Contains("ignore", System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            foreach (Transform bone in skin.bones)
            {
                if (bone == transformToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}