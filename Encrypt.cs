using System;
using System.IO;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PgpCore;

namespace PGP_API_PS
{
    public class Encrypt
    {
        [FunctionName("Encrypt")]
        public void Run([BlobTrigger("samples-workitems/out/{name}", Connection = "AzSAConnStr")]Stream decryptedBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger Encrypt function Processed blob\n Name:{name} \n Size: {decryptedBlob.Length} Bytes");

            //Create Destimation Blob Client
            BlobContainerClient container = new BlobContainerClient(Environment.GetEnvironmentVariable("AzSAConnStr"), "samples-workitems");
            string targetBlobName = name.Substring(0, name.Length - 3);
            string targetPath = string.Concat("/out_encrypted/", targetBlobName, "pgp");
            var targetBlob = container.GetBlobClient(targetPath);

            // Load keys
            EncryptionKeys encryptionKeys = new EncryptionKeys(Environment.GetEnvironmentVariable("PGP_PUB_KEY"));

            Stream encryptedBlob = new MemoryStream();

            // Initiate PGP
            using PGP pgp = new PGP(encryptionKeys);

            // Decrypt File
            pgp.EncryptStream(decryptedBlob, encryptedBlob);

            // Reset Stream pointer to beginning
            encryptedBlob.Seek(0, SeekOrigin.Begin);

            log.LogInformation($"Encrypted {encryptedBlob.Length}");

            //Upload to Storage Account
            targetBlob.Upload(encryptedBlob);

        }
    }
}
