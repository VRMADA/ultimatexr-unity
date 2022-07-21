// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioClipExt.PcmData.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Extensions.System;

namespace UltimateXR.Extensions.Unity.Audio
{
    public static partial class AudioClipExt
    {
        #region Private Types & Data

        /// <summary>
        ///     Container of PCM audio data.
        /// </summary>
        private readonly struct PcmData
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the sample data.
            /// </summary>
            public float[] Value { get; }

            /// <summary>
            ///     Gets the sample count.
            /// </summary>
            public int Length { get; }

            /// <summary>
            ///     Gets the number of audio channels.
            /// </summary>
            public int Channels { get; }

            /// <summary>
            ///     Gets the sample rate in Hz.
            /// </summary>
            public int SampleRate { get; }

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="value">Sample data</param>
            /// <param name="channels">Audio channel count</param>
            /// <param name="sampleRate">Sample rate in Hz</param>
            private PcmData(float[] value, int channels, int sampleRate)
            {
                Value      = value;
                Length     = value.Length;
                Channels   = channels;
                SampleRate = sampleRate;
            }

            #endregion

            #region Public Methods

            /// <summary>
            ///     Creates a <see cref="PcmData" /> object from a byte data array.
            /// </summary>
            /// <param name="bytes">Byte data array with the PCM header and sample data</param>
            /// <returns><see cref="PcmData" /> object with the audio data</returns>
            /// <exception cref="ArgumentOutOfRangeException">The PCM header contains invalid data</exception>
            public static PcmData FromBytes(byte[] bytes)
            {
                bytes.ThrowIfNull(nameof(bytes));

                PcmHeader pcmHeader = PcmHeader.FromBytes(bytes);
                if (pcmHeader.BitDepth != 16 && pcmHeader.BitDepth != 32 && pcmHeader.BitDepth != 8)
                {
                    throw new ArgumentOutOfRangeException(nameof(pcmHeader.BitDepth), pcmHeader.BitDepth, "Supported values are: 8, 16, 32");
                }

                float[] samples = new float[pcmHeader.AudioSampleCount];
                for (int i = 0; i < samples.Length; ++i)
                {
                    int   byteIndex = pcmHeader.AudioStartIndex + i * pcmHeader.AudioSampleSize;
                    float rawSample;
                    switch (pcmHeader.BitDepth)
                    {
                        case 8:
                            rawSample = bytes[byteIndex];
                            break;

                        case 16:
                            rawSample = BitConverter.ToInt16(bytes, byteIndex);
                            break;

                        case 32:
                            rawSample = BitConverter.ToInt32(bytes, byteIndex);
                            break;

                        default: throw new ArgumentOutOfRangeException(nameof(pcmHeader.BitDepth), pcmHeader.BitDepth, "Supported values are: 8, 16, 32");
                    }

                    samples[i] = pcmHeader.NormalizeSample(rawSample); // normalize sample between [-1f, 1f]
                }

                return new PcmData(samples, pcmHeader.Channels, pcmHeader.SampleRate);
            }

            #endregion
        }

        #endregion
    }
}