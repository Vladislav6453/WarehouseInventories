using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using Library.DB;
namespace WarehouseInventory.Data.Controllers;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
        
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        /*Database.EnsureDeleted();*/
        Database.EnsureCreated();
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeRole> EmployeeRoles { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceType> InvoiceTypes { get; set; }

    public virtual DbSet<MovementType> MovementTypes { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<StockMovement> StockMovements { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=192.168.200.13;user=student;password=student;database=DBWarehouseInventory", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.3.39-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(255);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.RoleId, "FK_User_UserRole_Id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Login).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.RoleId).HasColumnType("int(11)");

            entity.HasOne(d => d.Role).WithMany(p => p.Employees)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_UserRole_Id");
        });

        modelBuilder.Entity<EmployeeRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Role).HasMaxLength(50);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.CustomerId, "FK_Invoice_Customer_Id");

            entity.HasIndex(e => e.EmployeeId, "FK_Invoice_Employee_Id");

            entity.HasIndex(e => e.SupplierId, "FK_Invoice_Supplier_Id");

            entity.HasIndex(e => e.TypeId, "FK_Invoice_TypeInvoice_Id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.CustomerId).HasColumnType("int(11)");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.EmployeeId).HasColumnType("int(11)");
            entity.Property(e => e.Number).HasMaxLength(25);
            entity.Property(e => e.SupplierId).HasColumnType("int(11)");
            entity.Property(e => e.TotalAmount).HasPrecision(19, 2);
            entity.Property(e => e.TypeId).HasColumnType("int(11)");

            entity.HasOne(d => d.Customer).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Invoice_Customer_Id");

            entity.HasOne(d => d.Employee).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invoice_Employee_Id");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK_Invoice_Supplier_Id");

            entity.HasOne(d => d.InvoiceType).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invoice_TypeInvoice_Id");
        });

        modelBuilder.Entity<InvoiceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Type).HasMaxLength(255);
        });

        modelBuilder.Entity<MovementType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.CategoryId, "FK_Products_Categories_Id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.CategoryId).HasColumnType("int(11)");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Price).HasPrecision(19, 2);
            entity.Property(e => e.Quantity).HasColumnType("int(11)");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories_Id");
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.InvoiceId, "FK_StockMovements_Invoice_Id");

            entity.HasIndex(e => e.MovementTypeId, "FK_StockMovements_MovemenntType_Id");

            entity.HasIndex(e => e.ProductId, "FK_StockMovements_Products_Id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.InvoiceId).HasColumnType("int(11)");
            entity.Property(e => e.MovementTypeId).HasColumnType("int(11)");
            entity.Property(e => e.ProductId).HasColumnType("int(11)");
            entity.Property(e => e.Quantity).HasColumnType("int(11)");

            entity.HasOne(d => d.Invoice).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockMovements_Invoice_Id");

            entity.HasOne(d => d.MovementType).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.MovementTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockMovements_MovemenntType_Id");

            entity.HasOne(d => d.Product).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockMovements_Products_Id");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(255);
        });

        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<EmployeeRole>().HasData(
            new EmployeeRole{Id=1,Role = "Админ"},
            new EmployeeRole{Id=2,Role = "Сотрудник"},
            new EmployeeRole{Id=3,Role = "Менеджер"}
        );
            
        modelBuilder.Entity<MovementType>().HasData(
            new MovementType{Id=1,Name = "Приход"},
            new MovementType{Id=2,Name = "Расход"}
        );
        
        modelBuilder.Entity<InvoiceType>().HasData(
            new InvoiceType{Id=1,Type = "Приходная"},
            new InvoiceType{Id=2,Type = "Расходная"}
        );
        
        modelBuilder.Entity<Product>().HasData(
            new Product{Id=1,Name = "Смартфон iPhone 15", Description = "", Price = 15000, Quantity = 1, CategoryId = 1 },
            new Product{Id=2,Name = "Ноутбук MacBook Air", Description = "", Price = 999000, Quantity = 0, CategoryId = 1 },
            new Product{Id=3,Name = "Мышь Logitech", Description = "", Price = 2500, Quantity = 2, CategoryId = 2 }
        );
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
