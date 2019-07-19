using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.Profiling;

namespace UnityEditor.Recorder.Input
{
    class GameViewInput : BaseRenderTextureInput
    {
        bool m_ModifiedResolution;
        TextureFlipper m_VFlipper;
        RenderTexture m_CaptureTexture;

        GameViewInputSettings scSettings
        {
            get { return (GameViewInputSettings)settings; }
        }

        public override void NewFrameReady(RecordingSession session)
        {
            Profiler.BeginSample("GameViewInput.NewFrameReady");
#if UNITY_2019_1_OR_NEWER
            ScreenCapture.CaptureScreenshotIntoRenderTexture(m_CaptureTexture);
            m_VFlipper?.Flip(m_CaptureTexture);
#else
            readbackTexture = ScreenCapture.CaptureScreenshotAsTexture();
#endif
            Profiler.EndSample();
        }

        public override void BeginRecording(RecordingSession session)
        {
            outputWidth = scSettings.outputWidth;
            outputHeight = scSettings.outputHeight;
            
            int w, h;
            GameViewSize.GetGameRenderSize(out w, out h);
            if (w != outputWidth || h != outputHeight)
            {
                var size = GameViewSize.SetCustomSize(outputWidth, outputHeight) ?? GameViewSize.AddSize(outputWidth, outputHeight);
                if (GameViewSize.modifiedResolutionCount == 0)
                    GameViewSize.BackupCurrentSize();
                else
                {
                    if (size != GameViewSize.currentSize)
                    {
                        Debug.LogError("Requestion a resultion change while a recorder's input has already requested one! Undefined behaviour.");
                    }
                }
                GameViewSize.modifiedResolutionCount++;
                m_ModifiedResolution = true;
                GameViewSize.SelectSize(size);
            }

#if !UNITY_2019_1_OR_NEWER
            // Before 2019.1, we capture syncrhonously into a Texture2D, so we don't need to create
            // a RenderTexture that is used for reading asynchronously.
            return;
#else
            m_CaptureTexture = new RenderTexture(outputWidth, outputHeight, 0, RenderTextureFormat.ARGB32)
            {
                wrapMode = TextureWrapMode.Repeat
            };
            m_CaptureTexture.Create();

            if (scSettings.flipFinalOutput)
            {
                m_VFlipper = new TextureFlipper(false);
                m_VFlipper.Init(m_CaptureTexture);
                outputRT = m_VFlipper.workTexture;
            }
            else
                outputRT = m_CaptureTexture;
#endif
        }

        public override void FrameDone(RecordingSession session)
        {
            UnityHelpers.Destroy(readbackTexture);
            readbackTexture = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_ModifiedResolution)
                {
                    GameViewSize.modifiedResolutionCount--;
                    if (GameViewSize.modifiedResolutionCount == 0)
                        GameViewSize.RestoreSize();
                }
            }

            m_VFlipper?.Dispose();
            m_VFlipper = null;

            base.Dispose(disposing);
        }
    }
}
