using Google.Cloud.Storage.V1;
using ClosedXML;
using ClosedXML.Excel;

namespace MagicLand_System.Services
{
    public class FirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public FirebaseStorageService(string bucketName)
        {
            _storageClient = StorageClient.Create();
            _bucketName = bucketName;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var objectName = Path.GetFileName(fileName);
            var storageObject = await _storageClient.UploadObjectAsync(
                _bucketName,
                objectName,
                contentType,
                fileStream
            );

            return storageObject.MediaLink;
        }
        public async Task<MemoryStream> GetFileAsync(string fileName)
        {
            var memoryStream = new MemoryStream();
            await _storageClient.DownloadObjectAsync(_bucketName, fileName, memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

        public List<(string, int)> GetCellPairs(MemoryStream excelStream, string sheetName, string startCellAddress, string endCellAddress)
        {
            var cellPairs = new List<(string, int)>();

            using (var workbook = new XLWorkbook(excelStream))
            {
                var worksheet = workbook.Worksheet(sheetName);
                var range = worksheet.Range(startCellAddress, endCellAddress);

                for (int row = 1; row <= range.RowCount(); row++)
                {
                    var cellA = range.Cell(row, 1);
                    var cellB = range.Cell(row, 2);

                    cellPairs.Add(new(cellA.Value.ToString(), int.Parse(cellB.Value.ToString())));
                }
            }

            return cellPairs;
        }
    }
}
