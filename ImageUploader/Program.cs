using Azure.Storage.Blobs;
using System.Threading.Tasks;

// Cấu hình
string containerName = "images"; // Tên container nguồn để trigger function
var imageFiles = Directory.GetFiles("images");
int concurrentUploads = 50; // Số lượng upload song song
int totalUploads = 200; // Tổng số lần upload
var random = new Random();

var blobContainerClient = new BlobContainerClient(connectionString, containerName);
await blobContainerClient.CreateIfNotExistsAsync();

var tasks = new List<Task>();

for (int i = 0; i < totalUploads; i++)
{
    string filePath = imageFiles[random.Next(imageFiles.Length)];
    string blobName = $"testupload_{i}_{Path.GetFileName(filePath)}";
    tasks.Add(UploadFileAsync(blobContainerClient, filePath, blobName));
    if (tasks.Count == concurrentUploads)
    {
        await Task.WhenAll(tasks);
        tasks.Clear();
    }
}

if (tasks.Count > 0)
    await Task.WhenAll(tasks);

Console.WriteLine("Done!");

static async Task UploadFileAsync(BlobContainerClient container, string filePath, string blobName)
{
    using var stream = File.OpenRead(filePath);
    await container.UploadBlobAsync(blobName, stream);
    Console.WriteLine($"Uploaded: {blobName}");
}