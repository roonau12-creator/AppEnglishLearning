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
    public class TopicService : ITopicService
{
    private readonly ApplicationDbContext _context;

    public TopicService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Topic>> GetAllAsync()
    {
        return await _context.topics
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Topic?> GetTopicByIdAsync(int id)
    {
        return await _context.topics.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task CreateTopicAsync(Topic topic)
    {
        _context.topics.Add(topic);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTopicAsync(Topic topic)
    {
        _context.topics.Update(topic);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTopicAsync(int id)
    {
        var topic = await _context.topics
            .FirstOrDefaultAsync(x => x.Id == id);

        if (topic == null)
            return;

        _context.topics.Remove(topic);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsNameExistsAsync(string name, int? id = null)
    {
        return await _context.topics
            .AnyAsync(x =>
                x.Name.ToLower() == name.ToLower() &&
                (!id.HasValue || x.Id != id.Value));
    }

       
    }
}