using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Business;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.EntityFrameworkCore;
namespace AppLearningEnglish.Business.Services
{
     public class UserVocabularyService : IUserVocabularyService
    {
        private readonly ApplicationDbContext _context;

        public UserVocabularyService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================
        // GET ONE
        // ================================

        public async Task<UserVocabulary?> GetAsync(
            string userId,
            int wordId)
        {
            var query =
                from item in _context.UserVocabularies
                where item.ApplicationUserId == userId
                      && item.WordId == wordId
                select item;

            return await query
                .Include("Word.Meanings")
                .Include("Word.Examples")
                .FirstOrDefaultAsync();
        }

        // ================================
        // GET ALL
        // ================================

        public async Task<IEnumerable<UserVocabulary>> GetAllAsync(
            string userId)
        {
            var query =
                from item in _context.UserVocabularies
                where item.ApplicationUserId == userId
                orderby item.Id descending
                select item;

            return await query
                .Include("Word.Meanings")
                .Include("Word.Examples")
                .ToListAsync();
        }

        // ================================
        // WORDS TO REVIEW
        // ================================

        public async Task<IEnumerable<UserVocabulary>>
            GetWordsToReviewAsync(string userId)
        {
            var now = DateTime.UtcNow;

            var query =
                from item in _context.UserVocabularies
                where item.ApplicationUserId == userId
                      && item.NextReviewAt != null
                      && item.NextReviewAt <= now
                orderby item.NextReviewAt
                select item;

            var limit = await _context.applicationUsers
                .Where(x => x.Id == userId)
                .Select(x => x.VocabDailyTarget)
                .FirstOrDefaultAsync();

            if (limit <= 0)
            {
                limit = 10;
            }

            return await query
                .Include("Word.Meanings")
                .Include("Word.Examples")
                .Take(limit)
                .ToListAsync();
        }

        // ================================
        // LEARNING WORDS
        // ================================

        public async Task<IEnumerable<UserVocabulary>>
            GetLearningWordsAsync(string userId)
        {
            var query =
                from item in _context.UserVocabularies
                where item.ApplicationUserId == userId
                      && item.Status != "Mastered"
                orderby item.Familiarity
                select item;

            return await query
                .Include("Word.Meanings")
                .Include("Word.Examples")
                .ToListAsync();
        }

        // ================================
        // MASTERED WORDS
        // ================================

        public async Task<IEnumerable<UserVocabulary>>
            GetMasteredWordsAsync(string userId)
        {
            var query =
                from item in _context.UserVocabularies
                where item.ApplicationUserId == userId
                      && item.Status == "Mastered"
                orderby item.LastReviewedAt descending
                select item;

            return await query
                .Include("Word.Meanings")
                .Include("Word.Examples")
                .ToListAsync();
        }

        // ================================
        // ADD WORD
        // ================================

        public async Task AddAsync(
            string userId,
            int wordId)
        {
            var existing =
                await GetAsync(userId, wordId);

            if (existing != null)
            {
                return;
            }

            var word =
                await _context.words.FindAsync(wordId);

            if (word == null)
            {
                return;
            }

            var userVocabulary =
                new UserVocabulary
                {
                    ApplicationUserId= userId,
                    WordId = wordId,
                    Status = "New",
                    Familiarity = 0,
                    CorrectCount = 0,
                    WrongCount = 0,
                    LastReviewedAt = null,
                    NextReviewAt = DateTime.UtcNow,
                    EaseFactor = SrsScheduler.DefaultEase,
                    IntervalDays = 0,
                    Repetition = 0
                };

            _context.UserVocabularies.Add(
                userVocabulary);

            await _context.SaveChangesAsync();
        }

        // ================================
        // REMOVE WORD
        // ================================

        public async Task RemoveAsync(
            string userId,
            int wordId)
        {
            var item =
                await GetAsync(userId, wordId);

            if (item == null)
            {
                return;
            }

            _context.UserVocabularies.Remove(item);

            await _context.SaveChangesAsync();
        }

        // ================================
        // MARK CORRECT
        // ================================

        public async Task MarkCorrectAsync(
            string userId,
            int wordId)
        {
            var item =
                await GetAsync(userId, wordId);

            if (item == null)
            {
                await AddAsync(userId, wordId);

                item =
                    await GetAsync(userId, wordId);
            }

            if (item == null)
            {
                return;
            }

            item.CorrectCount++;
            SrsScheduler.ApplyReview(item, quality: 4, await GetIntervalPercentAsync(userId));
            await _context.SaveChangesAsync();
        }

        // ================================
        // MARK WRONG
        // ================================

        public async Task MarkWrongAsync(
            string userId,
            int wordId)
        {
            var item =
                await GetAsync(userId, wordId);

            if (item == null)
            {
                await AddAsync(userId, wordId);

                item =
                    await GetAsync(userId, wordId);
            }

            if (item == null)
            {
                return;
            }

            item.WrongCount++;
            SrsScheduler.ApplyReview(item, quality: 1, await GetIntervalPercentAsync(userId));
            await _context.SaveChangesAsync();
        }

        // ================================
        // UPDATE FAMILIARITY
        // ================================

        public async Task UpdateFamiliarityAsync(
            string userId,
            int wordId,
            int familiarity)
        {
            if (familiarity < 0)
            {
                familiarity = 0;
            }

            if (familiarity > 5)
            {
                familiarity = 5;
            }

            var item =
                await GetAsync(userId, wordId);

            if (item == null)
            {
                return;
            }

            SrsScheduler.ApplyReview(item, quality: familiarity, await GetIntervalPercentAsync(userId));
            item.Familiarity = familiarity;
            item.Status = SrsScheduler.GetStatus(item.Familiarity, item.IntervalDays, item.Repetition);
            await _context.SaveChangesAsync();
        }

        private async Task<int> GetIntervalPercentAsync(string userId)
        {
            var percent = await _context.applicationUsers
                .Where(x => x.Id == userId)
                .Select(x => x.ReviewIntervalPercent)
                .FirstOrDefaultAsync();

            return percent <= 0 ? 100 : percent;
        }

        // ================================
        // TOTAL WORDS
        // ================================

        public async Task<int> GetTotalWordsAsync(
            string userId)
        {
            var query =
                from item in _context.UserVocabularies
                where item.ApplicationUserId == userId
                select item;

            return await query.CountAsync();
        }

        // ================================
        // MASTERED COUNT
        // ================================

        public async Task<int> GetMasteredCountAsync(
            string userId)
        {
            var query =
                from item in _context.UserVocabularies
                where item.ApplicationUserId == userId
                      && item.Status == "Mastered"
                select item;

            return await query.CountAsync();
        }

        // ================================
        // REVIEW COUNT
        // ================================

        public async Task<int> GetReviewCountAsync(
            string userId)
        {
            var now = DateTime.UtcNow;

            var query =
                from item in _context.UserVocabularies
                where item.ApplicationUserId == userId
                      && item.NextReviewAt != null
                      && item.NextReviewAt <= now
                select item;

            return await query.CountAsync();
        }

        public async Task<int> GetVocabDailyTargetAsync(
            string userId)
        {
            var target = await _context.applicationUsers
                .Where(x => x.Id == userId)
                .Select(x => x.VocabDailyTarget)
                .FirstOrDefaultAsync();

            return target > 0 ? target : 10;
        }

        public async Task RateAsync(string userId, int wordId, int quality)
        {
            var item = await GetAsync(userId, wordId);
            if (item == null)
            {
                await AddAsync(userId, wordId);
                item = await GetAsync(userId, wordId);
            }

            if (item == null)
            {
                return;
            }

            if (quality >= 3)
            {
                item.CorrectCount++;
            }
            else
            {
                item.WrongCount++;
            }

            SrsScheduler.ApplyReview(item, quality, await GetIntervalPercentAsync(userId));
            await _context.SaveChangesAsync();
        }

        public async Task ResetAsync(string userId, int wordId)
        {
            var item = await GetAsync(userId, wordId);
            if (item == null)
            {
                return;
            }

            item.Status = "New";
            item.Familiarity = 0;
            item.CorrectCount = 0;
            item.WrongCount = 0;
            item.EaseFactor = SrsScheduler.DefaultEase;
            item.IntervalDays = 0;
            item.Repetition = 0;
            item.LastReviewedAt = null;
            item.NextReviewAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserVocabulary>> GetQuickReviewAsync(string userId)
        {
            return await _context.UserVocabularies
                .Where(x => x.ApplicationUserId == userId)
                .Include("Word.Meanings")
                .Include("Word.Examples")
                .OrderBy(x => x.Familiarity)
                .ThenByDescending(x => x.WrongCount)
                .Take(12)
                .ToListAsync();
        }

        public async Task ToggleFavoriteAsync(string userId, int wordId)
        {
            var item = await GetAsync(userId, wordId);
            if (item == null)
            {
                return;
            }

            item.IsFavorite = !item.IsFavorite;
            await _context.SaveChangesAsync();
        }

        public async Task ToggleHardAsync(string userId, int wordId)
        {
            var item = await GetAsync(userId, wordId);
            if (item == null)
            {
                return;
            }

            item.IsHard = !item.IsHard;
            await _context.SaveChangesAsync();
        }
    }
}