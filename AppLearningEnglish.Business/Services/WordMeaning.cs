using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models;
using Microsoft.EntityFrameworkCore;
using AppLearningEnglish.DataAccess.Data;
namespace AppLearningEnglish.Business.Services
{
     public class WordMeaningService : IWordMeaningService
    {
        private readonly ApplicationDbContext _context;

        public WordMeaningService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================
        // GET ALL
        // =====================================

        public async Task<IEnumerable<WordMeaning>>
            GetAllWordMeaningAsync()
        {
            var query =
                from wordMeaning in _context.wordMeanings
                orderby wordMeaning.WordId ascending
                select wordMeaning;

            return await query
                .Include("Word")
                .ToListAsync();
        }

        // =====================================
        // SEARCH + FILTER
        // =====================================

        public async Task<IEnumerable<WordMeaning>>
            SearchWordMeaningAsync(
                string? search,
                int? wordId,
                string? language)
        {
            IQueryable<WordMeaning> query =
                _context.wordMeanings;

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword =
                    search.Trim().ToLower();

                query =
                    from wordMeaning in query
                    where
                        (
                            wordMeaning.Meaning
                                .ToLower()
                                .Contains(keyword)
                        )
                        ||
                        (
                            wordMeaning.Word != null
                            &&
                            wordMeaning.Word.WordText
                                .ToLower()
                                .Contains(keyword)
                        )
                    select wordMeaning;
            }

            // FILTER WORD
            if (wordId.HasValue &&
                wordId.Value > 0)
            {
                int selectedWordId =
                    wordId.Value;

                query =
                    from wordMeaning in query
                    where wordMeaning.WordId
                          == selectedWordId
                    select wordMeaning;
            }

            // FILTER LANGUAGE
            if (!string.IsNullOrWhiteSpace(language))
            {
                string selectedLanguage =
                    language.Trim();

                query =
                    from wordMeaning in query
                    where wordMeaning.Language
                          == selectedLanguage
                    select wordMeaning;
            }

            query =
                from wordMeaning in query
                orderby wordMeaning.WordId ascending
                select wordMeaning;

            return await query
                .Include("Word")
                .ToListAsync();
        }

        // =====================================
        // GET BY WORD
        // =====================================

        public async Task<IEnumerable<WordMeaning>>
            GetByWordIdAsync(int wordId)
        {
            var query =
                from wordMeaning in _context.wordMeanings
                where wordMeaning.WordId == wordId
                orderby wordMeaning.Language ascending
                select wordMeaning;

            return await query
                .Include("Word")
                .ToListAsync();
        }

        // =====================================
        // GET BY ID
        // =====================================

        public async Task<WordMeaning?>
            GetWordMeaningByIdAsync(int id)
        {
            var query =
                from wordMeaning in _context.wordMeanings
                where wordMeaning.Id == id
                select wordMeaning;

            return await query
                .Include("Word")
                .FirstOrDefaultAsync();
        }

        // =====================================
        // CREATE
        // =====================================

        public async Task CreateWordMeaningAsync(
            WordMeaning wordMeaning)
        {
            wordMeaning.Language =
                wordMeaning.Language.Trim();

            wordMeaning.Meaning =
                wordMeaning.Meaning.Trim();

            _context.wordMeanings.Add(wordMeaning);

            await _context.SaveChangesAsync();
        }

        // =====================================
        // UPDATE
        // =====================================

        public async Task UpdateWordMeaningAsync(
            WordMeaning wordMeaning)
        {
            var existingWordMeaning =
                await GetWordMeaningByIdAsync(
                    wordMeaning.Id);

            if (existingWordMeaning == null)
                return;

            existingWordMeaning.WordId =
                wordMeaning.WordId;

            existingWordMeaning.Language =
                wordMeaning.Language.Trim();

            existingWordMeaning.Meaning =
                wordMeaning.Meaning.Trim();

            await _context.SaveChangesAsync();
        }

        // =====================================
        // DELETE
        // =====================================

        public async Task<bool> DeleteWordMeaningAsync(
            int id)
        {
            var wordMeaning =
                await GetWordMeaningByIdAsync(id);

            if (wordMeaning == null)
                return false;

            _context.wordMeanings.Remove(
                wordMeaning);

            await _context.SaveChangesAsync();

            return true;
        }

        // =====================================
        // CHECK DUPLICATE
        // =====================================

        public async Task<bool> IsMeaningExistsAsync(
            int wordId,
            string language,
            string meaning,
            int? id = null)
        {
            string normalizedLanguage =
                language.Trim().ToLower();

            string normalizedMeaning =
                meaning.Trim().ToLower();

            var query =
                from wordMeaning in _context.wordMeanings
                where wordMeaning.WordId == wordId
                      &&
                      wordMeaning.Language
                          .ToLower()
                          == normalizedLanguage
                      &&
                      wordMeaning.Meaning
                          .ToLower()
                          == normalizedMeaning
                select wordMeaning;

            if (id.HasValue)
            {
                query =
                    from wordMeaning in query
                    where wordMeaning.Id != id.Value
                    select wordMeaning;
            }

            return await query.AnyAsync();
        }
    }
}