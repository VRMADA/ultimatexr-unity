// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioClipExt.PcmHeader.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using UnityEngine;

namespace UltimateXR.Extensions.Unity.Audio
{
    public static partial class AudioClipExt
    {
        #region Private Types & Data

        /// <summary>
        ///     Describes a PCM audio data header.
        /// </summary>
        private readonly struct PcmHeader
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the bits per audio sample.
            /// </summary>
            public int BitDepth { get; }

            /// <summary>
            ///     Gets the audio total sample size in bytes.
            /// </summary>
            public int AudioSampleSize { get; }

            /// <summary>
            ///     Gets the number of audio samples.
            /// </summary>
            public int AudioSampleCount { get; }

            /// <summary>
            ///     Gets the number of audio channels.
            /// </summary>
            public ushort Channels { get; }

            /// <summary>
            ///     Gets the sample rate in audio samples per second.
            /// </summary>
            public int SampleRate { get; }

            /// <summary>
            ///     Gets the data index where the audio data starts.
            /// </summary>
            public int AudioStartIndex { get; }

            /// <summary>
            ///     Gets the audio data bytes per second.
            /// </summary>
            public int ByteRate { get; }

            /// <summary>
            ///     Gets the data block alignment.
            /// </summary>
            public ushort BlockAlign { get; }

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="bitDepth">Sample bit size</param>
            /// <param name="audioSize">Total audio data size in bytes</param>
            /// <param name="audioStartIndex">Index where the audio sample data starts</param>
            /// <param name="channels">The number of audio channels</param>
            /// <param name="sampleRate">The number of samples per second</param>
            /// <param name="byteRate">The number of bytes per second</param>
            /// <param name="blockAlign">The block alignment</param>
            private PcmHeader(int    bitDepth,
                              int    audioSize,
                              int    audioStartIndex,
                              ushort channels,
                              int    sampleRate,
                              int    byteRate,
                              ushort blockAlign)
            {
                BitDepth       = bitDepth;
                _negativeDepth = Mathf.Pow(2f, BitDepth - 1f);
                _positiveDepth = _negativeDepth - 1f;

                AudioSampleSize  = bitDepth / 8;
                AudioSampleCount = Mathf.FloorToInt(audioSize / (float)AudioSampleSize);
                AudioStartIndex  = audioStartIndex;

                Channels   = channels;
                SampleRate = sampleRate;
                ByteRate   = byteRate;
                BlockAlign = blockAlign;
            }

            #endregion

            #region Public Methods

            /// <summary>
            ///     Creates a <see cref="PcmHeader" /> object reading from a byte array.
            /// </summary>
            /// <param name="pcmBytes">Source byte array</param>
            /// <returns><see cref="PcmHeader" /> object</returns>
            public static PcmHeader FromBytes(byte[] pcmBytes)
            {
                using var memoryStream = new MemoryStream(pcmBytes);
                return FromStream(memoryStream);
            }

            /// <summary>
            ///     Creates a <see cref="PcmHeader" /> object reading from a data stream.
            /// </summary>
            /// <param name="pcmStream">Source data</param>
            /// <returns><see cref="PcmHeader" /> object</returns>
            public static PcmHeader FromStream(Stream pcmStream)
            {
                pcmStream.Position = SizeIndex;
                using BinaryReader reader = new BinaryReader(pcmStream);

                int    headerSize      = reader.ReadInt32();  // 16
                ushort audioFormatCode = reader.ReadUInt16(); // 20

                string audioFormat = GetAudioFormatFromCode(audioFormatCode);
                if (audioFormatCode != 1 && audioFormatCode == 65534)
                {
                    // Only uncompressed PCM wav files are supported.
                    throw new ArgumentOutOfRangeException(nameof(pcmStream),
                                                          $"Detected format code '{audioFormatCode}' {audioFormat}, but only PCM and WaveFormatExtensible uncompressed formats are currently supported.");
                }

                ushort channelCount = reader.ReadUInt16(); // 22
                int    sampleRate   = reader.ReadInt32();  // 24
                int    byteRate     = reader.ReadInt32();  // 28
                ushort blockAlign   = reader.ReadUInt16(); // 32
                ushort bitDepth     = reader.ReadUInt16(); //34

                pcmStream.Position = SizeIndex + headerSize + 2 * sizeof(int); // Header end index
                int audioSize = reader.ReadInt32();                            // Audio size index

                return new PcmHeader(bitDepth, audioSize, (int)pcmStream.Position, channelCount, sampleRate, byteRate, blockAlign); // audio start index
            }

            /// <summary>
            ///     Normalizes a raw audio sample.
            /// </summary>
            /// <param name="rawSample">Audio sample to normalize</param>
            /// <returns>Normalized audio sample</returns>
            public float NormalizeSample(float rawSample)
            {
                float sampleDepth = rawSample < 0 ? _negativeDepth : _positiveDepth;
                return rawSample / sampleDepth;
            }

            #endregion

            #region Private Methods

            /// <summary>
            ///     Gets the audio format string from the numerical code.
            /// </summary>
            /// <param name="code">Numerical audio format code</param>
            /// <returns>Audio format string</returns>
            /// <exception cref="ArgumentOutOfRangeException">The code is not valid</exception>
            private static string GetAudioFormatFromCode(ushort code)
            {
                switch (code)
                {
                    case 1:     return "PCM";
                    case 2:     return "ADPCM";
                    case 3:     return "IEEE";
                    case 7:     return "?-law";
                    case 65534: return "WaveFormatExtensible";
                    default:    throw new ArgumentOutOfRangeException(nameof(code), code, "Unknown wav code format.");
                }
            }

            #endregion

            #region Private Types & Data

            private const int SizeIndex = 16;

            private readonly float _positiveDepth;
            private readonly float _negativeDepth;

            #endregion
        }

        #endregion
    }
}