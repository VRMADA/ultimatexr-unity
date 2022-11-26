// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHandIntegrationEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Devices.Visualization;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Avatar
{
    /// <summary>
    ///     Custom editor for <see cref="UxrHandIntegration" />.
    /// </summary>
    [CustomEditor(typeof(UxrHandIntegration))]
    public partial class UxrHandIntegrationEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Caches the serialized properties, the different variations and creates the temporal hand gizmo.
        /// </summary>
        private void OnEnable()
        {
            _propertyGizmoHandSize          = serializedObject.FindProperty("_gizmoHandSize");
            _propertyHandSide               = serializedObject.FindProperty("_handSide");
            _propertyObjectVariationName    = serializedObject.FindProperty("_objectVariationName");
            _propertyMaterialVariationName  = serializedObject.FindProperty("_materialVariationName");
            _propertySelectedRenderPipeline = serializedObject.FindProperty("_selectedRenderPipeline");

            _handIntegration        = serializedObject.targetObject as UxrHandIntegration;
            _objectVariationNames   = GetCommonObjectVariationNames();
            _materialVariationNames = GetCommonMaterialVariationNames(_propertyObjectVariationName.name);

            GetControllerPipelineRenderers(out _renderers, out _renderPipelineVariations, out _renderPipelineVariationStrings, out _selectedRenderPipelineIndex);

            serializedObject.Update();
            _propertySelectedRenderPipeline.intValue = _selectedRenderPipelineIndex;
            serializedObject.ApplyModifiedProperties();

            UxrGrabber[] grabbers = _handIntegration.GetComponentsInChildren<UxrGrabber>();

            foreach (UxrGrabber grabber in grabbers)
            {
                if (grabber.HandRenderer == null)
                {
                    // Do nothing, we just try to force to get the hand renderer reference if it doesn't exist or is inactive
                }
            }

            CreateTemporalObjects();
        }

        /// <summary>
        ///     Destroys the temporal hand gizmo.
        /// </summary>
        private void OnDisable()
        {
            DestroyTemporalObjects();
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            bool variationChanged      = false;
            bool renderPipelineChanged = false;

            // Gizmo hand size (read-only so that it can only be changed in debug mode).

            GUI.enabled = false;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_propertyGizmoHandSize, ContentGizmoHandSize);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                RegenerateTemporalObjects();
            }

            GUI.enabled = true;

            // Hand side

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_propertyHandSide, ContentHandSide);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                RegenerateTemporalObjects();
            }

            // Select object variation

            if (_objectVariationNames.Count > 0)
            {
                EditorGUI.BeginChangeCheck();
                int handIndex = EditorGUILayout.Popup(ContentObjectVariationName, _objectVariationNames.IndexOf(_propertyObjectVariationName.stringValue), UxrEditorUtils.ToGUIContentArray(_objectVariationNames));
                if (EditorGUI.EndChangeCheck())
                {
                    variationChanged                         = true;
                    _propertyObjectVariationName.stringValue = _objectVariationNames[handIndex];
                    serializedObject.ApplyModifiedProperties();
                }

                _materialVariationNames = GetCommonMaterialVariationNames(_propertyObjectVariationName.stringValue);
            }

            // Select material variation

            if (_materialVariationNames.Count > 0)
            {
                EditorGUI.BeginChangeCheck();
                int materialIndex = EditorGUILayout.Popup(ContentMaterialVariationName, _materialVariationNames.IndexOf(_propertyMaterialVariationName.stringValue), UxrEditorUtils.ToGUIContentArray(_materialVariationNames));
                if (EditorGUI.EndChangeCheck())
                {
                    variationChanged                           = true;
                    _propertyMaterialVariationName.stringValue = _materialVariationNames[materialIndex];
                }
            }

            // Select Render Pipeline variation

            if (_renderPipelineVariations.Count > 1)
            {
                EditorGUI.BeginChangeCheck();
                _propertySelectedRenderPipeline.intValue = EditorGUILayout.Popup(ContentRenderPipeline, _propertySelectedRenderPipeline.intValue, UxrEditorUtils.ToGUIContentArray(_renderPipelineVariationStrings));
                if (EditorGUI.EndChangeCheck())
                {
                    renderPipelineChanged        = true;
                    _selectedRenderPipelineIndex = _propertySelectedRenderPipeline.intValue;
                }
            }

            serializedObject.ApplyModifiedProperties();

            // Enable new variation

            if (variationChanged)
            {
                EnableCurrentVariation(_propertyObjectVariationName.stringValue, _propertyMaterialVariationName.stringValue);
            }

            if (renderPipelineChanged)
            {
                EnableCurrentRenderPipeline(_renderers, _renderPipelineVariations[_selectedRenderPipelineIndex]);
            }

            // Alignment to avatar hand

            EditorGUILayout.Space();

            if (UxrEditorUtils.CenteredButton(new GUIContent("Align to avatar")))
            {
                Undo.RegisterCompleteObjectUndo(_handIntegration.transform, "Align hand integration");

                if (!_handIntegration.TryToMatchHand())
                {
                    EditorUtility.DisplayDialog("Missing required bone references", $"The hand integration could not find an {nameof(UxrAvatar)} component up in the hierarchy or the {nameof(UxrAvatar)} is missing finger bone references in the rig section. The finger bones are required to try to align the hand integration with the avatar's hand.", "OK");
                }
            }

            // Check Undo/Redo/Revert:

            if (_propertyGizmoHandSize.enumValueIndex != (int)_gizmoHandSize)
            {
                RegenerateTemporalObjects();
            }

            if (_propertySelectedRenderPipeline.intValue != _selectedRenderPipelineIndex)
            {
                _selectedRenderPipelineIndex = _propertySelectedRenderPipeline.intValue;
                EnableCurrentRenderPipeline(_renderers, _renderPipelineVariations[_selectedRenderPipelineIndex]);
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Called when there is an undo/redo.
        /// </summary>
        private void OnUndoRedo()
        {
            RegenerateTemporalObjects();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets all the common object variation names among the available <see cref="UxrControllerHand" /> components hanging
        ///     from the object.
        /// </summary>
        /// <returns>Common available object variation names</returns>
        private IReadOnlyList<string> GetCommonObjectVariationNames()
        {
            List<string> objectVariationNames = new List<string>();

            if (_handIntegration)
            {
                _controllerHands = _handIntegration.GetComponentsInChildren<UxrControllerHand>(true);
                bool isFirst = true;

                foreach (UxrControllerHand hand in _controllerHands)
                {
                    objectVariationNames = isFirst ? hand.Variations.Select(v => v.Name).ToList() : objectVariationNames.Intersect(hand.Variations.Select(v => v.Name)).ToList();
                    isFirst              = false;
                }
            }

            return objectVariationNames;
        }

        /// <summary>
        ///     Gets all the common material variation names among the available <see cref="UxrControllerHand" /> components
        ///     hanging from the object, for a given object variation.
        /// </summary>
        /// <returns>Common available material variation names for the given object variation</returns>
        private IReadOnlyList<string> GetCommonMaterialVariationNames(string objectVariationName)
        {
            List<string> materialVariationNames = new List<string>();

            if (_handIntegration)
            {
                _controllerHands = _handIntegration.GetComponentsInChildren<UxrControllerHand>(true);
                bool isFirst = true;

                foreach (UxrControllerHand hand in _controllerHands)
                {
                    foreach (UxrControllerHand.ObjectVariation objectVariation in hand.Variations)
                    {
                        if (objectVariation.Name == objectVariationName)
                        {
                            materialVariationNames = isFirst ? objectVariation.MaterialVariations.Select(v => v.Name).ToList() : materialVariationNames.Intersect(objectVariation.MaterialVariations.Select(v => v.Name)).ToList();
                            isFirst                = false;
                        }
                    }
                }
            }

            return materialVariationNames;
        }

        /// <summary>
        ///     Retrieves the available renderers that are part of controllers, the list of available material render pipeline
        ///     variations and the currently selected render pipeline.
        /// </summary>
        /// <param name="renderers">Returns the renderers that are part of controllers, if any</param>
        /// <param name="renderPipelineVariations">Returns the available material render pipeline variations, if any</param>
        /// <param name="renderPipelineVariationStrings">
        ///     Returns the UI strings that <paramref name="renderPipelineVariations" />
        ///     should be mapped with
        /// </param>
        /// <param name="selectedRenderPipelineIndex">
        ///     Returns the index in <paramref name="renderPipelineVariations" /> of the
        ///     render pipeline that was found to be currently enabled
        /// </param>
        private void GetControllerPipelineRenderers(out List<Renderer> renderers, out List<RenderPipeline> renderPipelineVariations, out List<string> renderPipelineVariationStrings, out int selectedRenderPipelineIndex)
        {
            renderers                      = new List<Renderer>();
            renderPipelineVariations       = new List<RenderPipeline>();
            renderPipelineVariationStrings = new List<string>();
            selectedRenderPipelineIndex    = -1;

            if (_handIntegration)
            {
                foreach (Renderer renderer in _handIntegration.GetComponentsInChildren<Renderer>(true))
                {
                    foreach (Material material in renderer.sharedMaterials.Where(material => material != null))
                    {
                        if (material.name.Contains(MaterialUrp))
                        {
                            renderers.Add(renderer);

                            if (!renderPipelineVariations.Contains(RenderPipeline.Urp))
                            {
                                renderPipelineVariations.Add(RenderPipeline.Urp);
                                renderPipelineVariationStrings.Add(UrpUI);
                            }

                            selectedRenderPipelineIndex = renderPipelineVariations.IndexOf(RenderPipeline.Urp);

                            if (!renderPipelineVariations.Contains(RenderPipeline.Brp) && GetRenderPipelineVariant(material, RenderPipeline.Brp))
                            {
                                renderPipelineVariations.Add(RenderPipeline.Brp);
                                renderPipelineVariationStrings.Add(BrpUI);
                            }
                        }
                        else if (material.name.Contains(MaterialBrp))
                        {
                            renderers.Add(renderer);

                            if (!renderPipelineVariations.Contains(RenderPipeline.Brp))
                            {
                                renderPipelineVariations.Add(RenderPipeline.Brp);
                                renderPipelineVariationStrings.Add(BrpUI);
                            }

                            selectedRenderPipelineIndex = renderPipelineVariations.IndexOf(RenderPipeline.Brp);

                            if (!renderPipelineVariations.Contains(RenderPipeline.Urp) && GetRenderPipelineVariant(material, RenderPipeline.Urp))
                            {
                                renderPipelineVariations.Add(RenderPipeline.Urp);
                                renderPipelineVariationStrings.Add(UrpUI);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the given material variation if it exists.
        /// </summary>
        /// <param name="material">Material to get the variation of</param>
        /// <param name="renderPipeline">Render pipeline to get the variation for</param>
        /// <returns>The material variation for the given render pipeline if it exists</returns>
        private Material GetRenderPipelineVariant(Material material, RenderPipeline renderPipeline)
        {
            string materialName = material.name;
            string directory    = Path.GetDirectoryName(AssetDatabase.GetAssetPath(material));

            if (directory == null)
            {
                return null;
            }

            if (renderPipeline == RenderPipeline.Brp)
            {
                if (materialName.Contains(MaterialUrp))
                {
                    return AssetDatabase.LoadAssetAtPath<Material>(Path.Combine(directory, materialName.Replace(MaterialUrp, MaterialBrp)) + ".mat");
                }
            }
            else if (renderPipeline == RenderPipeline.Urp)
            {
                if (materialName.Contains(MaterialBrp))
                {
                    return AssetDatabase.LoadAssetAtPath<Material>(Path.Combine(directory, materialName.Replace(MaterialBrp, MaterialUrp)) + ".mat");
                }
            }

            return null;
        }

        /// <summary>
        ///     Enables the current object/material variation.
        /// </summary>
        /// <param name="objectVariationName">Object variation name</param>
        /// <param name="materialVariationName">Material variation name</param>
        private void EnableCurrentVariation(string objectVariationName, string materialVariationName)
        {
            if (_handIntegration)
            {
                foreach (UxrControllerHand hand in _handIntegration.GetComponentsInChildren<UxrControllerHand>(true))
                {
                    // First pass: Disable all, because the selected variation may share a GameObject with another variation

                    foreach (UxrControllerHand.ObjectVariation objectVariation in hand.Variations)
                    {
                        objectVariation.GameObject.SetActive(false);
                    }

                    // Second pass: Enable selected one and assign material

                    UxrControllerHand.ObjectVariation selectedObjectVariation = hand.Variations.FirstOrDefault(v => v.Name == objectVariationName);

                    if (selectedObjectVariation != null)
                    {
                        selectedObjectVariation.GameObject.SetActive(true);

                        UxrControllerHand.ObjectVariation.MaterialVariation selectedMaterialVariation = selectedObjectVariation.MaterialVariations.FirstOrDefault(v => v.Name == materialVariationName);

                        if (selectedMaterialVariation != null)
                        {
                            foreach (Renderer renderer in selectedObjectVariation.GameObject.GetComponentsInChildren<Renderer>(true))
                            {
                                renderer.sharedMaterial = selectedMaterialVariation.Material;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Enables the materials for the given render pipeline.
        /// </summary>
        /// <param name="renderers">List of renderers whose materials to process</param>
        /// <param name="renderPipeline">Render pipeline to enable</param>
        private void EnableCurrentRenderPipeline(IReadOnlyList<Renderer> renderers, RenderPipeline renderPipeline)
        {
            foreach (Renderer renderer in renderers)
            {
                Material[] sharedMaterials = renderer.sharedMaterials;

                for (int i = 0; i < sharedMaterials.Length; ++i)
                {
                    Material materialVariation = GetRenderPipelineVariant(sharedMaterials[i], renderPipeline);

                    if (materialVariation != null)
                    {
                        sharedMaterials[i] = materialVariation;
                    }
                }

                renderer.sharedMaterials = sharedMaterials;
            }
        }

        /// <summary>
        ///     Regenerates the hand gizmo.
        /// </summary>
        private void RegenerateTemporalObjects()
        {
            DestroyTemporalObjects();
            CreateTemporalObjects();
        }

        /// <summary>
        ///     Creates the hand gizmo.
        /// </summary>
        private void CreateTemporalObjects()
        {
            string handAssetGuid = null;

            if (_propertyGizmoHandSize.enumValueIndex == (int)UxrHandIntegration.GizmoHandSize.Big)
            {
                handAssetGuid  = _handIntegration.HandSide == UxrHandSide.Left ? LeftBigHandAssetGuid : RightBigHandAssetGuid;
                _gizmoHandSize = UxrHandIntegration.GizmoHandSize.Big;
            }
            else if (_propertyGizmoHandSize.enumValueIndex == (int)UxrHandIntegration.GizmoHandSize.Small)
            {
                handAssetGuid  = _handIntegration.HandSide == UxrHandSide.Left ? LeftSmallHandAssetGuid : RightSmallHandAssetGuid;
                _gizmoHandSize = UxrHandIntegration.GizmoHandSize.Small;
            }

            if (!string.IsNullOrEmpty(handAssetGuid))
            {
                GameObject handAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(handAssetGuid));

                if (handAsset == null)
                {
                    return;
                }

                _handGizmoRoot           = Instantiate(handAsset);
                _handGizmoRoot.hideFlags = HideFlags.HideAndDontSave;
                _handGizmoRenderer       = _handGizmoRoot.GetComponentInChildren<Renderer>();

                if (_handGizmoRoot != null)
                {
                    _handGizmoRoot.transform.SetParent(_handIntegration.transform);
                    _handGizmoRoot.transform.SetPositionAndRotation(_handIntegration.transform);
                }
                else
                {
                    Debug.LogWarning("Hand gizmo could not be loaded");
                }
            }
        }

        /// <summary>
        ///     Destroys the hand gizmo.
        /// </summary>
        private void DestroyTemporalObjects()
        {
            if (_handGizmoRoot != null)
            {
                DestroyImmediate(_handGizmoRoot);
            }

            _handGizmoRoot     = null;
            _handGizmoRenderer = null;
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentGizmoHandSize         => new GUIContent("Gizmo Hand Size", "The hand size ");
        private GUIContent ContentHandSide              => new GUIContent("Hand Side");
        private GUIContent ContentObjectVariationName   => new GUIContent("Hand",            "The type of hand that is shown with the controller");
        private GUIContent ContentMaterialVariationName => new GUIContent("Material",        "The hand material");
        private GUIContent ContentRenderPipeline        => new GUIContent("Render Pipeline", "The render pipeline that will be used, so that the correct materials are loaded");

        private const string LeftBigHandAssetGuid    = "93019d606e943b7429d29c694143ad6e";
        private const string RightBigHandAssetGuid   = "d45d3edc285cfd64a86fddd771e13f47";
        private const string LeftSmallHandAssetGuid  = "672e7f52fe868134a80bf396141fe261";
        private const string RightSmallHandAssetGuid = "0a1c1014903295a408b755f1fed2863e";

        private const string MaterialBrp = "_BRP";
        private const string MaterialUrp = "_URP";
        private const string BrpUI       = "Built-in Render Pipeline";
        private const string UrpUI       = "Universal Render Pipeline";

        private SerializedProperty _propertyGizmoHandSize;
        private SerializedProperty _propertyHandSide;
        private SerializedProperty _propertyObjectVariationName;
        private SerializedProperty _propertyMaterialVariationName;
        private SerializedProperty _propertySelectedRenderPipeline;

        private UxrHandIntegration               _handIntegration;
        private UxrHandIntegration.GizmoHandSize _gizmoHandSize;
        private GameObject                       _handGizmoRoot;
        private Renderer                         _handGizmoRenderer;
        private IReadOnlyList<UxrControllerHand> _controllerHands;
        private IReadOnlyList<string>            _objectVariationNames;
        private IReadOnlyList<string>            _materialVariationNames;
        private List<RenderPipeline>             _renderPipelineVariations;
        private List<string>                     _renderPipelineVariationStrings;
        private List<Renderer>                   _renderers;
        private int                              _selectedRenderPipelineIndex;

        #endregion
    }
}