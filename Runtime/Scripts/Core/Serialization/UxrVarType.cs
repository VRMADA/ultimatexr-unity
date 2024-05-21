// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrVarType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.StateSync;

namespace UltimateXR.Core.Serialization
{
    /// <summary>
    ///     Enumerates the variable types supported by serialization, for example
    ///     <see cref="UxrPropertyChangedSyncEventArgs" />.
    /// </summary>
    public enum UxrVarType
    {
        Unknown = 0,

        // C#

        Bool         = 1,
        SignedByte   = 2,
        Byte         = 3,
        Char         = 4,
        Int          = 5,
        UnsignedInt  = 6,
        Long         = 7,
        UnsignedLong = 8,
        Float        = 9,
        Double       = 10,
        Decimal      = 11,
        String       = 12,
        Enum         = 13,
        Type         = 20,
        Guid         = 21,
        Tuple        = 22,

        // C# collections

        Array         = 30,
        ObjectArray   = 31,
        List          = 32,
        ObjectList    = 33,
        Dictionary    = 34,
        HashSet       = 35,
        ObjectHashSet = 36,

        // Other C# types

        DateTime = 50,
        TimeSpan = 51,

        // Unity

        Vector2    = 100,
        Vector3    = 101,
        Vector4    = 102,
        Color      = 103,
        Color32    = 104,
        Quaternion = 105,
        Matrix4x4  = 106,

        // UXR

        IUxrUnique       = 200,
        IUxrSerializable = 201,
        UxrAxis          = 202
    }
}