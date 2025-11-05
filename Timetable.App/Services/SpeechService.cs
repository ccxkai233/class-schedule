using System.Speech.Synthesis;
using System.Globalization;

namespace Timetable.App.Services;

public class SpeechService : ISpeechService
{
    private readonly SpeechSynthesizer _synthesizer = new();

    public void SelectVoice(string? culture)
    {
        if (!string.IsNullOrWhiteSpace(culture))
        {
            try
            {
                _synthesizer.SelectVoiceByHints(VoiceGender.NotSet, VoiceAge.NotSet, 0, new CultureInfo(culture));
            }
            catch (Exception ex)
            {
                // Handle cases where the voice culture is not installed on the system
                Console.WriteLine($"Could not set voice to '{culture}': {ex.Message}");
            }
        }
    }

    public void SpeakAsync(string text)
    {
        // Cancel any previous speech before starting a new one
        _synthesizer.SpeakAsyncCancelAll();
        _synthesizer.SpeakAsync(text);
    }
}