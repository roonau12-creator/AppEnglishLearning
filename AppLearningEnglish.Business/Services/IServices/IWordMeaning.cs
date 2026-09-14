using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Models;
namespace AppLearningEnglish.Business.Services.IServices
{
    public interface IWordMeaningService
    {
        Task<IEnumerable<WordMeaning>> GetAllWordMeaningAsync();

        Task<IEnumerable<WordMeaning>> SearchWordMeaningAsync(
            string? search,
            int? wordId,
            string? language);

        Task<IEnumerable<WordMeaning>> GetByWordIdAsync(
            int wordId);

        Task<WordMeaning?> GetWordMeaningByIdAsync(
            int id);

        Task CreateWordMeaningAsync(
            WordMeaning wordMeaning);

        Task UpdateWordMeaningAsync(
            WordMeaning wordMeaning);

        Task<bool> DeleteWordMeaningAsync(
            int id);

        Task<bool> IsMeaningExistsAsync(
            int wordId,
            string language,
            string meaning,
            int? id = null);
    }
}