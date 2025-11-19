using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Backend_hackaton.Models;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DevicePropertyValue> DevicePropertyValues { get; set; }

    public virtual DbSet<Model> Models { get; set; }

    public virtual DbSet<ModelProperty> ModelProperties { get; set; }

    public virtual DbSet<OperationalLog> OperationalLogs { get; set; }

    public virtual DbSet<Property> Properties { get; set; }

    public virtual DbSet<TypeDevice> TypeDevices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=12345");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("devices_pkey");

            entity.ToTable("Device");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("ID");
            entity.Property(e => e.FkModel).HasColumnName("FK_Model");

            entity.HasOne(d => d.FkModelNavigation).WithMany(p => p.Devices)
                .HasForeignKey(d => d.FkModel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Model");
        });

        modelBuilder.Entity<DevicePropertyValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Device_property_value_pkey");

            entity.ToTable("Device_property_value");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.FkDevice).HasColumnName("FK_Device");
            entity.Property(e => e.FkModelProperty).HasColumnName("FK_Model_property");
            entity.Property(e => e.Value).HasColumnType("character varying");

            entity.HasOne(d => d.FkDeviceNavigation).WithMany(p => p.DevicePropertyValues)
                .HasForeignKey(d => d.FkDevice)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Device");

            entity.HasOne(d => d.FkModelPropertyNavigation).WithMany(p => p.DevicePropertyValues)
                .HasForeignKey(d => d.FkModelProperty)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Model_property");
        });

        modelBuilder.Entity<Model>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Model_pkey");

            entity.ToTable("Model");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.Description).HasColumnType("character varying");
            entity.Property(e => e.FkType).HasColumnName("FK_Type");
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.FkTypeNavigation).WithMany(p => p.Models)
                .HasForeignKey(d => d.FkType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Type");
        });

        modelBuilder.Entity<ModelProperty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Model_property_pkey");

            entity.ToTable("Model_property");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.FkModel).HasColumnName("FK_Model");
            entity.Property(e => e.FkProperty).HasColumnName("FK_Property");

            entity.HasOne(d => d.FkModelNavigation).WithMany(p => p.ModelProperties)
                .HasForeignKey(d => d.FkModel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Model");

            entity.HasOne(d => d.FkPropertyNavigation).WithMany(p => p.ModelProperties)
                .HasForeignKey(d => d.FkProperty)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Property");
        });

        modelBuilder.Entity<OperationalLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Operational_log_pkey");

            entity.ToTable("Operational_log");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.Comment).HasColumnType("character varying");
            entity.Property(e => e.DateOperation)
                .HasColumnType("time with time zone")
                .HasColumnName("Date_operation");
            entity.Property(e => e.FkDevice).HasColumnName("FK_Device");
            entity.Property(e => e.Place).HasColumnType("character varying");

            entity.HasOne(d => d.FkDeviceNavigation).WithMany(p => p.OperationalLogs)
                .HasForeignKey(d => d.FkDevice)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Device");
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Property_pkey");

            entity.ToTable("Property");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<TypeDevice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Type_device_pkey");

            entity.ToTable("Type_device");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.Description).HasColumnType("character varying");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
