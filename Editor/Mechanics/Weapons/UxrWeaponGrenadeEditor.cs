// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrWeaponGrenadeEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Mechanics.Weapons;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Mechanics.Weapons
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrGrenadeWeapon" />.
    /// </summary>
    [CustomEditor(typeof(UxrGrenadeWeapon))]
    [CanEditMultipleObjects]
    public class UxrWeaponGrenadeEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Creates references to the serialized properties
        /// </summary>
        private void OnEnable()
        {
            _propertyActivationTrigger            = serializedObject.FindProperty("_activationTrigger");
            _propertyExplodeOnCollision           = serializedObject.FindProperty("_explodeOnCollision");
            _propertyTimerSeconds                 = serializedObject.FindProperty("_timerSeconds");
            _propertyPin                          = serializedObject.FindProperty("_pin");
            _propertyAudioRemovePin               = serializedObject.FindProperty("_audioRemovePin");
            _propertyHapticRemovePin              = serializedObject.FindProperty("_hapticRemovePin");
            _propertyImpactExplosionCollisionMask = serializedObject.FindProperty("_impactExplosionCollisionMask");
            _propertyExplosionPrefabPool          = serializedObject.FindProperty("_explosionPrefabPool");
            _propertyExplosionPrefabLife          = serializedObject.FindProperty("_explosionPrefabLife");
            _propertyDamageRadius                 = serializedObject.FindProperty("_damageRadius");
            _propertyDamageNear                   = serializedObject.FindProperty("_damageNear");
            _propertyDamageFar                    = serializedObject.FindProperty("_damageFar");
            _propertyCreatePhysicsExplosion       = serializedObject.FindProperty("_createPhysicsExplosion");
            _propertyPhysicsExplosionForce        = serializedObject.FindProperty("_physicsExplosionForce");
        }

        /// <summary>
        ///     Draws the custom inspector and handles user input.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("General Parameters:", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_propertyActivationTrigger,  ContentActivationTrigger);
            EditorGUILayout.PropertyField(_propertyExplodeOnCollision, ContentExplodeOnCollision);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Timer:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_propertyTimerSeconds, ContentTimerSeconds);

            if (_propertyActivationTrigger.enumValueIndex == (int)UxrGrenadeActivationMode.TriggerPin)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Pin:", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(_propertyPin,             ContentPin);
                EditorGUILayout.PropertyField(_propertyAudioRemovePin,  ContentAudioRemovePin,  true);
                EditorGUILayout.PropertyField(_propertyHapticRemovePin, ContentHapticRemovePin, true);
            }

            if (_propertyExplodeOnCollision.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Explode On Collision:", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_propertyImpactExplosionCollisionMask, ContentImpactExplosionCollisionMask);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Explosion:", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_propertyExplosionPrefabPool, ContentExplosionPrefabPool, true);
            EditorGUILayout.PropertyField(_propertyExplosionPrefabLife, ContentExplosionPrefabLife);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Damage:", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_propertyDamageRadius, ContentDamageRadius);
            EditorGUILayout.PropertyField(_propertyDamageNear,   ContentDamageNear);
            EditorGUILayout.PropertyField(_propertyDamageFar,    ContentDamageFar);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Physics:", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_propertyCreatePhysicsExplosion, ContentCreatePhysicsExplosion);

            if (_propertyCreatePhysicsExplosion.boolValue)
            {
                EditorGUILayout.PropertyField(_propertyPhysicsExplosionForce, ContentPhysicsExplosionForce);
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentActivationTrigger            { get; } = new GUIContent("Activation Trigger",       "");
        private GUIContent ContentExplodeOnCollision           { get; } = new GUIContent("Explode On Collision",     "");
        private GUIContent ContentTimerSeconds                 { get; } = new GUIContent("Timer Seconds",            "");
        private GUIContent ContentPin                          { get; } = new GUIContent("Pin Object",               "");
        private GUIContent ContentAudioRemovePin               { get; } = new GUIContent("Audio Remove Pin",         "");
        private GUIContent ContentHapticRemovePin              { get; } = new GUIContent("Haptic Remove Pin",        "");
        private GUIContent ContentImpactExplosionCollisionMask { get; } = new GUIContent("Collision Mask",           "");
        private GUIContent ContentExplosionPrefabPool          { get; } = new GUIContent("Explosion Prefab Pool",    "");
        private GUIContent ContentExplosionPrefabLife          { get; } = new GUIContent("Explosion Prefab Life",    "");
        private GUIContent ContentDamageRadius                 { get; } = new GUIContent("Damage Radius",            "");
        private GUIContent ContentDamageNear                   { get; } = new GUIContent("Damage Near",              "");
        private GUIContent ContentDamageFar                    { get; } = new GUIContent("Damage Far",               "");
        private GUIContent ContentCreatePhysicsExplosion       { get; } = new GUIContent("Create Physics Explosion", "");
        private GUIContent ContentPhysicsExplosionForce        { get; } = new GUIContent("Physics Explosion Force",  "");

        private SerializedProperty _propertyActivationTrigger;
        private SerializedProperty _propertyExplodeOnCollision;
        private SerializedProperty _propertyTimerSeconds;
        private SerializedProperty _propertyPin;
        private SerializedProperty _propertyAudioRemovePin;
        private SerializedProperty _propertyHapticRemovePin;
        private SerializedProperty _propertyImpactExplosionCollisionMask;
        private SerializedProperty _propertyExplosionPrefabPool;
        private SerializedProperty _propertyExplosionPrefabLife;
        private SerializedProperty _propertyDamageRadius;
        private SerializedProperty _propertyDamageNear;
        private SerializedProperty _propertyDamageFar;
        private SerializedProperty _propertyCreatePhysicsExplosion;
        private SerializedProperty _propertyPhysicsExplosionForce;

        #endregion
    }
}