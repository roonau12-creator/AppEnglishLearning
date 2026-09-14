using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Models;
namespace AppLearningEnglish.Business.Services.IServices
{
    public interface IUserVocabularyService
    {
        Task<UserVocabulary?> GetAsync(
            string userId,
            int wordId);

        Task<IEnumerable<UserVocabulary>> GetAllAsync(
            string userId);

        Task<IEnumerable<UserVocabulary>> GetWordsToReviewAsync(
            string userId);

        Task<IEnumerable<UserVocabulary>> GetLearningWordsAsync(
            string userId);

        Task<IEnumerable<UserVocabulary>> GetMasteredWordsAsync(
            string userId);

        Task AddAsync(
            string userId,
            int wordId);

        Task RemoveAsync(
            string userId,
            int wordId);

        Task MarkCorrectAsync(
            string userId,
            int wordId);

        Task MarkWrongAsync(
            string userId,
            int wordId);

        Task UpdateFamiliarityAsync(
            string userId,
            int wordId,
            int familiarity);

        Task<int> GetTotalWordsAsync(
            string userId);

        Task<int> GetMasteredCountAsync(
            string userId);

        Task<int> GetReviewCountAsync(
            string userId);

        Task<int> GetVocabDailyTargetAsync(
            string userId);

        Task ToggleFavoriteAsync(string userId, int wordId);

        Task ToggleHardAsync(string userId, int wordId);

        Task RateAsync(string userId, int wordId, int quality);

        Task ResetAsync(string userId, int wordId);

        Task<IEnumerable<UserVocabulary>> GetQuickReviewAsync(string userId);
    }
}