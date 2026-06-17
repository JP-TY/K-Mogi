using System;
using UnityEngine;

namespace KMogi.Runtime.Speech
{
    /// <summary>
    /// Asynchronous microphone capture into a buffer, kept off the per-frame Update loop. This is
    /// scaffolding for the future STT path (<see cref="SentisWhisperTranscriber"/>); the canned demo
    /// does not require it. Safely no-ops when no microphone is present (common in CI/editor).
    /// </summary>
    public sealed class AudioRecorder : MonoBehaviour
    {
        [SerializeField] private int sampleRate = 16000;
        [SerializeField] private int maxClipSeconds = 15;

        private AudioClip _clip;
        private string _device;
        private bool _recording;

        /// <summary>Raised when recording stops, with the captured samples and their sample rate.</summary>
        public event Action<float[], int> RecordingFinished;

        public bool IsRecording => _recording;

        public bool HasMicrophone => Microphone.devices != null && Microphone.devices.Length > 0;

        public void StartRecording()
        {
            if (_recording)
            {
                return;
            }

            if (!HasMicrophone)
            {
                Debug.LogWarning("[KMogi] AudioRecorder: no microphone device available.");
                return;
            }

            _device = Microphone.devices[0];
            _clip = Microphone.Start(_device, false, Mathf.Max(1, maxClipSeconds), sampleRate);
            _recording = true;
        }

        public void StopRecording()
        {
            if (!_recording)
            {
                return;
            }

            int position = Microphone.GetPosition(_device);
            Microphone.End(_device);
            _recording = false;

            if (_clip == null)
            {
                return;
            }

            int frameCount = position > 0 ? position : _clip.samples;
            var samples = new float[frameCount * _clip.channels];
            _clip.GetData(samples, 0);
            RecordingFinished?.Invoke(samples, _clip.frequency);
        }

        private void OnDisable()
        {
            if (_recording && !string.IsNullOrEmpty(_device))
            {
                Microphone.End(_device);
            }

            _recording = false;
        }
    }
}
