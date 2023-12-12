using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OpenAI;

/// <summary>
/// Manages interactions with OpenAI's ChatGPT service.
/// This class handles user input, sends it to ChatGPT for processing,
/// and manages the display of both user inputs and ChatGPT responses within the Chat scroll view UI.
/// </summary>
public class ChatGPTManager : MonoBehaviour
{
    // UI elements for displaying the chat interface
    [SerializeField] private Transform contentPanel;
    [SerializeField] private GameObject userMessagePrefab;
    [SerializeField] private GameObject chatGPTMessagePrefab;

    // STT and TTS
    [SerializeField] private SpeechToText speechToText;
    [SerializeField] private TextToSpeech textToSpeech;

    private OpenAIApi openAI = new OpenAIApi("YOUR_OPENAI_API_KEY");  // Replace with your OpenAI API key
    private List<ChatMessage> messages = new List<ChatMessage>(); // History of messages
    private bool responseReceived = false;
    private GameObject currentLoadingMessageObj; // Current message object that displays a loading indicator while awaiting response


    private void Awake()
    {
        // Subscribe to the STT event
        speechToText.OnTranscriptionComplete += AddUserMessage;

    }

    private void OnDestroy()
    {
        speechToText.OnTranscriptionComplete -= AddUserMessage;
    }

    /// <summary>
    /// Add a user message to the chat and send it to ChatGPT for processing.
    /// </summary>
    public void AddUserMessage(string userText)
    {
        ChatMessage newMessage = new ChatMessage()
        {
            Role = "user",
            Content = userText
        };

        CreateMessage(userText, true); // true indicating it's a user message
        messages.Add(newMessage); // Add to message history

        AskChatGPT($"In 15 words or less, {userText}");
    }

    private async void AskChatGPT(string newText)
    {
        responseReceived = false;

        // Show loading message while waiting for ChatGPT's response
        currentLoadingMessageObj = CreateMessage("", false);
        StartCoroutine(ShowLoadingAnimation(currentLoadingMessageObj));

        // Add the new message to the conversation history and send to ChatGPT
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = newText;
        newMessage.Role = "user";
        messages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-3.5-turbo";

        var response = await openAI.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);

            textToSpeech.MakeAudioRequest(chatResponse.Content); // Convert ChatGPT response to speech

            // Update ChatGPT UI text with the received response
            if (currentLoadingMessageObj != null)
            {
                TMP_Text textComponent = currentLoadingMessageObj.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = chatResponse.Content;

                    LayoutRebuilder.ForceRebuildLayoutImmediate(currentLoadingMessageObj.GetComponent<RectTransform>());
                    LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel.GetComponent<RectTransform>());

                    StartCoroutine(ScrollToBottom());
                }
            }
            responseReceived = true;
        }
    }

    // Shows a loading animation while waiting for a response.
    private IEnumerator ShowLoadingAnimation(GameObject messageObj)
    {
        string[] loadingSteps = { ".", "..", "..." };
        int step = 0;

        while (!responseReceived && messageObj != null)
        {
            TMP_Text textComponent = messageObj.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = loadingSteps[step % loadingSteps.Length];
            }
            step++;
            yield return new WaitForSeconds(0.5f);
        }
    }

    // Instantiate a user/ChatGPT message prefab and add it to the chat UI
    private GameObject CreateMessage(string text, bool isUserMessage)
    {
        GameObject prefabToUse = isUserMessage ? userMessagePrefab : chatGPTMessagePrefab;
        GameObject messageObj = Instantiate(prefabToUse, contentPanel);
        messageObj.GetComponentInChildren<TMP_Text>().text = text;

        // Ensure the message layout is updated immediately
        LayoutRebuilder.ForceRebuildLayoutImmediate(messageObj.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel.GetComponent<RectTransform>());

        StartCoroutine(ScrollToBottom());
        return messageObj;
    }

    // Scroll the chat content to the latest message
    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        contentPanel.GetComponentInParent<ScrollRect>().normalizedPosition = new Vector2(0, 0);
    }
}
