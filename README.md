# AR Food Chat

## Documentation
For detailed documentation, please visit: [AR Food Chat Documentation](https://docs.google.com/document/d/1nZxGlG1p7KXSEAtTChv40D7kMs2SQ1lDM1qrsHjhr4g/edit?usp=sharing)

For a demo, please visit this link: [Demo Video](https://youtu.be/OvZavwg-3Z4?si=-xdcde0flM5MaRlu)

![demo](Sample/demo.gif)

## Pre-Installation Steps
Before running the application, please make the following modification:
1. Navigate to `Packages → OpenAIUnity → Runtime → DataTypes`.
2. Locate the `CreateChatCompletionResponse` struct.
3. Add the following line of code: `public string SystemFingerprint { get; set; }`.
   
## Overview
AR Food Chat is an innovative AR application that combines advanced AR capabilities with AI-driven interaction. It enables users to engage with their surrounding environment and obtain information about items through an AR Assistant, enhancing the user experience with AI-driven conversation and speech synthesis. 

The core functionality of the app revolves around object detection, user interaction via AR, and seamless integration with OpenAI's ChatGPT, Whisper, and Amazon Polly for natural language processing and text-to-speech capabilities.

## Key Features
- **Object Detection in AR**: Utilizes Unity’s AR Foundation and Barracuda for real-time object detection and AR experiences.
- **Interactive 3D Assets**: Users can interact with 3D models (e.g., an apple) placed in the AR environment.
- **ChatGPT Integration**: On interaction, the app sends a user message prompt to ChatGPT, receiving informative responses.
- **Speech Processing**: Incorporates OpenAI's Whisper API for speech-to-text (STT) and Amazon Polly for text-to-speech (TTS) services.
- **User Interface**: Features Shader Graph, which is employed to provide highlighting visual effects on interactive objects within the AR scene. Additionally, a chatbot messaging prefab displays the history of prompts and responses between the user and ChatGPT.

## Platform and Technical Requirements
- **Platform**: iOS
- **Unity Version**: 2021.3.18
  ```plaintext
    "com.unity.xr.arfoundation": "4.2.7",
    "com.unity.barracuda": "2.0.0",
    "com.unity.shadergraph": "12.1.10"
## API Integration
- **Speech to Text**: User speech → OpenAI Whisper API (STT) → User text
- **Chat Interaction**: User text → OpenAI API (ChatGPT) → reply: ChatGPT text
- **Text to Speech**: ChatGPT text → Amazon Polly (TTS) → ChatGPT audio

### OpenAI API
1. Obtain an API key from OpenAI.
2. Pass your API key into the `OpenAIApi` instance in the `SpeechToText` and `ChatGPTManager` classes.
   - Example: `var openai = new OpenAIApi("YOUR_OPENAI_API_KEY”);`

### Amazon Polly
1. Set up an Amazon Web Services (AWS) account and create a Polly instance.
2. Obtain Access Key and Secret Key from AWS.
3. Pass these keys into the `TextToSpeech` class.
   - Example: `var credentials = new BasicAWSCredentials("YOUR_ACCESS_KEY", "YOUR_SECRET_KEY");`

> Note: Please reach out if you need assistance obtaining keys for OpenAI API and Amazon Polly.
