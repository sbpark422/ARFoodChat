using System;
using System.Collections;
using UnityEngine;
using TMPro;
using OpenAI;

/// <summary>
/// This class handles speech-to-text (STT) functionality using OpenAI's Whisper model.
/// It captures audio from the user, converts it to text, and triggers an event upon transcription completion.
/// </summary>
public class SpeechToText : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource; 
    [SerializeField] private AudioClip startRecordingClip; 

    public event Action<string> OnTranscriptionComplete;

    private const string FileName = "output.wav";
    private const int Duration = 5; // Recording timer duration set to 5 seconds
    private AudioClip clip;
    //Replace 'YOUR_OPENAI_API_KEY' with your actual OpenAI API key
    //private OpenAIApi openAiApi = new OpenAIApi("YOUR_OPENAI_API_KEY");
    private OpenAIApi openAiApi = new OpenAIApi("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");

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void StartRecording()
    {
        if (startRecordingClip != null)
        {
            audioSource.PlayOneShot(startRecordingClip); // Play the recording start sound
        }

        clip = Microphone.Start(null, false, Duration, 44100); // Use default microphone
        StartCoroutine(RecordingTimer());
    }

    private IEnumerator RecordingTimer()
    {
        yield return new WaitForSeconds(Duration);
        EndRecording();
    }

    private async void EndRecording()
    {
        Microphone.End(null);
        byte[] data = SaveWav.Save(FileName, clip);
        var req = new CreateAudioTranscriptionsRequest
        {
            FileData = new FileData() { Data = data, Name = "audio.wav" },
            Model = "whisper-1",
            Language = "en"
        };
        var res = await openAiApi.CreateAudioTranscription(req);
        OnTranscriptionComplete?.Invoke(res.Text);
    }
}
