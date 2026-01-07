using DexRobotPDA.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DexRobotPDA.DataModel
{
    public class DailyDbContext : DbContext
    {
        public DailyDbContext(DbContextOptions<DailyDbContext> options) : base(options)
        {
        }

        public virtual DbSet<EmployeeModel> Employees { get; set; }
        public virtual DbSet<ProductTaskModel> ProductTasks { get; set; }
        public virtual DbSet<MotorModel> Motors { get; set; }
        public virtual DbSet<ServoModel> Servos { get; set; }
        public virtual DbSet<FingerModel> Fingers { get; set; }
        public virtual DbSet<PalmModel> Palms { get; set; }
        public virtual DbSet<MotorWormDetectModel> Detect1 { get; set; }
        public virtual DbSet<SplitWormDetectModel> Detect2 { get; set; }
        public virtual DbSet<SplitCalibrateDetectModel> Detect3 { get; set; }
        public virtual DbSet<FingerCalibrateDetectModel> Detect4 { get; set; }
        public virtual DbSet<PalmCalibrateDetectModel> Detect5 { get; set; }
        public virtual DbSet<EventLogModel> EventLogs { get; set; }
        public virtual DbSet<DX023CalibrateDetectsModel> DX023CalibrateDetects { get; set; }
        public virtual DbSet<DX023FunctionalDetectsModel> DX023FunctionalDetects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}