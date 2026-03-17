namespace ClockTrack.Services
{
    using System.Threading.Tasks;

    public interface ISpeechToTextService
    {
        Task<string> TranscribeAudioAsync(string audioFilePath);
    }

}
