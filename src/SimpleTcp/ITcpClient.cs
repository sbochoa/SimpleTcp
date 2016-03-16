using System.Net.Sockets;
using System.Threading.Tasks;

namespace SimpleTcp
{
    public interface ITcpClient
    {
        void Close();
        void Connect();
        void Send(byte[] data);
        Task SendAsync(byte[] data);
    }
}