using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ParkifyAPI.Data.Contexts;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParkifyAPI.Common.Model;

namespace ParkifyAPI.Services
{
    public class ReservationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // 5 dakikada bir çalışır

        public ReservationCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ParkifyDbContext>();
                    var now = DateTime.UtcNow;

                    var expired = await context.Reservations
                        .Where(r => r.IsActive && r.EndTime < now)
                        .ToListAsync(stoppingToken);

                    foreach (var r in expired)
                    {
                        r.IsActive = false;

                        var space = await context.ParkingSpaces
                            .FirstOrDefaultAsync(ps => ps.LotId == r.LotId && ps.SpaceNumber == r.SpaceNumber, cancellationToken: stoppingToken);

                        if (space != null)
                        {
                            space.IsReserved = false;
                            space.PlateNumber = null;
                        }
                    }

                    await context.SaveChangesAsync(stoppingToken);
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}