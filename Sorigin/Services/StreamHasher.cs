using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sorigin.Services
{
    public interface IStreamHasher
    {
        Task<string> CalculateHashAsync(Stream stream);
    }

    public class StreamHasher : IStreamHasher
    {
        public async Task<string> CalculateHashAsync(Stream stream)
        {
            long originalPosition = stream.Position;
            stream.Position = 0;

            using SHA256 sha = SHA256.Create();
            byte[] fileHash = await sha.ComputeHashAsync(stream);
            string fileHashAsStr = SerializeBytes(fileHash);

            stream.Position = originalPosition;
            return fileHashAsStr;
        }

        public static string SerializeBytes(byte[] value)
        {
            StringBuilder sb = new();
            foreach (byte b in value)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}