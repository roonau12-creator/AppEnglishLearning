using AppLearningEnglish.Business.Services.IServices;

namespace AppLearningEnglish.Business.Services
{
    public class StudyActivityService : IStudyActivityService
    {
        private readonly IAchievementService _achievementService;
        private readonly IDailyStreakService _dailyStreakService;
        private readonly IProductHubService _product;

        public StudyActivityService(
            IAchievementService achievementService,
            IDailyStreakService dailyStreakService,
            IProductHubService product)
        {
            _achievementService = achievementService;
            _dailyStreakService = dailyStreakService;
            _product = product;
        }

        public async Task<int> TrackAsync(
            string userId,
            int points,
            int minutes)
        {
            var total = await _achievementService.AddPointsAsync(userId, points);

            await _dailyStreakService.RecordStudyAsync(userId, minutes, points);
            await _product.AwardStudyAsync(userId, points, minutes);

            return total;
        }
    }
}
