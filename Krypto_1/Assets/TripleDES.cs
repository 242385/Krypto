using System;
using System.Collections;
using System.Collections.Generic;
using AnotherFileBrowser.Windows;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class TripleDES : MonoBehaviour
{
    byte[] _fileBytes;
    byte[][] _blockArray;
    public string _exampleKey = "Br0W4r3k";
    BitArray _bitKey;

    int[] _initialPermutation = new int[]
    {
        58, 50, 42, 34, 26, 18, 10, 2,
        60, 52, 44, 36, 28, 20, 12, 4,
        62, 54, 46, 38, 30, 22, 14, 6,
        64, 56, 48, 40, 32, 24, 16, 8,
        57, 49, 41, 33, 25, 17, 9, 1,
        59, 51, 43, 35, 27, 19, 11, 3,
        61, 53, 45, 37, 29, 21, 13, 5,
        63, 55, 47, 39, 31, 23, 15, 7
    };

    int[] _inverseInitialPermutation = new int[]
    {
        40, 8, 48, 16, 56, 24, 64, 32,
        39, 7, 47, 15, 55, 23, 63, 31,
        38, 6, 46, 14, 54, 22, 62, 30,
        37, 5, 45, 13, 53, 21, 61, 29,
        36, 4, 44, 12, 52, 20, 60, 28,
        35, 3, 43, 11, 51, 19, 59, 27,
        34, 2, 42, 10, 50, 18, 58, 26,
        33, 1, 41, 9, 49, 17, 57, 25
    };

    int[] _pc1 = new int[]
    {
        57, 49, 41, 33, 25, 17, 9,
        1, 58, 50, 42, 34, 26, 18,
        10, 2, 59, 51, 43, 35, 27,
        19, 11, 3, 60, 52, 44, 36,
        63, 55, 47, 39, 31, 23, 15,
        7, 62, 54, 46, 38, 30, 22,
        14, 6, 61, 53, 45, 37, 29,
        21, 13, 5, 28, 20, 12, 4
    };


    public void Encrypt()
    {
    }

    public void Decrypt()
    {
    }

    void GenerateKeys()
    {
    }

    void SingleBlockEncrypting(byte[] block)
    {
        byte[] output = new byte[8];

        // Initial permutation
        for (int i = 0; i < _initialPermutation.Length; i++)
        {
            // Calculate the bit index and byte index for the input array
            int bitIndex = _initialPermutation[i] - 1;
            int byteIndex = bitIndex / 8;
            int offset = bitIndex % 8;

            // Extract the bit value from the input array and append it to the output array
            byte bitValue = (byte)((block[byteIndex] >> (7 - offset)) & 1);
            output[i / 8] |= (byte)(bitValue << (7 - (i % 8)));
        }

        // Splitting into two halves
        int halfLength = output.Length / 2;
        byte[] leftHalf = new byte[halfLength];
        byte[] rightHalf = new byte[halfLength];

        Array.Copy(output, 0, leftHalf, 0, halfLength);
        Array.Copy(output, halfLength, rightHalf, 0, halfLength);

        byte[] byteKey = ConvertStringToBytes(_exampleKey);
        int numOfBytes = (_pc1.Length + 7) / 8; // calculate the number of bytes in the output
        byte[] pc1Key = new byte[numOfBytes];

        for (int i = 0; i < _pc1.Length; i++)
        {
            int pc1BitPos = _pc1[i] - 1; // convert from 1-based index to 0-based index
            int bytePos = pc1BitPos / 8;
            int bitPos = 7 - (pc1BitPos % 8);
            byte mask = (byte)(1 << bitPos);

            if ((byteKey[bytePos] & mask) != 0)
            {
                pc1Key[i / 8] |= (byte)(1 << (7 - (i % 8)));
            }
        }

        _bitKey = new BitArray(pc1Key.Length * 8);
        for (int i = 0; i < pc1Key.Length; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                _bitKey.Set(i * 8 + j, (pc1Key[i] & (1 << (7 - j))) != 0);
            }
        }

        int halfLengthBits = _bitKey.Length / 2;
        BitArray LPT = new BitArray(halfLengthBits);
        BitArray RPT = new BitArray(halfLengthBits); 

        for (int i = 0; i < halfLengthBits; i++)
        {
            LPT[i] = _bitKey[i];
            RPT[i] = _bitKey[halfLengthBits + i];
        }
        
        DebugByteShowBits(byteKey);
        DebugBits7(_bitKey);
        DebugBits7(LPT);
        DebugBits7(RPT);
    }

    void SingleBlockDecrypting(byte[] block)
    {
    }

    byte[] ConvertStringToBytes(string s)
    {
        return Encoding.ASCII.GetBytes(s);
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
            _blockArray = SplitTo64BitBlocks(_fileBytes);
            foreach (byte[] block in _blockArray)
            {
                SingleBlockEncrypting(block);
            }
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

    byte[][] SplitTo64BitBlocks(byte[] data)
    {
        // Calculate the number of 64-bit blocks needed
        int numBlocks = (data.Length + 7) / 8;

        // Create a 2D array to hold the blocks
        byte[][] blocks = new byte[numBlocks][];
        for (int i = 0; i < numBlocks; i++)
        {
            blocks[i] = new byte[8];
        }

        // Copy the data into the blocks, padding with zeros if necessary
        for (int i = 0; i < data.Length; i++)
        {
            blocks[i / 8][i % 8] = data[i];
        }

        for (int i = data.Length; i < numBlocks * 8; i++)
        {
            blocks[i / 8][i % 8] = 0;
        }

        return blocks;
    }

    void DebugByteShowBits(byte[] bytes)
    {
        string s = "";
        foreach (byte b in bytes)
        {
            for (int i = 7; i >= 0; i--)
            {
                s += (b & (1 << i)) != 0 ? "1" : "0";
            }

            s += " ";
        }

        Debug.Log(s);
    }
    
    void DebugBits7(BitArray bits)
    {
        string s = "";
        for (int i = 0; i < bits.Length; i++)
        {
            s += bits[i] ? "1" : "0";
            if ((i + 1) % 7 == 0)
                s += " ";
        }

        Debug.Log(s);
    }
}