using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Models;
namespace AppLearningEnglish.Business.Services.IServices
{
    public interface ITopicService
{
    Task<IEnumerable<Topic>> GetAllAsync();

    Task<Topic?> GetTopicByIdAsync(int id);

    Task CreateTopicAsync(Topic topic);

    Task UpdateTopicAsync(Topic topic);

    Task DeleteTopicAsync(int id);

    Task<bool> IsNameExistsAsync(string name, int? id = null);
}
}