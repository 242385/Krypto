using System;
using System.Collections;
using System.Collections.Generic;
using AnotherFileBrowser.Windows;
using System.IO;
using UnityEngine;

public class TripleDES : MonoBehaviour
{
    private byte[] _fileBytes;
    
    public void Encrypt()
    {
        
    }

    public void Decrypt()
    {
        
    }

    public void GenerateKeys()
    {
        
    }

    public void OpenFile()
    {
        var bp = new BrowserProperties
        {
            filter = "All Files (*.*)|*.*",
            filterIndex = 0
        };

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            _fileBytes = File.ReadAllBytes(path);
        });
    }

    public void SaveFile()
    {
        var bp = new BrowserProperties
        {
            filter = "All Files (*.*)|*.*",
            filterIndex = 0
        };

        new FileBrowser().SaveFileBrowser(bp, "test", ".txt", path =>
        {
            File.WriteAllBytes(path, _fileBytes);
            Debug.Log(path);
        });
    }
}
