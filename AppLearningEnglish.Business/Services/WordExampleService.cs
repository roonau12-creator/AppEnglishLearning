using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models;
using AppLearningEnglish.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
namespace AppLearningEnglish.Business.Services
{
    public class WordExampleService : IWordExampleService
    {
        private readonly ApplicationDbContext _context;

        public WordExampleService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET ALL
        // ==========================================

        public async Task<IEnumerable<WordExample>>
            GetAllWordExampleAsync()
        {
            var query =
                from example in _context.wordExamples
                orderby example.WordId ascending
                select example;

            return await query
                .Include("Word")
                .ToListAsync();
        }

        // ==========================================
        // SEARCH
        // ==========================================

        public async Task<IEnumerable<WordExample>>
            SearchWordExampleAsync(
                string? search,
                int? wordId)
        {
            IQueryable<WordExample> query =
                _context.wordExamples;

            // Search English + Vietnamese
            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword =
                    search.Trim().ToLower();

                query =
                    from example in query
                    where
                        example.EnglishSentence
                            .ToLower()
                            .Contains(keyword)
                        ||
                        example.VietnameseMeaning
                            .ToLower()
                            .Contains(keyword)
                        ||
                        (
                            example.Word != null
                            &&
                            example.Word.WordText
                                .ToLower()
                                .Contains(keyword)
                        )
                    select example;
            }

            // Filter Word
            if (wordId.HasValue &&
                wordId.Value > 0)
            {
                int selectedWordId =
                    wordId.Value;

                query =
                    from example in query
                    where example.WordId ==
                          selectedWordId
                    select example;
            }

            query =
                from example in query
                orderby example.WordId ascending
                select example;

            return await query
                .Include("Word")
                .ToListAsync();
        }

        // ==========================================
        // GET BY WORD
        // ==========================================

        public async Task<IEnumerable<WordExample>>
            GetByWordIdAsync(int wordId)
        {
            var query =
                from example in _context.wordExamples
                where example.WordId == wordId
                orderby example.Id ascending
                select example;

            return await query
                .Include("Word")
                .ToListAsync();
        }

        // ==========================================
        // GET BY ID
        // ==========================================

        public async Task<WordExample?>
            GetWordExampleByIdAsync(int id)
        {
            var query =
                from example in _context.wordExamples
                where example.Id == id
                select example;

            return await query
                .Include("Word")
                .FirstOrDefaultAsync();
        }

        // ==========================================
        // CREATE
        // ==========================================

        public async Task CreateWordExampleAsync(
            WordExample wordExample)
        {
            wordExample.EnglishSentence =
                wordExample.EnglishSentence.Trim();

            wordExample.VietnameseMeaning =
                wordExample.VietnameseMeaning.Trim();

            _context.wordExamples.Add(
                wordExample);

            await _context.SaveChangesAsync();
        }

        // ==========================================
        // UPDATE
        // ==========================================

        public async Task UpdateWordExampleAsync(
            WordExample wordExample)
        {
            var existingExample =
                await GetWordExampleByIdAsync(
                    wordExample.Id);

            if (existingExample == null)
                return;

            existingExample.WordId =
                wordExample.WordId;

            existingExample.EnglishSentence =
                wordExample.EnglishSentence.Trim();

            existingExample.VietnameseMeaning =
                wordExample.VietnameseMeaning.Trim();

            await _context.SaveChangesAsync();
        }

        // ==========================================
        // DELETE
        // ==========================================

        public async Task<bool> DeleteWordExampleAsync(
            int id)
        {
            var example =
                await GetWordExampleByIdAsync(id);

            if (example == null)
                return false;

            _context.wordExamples.Remove(example);

            await _context.SaveChangesAsync();

            return true;
        }

        // ==========================================
        // CHECK DUPLICATE
        // ==========================================

        public async Task<bool> IsExampleExistsAsync(
            int wordId,
            string englishSentence,
            int? id = null)
        {
            string normalizedSentence =
                englishSentence
                    .Trim()
                    .ToLower();

            var query =
                from example in _context.wordExamples
                where example.WordId == wordId
                      &&
                      example.EnglishSentence
                          .ToLower()
                          == normalizedSentence
                select example;

            if (id.HasValue)
            {
                query =
                    from example in query
                    where example.Id != id.Value
                    select example;
            }

            return await query.AnyAsync();
        }
    }
}