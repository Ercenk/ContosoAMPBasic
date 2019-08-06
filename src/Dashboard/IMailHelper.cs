using System;
using System.Threading.Tasks;

namespace Dashboard
{
    public interface IMailHelper
    {
        Task SendActivateEmailAsync(Guid subscriptionId);
    }
}