using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace UnityEditor.Recorder
{
    abstract class BaseTextureRecorder<T> : GenericRecorder<T> where T : RecorderSettings
    {
        int       m_OngoingAsyncGPURequestsCount;
        bool      m_DelayedEncoderDispose;
        bool      m_UseAsyncGPUReadback;
        Texture2D m_ReadbackTexture;

        protected abstract TextureFormat readbackTextureFormat { get; }

        public override bool BeginRecording(RecordingSession session)
        {
            if (!base.BeginRecording(session))
                return false;

            m_UseAsyncGPUReadback = SystemInfo.supportsAsyncGPUReadback;
            m_OngoingAsyncGPURequestsCount = 0;
            m_DelayedEncoderDispose = false;
            return true;
        }

        public override void RecordFrame(RecordingSession session)
        {
            var input = (BaseRenderTextureInput)m_Inputs[0];

            if (input.readbackTexture != null)
            {
                WriteFrame(input.readbackTexture);
                return;
            }

            var renderTexture = input.outputRT;

            if (m_UseAsyncGPUReadback)
            {
                AsyncGPUReadback.Request(
                    renderTexture, 0, readbackTextureFormat, ReadbackDone);
                ++m_OngoingAsyncGPURequestsCount;
                return;
            }

            var width = renderTexture.width;
            var height = renderTexture.height;

            if (m_ReadbackTexture == null)
                m_ReadbackTexture = CreateReadbackTexture(width, height);

            var backupActive = RenderTexture.active;
            RenderTexture.active = renderTexture;
            m_ReadbackTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            m_ReadbackTexture.Apply();
            RenderTexture.active = backupActive;
            WriteFrame(m_ReadbackTexture);
        }

        private void ReadbackDone(AsyncGPUReadbackRequest r)
        {
            Profiler.BeginSample("BaseTextureRecorder.ReadbackDone");
            WriteFrame(r);
            Profiler.EndSample();
            --m_OngoingAsyncGPURequestsCount;
            if (m_OngoingAsyncGPURequestsCount == 0 && m_DelayedEncoderDispose)
                DisposeEncoder();
        }

        public override void EndRecording(RecordingSession session)
        {
            base.EndRecording(session);
            if (m_OngoingAsyncGPURequestsCount > 0)
                m_DelayedEncoderDispose = true;
            else
                DisposeEncoder();
        }

        private Texture2D CreateReadbackTexture(int width, int height)
        {
            return new Texture2D(width, height, readbackTextureFormat, false);
        }

        protected virtual void WriteFrame(AsyncGPUReadbackRequest r)
        {
            if (m_ReadbackTexture == null)
                m_ReadbackTexture = CreateReadbackTexture(r.width, r.height);
            Profiler.BeginSample("BaseTextureRecorder.LoadRawTextureData");
            m_ReadbackTexture.LoadRawTextureData(r.GetData<byte>());
            Profiler.EndSample();
            WriteFrame(m_ReadbackTexture);
        }

        protected abstract void WriteFrame(Texture2D t);

        protected virtual void DisposeEncoder()
        {
            UnityHelpers.Destroy(m_ReadbackTexture);
        }
    }
}
