﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class EmojiTexture
{

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern IntPtr EmojiTexture_alloc(int size);

    [DllImport("__Internal")]
    private static extern void EmojiTexture_free(IntPtr buffer);

    [DllImport("__Internal")]
    private static extern void EmojiTexture_render(string text, IntPtr buffer, int width, int height);
#elif UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass _EmojiTextureClass;
    private static AndroidJavaClass EmojiTextureClass {
        get {
            if(_EmojiTextureClass == null){
                _EmojiTextureClass = new AndroidJavaClass("com.ibicha.emojitexture.EmojiTexture");
            }
            return _EmojiTextureClass;
        }
    }
#endif


    public string Text
    {
        get
        {
            return text;
        }
        set
        {
            if (value != text)
            {
                text = value;
#if UNITY_IOS && !UNITY_EDITOR
                EmojiTexture_render(text, buffer, texture.width, texture.height);
                isbyteBufferDirty = true;
                texture.LoadRawTextureData(buffer, bufferSize);
                texture.Apply();
#elif UNITY_ANDROID && !UNITY_EDITOR
                byteBuffer = EmojiTextureClass.CallStatic<byte[]>("render", text, texture.width, texture.height);
                isbyteBufferDirty = false;
                texture.LoadRawTextureData(byteBuffer);
                texture.Apply();
#endif
            }
        }
    }

    public byte[] ByteBuffer
    {
        get
        {
            if (isbyteBufferDirty || byteBuffer == null)
            {
                if (buffer != IntPtr.Zero && bufferSize > 0)
                {
                    Marshal.Copy(buffer, byteBuffer, 0, bufferSize);
                    isbyteBufferDirty = false;
                }
            }
            return byteBuffer;
        }
    }

    private Texture2D texture;
    private string text;
    private IntPtr buffer;
    private int bufferSize;

    private byte[] byteBuffer;
    private bool isbyteBufferDirty;

    public EmojiTexture() : this(null) { }

    public EmojiTexture(string text) : this(text, 256, 256) { }

    public EmojiTexture(string text, int size) : this(text, size, size) { }

    public EmojiTexture(int size) : this(null, size, size) { }

    public EmojiTexture(int width, int height) : this(null, width, height) { }

    public EmojiTexture(string text, int width, int height)
    {

        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
#if UNITY_IOS && !UNITY_EDITOR
        bufferSize = width * height * 4;
        buffer = EmojiTexture_alloc(bufferSize);
#else
        bufferSize = 0;
        buffer = IntPtr.Zero;
        byteBuffer = null;
        isbyteBufferDirty = false;
#endif
        Text = text;
    }

    ~EmojiTexture()
    {
#if UNITY_IOS && !UNITY_EDITOR
        if(buffer != IntPtr.Zero)
        EmojiTexture_free(buffer);
#endif
    }

    static public implicit operator Texture(EmojiTexture emojiTexture)
    {
        return emojiTexture.texture;
    }

}