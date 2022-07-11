// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioClipExt.StreamedAudioClip.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using UltimateXR.Extensions.System;
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Audio
{
    public static partial class AudioClipExt
    {
        #region Public Types & Data

        /// <summary>
        ///     Describes a PCM audio clip.
        /// </summary>
        public sealed class StreamedPcmClip : IDisposable
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the <see cref="AudioClip" /> described by the object.
            /// </summary>
            public AudioClip InnerClip { get; }

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="pcmStream">PCM data</param>
            /// <param name="clipName">Name assigned to the audio clip</param>
            /// <param name="header">PCM data header</param>
            private StreamedPcmClip(Stream pcmStream, string clipName, in PcmHeader header)
            {
                _pcmHeader = header;
                _pcmStream = pcmStream;
                _pcmReader = new BinaryReader(pcmStream);

                InnerClip = AudioClip.Create(clipName, header.AudioSampleCount, header.Channels, header.SampleRate, true, OnPcmRead, OnPcmSetPosition);
            }

            #endregion

            #region Implicit IDisposable

            /// <inheritdoc />
            public void Dispose()
            {
                _pcmReader.Dispose();
            }

            #endregion

            #region Public Methods

            /// <summary>
            ///     Creates a <see cref="StreamedPcmClip" /> object from a data stream.
            /// </summary>
            /// <param name="pcmStream">Source data stream</param>
            /// <param name="clipName">Name that will be assigned to the clip</param>
            /// <returns><see cref="StreamedPcmClip" /> describing the PCM audio clip</returns>
            /// <exception cref="ArgumentOutOfRangeException">The bit depth is not supported</exception>
            public static StreamedPcmClip Create(Stream pcmStream, string clipName = "pcm")
            {
                pcmStream.ThrowIfNull(nameof(pcmStream));
                clipName.ThrowIfNullOrWhitespace(nameof(clipName));
                var pcmHeader = PcmHeader.FromStream(pcmStream);
                if (pcmHeader.BitDepth != 16 && pcmHeader.BitDepth != 32 && pcmHeader.BitDepth != 8)
                {
                    throw new ArgumentOutOfRangeException(nameof(pcmHeader.BitDepth), pcmHeader.BitDepth, "Supported values are: 8, 16, 32");
                }

                return new StreamedPcmClip(pcmStream, clipName, in pcmHeader);
            }

            #endregion

            #region Event Trigger Methods

            /// <summary>
            ///     PCM reader callback.
            /// </summary>
            /// <param name="data">Source data</param>
            /// <exception cref="ArgumentOutOfRangeException">Unsupported audio bit depth</exception>
            private void OnPcmRead(float[] data)
            {
                for (int i = 0; i < data.Length && _pcmStream.Position < _pcmStream.Length; ++i)
                {
                    float rawSample;
                    switch (_pcmHeader.AudioSampleSize)
                    {
                        case 1:
                            rawSample = _pcmReader.ReadByte();
                            break;

                        case 2:
                            rawSample = _pcmReader.ReadInt16();
                            break;

                        case 3:
                            rawSample = _pcmReader.ReadInt32();
                            break;

                        default: throw new ArgumentOutOfRangeException(nameof(_pcmHeader.BitDepth), _pcmHeader.BitDepth, "Supported values are: 8, 16, 32");
                    }
                    data[i] = _pcmHeader.NormalizeSample(rawSample); // needs to be scaled to be within the range of - 1.0f to 1.0f.
                }
            }

            /// <summary>
            ///     PCM reader positioning callback.
            /// </summary>
            /// <param name="newPosition">New index where to position the read cursor</param>
            private void OnPcmSetPosition(int newPosition)
            {
                _pcmStream.Position = _pcmHeader.AudioStartIndex + newPosition * _pcmHeader.AudioSampleSize;
            }

            #endregion

            #region Private Types & Data

            private readonly Stream       _pcmStream;
            private readonly BinaryReader _pcmReader;
            private readonly PcmHeader    _pcmHeader;

            #endregion
        }

        #endregion
    }
}