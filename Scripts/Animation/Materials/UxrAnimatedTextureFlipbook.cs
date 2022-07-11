// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAnimatedTextureFlipbook.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UltimateXR.Animation.Materials
{
    /// <summary>
    ///     Component that allows to animate a material's texture that contains multiple animation frames.
    /// </summary>
    public class UxrAnimatedTextureFlipbook : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool                      _animateSelf = true;
        [SerializeField] private GameObject                _targetGameObject;
        [SerializeField] private string                    _scaleOffsetVarName = UxrConstants.Shaders.StandardMainTextureScaleOffsetVarName;
        [SerializeField] private int                       _flipBookColumns    = 1;
        [SerializeField] private int                       _flipBookRows       = 1;
        [SerializeField] private int                       _totalFrames        = 1;
        [SerializeField] private UxrFlipbookAnimationMode  _loopMode           = UxrFlipbookAnimationMode.SingleSequence;
        [SerializeField] private bool                      _randomFrameStart;
        [SerializeField] private float                     _fps          = 10;
        [SerializeField] private UxrFlipbookFinishedAction _whenFinished = UxrFlipbookFinishedAction.DoNothing;
        [SerializeField] private bool                      _useUnscaledTime;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Called when the animation finished.
        /// </summary>
        public event Action Finished;

        /// <summary>
        ///     Gets or sets the target renderer whose material will be animated.
        /// </summary>
        public Renderer TargetRenderer { get; set; }

        /// <summary>
        ///     Gets or sets the material's shader scale/offset variable name, usually _MainTex_ST.
        /// </summary>
        public string ScaleOffsetVarName
        {
            get => _scaleOffsetVarName;
            set => _scaleOffsetVarName = value;
        }

        /// <summary>
        ///     Gets or sets the number of columns in the texture animation sheet.
        /// </summary>
        public int FlipBookColumns
        {
            get => _flipBookColumns;
            set => _flipBookColumns = value;
        }

        /// <summary>
        ///     Gets or sets the number of rows in the texture animation sheet.
        /// </summary>
        public int FlipBookRows
        {
            get => _flipBookRows;
            set => _flipBookRows = value;
        }

        /// <summary>
        ///     Gets or sets the total number of frames in the texture animation sheet.
        /// </summary>
        public int TotalFrames
        {
            get => _totalFrames;
            set => _totalFrames = value;
        }

        /// <summary>
        ///     Gets or sets the animation loop mode.
        /// </summary>
        public UxrFlipbookAnimationMode LoopMode
        {
            get => _loopMode;
            set => _loopMode = value;
        }

        /// <summary>
        ///     Gets or sets whether to start the animation in a random frame position.
        /// </summary>
        public bool RandomFrameStart
        {
            get => _randomFrameStart;
            set => _randomFrameStart = value;
        }

        /// <summary>
        ///     Gets or sets the frames per second to play the animation.
        /// </summary>
        public float FPS
        {
            get => _fps;
            set => _fps = value;
        }

        /// <summary>
        ///     Gets or sets the action to perform when the animation finished. The only animation that can finish is when
        ///     <see cref="AnimationPlayMode" /> is <see cref="UxrFlipbookAnimationMode.SingleSequence" />.
        /// </summary>
        public UxrFlipbookFinishedAction WhenFinished
        {
            get => _whenFinished;
            set => _whenFinished = value;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes internal variables
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (TargetRenderer == null)
            {
                TargetRenderer = _animateSelf || !_targetGameObject ? GetComponent<Renderer>() : _targetGameObject.GetComponent<Renderer>();
            }

            _hasFinished = false;
            _frameStart  = 0;
            SetFrame(0);
        }

        /// <summary>
        ///     Called each time the object is enabled. Reset timer and set the curve state to unfinished.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _startTime       = _useUnscaledTime ? Time.unscaledTime : Time.time;
            _hasFinished     = false;
            _lastFrame       = -1;
            _lastLinearFrame = -1;

            if (_randomFrameStart)
            {
                _frameStart = Mathf.RoundToInt(Random.value * (_totalFrames - 1));
            }

            if (TargetRenderer && _whenFinished == UxrFlipbookFinishedAction.DisableRenderer)
            {
                TargetRenderer.enabled = true;
            }
        }

        /// <summary>
        ///     Enables the correct flipbook frame and checks if it finished
        /// </summary>
        private void Update()
        {
            if (_hasFinished)
            {
                return;
            }

            float currentTime = _useUnscaledTime ? Time.unscaledTime : Time.time;
            int   linearFrame = (int)((currentTime - _startTime) * _fps);

            switch (_loopMode)
            {
                case UxrFlipbookAnimationMode.SingleSequence:

                    if (linearFrame >= _totalFrames)
                    {
                        ExecuteFinishAction();
                        _hasFinished = true;
                    }
                    else
                    {
                        SetFrame(_totalFrames > 0 ? (linearFrame + _frameStart) % _totalFrames : 0);
                    }

                    break;

                case UxrFlipbookAnimationMode.Loop:

                    SetFrame(_totalFrames > 0 ? (linearFrame + _frameStart) % _totalFrames : 0);

                    break;

                case UxrFlipbookAnimationMode.PingPong:

                    if (_totalFrames > 1)
                    {
                        if (linearFrame < _totalFrames)
                        {
                            SetFrame(linearFrame);
                        }
                        else
                        {
                            bool forward      = ((linearFrame - _totalFrames) / (_totalFrames - 1) & 1) == 1;
                            int  correctFrame = (linearFrame - _totalFrames) % (_totalFrames - 1);
                            SetFrame(forward ? correctFrame + 1 : _totalFrames - correctFrame - 2);
                        }
                    }
                    else if (_lastFrame != 0)
                    {
                        SetFrame(0);
                    }

                    break;

                case UxrFlipbookAnimationMode.RandomFrame:

                    if (linearFrame != _lastLinearFrame)
                    {
                        SetFrame(Random.Range(0, _totalFrames));
                    }

                    break;

                case UxrFlipbookAnimationMode.RandomFrameNoRepetition:

                    if (linearFrame != _lastLinearFrame)
                    {
                        if (_totalFrames < 2)
                        {
                            SetFrame(0);
                        }
                        else if (_totalFrames == 2)
                        {
                            SetFrame(_lastFrame == 0 ? 1 : 0);
                        }
                        else
                        {
                            int frame = Random.Range(0, _totalFrames);

                            while (frame == _lastFrame)
                            {
                                frame = Random.Range(0, _totalFrames);
                            }

                            SetFrame(frame);
                        }
                    }

                    break;

                default: throw new ArgumentOutOfRangeException();
            }

            _lastLinearFrame = linearFrame;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Sets the current flipbook texture frame
        /// </summary>
        /// <param name="frame">Flipbook frame</param>
        private void SetFrame(int frame)
        {
            if (TargetRenderer && _lastFrame != frame)
            {
                Vector4 vecScaleOffset = TargetRenderer.material.GetVector(_scaleOffsetVarName);

                if (_flipBookColumns > 0)
                {
                    int column = frame % _flipBookColumns;
                    vecScaleOffset.x = 1.0f / _flipBookColumns;
                    vecScaleOffset.z = column * vecScaleOffset.x;
                }

                if (_flipBookRows > 0 && _flipBookColumns > 0)
                {
                    int row = frame / _flipBookColumns;
                    vecScaleOffset.y = 1.0f / _flipBookRows;
                    vecScaleOffset.w = 1.0f - (row + 1) * vecScaleOffset.y;
                }

                TargetRenderer.material.SetVector(_scaleOffsetVarName, vecScaleOffset);
                _lastFrame = frame;
            }
        }

        /// <summary>
        ///     Executes the action when the animation finished.
        /// </summary>
        private void ExecuteFinishAction()
        {
            Finished?.Invoke();

            switch (_whenFinished)
            {
                case UxrFlipbookFinishedAction.DoNothing: break;

                case UxrFlipbookFinishedAction.DisableRenderer:

                    if (TargetRenderer)
                    {
                        TargetRenderer.enabled = false;
                    }

                    break;

                case UxrFlipbookFinishedAction.DisableGameObject:

                    if (TargetRenderer)
                    {
                        TargetRenderer.gameObject.SetActive(false);
                    }

                    break;

                case UxrFlipbookFinishedAction.DestroyGameObject:

                    if (TargetRenderer)
                    {
                        Destroy(TargetRenderer.gameObject);
                    }

                    break;

                default: throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Private Types & Data

        private int   _frameStart;
        private float _startTime;
        private bool  _hasFinished;
        private int   _lastFrame       = -1;
        private int   _lastLinearFrame = -1;

        #endregion
    }
}