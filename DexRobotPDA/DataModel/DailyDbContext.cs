using DexRobotPDA.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DexRobotPDA.DataModel
{
    public class DailyDbContext : DbContext
    {
        public DailyDbContext(DbContextOptions<DailyDbContext> options) : base(options){}
        public virtual DbSet<TeamModel> Teams { get; set; }
        public virtual DbSet<EmployeeModel> Employees { get; set; }
        public virtual DbSet<ProductTaskModel> ProductTasks { get; set; }
        public virtual DbSet<ProductionBatchModel> ProductionBatches { get; set; }
        public virtual DbSet<MotorModel> Motors { get; set; }
        public virtual DbSet<FingerModel> Fingers { get; set; }
        public virtual DbSet<SplitModel> Splits { get; set; }
        public virtual DbSet<PalmModel> Palms { get; set; }
        public virtual DbSet<MaterialModel> Materials { get; set; }
        public virtual DbSet<MotorWormDetectModel> Detect1 { get; set; }
        public virtual DbSet<SplitWormDetectModel> Detect2 { get; set; }
        public virtual DbSet<SplitCalibrateDetectModel> Detect3 { get; set; }
        public virtual DbSet<FingerCalibrateDetectModel> Detect4 { get; set; }
        public virtual DbSet<PalmCalibrateDetectModel> Detect5 { get; set; }
        public virtual DbSet<EventLogModel> EventLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置TeamModel
            modelBuilder.Entity<TeamModel>(entity =>
            {
                entity.HasKey(t => t.id);
                entity.HasIndex(t => t.team_id).IsUnique();
            });

            // 配置EmployeeModel
            modelBuilder.Entity<EmployeeModel>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.HasIndex(t => t.employee_id).IsUnique();
                entity.HasOne(e => e.team)
                    .WithMany(t => t.Employees)
                    .HasForeignKey(e => e.team_id)
                    .HasPrincipalKey(t => t.team_id);
            });
            
            // 配置ProductTaskModel
            modelBuilder.Entity<ProductTaskModel>(entity =>
            {
                entity.HasKey(t => t.id);
                entity.HasIndex(t => t.task_id).IsUnique(); 
                entity.HasOne(p => p.Assignee)  
                      .WithMany(e => e.ProductTasks) 
                      .HasForeignKey(p => p.assignee_id)  
                      .HasPrincipalKey(e => e.employee_id) 
                      .OnDelete(DeleteBehavior.SetNull); 
            });
            
            // 配置ProductionBatchModel
            modelBuilder.Entity<ProductionBatchModel>(entity =>
            {
                entity.HasKey(t => t.id);
                entity.HasIndex(t => t.batch_number).IsUnique(); 
                
                // 配置与ProductTaskModel的关系
                entity.HasOne(e => e.ProductTask)
                    .WithMany(t => t.ProductionBatches)
                    .HasForeignKey(e => e.task_id)
                    .HasPrincipalKey(t => t.task_id);
                entity.HasOne(e => e.Team)
                    .WithMany(t => t.Batches)
                    .HasForeignKey(e => e.team_id)
                    .HasPrincipalKey(t => t.team_id) 
                    .OnDelete(DeleteBehavior.SetNull);
            });
            
            // 配置MotorModel - 在这里集中配置所有关系
            modelBuilder.Entity<MotorModel>(entity =>
            {
                entity.HasKey(t => t.id);
                entity.HasIndex(t => t.motor_id).IsUnique(); 
                
                entity.HasOne(m => m.WormMaterial)
                    .WithMany(mat => mat.MotorsAsWormMaterial) 
                    .HasForeignKey(m => m.worm_material_id)
                    .HasPrincipalKey(t => t.material_id) 
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(m => m.AdhesiveMaterial)
                    .WithMany(mat => mat.MotorsAsAdhesiveMaterial) 
                    .HasForeignKey(m => m.adhesive_material_id)
                    .HasPrincipalKey(t => t.material_id) 
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(m => m.TaskModel)
                    .WithMany(b => b.Motors)
                    .HasForeignKey(m => m.task_id)
                    .HasPrincipalKey(t => t.task_id);

                entity.HasOne(m => m.Operator)
                    .WithMany(e => e.Motors)
                    .HasForeignKey(m => m.operator_id)
                    .HasPrincipalKey(t => t.employee_id) 
                    .IsRequired(false);
                
                entity.HasOne(m => m.Finger)
                    .WithMany(f => f.Motors)
                    .HasForeignKey(m => m.finger_id)
                    .HasPrincipalKey(f => f.finger_id) 
                    .IsRequired(false);
                
                entity.HasOne(m => m.MotorMaterial)
                    .WithOne(mt => mt.MotorMaterial)
                    .HasForeignKey<MotorModel>(m => m.motor_id)
                    .HasPrincipalKey<MaterialModel>(mt => mt.material_id);
            });
            
            // 配置FingerModel
            modelBuilder.Entity<FingerModel>(entity =>
            {
                entity.HasKey(t => t.id);
                entity.HasIndex(t => t.finger_id).IsUnique(); 
                
                entity.HasOne(m => m.Palm)
                    .WithMany(p => p.Fingers)
                    .HasForeignKey(m => m.palm_id)
                    .HasPrincipalKey(p => p.palm_id) 
                    .IsRequired(false);
                
                entity.HasOne(m => m.TaskModel)
                    .WithMany(b => b.Fingers)
                    .HasForeignKey(m => m.task_id)
                    .HasPrincipalKey(t => t.task_id);

                entity.HasOne(m => m.Operator)
                    .WithMany(e => e.Fingers)
                    .HasForeignKey(m => m.operator_id)
                    .HasPrincipalKey(t => t.employee_id) 
                    .IsRequired(false);
                
                entity.HasOne(m => m.FingerMaterial)
                    .WithOne(mt => mt.FingerShellMaterial)
                    .HasForeignKey<FingerModel>(m => m.finger_id)
                    .HasPrincipalKey<MaterialModel>(mt => mt.material_id);
            });
            
            // 配置SplitModel
            modelBuilder.Entity<SplitModel>(entity =>
            {
                entity.HasKey(t => t.id);
                entity.HasIndex(t => t.split_id).IsUnique(); 
                entity.HasOne(s => s.TaskModel)
                    .WithMany(t => t.Splits) // 假设ProductTaskModel有Splits集合
                    .HasForeignKey(s => s.task_id)
                    .HasPrincipalKey(t => t.task_id)
                    .OnDelete(DeleteBehavior.Restrict); // 防止级联删除任务时删除Split

                // 关联操作员（EmployeeModel）
                entity.HasOne(s => s.Operator)
                    .WithMany(e => e.Splits) // 假设EmployeeModel有Splits集合
                    .HasForeignKey(s => s.operator_id)
                    .HasPrincipalKey(e => e.employee_id)
                    .OnDelete(DeleteBehavior.Restrict);

                // 关联物料表（MaterialModel）
                entity.HasOne(s => s.SplitMaterial)
                    .WithOne(m => m.SplitMaterial) // 假设MaterialModel有Split导航属性
                    .HasForeignKey<SplitModel>(s => s.split_id) // 外键为split_id
                    .HasPrincipalKey<MaterialModel>(m => m.material_id)
                    .OnDelete(DeleteBehavior.Restrict);

                // 一对一关联PalmModel（核心配置）
                entity.HasOne(s => s.Palm)
                    .WithOne(p => p.Split)
                    .HasForeignKey<SplitModel>(s => s.palm_id) // 外键在SplitModel
                    .HasPrincipalKey<PalmModel>(p => p.palm_id)
                    .OnDelete(DeleteBehavior.SetNull); // 若Palm删除，Split的palm_i
                
            });
            
            // 配置PalmModel
            modelBuilder.Entity<PalmModel>(entity =>
            {
                entity.HasKey(t => t.id);
                entity.HasIndex(t => t.palm_id).IsUnique(); 
                entity.HasOne(m => m.TaskModel)
                    .WithMany(b => b.Palms)
                    .HasForeignKey(m => m.task_id)
                    .HasPrincipalKey(t => t.task_id);
                
                entity.HasOne(p => p.Split)
                    .WithOne(s => s.Palm)
                    .HasForeignKey<SplitModel>(s => s.palm_id)
                    .HasPrincipalKey<PalmModel>(p => p.palm_id);
                
                entity.HasOne(m => m.Operator)
                    .WithMany(e => e.Palms)
                    .HasForeignKey(m => m.operator_id)
                    .HasPrincipalKey(t => t.employee_id) 
                    .IsRequired(false);
                
                entity.HasOne(m => m.PalmMaterial)
                    .WithOne(mt => mt.PalmShellMaterial)
                    .HasForeignKey<PalmModel>(m => m.palm_id)
                    .HasPrincipalKey<MaterialModel>(mt => mt.material_id);
            });

            // 配置MaterialModel
            modelBuilder.Entity<MaterialModel>(entity =>
            {
                entity.HasKey(t => t.id);
                entity.HasIndex(t => t.material_id).IsUnique();
            });
            
            modelBuilder.Entity<MotorWormDetectModel>(entity =>
            {
                entity.HasKey(t => t.id);
                
                entity.HasOne(m => m.Motor)
                    .WithMany(m => m.Detect1)
                    .HasForeignKey(m => m.motor_id)
                    .HasPrincipalKey(t => t.motor_id)
                    .IsRequired(false);
                
                entity.HasOne(m => m.Inspector)
                    .WithMany(m => m.Detect1)
                    .HasForeignKey(m => m.inspector_id)
                    .HasPrincipalKey(t => t.employee_id)
                    .IsRequired(false);
            });
        }
    }
}
    