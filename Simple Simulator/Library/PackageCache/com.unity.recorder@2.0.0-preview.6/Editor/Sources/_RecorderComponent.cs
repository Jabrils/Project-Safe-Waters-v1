using System.Collections;
using UnityEngine;

namespace UnityEditor.Recorder
{
    [ExecuteInEditMode]
    class RecorderComponent : MonoBehaviour
    {
        public RecordingSession session { get; set; }

        public void Update()
        {
            if (session != null && session.isRecording)
                session.PrepareNewFrame();
        }

        IEnumerator RecordFrame()
        {
            yield return new WaitForEndOfFrame();
            if (session != null && session.isRecording)
            {
                session.RecordFrame();

                switch (session.recorder.settings.recordMode)
                {
                    case RecordMode.Manual:
                        break;
                    case RecordMode.SingleFrame:
                    {
                        if (session.recorder.recordedFramesCount == 1)
                            Destroy(this);
                        break;
                    }
                    case RecordMode.FrameInterval:
                    {
                        if (session.frameIndex > session.settings.endFrame)
                            Destroy(this);
                        break;
                    }
                    case RecordMode.TimeInterval:
                    {
                        if (session.settings.frameRatePlayback == FrameRatePlayback.Variable)
                        {
                            if (session.currentFrameStartTS >= session.settings.endTime)
                                Destroy(this);
                        }
                        else
                        {
                            var expectedFrames = (session.settings.endTime - session.settings.startTime) * session.settings.frameRate;
                            if (session.RecordedFrameSpan >= expectedFrames)
                                Destroy(this);
                        }
                        break;
                    }
                }
            }
        }

        public void LateUpdate()
        {
            if (session != null && session.isRecording)
                StartCoroutine(RecordFrame());
        }

        public void OnDestroy()
        {
            if (session != null)
                session.Dispose();
        }
    }
}
