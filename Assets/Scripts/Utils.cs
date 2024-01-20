using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;

// [UnityEngine.Serialization.FormerlySerializedAs("old")]
public static class Utils
{
    public const string PLAYER_NAME_KEY = "PlayerName";
    private static readonly Regex LobbyCode = new Regex("^[0-9A-F]{8}$", RegexOptions.Compiled);

    private static readonly Regex IPv4Address =
        new Regex("^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$",
            RegexOptions.Compiled);

    public static string PlayerName = "Player";

    // min-max for Hue Saturation Value
    // random, saturated, not-too-dark, opaque color
    public static Color RandomColor => UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f, 1, 1);

    public static string GetLanAddress()
    {
        string lanIP;
#if UNITY_IOS
            lanIP =
 NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().Where(x => x.Name.Equals("en0")).First().GetIPProperties().UnicastAddresses.Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork).First().Address.ToString();
#else
        lanIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList
            .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
#endif
        return lanIP;
    }

    public static string AddressToCode(string address)
    {
        if (IPv4Address.IsMatch(address))
        {
            return String.Concat(address.Split('.').Select(x => int.Parse(x).ToString("X2")));
        }

        return address;
    }

    public static string CodeToAddress(string code)
    {
        if (LobbyCode.IsMatch(code))
        {
            int[] parts = new int[4];
            for (int i = 0; i < 4; i++)
            {
                parts[i] = int.Parse(code.Substring(2 * i, 2), NumberStyles.HexNumber);
            }

            return String.Join(".", parts);
        }

        return code;
    }
}