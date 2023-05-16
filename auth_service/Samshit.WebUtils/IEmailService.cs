using System.Threading.Tasks;

namespace Samshit.WebUtils
{
    public interface IEmailService
    {
        Task<bool> SendMessageAsync(IEmailMessage message);      
    }
}
