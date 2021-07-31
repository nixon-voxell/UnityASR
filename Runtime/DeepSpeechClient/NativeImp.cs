﻿using DeepSpeechClient.Enums;

using System;
using System.Runtime.InteropServices;

namespace DeepSpeechClient
{
  /// <summary>
  /// Wrapper for the native implementation of libPath
  /// </summary>
  internal static class NativeImp
  {
    private const string libPath = "libdeepspeech";
    #region Native Implementation
    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl,
      CharSet = CharSet.Ansi, SetLastError = true)]
    internal static extern IntPtr DS_Version();

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal unsafe static extern ErrorCodes DS_CreateModel(string aModelPath,
      ref IntPtr** pint);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal unsafe static extern IntPtr DS_ErrorCodeToErrorMessage(int aErrorCode);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal unsafe static extern uint DS_GetModelBeamWidth(IntPtr** aCtx);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal unsafe static extern ErrorCodes DS_SetModelBeamWidth(IntPtr** aCtx,
      uint aBeamWidth);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal unsafe static extern ErrorCodes DS_CreateModel(string aModelPath,
      uint aBeamWidth,
      ref IntPtr** pint);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal unsafe static extern int DS_GetModelSampleRate(IntPtr** aCtx);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern ErrorCodes DS_EnableExternalScorer(IntPtr** aCtx,
      string aScorerPath);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern ErrorCodes DS_AddHotWord(IntPtr** aCtx,
      string aWord,
      float aBoost);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern ErrorCodes DS_EraseHotWord(IntPtr** aCtx,
      string aWord);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern ErrorCodes DS_ClearHotWords(IntPtr** aCtx);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern ErrorCodes DS_DisableExternalScorer(IntPtr** aCtx);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern ErrorCodes DS_SetScorerAlphaBeta(IntPtr** aCtx,
      float aAlpha,
      float aBeta);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl,
      CharSet = CharSet.Ansi, SetLastError = true)]
    internal static unsafe extern IntPtr DS_SpeechToText(IntPtr** aCtx,
      short[] aBuffer,
      uint aBufferSize);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    internal static unsafe extern IntPtr DS_SpeechToTextWithMetadata(IntPtr** aCtx,
      short[] aBuffer,
      uint aBufferSize,
      uint aNumResults);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern void DS_FreeModel(IntPtr** aCtx);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern ErrorCodes DS_CreateStream(IntPtr** aCtx,
      ref IntPtr** retval);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern void DS_FreeStream(IntPtr** aSctx);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern void DS_FreeMetadata(IntPtr metadata);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern void DS_FreeString(IntPtr str);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl,
      CharSet = CharSet.Ansi, SetLastError = true)]
    internal static unsafe extern void DS_FeedAudioContent(IntPtr** aSctx,
      short[] aBuffer,
      uint aBufferSize);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern IntPtr DS_IntermediateDecode(IntPtr** aSctx);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern IntPtr DS_IntermediateDecodeWithMetadata(IntPtr** aSctx,
      uint aNumResults);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl,
      CharSet = CharSet.Ansi, SetLastError = true)]
    internal static unsafe extern IntPtr DS_FinishStream(IntPtr** aSctx);

    [DllImport(libPath, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern IntPtr DS_FinishStreamWithMetadata(IntPtr** aSctx,
      uint aNumResults);
    #endregion
  }
}
