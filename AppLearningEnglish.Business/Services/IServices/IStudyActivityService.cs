namespace AppLearningEnglish.Business.Services.IServices
{
    /// <summary>
    /// Ghi nhận một hoạt động học: cộng điểm, đồng bộ achievement
    /// và ghi chuỗi ngày học (streak) trong cùng một lần gọi.
    /// </summary>
    public interface IStudyActivityService
    {
        Task<int> TrackAsync(
            string userId,
            int points,
            int minutes);
    }
}
