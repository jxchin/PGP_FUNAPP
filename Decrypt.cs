using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PgpCore;
using Azure.Storage.Blobs;

namespace PGP_API_PS
{
    public class Decrypt
    {
        [FunctionName("Decrypt")]
        public void Run([BlobTrigger("samples-workitems/in/{name}", Connection = "AzSAConnStr")]Stream encryptedBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger Decrypt function Processed blob\n Name:{name} \n Size: {encryptedBlob.Length} Bytes");

            //Create Destimation Blob Client
            BlobContainerClient container = new BlobContainerClient(Environment.GetEnvironmentVariable("AzSAConnStr"), "samples-workitems");
            string targetBlobName = name.Substring(0, name.Length - 3);
            string targetPath = string.Concat("/in_decrypted/", targetBlobName, "txt");
            var targetBlob = container.GetBlobClient(targetPath);

            // Load keys
            EncryptionKeys decryptionKeys = new EncryptionKeys(Environment.GetEnvironmentVariable("PGP_PRV_KEY"), Environment.GetEnvironmentVariable("PGP_PRV_KEY_PASSPHRASE"));
            Stream decryptedBlob = new MemoryStream();

            // Initiate PGP
            using PGP pgp = new PGP(decryptionKeys);

            //Verify File is Signed by Public Key
            //bool verified;
            //verified = pgp.VerifyStream(encryptedBlob);

            //log.LogInformation($"Verified: {verified}");

            //Decrypt File
            pgp.DecryptStream(encryptedBlob, decryptedBlob);

            // Reset Stream pointer to beginning
            decryptedBlob.Seek(0, SeekOrigin.Begin);

            log.LogInformation($"Decrypted {decryptedBlob.Length}");

            //Upload to Storage Account
            targetBlob.Upload(decryptedBlob);
        }
    }
}
