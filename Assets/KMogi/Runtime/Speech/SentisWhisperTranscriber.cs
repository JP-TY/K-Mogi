using UnityEngine;

namespace KMogi.Runtime.Speech
{
    /// <summary>
    /// Planned on-device Japanese STT using Unity Sentis (com.unity.ai.inference, already in the
    /// project) to run a quantized Whisper model — satisfying the PRD's no-cloud constraint.
    ///
    /// Implementation outline (deferred):
    ///   1. Import a Whisper ONNX model (e.g. whisper-tiny/base, int8) as a Sentis ModelAsset.
    ///   2. Resample mic audio to 16 kHz mono and compute the log-Mel spectrogram.
    ///   3. Run the encoder/decoder Worker, greedy/beam decode token ids, detokenize to text.
    ///   4. Constrain/force the language token to Japanese.
    /// This stub keeps the seam present and compiling; it returns empty text and warns once.
    /// </summary>
    public sealed class SentisWhisperTranscriber : ISpeechTranscriber
    {
        private bool _warned;

        public TranscriptionResult Transcribe(float[] samples, int sampleRate)
        {
            if (!_warned)
            {
                Debug.LogWarning(
                    "[KMogi] SentisWhisperTranscriber is a stub. Wire a Whisper model via Unity Sentis " +
                    "for on-device Japanese STT. Falling back to empty transcription.");
                _warned = true;
            }

            double duration = samples != null && sampleRate > 0
                ? (double)samples.Length / sampleRate
                : 0.0;
            return new TranscriptionResult(string.Empty, duration);
        }
    }
}
