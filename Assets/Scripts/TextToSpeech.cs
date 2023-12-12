using System;
using Amazon;
using System.IO;
using UnityEngine;
using Amazon.Polly;
using Amazon.Runtime;
using Amazon.Polly.Model;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// The TextToSpeech class uses Amazon Polly to convert text to speech.
/// It sends requests to synthesize speech from text, receives the audio stream, and plays it through an AudioSource.
/// </summary>
public class TextToSpeech : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    public async void MakeAudioRequest(string message)
    {
        // Replace with your Amazon credentials
        //var credentials = new BasicAWSCredentials("YOUR_ACCESS_KEY", "YOUR_SECRET_KEY");
        var credentials = new BasicAWSCredentials("AKIA23UCKYXR3SWWVRA6", "eor9YvF5Ch48rKYjSKcvlxayr78NJlNEsQWOxvuk");
        var client = new AmazonPollyClient(credentials, RegionEndpoint.USWest2);

        // Create request with desired settings
        var request = new SynthesizeSpeechRequest()
        {
            Text = message,
            Engine = Engine.Neural,
            VoiceId = VoiceId.Matthew,
            OutputFormat = OutputFormat.Mp3
        };

        var response = await client.SynthesizeSpeechAsync(request);

        WriteIntoFile(response.AudioStream);

        string audioPath;
        
        #if UNITY_ANDROID && !UNITY_EDITOR
            audioPath = $"jar:file://{Application.persistentDataPath}/audio.mp3";
        #elif (UNITY_IOS || UNITY_OSX) && !UNITY_EDITOR
            audioPath = $"file://{Application.persistentDataPath}/audio.mp3";
        #else
            //audioPath = $"{Application.persistentDataPath}/audio.mp3";
            audioPath = $"file://{Application.persistentDataPath}/audio.mp3";
        #endif

        // Use UnityWebRequest to retrieve the audio clip from the given path
        using (var www = UnityWebRequestMultimedia.GetAudioClip(audioPath, AudioType.MPEG))
        {
            var op = www.SendWebRequest();

            while (!op.isDone) await Task.Yield(); // allows other awaiting tasks to run

            var clip = DownloadHandlerAudioClip.GetContent(www); // extract the audio clip from the response

            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    // Writes the audio stream to a local file for playback
    private void WriteIntoFile(Stream stream)
    {
        using (var fileStream = new FileStream($"{Application.persistentDataPath}/audio.mp3", FileMode.Create))
        {
            byte[] buffer = new byte[8 * 1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }
        }
    }
}

