/*
 * FileName: SaveManager.cs
 * Author: zhiyan114
 * Description: This file handles all the save data in the game.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using ProtoBuf;
using System.Reflection;
using UnityEngine;
using System.ComponentModel;

// add [ProtoMap(DisableMap = true)] to Dictionary in order to fix IL2CPP compilation error
// add [DefaultValue(true)] if any value are not default
[ProtoContract]
public class SaveData
{
    [ProtoMember(1)]
    public int HighScore { get; set; } = 0;
    [ProtoMember(2)]
    public int Balance { get; set; } = 0;
    [ProtoMember(3)]
    public bool FastSpeed { get; set; } = false;
    [ProtoMember(4)]
    [DefaultValue(true)]
    public bool DiscordPresence { get; set; } = true;
    [ProtoMember(5)]
    [DefaultValue(1)]
    public int CurrentSkin { get; set; }  = 1;
    [ProtoMember(6)]
    public List<int> OwnedSkin { get; set; } = new List<int>() { 1 };

}
static public class SaveManager
{
    static private string FilePath = "Default_Save.bin";
    static public string SavePath
    {
        set { FilePath = value; }
        get { return FilePath; }
    }
    static private byte[] AesKey = new byte[16];
    static public byte[] SetKey
    {
        set
        {
            switch (value.Length)
            {
                case 16:
                    break;
                case 24:
                    break;
                case 32:
                    break;
                default:
                    throw new CryptoException("AES Key size must be either 16, 24, 32 byte long.");
            }
            AesKey = value;
        }
    }
    static public SaveData Data = new SaveData { };

    /*
     * Description: Save the data into into a save file
     * Return: bool - If save is successful or not
     */
    static public bool SaveToDisk()
    {
        try
        {
            if(SaveFileExist())
                File.Delete(FilePath);
            using (FileStream SaveFile = new FileStream(FilePath, FileMode.Create))
            {
                byte[] RandIV = new byte[12];
                new System.Random().NextBytes(RandIV);
                SaveFile.Write(RandIV, 0, RandIV.Length);
                BufferedAeadBlockCipher buffblockcipher = new BufferedAeadBlockCipher(new GcmBlockCipher(new AesEngine()));
                buffblockcipher.Init(true, new AeadParameters(new KeyParameter(AesKey), 128, RandIV));
                using (CipherStream cryptstream = new CipherStream(SaveFile, null, buffblockcipher))
                    Serializer.Serialize(cryptstream, Data);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return false;
        }
        return true;
    }
    /*
     * Description: Loads the save data from the disk and update SaveData dictionary accordingly. Probably should only be ran once per game session.
     * Return:
     *  true - the data has been successfully loaded
     *  false - The data cannot be loaded
     */
    static public bool LoadFromDisk()
    {
        if (!SaveFileExist()) return false;
        try
        {
            using (FileStream SaveFile = new FileStream(FilePath, FileMode.Open))
            {
                byte[] IV = new byte[12];
                SaveFile.Read(IV, 0, IV.Length);
                BufferedAeadBlockCipher buffblockcipher = new BufferedAeadBlockCipher(new GcmBlockCipher(new AesEngine()));
                buffblockcipher.Init(false, new AeadParameters(new KeyParameter(AesKey), 128, IV));
                using (CipherStream cryptstream = new CipherStream(SaveFile, buffblockcipher, null))
                {
                    SaveData internalDat = Serializer.Deserialize<SaveData>(cryptstream);
                    foreach (PropertyInfo prop in internalDat.GetType().GetProperties())
                        Data.GetType().GetProperty(prop.Name).SetValue(Data, prop.GetValue(internalDat, null));
                }
            }
        } catch (FileNotFoundException)
        {
            // Save files aren't existed yet lol
            return false;
        }
        catch (InvalidCipherTextException)
        {
            // Someone prob tampered with the save file
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return false;
        }
        return true;
    }
    /*
     * Description: Check if the save file exists
     * Return: bool - if the file exist or not
     */
    static public bool SaveFileExist()
    {
        return File.Exists(FilePath);
    }
    /*
     * Description: Delete the save file with the option to delete the loaded save as well
     * Args: DeleteLoadedSave - Weather or not to reset the "Data" variable.
     * Return:
     *  true - the files has been successfully 
     *  false - The save file doesn't exist. (even if the Data was reset to default)
     */
    static public bool DeleteSave(bool DeleteLoadedSave = true)
    {
        if (DeleteLoadedSave)
            Data = new SaveData { };
        if (!SaveFileExist())
            return false;
        File.Delete(FilePath);
        return true;
    }
}