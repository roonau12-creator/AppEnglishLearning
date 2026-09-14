using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Models;
namespace AppLearningEnglish.Business.Services.IServices
{
    public interface IWordService
    {
        Task<IEnumerable<Word>> GetAllWordAsync();

        Task<IEnumerable<Word>> SearchWordAsync(
            string? search,
            string? partOfSpeech);

        Task<Word?> GetWordByIdAsync(int id);

        Task<IEnumerable<Word>> GetWordsByLessonIdAsync(int lessonId);

        Task CreateWordAsync(Word word);

        Task UpdateWordAsync(Word word);

        Task<bool> DeleteWordAsync(int id);

        Task<bool> IsWordExistsAsync(
            string wordText,
            int? id = null);
    }
}