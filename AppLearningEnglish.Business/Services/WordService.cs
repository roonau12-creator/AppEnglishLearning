using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.EntityFrameworkCore;
namespace AppLearningEnglish.Business.Services
{
    public class WordService : IWordService
    {
        private readonly ApplicationDbContext _context;

        public WordService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL
        // =========================

        public async Task<IEnumerable<Word>> GetAllWordAsync()
        {
            var query =
                from word in _context.words
                orderby word.WordText ascending
                select word;

            return await query.ToListAsync();
        }

        // =========================
        // SEARCH
        // =========================

        public async Task<IEnumerable<Word>> SearchWordAsync(
            string? search,
            string? partOfSpeech)
        {
            IQueryable<Word> query = _context.words;

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword =
                    search.Trim().ToLower();

                query =
                    from word in query
                    where word.WordText
                              .ToLower()
                              .Contains(keyword)
                       || (
                            word.Definition != null
                            && word.Definition
                                .ToLower()
                                .Contains(keyword)
                          )
                       || (
                            word.Pronunciation != null
                            && word.Pronunciation
                                .ToLower()
                                .Contains(keyword)
                          )
                    select word;
            }

            // Filter Part Of Speech
            if (!string.IsNullOrWhiteSpace(partOfSpeech))
            {
                string selectedPartOfSpeech =
                    partOfSpeech.Trim();

                query =
                    from word in query
                    where word.PartOfSpeech ==
                          selectedPartOfSpeech
                    select word;
            }

            query =
                from word in query
                orderby word.WordText ascending
                select word;

            return await query.ToListAsync();
        }

        // =========================
        // GET BY ID
        // =========================

        public async Task<Word?> GetWordByIdAsync(int id)
        {
            var query =
                from word in _context.words
                where word.Id == id
                select word;

            return await query
                .Include("Lesson")
                .Include("Meanings")
                .Include("Examples")
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Word>> GetWordsByLessonIdAsync(int lessonId)
        {
            var query =
                from word in _context.words
                where word.LessonId == lessonId
                orderby word.WordText
                select word;

            return await query
                .Include("Meanings")
                .Include("Examples")
                .Include("Lesson")
                .ToListAsync();
        }

        // =========================
        // CREATE
        // =========================

        public async Task CreateWordAsync(Word word)
        {
            word.WordText =
                word.WordText.Trim();

            _context.words.Add(word);

            await _context.SaveChangesAsync();
        }

        // =========================
        // UPDATE
        // =========================

        public async Task UpdateWordAsync(Word word)
        {
            var existingWord =
                await GetWordByIdAsync(word.Id);

            if (existingWord == null)
                return;

            existingWord.WordText =
                word.WordText.Trim();

            existingWord.Pronunciation =
                word.Pronunciation;

            existingWord.PartOfSpeech =
                word.PartOfSpeech;

            existingWord.Definition =
                word.Definition;

            existingWord.AudioUrl =
                word.AudioUrl;

            existingWord.ImageUrl =
                word.ImageUrl;

            existingWord.LessonId =
                word.LessonId;

            await _context.SaveChangesAsync();
        }

        // =========================
        // DELETE
        // =========================

        public async Task<bool> DeleteWordAsync(int id)
        {
            var word =
                await GetWordByIdAsync(id);

            if (word == null)
                return false;

            _context.words.Remove(word);

            await _context.SaveChangesAsync();

            return true;
        }

        // =========================
        // CHECK DUPLICATE
        // =========================

        public async Task<bool> IsWordExistsAsync(
            string wordText,
            int? id = null)
        {
            string normalizedWord =
                wordText.Trim().ToLower();

            var query =
                from word in _context.words
                where word.WordText.ToLower()
                    == normalizedWord
                select word;

            if (id.HasValue)
            {
                query =
                    from word in query
                    where word.Id != id.Value
                    select word;
            }

            return await query.AnyAsync();
        }
    }
}