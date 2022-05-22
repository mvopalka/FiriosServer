using System.Security.Cryptography;

namespace FiriosServer.Services;

public class FiriosSourceAuthentificationService
{
    private HashAlgorithmName _hashAlgorithm = HashAlgorithmName.SHA512;
    private Dictionary<string, string> AuthDb { get; set; } = new Dictionary<string, string>();
    private readonly IConfiguration _configuration;
    private ECDsa _ecdsa;

    public FiriosSourceAuthentificationService(IConfiguration configuration)
    {
        _configuration = configuration;
        _ecdsa = ECDsa.Create();
        string conf = _configuration["DSA:PrivateKey"];
        var b = StringToByteArray(conf);
        int a;
        _ecdsa.ImportECPrivateKey(b, out a);
    }

    public static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
    }
    public static string ByteArrayToString(byte[] bytes)
    {
        var hex = Convert.ToHexString(bytes);
        if (hex.Length % 2 != 0)
        {
            hex = "0" + hex;
        }

        return hex;
    }


    public IDictionary<string, bool> GetIds()
    {
        var ids = new Dictionary<string, bool>();
        foreach (var pair in AuthDb)
        {
            var isAuthenticated = !string.IsNullOrEmpty(pair.Value);
            ids.Add(pair.Key, isAuthenticated);
        }
        return ids;
    }

    public bool Validate(string id, string signature)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }
        if (!AuthDb.ContainsKey(id))
        {
            AuthDb.Add(id, "");
        }
        if (string.IsNullOrEmpty(signature))
        {
            return false;
        }

        if (AuthDb[id] == signature)
        {
            return true;
        }

        var idBytes = StringToByteArray(id);
        var signatureBytes = StringToByteArray(signature);

        bool isValid = _ecdsa.VerifyData(idBytes, signatureBytes, _hashAlgorithm);
        if (isValid)
        {
            AuthDb[id] = signature;
        }

        return isValid;
    }

    public string GetValidationString(string id)
    {
        if (AuthDb.ContainsKey(id) && !string.IsNullOrEmpty(AuthDb[id]))
        {
            return AuthDb[id];
        }

        return "";
    }

    public string GenerateSignature(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return "";
        }
        var idBytes = StringToByteArray(id);
        var signatureBytes = _ecdsa.SignData(idBytes, _hashAlgorithm);
        var signature = ByteArrayToString(signatureBytes);
        AuthDb[id] = signature;
        return signature;
    }
}