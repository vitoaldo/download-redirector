using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace download_redirector.Services;

public static class CryptoHelper
{
    private const string RegistryKeyPath = @"SOFTWARE\DownloadRedirector\AIKeys";

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DATA_BLOB
    {
        public int cbData;
        public IntPtr pbData;
    }

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool CryptProtectData(ref DATA_BLOB pDataIn, string szDataDescr, IntPtr pOptionalEntropy, IntPtr pvReserved, IntPtr pPromptStruct, int dwFlags, ref DATA_BLOB pDataOut);

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool CryptUnprotectData(ref DATA_BLOB pDataIn, IntPtr ppszDataDescr, IntPtr pOptionalEntropy, IntPtr pvReserved, IntPtr pPromptStruct, int dwFlags, ref DATA_BLOB pDataOut);

    public static void SaveEncryptedKey(string providerName, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            key.DeleteValue(providerName, false);
            return;
        }

        byte[] plainBytes = Encoding.UTF8.GetBytes(apiKey);
        DATA_BLOB plainBlob = new DATA_BLOB { cbData = plainBytes.Length, pbData = Marshal.AllocHGlobal(plainBytes.Length) };
        Marshal.Copy(plainBytes, 0, plainBlob.pbData, plainBytes.Length);
        DATA_BLOB cipherBlob = new DATA_BLOB();

        try
        {
            if (CryptProtectData(ref plainBlob, "", IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, ref cipherBlob))
            {
                byte[] cipherBytes = new byte[cipherBlob.cbData];
                Marshal.Copy(cipherBlob.pbData, cipherBytes, 0, cipherBlob.cbData);
                
                using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
                key.SetValue(providerName, cipherBytes, RegistryValueKind.Binary);
            }
        }
        finally
        {
            if (plainBlob.pbData != IntPtr.Zero) Marshal.FreeHGlobal(plainBlob.pbData);
            if (cipherBlob.pbData != IntPtr.Zero) Marshal.FreeHGlobal(cipherBlob.pbData);
        }
    }

    public static string GetDecryptedKey(string providerName)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (key?.GetValue(providerName) is byte[] cipherBytes)
            {
                DATA_BLOB cipherBlob = new DATA_BLOB { cbData = cipherBytes.Length, pbData = Marshal.AllocHGlobal(cipherBytes.Length) };
                Marshal.Copy(cipherBytes, 0, cipherBlob.pbData, cipherBytes.Length);
                DATA_BLOB plainBlob = new DATA_BLOB();

                try
                {
                    if (CryptUnprotectData(ref cipherBlob, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, ref plainBlob))
                    {
                        byte[] plainBytes = new byte[plainBlob.cbData];
                        Marshal.Copy(plainBlob.pbData, plainBytes, 0, plainBlob.cbData);
                        return Encoding.UTF8.GetString(plainBytes);
                    }
                }
                finally
                {
                    if (cipherBlob.pbData != IntPtr.Zero) Marshal.FreeHGlobal(cipherBlob.pbData);
                    if (plainBlob.pbData != IntPtr.Zero) Marshal.FreeHGlobal(plainBlob.pbData);
                }
            }
        }
        catch { }
        return string.Empty;
    }
}
