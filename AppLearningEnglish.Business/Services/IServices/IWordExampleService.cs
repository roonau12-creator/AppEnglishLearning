using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Models;
namespace AppLearningEnglish.Business.Services.IServices
{
    public interface IWordExampleService
    {
        Task<IEnumerable<WordExample>> GetAllWordExampleAsync();

        Task<IEnumerable<WordExample>> SearchWordExampleAsync(
            string? search,
            int? wordId);

        Task<IEnumerable<WordExample>> GetByWordIdAsync(
            int wordId);

        Task<WordExample?> GetWordExampleByIdAsync(
            int id);

        Task CreateWordExampleAsync(
            WordExample wordExample);

        Task UpdateWordExampleAsync(
            WordExample wordExample);

        Task<bool> DeleteWordExampleAsync(
            int id);

        Task<bool> IsExampleExistsAsync(
            int wordId,
            string englishSentence,
            int? id = null);
    }
}