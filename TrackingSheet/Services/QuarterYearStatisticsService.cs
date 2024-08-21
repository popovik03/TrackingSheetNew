using Microsoft.EntityFrameworkCore;
using TrackingSheet.Data;
using TrackingSheet.Models;

namespace TrackingSheet.Services
{
    public class QuarterYearStatisticsService
    {
        private readonly MVCDbContext _context;

        public QuarterYearStatisticsService(MVCDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProblemTypeStatisticsViewModel>> GetIncidentStatisticsASync(int year, int quarter)
        {
            DateTime startDate;
            DateTime endDate;

            // Определяем начальную и конечную даты в зависимости от квартала
            if (quarter == 5)
            {
                // Для полного года
                startDate = new DateTime(year, 1, 1);
                endDate = new DateTime(year, 12, 31);
            }
            else
            {
                // Для кварталов
                startDate = new DateTime(year, (quarter - 1) * 3 + 1, 1);
                endDate = startDate.AddMonths(3).AddDays(-1);
            }

            var incidentStatistics = await _context.IncidentList
                .Where(i => i.Date >= startDate && i.Date <= endDate)
                .GroupBy(i => i.ProblemType)
                .Select(g => new ProblemTypeStatisticsViewModel
                {
                    ProblemType = g.Key,
                    Count = g.Count(),
                    SuccessCount = g.Count(i => i.Status == "Success"),
                    FailCount = g.Count(i => i.Status == "Fail"),
                    TotalSuccessFailCount = g.Count(i => i.Status == "Success") + g.Count(i => i.Status == "Fail"),
                    SavedNPTCount = (int)g.Sum(i => i.SavedNPT)
                })
                .ToListAsync();

            return incidentStatistics;
        }
    }
}
