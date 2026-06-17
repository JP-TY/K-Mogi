namespace KMogi.Runtime.Speech
{
    /// <summary>Recognized text plus the measured spoken duration (used for pacing analysis).</summary>
    public sealed class TranscriptionResult
    {
        public string Text { get; }
        public double DurationSeconds { get; }

        public TranscriptionResult(string text, double durationSeconds)
        {
            Text = text ?? string.Empty;
            DurationSeconds = durationSeconds;
        }
    }

    /// <summary>
    /// Converts captured audio to Japanese text on-device. The demo uses <see cref="MockTranscriber"/>
    /// (text supplied directly); <see cref="SentisWhisperTranscriber"/> is the planned real engine.
    /// </summary>
    public interface ISpeechTranscriber
    {
        TranscriptionResult Transcribe(float[] samples, int sampleRate);
    }
}
