/*
This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software Foundation,
Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.

The Original Code is Copyright (C) 2020 Voxell Technologies.
All rights reserved.
*/

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using DeepSpeechClient;
using DeepSpeechClient.Models;
using Voxell.Inspector;

namespace Voxell.Speech
{
  public class AutomaticSpeechRecognition : MonoBehaviour
  {
    public int frequency = 44100;
    public int duration = 10;
    public string modelpath;
    public DeepSpeech deepSpeech;
    public DeepSpeechStream deepSpeechStream;
    [InspectOnly] public AudioClip audioClip;
    public AudioSource audioSource;

    private int lastPos, pos;
    private bool continueDecode = false;
    private Thread thread;
    private List<float> audioSample;
    private bool sampleIsFree = false;

    void Start()
    {
      duration = Mathf.Max(1, duration);
      deepSpeech = new DeepSpeech(modelpath);
      deepSpeechStream = deepSpeech.CreateStream();
      audioClip = Microphone.Start(null, true, duration, frequency);
      audioSource.clip = audioClip;
      audioSample = new List<float>();

      continueDecode = true;
      sampleIsFree = false;
      thread = new Thread(new ThreadStart(DecodeTask));
      thread.Start();
    }

    void Update()
    {
      if(!audioSource.isPlaying) audioSource.Play();
      if ((pos = Microphone.GetPosition(null)) > 0)
      {
        if (lastPos > pos) lastPos = 0;
        if (pos - lastPos > 0)
        {
          // Allocate the space for the sample.
          float[] sample = new float[(pos - lastPos) * audioClip.channels];

          // Get the data from microphone.
          audioClip.GetData(sample, lastPos);
          audioSample.AddRange(sample);
          lastPos = pos; 
          sampleIsFree = true;
        }
      }
    }

    void DecodeTask()
    {
      int decodePos = 0;
      try
      {
        while (continueDecode)
        {
          if (decodePos < audioSample.Count && sampleIsFree)
          {
            short[] sampleShorts = AudioFloatToInt16(audioSample.GetRange(decodePos, audioSample.Count-decodePos));
            deepSpeech.FeedAudioContent(deepSpeechStream, sampleShorts, (uint)sampleShorts.Length);
            string output = deepSpeech.IntermediateDecode(deepSpeechStream);
            print(output);
            decodePos = audioSample.Count;
            sampleIsFree = false;
          }
        }
      } catch (ThreadAbortException)
      {
        Debug.Log("DecodeTask aborted.");
      } finally
      {
        print(audioSample.Count);
        string finalSentence = deepSpeech.FinishStream(deepSpeechStream);
        print(finalSentence);
      }
    }

    private static short[] AudioFloatToInt16(List<float> data)
    {
      Int16 maxValue = Int16.MaxValue;
      short[] shorts = new short[data.Count];

      for (int i=0; i < shorts.Length; i++)
        shorts[i] = Convert.ToInt16 (data[i] * maxValue);

      return shorts;
    }

    void OnDisable()
    {
      continueDecode = false;
      thread.Join();
      deepSpeech?.FreeStream(deepSpeechStream);
      deepSpeech?.Dispose();
    }
  }
}