using System.Collections.Generic;
using System.Threading.Tasks;

namespace UsosFix.Services;

public interface IMailService
{
    Task<bool> SendAsync(IEnumerable<int> subjectIds);
    Task<bool> SendAsync(int subjectId);
}