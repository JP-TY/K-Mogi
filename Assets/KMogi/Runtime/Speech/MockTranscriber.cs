using System.Collections.Generic;

namespace KMogi.Runtime.Speech
{
    /// <summary>
    /// Stand-in transcriber for editor demos. It ignores the audio buffer and returns text supplied
    /// out-of-band — either typed into the debug input field (simulating a recognized utterance) or
    /// queued from the canned lesson. This keeps the speech → parse → evaluate pipeline intact while
    /// real on-device STT is deferred.
    /// </summary>
    public sealed class MockTranscriber : ISpeechTranscriber
    {
        private readonly Queue<string> _pending = new Queue<string>();

        /// <summary>Queue text to be returned by the next <see cref="Transcribe"/> call.</summary>
        public void Enqueue(string text)
        {
            _pending.Enqueue(text ?? string.Empty);
        }

        public TranscriptionResult Transcribe(float[] samples, int sampleRate)
        {
            string text = _pending.Count > 0 ? _pending.Dequeue() : string.Empty;
            double duration = samples != null && sampleRate > 0
                ? (double)samples.Length / sampleRate
                : 0.0;
            return new TranscriptionResult(text, duration);
        }
    }
}
