// code belongs to https://github.com/Babilinski/deep-speech-unity

using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DeepSpeechClient;
using DeepSpeechClient.Interfaces;
using DeepSpeechClient.Models;
using Voxell;
using Voxell.Inspector;

public class ContinuousSpeechRecognition : MonoBehaviour
{
  [SerializeField] public KeyCode pushToTalkKey = KeyCode.Space;

  [SerializeField] public bool autoDetectVoice = false;

  [Header("DeepSpeech")]
  [StreamingAssetFilePath] public string modelPath;
  [StreamingAssetFilePath] public string externalScorerPath;
  [InspectOnly] public string resultText;

  [Header("Voice Detection Settings")]
  [SerializeField, Tooltip("Time in seconds of detected silence before voice request is sent")]
  protected float _silenceTimer = 1.0f;

  [SerializeField, Tooltip("The minimum volume to detect voice input for"), Range(0.0f, 1.0f)]
  protected float _minimumSpeakingSampleValue = 0.5f;

  // Audio Detection
  private float _timeAtSilenceBegan;
  private bool _audioDetected;
  private bool _clearData;
  private float[] _lastBuffer;

  // Audio Stuff
  [InspectOnly] public AudioClip _clip = null;
  private string _device = null;

  // Conditions
  private bool transmitToggled = false;
  private bool _recording = false;

  // Sampling 
  private int _previousPosition = 0;
  private int _sampleIndex = 0;
  private int _recordFrequency = 0;
  private int _recordSampleSize = 0;
  private int _targetFrequency = 0;
  private int _targetSampleSize = 0;
  private float[] _sampleBuffer = null;
  private int _frequency;
  private int _sampleSize;

  // Deep Speech 
  private IDeepSpeech _sttClient;

  /// <summary>
  /// Stream used to feed data into the acoustic model.
  /// </summary>
  private DeepSpeechStream _sttStream;

  // Threading
  private bool _hasStream;
  private readonly ConcurrentQueue<short[]> _threadedBufferQueue = new ConcurrentQueue<short[]>();
  private int _threadSafeBoolBackValue = 0;

  public bool StreamingIsBusy
  {
    get => (Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 1, 1) == 1);
    set
    {
      if (value) Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 1, 0);
      else Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 0, 1);
    }
  }

  private void Start()
  {
    _sttClient = new DeepSpeech(FileUtilx.GetStreamingAssetFilePath(modelPath));
    _sttClient.EnableExternalScorer(FileUtilx.GetStreamingAssetFilePath(externalScorerPath));
    // If you want to add bias to a word. Only supports single words.
    //_sttClient.AddHotWord("select",20.0f);

    // You can also add a negative value
    //_sttClient.AddHotWord("cue",-250.0f);

    _frequency = _sttClient.GetModelSampleRate();
    _sampleSize = _frequency / 2;

    StartRecording();
  }
  private void FixedUpdate()
  {
    if (!_recording)
    {
      return;
    }

    bool transmit = transmitToggled || Input.GetKey(pushToTalkKey);
    int currentPosition = Microphone.GetPosition(_device);

    // This means we wrapped around
    if (currentPosition < _previousPosition)
    {
      while (_sampleIndex < _recordFrequency)
      {
        ReadSample(transmit);
      }

      _sampleIndex = 0;
    }

    // Read non-wrapped samples
    _previousPosition = currentPosition;

    while (_sampleIndex + _recordSampleSize <= currentPosition)
    {
      ReadSample(transmit);
    }
  }

  void Resample(float[] src, float[] dst)
  {
    if (src.Length == dst.Length)
    {
      Array.Copy(src, 0, dst, 0, src.Length);
    }
    else
    {
      //TODO: Low-pass filter 
      float rec = 1.0f / (float) dst.Length;

      for (int i = 0; i < dst.Length; ++i)
      {
        float interp = rec * (float) i * (float) src.Length;
        dst[i] = src[(int) interp];
      }
    }
  }

  void ReadSample(bool transmit)
  {
    // Extract data
    _clip.GetData(_sampleBuffer, _sampleIndex);
    // Grab a new sample buffer
    float[] targetSampleBuffer = new float[_sampleSize];

    // Resample our real sample into the buffer
    Resample(_sampleBuffer, targetSampleBuffer);


    // Forward index
    _sampleIndex += _recordSampleSize;

    // Auto detect speech, but no need to do if we're pushing a key to transmit
    if (autoDetectVoice && !transmit)
    {
      // Determine if the microphone noise levels have been loud enough
      float maxVolume = 0.0f;
      for (int i = 0; i < _sampleBuffer.Length; i++)
      {
        if (_sampleBuffer[i] > maxVolume)
        {
          maxVolume = _sampleBuffer[i];
        }
      }

      if (maxVolume >= _minimumSpeakingSampleValue)
      {
        transmit = true;
        _audioDetected = true;
        _clearData = false;
      }
      else
      {
        if (_audioDetected == true) // User first stopped talking after talking
        {
          _timeAtSilenceBegan = Time.time;
          _audioDetected = false;
        }
        else
        {
          if (Time.time - _timeAtSilenceBegan > _silenceTimer)
          {
            _audioDetected = false;
            _clearData = true;
          }
          else
          {
            transmit = true;
          }
        }
      }
    }

    // If we have an event, and 
    if (transmit && !_clearData)
    {
      if (_hasStream == false)
      {
        _sttStream = _sttClient.CreateStream();
        _hasStream = true;
      }

      if (_lastBuffer != null)
      {
        float[] longArray = new float[_lastBuffer.Length + targetSampleBuffer.Length];
        Array.Copy(_lastBuffer, longArray, _lastBuffer.Length);
        Array.Copy(targetSampleBuffer, 0, longArray, _lastBuffer.Length, targetSampleBuffer.Length);
        TransmitBuffer(longArray);
        _lastBuffer = null;
      }
      else
      {
        TransmitBuffer(targetSampleBuffer);
      }
    }
    else if (_clearData || !autoDetectVoice)
    {
      if (_hasStream && StreamingIsBusy == false)
      {
        _hasStream = false;
        _sttClient.FreeStream(_sttStream);
        _lastBuffer = targetSampleBuffer;
      }
    }
  }

  void TransmitBuffer(float[] buffer)
  {
    short[] sampleShorts = AudioFloatToInt16(buffer);
    _threadedBufferQueue.Enqueue(sampleShorts);
    if (StreamingIsBusy == false)
    {
      Task.Run(ThreadedWork).ConfigureAwait(false);
    }
  }

  private async void ThreadedWork()
  {
    StreamingIsBusy = true;
    while (_threadedBufferQueue.Count > 0)
    {
      if (_threadedBufferQueue.TryDequeue(out short[] voiceResult))
      {
        _sttClient.FeedAudioContent(_sttStream, voiceResult, Convert.ToUInt32(voiceResult.Length));
        string output = _sttClient.IntermediateDecode(_sttStream);
        // debugging logs, TODO: remove this
        if (output != resultText) Debug.Log(resultText);
        resultText = output;
        await Task.Delay(10);
      }
    }

    StreamingIsBusy = false;
  }
  private static short[] AudioFloatToInt16(float[] data)
  {
    Int16 maxValue = short.MaxValue;
    short[] shorts = new short[data.Length];

    for (int i = 0; i < shorts.Length; i++)
      shorts[i] = Convert.ToInt16(data[i] * maxValue);

    return shorts;
  }

  public bool StartRecording()
  {
    if (_recording)
    {
      Debug.LogError("Already recording");
      return false;
    }

    _targetFrequency = _frequency;
    _targetSampleSize = _sampleSize;

    Microphone.GetDeviceCaps(_device, out var minFreq, out var maxFreq);

    _recordFrequency = minFreq == 0 && maxFreq == 0 ? 44100 : maxFreq;
    _recordSampleSize = _recordFrequency / (_targetFrequency / _targetSampleSize);

    _clip = Microphone.Start(_device, true, 1, _recordFrequency);
    _sampleBuffer = new float[_recordSampleSize];
    _recording = true;

    _sttStream = _sttClient.CreateStream();
    return _recording;
  }
}