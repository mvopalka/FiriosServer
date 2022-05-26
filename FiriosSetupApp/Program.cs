// See https://aka.ms/new-console-template for more information

using Org.BouncyCastle.Utilities.IO.Pem;
using System.Security.Cryptography;
using System.Text.Json;
using WebPush;

var vapidKeys = VapidHelper.GenerateVapidKeys();

var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP521);
var privateSec1Der = ecdsa.ExportPkcs8PrivateKey();

var stringWriter = new StringWriter();
var pemWriter = new PemWriter(stringWriter);
pemWriter.WriteObject(new PemObject("PRIVATE KEY", privateSec1Der));

//Console.WriteLine("Public VAPID key: \n{0}", vapidKeys.PublicKey);
//Console.WriteLine("Private VAPID key: \n{0}", vapidKeys.PrivateKey);
//Console.WriteLine("Private DSA key:\n{0}", stringWriter);

var hexStringDSA = Convert.ToHexString(ecdsa.ExportECPrivateKey());

var dsa = new DSA
{
    //PrivateKey = stringWriter.ToString()
    PrivateKey = hexStringDSA
};
var vapid = new VAPID
{
    privateKey = vapidKeys.PrivateKey,
    publicKey = vapidKeys.PublicKey,
    subject = "mailto:stoaler@centrum.cz"
};

var config = new FiriosConfig
{
    DSA = dsa,
    Vapid = vapid
};

Console.WriteLine(JsonSerializer.Serialize(config));

Console.ReadKey();