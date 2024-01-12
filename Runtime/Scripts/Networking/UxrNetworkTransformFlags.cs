// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrNetworkTransformFlags.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Networking
{
    /// <summary>
    ///     Describes the different elements that can be synchronized by a network transform component.
    /// </summary>
    public enum UxrNetworkTransformFlags
    {
        None = 0,

        // Flags for separate transform components: 

        PositionX = 1 << 0,
        PositionY = 1 << 1,
        PositionZ = 1 << 2,
        RotationX = 1 << 3,
        RotationY = 1 << 4,
        RotationZ = 1 << 5,
        ScaleX    = 1 << 6,
        ScaleY    = 1 << 7,
        ScaleZ    = 1 << 8,

        /// <summary>
        ///     When set, it tells that the transform is a child of another transform that is also tracked. This usually means that
        ///     it doesn't require a NetworkObject, just a NetworkTransform.
        /// </summary>
        ChildTransform = 1 << 31,

        // Composite flags for root transforms:

        Position            = PositionX | PositionY | PositionZ,
        Rotation            = RotationX | RotationY | RotationZ,
        Scale               = ScaleX | ScaleY | ScaleZ,
        PositionAndRotation = Position | Rotation,
        All                 = Position | Rotation | Scale,

        // Composite flags for transforms that have another network transform above:

        ChildRotation            = ChildTransform | RotationX | RotationY | RotationZ,
        ChildScale               = ChildTransform | ScaleX | ScaleY | ScaleZ,
        ChildPositionAndRotation = ChildTransform | Position | Rotation,
        ChildAll                 = ChildTransform | Position | Rotation | Scale
    }
}