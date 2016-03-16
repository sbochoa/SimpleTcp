using System.Threading.Tasks;

namespace SimpleTcp
{
    public interface ITcpServer
    {
        void Start();
        void Stop();
    }
}