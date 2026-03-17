using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClockTrack.Models;
using ClockTrack.Services;

namespace ClockTrack.Data
{
    public class ClockTrackContext : DbContext
    {
        private readonly ITenantService? _tenantService;

        public ClockTrackContext(DbContextOptions<ClockTrackContext> options)
            : base(options)
        {
        }

        public ClockTrackContext(DbContextOptions<ClockTrackContext> options, ITenantService tenantService)
            : base(options)
        {
            _tenantService = tenantService;
        }

        // Tabela de Tenants (SaaS)
        public DbSet<ClockTrack.Models.Tenant> Tenants { get; set; } = default!;

        public DbSet<ClockTrack.Models.Department> Department { get; set; } = default!;
        public DbSet<ClockTrack.Models.Attorney> Attorney { get; set; } = default!;
        public DbSet<ClockTrack.Models.ProcessRecord> ProcessRecord { get; set; } = default!;
        public DbSet<ClockTrack.Models.Client>? Client { get; set; }
        public DbSet<ClockTrack.Models.ActivityType> ActivityTypes { get; set; } = default!;

        public DbSet<ClockTrack.Models.Mensalista> Mensalista { get; set; } = default!;
     

        public DbSet<ClockTrack.Models.PercentualArea> PercentualAreas { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar filtros globais para multi-tenancy
            // Todas as queries automaticamente filtrarão pelo TenantId
            modelBuilder.Entity<Attorney>().HasQueryFilter(e => e.TenantId == _tenantService!.GetTenantId());
            modelBuilder.Entity<Client>().HasQueryFilter(e => e.TenantId == _tenantService!.GetTenantId());
            modelBuilder.Entity<Department>().HasQueryFilter(e => e.TenantId == _tenantService!.GetTenantId());
            modelBuilder.Entity<ProcessRecord>().HasQueryFilter(e => e.TenantId == _tenantService!.GetTenantId());
            modelBuilder.Entity<Parametros>().HasQueryFilter(e => e.TenantId == _tenantService!.GetTenantId());
            modelBuilder.Entity<ValorCliente>().HasQueryFilter(e => e.TenantId == _tenantService!.GetTenantId());
            modelBuilder.Entity<Mensalista>().HasQueryFilter(e => e.TenantId == _tenantService!.GetTenantId());
            modelBuilder.Entity<PercentualArea>().HasQueryFilter(e => e.TenantId == _tenantService!.GetTenantId());
            modelBuilder.Entity<DocumentAnalysis>().HasQueryFilter(e => e.TenantId == _tenantService!.GetTenantId());
            modelBuilder.Entity<ActivityType>().HasQueryFilter(e => e.TenantId == _tenantService!.GetTenantId());

            // Configurar relacionamentos com Tenant
            modelBuilder.Entity<Attorney>()
                .HasOne(e => e.Tenant)
                .WithMany(t => t.Attorneys)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Client>()
                .HasOne(e => e.Tenant)
                .WithMany(t => t.Clients)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Department>()
                .HasOne(e => e.Tenant)
                .WithMany(t => t.Departments)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProcessRecord>()
                .HasOne(e => e.Tenant)
                .WithMany(t => t.ProcessRecords)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Parametros>()
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ValorCliente>()
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Mensalista>()
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PercentualArea>()
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DocumentAnalysis>()
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityType>()
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProcessRecord>()
                .HasOne(e => e.ActivityType)
                .WithMany(a => a.ProcessRecords)
                .HasForeignKey(e => e.ActivityTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índice único para subdomínio
            modelBuilder.Entity<Tenant>()
                .HasIndex(t => t.Subdomain)
                .IsUnique();

            // Outras configurações de modelo podem ir aqui, se necessário
        }

        public override int SaveChanges()
        {
            SetTenantId();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTenantId();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetTenantId()
        {
            if (_tenantService == null)
                return;

            var entries = ChangeTracker.Entries<ITenantEntity>()
                .Where(e => e.State == EntityState.Added);

            foreach (var entry in entries)
            {
                // Só define TenantId se ainda não foi definido (é 0)
                if (entry.Entity.TenantId == 0)
                {
                    entry.Entity.TenantId = _tenantService.GetTenantId();
                }
            }
        }

        public DbSet<ClockTrack.Models.PercentualArea>? PercentualArea { get; set; }

        public DbSet<ClockTrack.Models.ValorCliente>? ValorCliente { get; set; }

        public DbSet<ClockTrack.Models.Parametros>? Parametros { get; set; }

        public DbSet<ClockTrack.Models.DocumentAnalysis> DocumentAnalysis { get; set; } = default!;
    }
}
