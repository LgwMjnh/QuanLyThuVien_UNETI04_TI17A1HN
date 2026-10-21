using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<TheLoai> TheLoais { get; set; }
        public DbSet<Sach> Sachs { get; set; }
        public DbSet<DocGia> DocGias { get; set; }
        public DbSet<PhieuMuon> PhieuMuons { get; set; }
        public DbSet<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tai khoan - Doc gia
            modelBuilder.Entity<TaiKhoan>()
                .HasOne(tk => tk.DocGia)
                .WithOne(dg => dg.TaiKhoan)
                .HasForeignKey<DocGia>(dg => dg.MaTaiKhoan)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Phieu muon - Doc gia
            modelBuilder.Entity<PhieuMuon>()
                .HasOne(pm => pm.DocGia)
                .WithMany()
                .HasForeignKey(pm => pm.MaDocGia)
                .OnDelete(DeleteBehavior.Restrict);

            // Cau hinh bang chi tiet phieu muon
            modelBuilder.Entity<ChiTietPhieuMuon>(entity =>
            {
                entity.ToTable("ChiTietPhieuMuons");

                // Khoa chinh gom MaPhieuMuon va MaSach
                entity.HasKey(ct => new
                {
                    ct.MaPhieuMuon,
                    ct.MaSach
                });

                // Anh xa ten cot so luong
                entity.Property(ct => ct.SoLuong)
                    .HasColumnName("SoLuongMuon");

                // Chi tiet phieu muon - Phieu muon
                entity.HasOne(ct => ct.PhieuMuon)
                    .WithMany(pm => pm.ChiTietPhieuMuons)
                    .HasForeignKey(ct => ct.MaPhieuMuon)
                    .OnDelete(DeleteBehavior.Cascade);

                // Chi tiet phieu muon - Sach
                entity.HasOne(ct => ct.Sach)
                    .WithMany()
                    .HasForeignKey(ct => ct.MaSach)
                    .OnDelete(DeleteBehavior.Restrict);
            });

      
            modelBuilder.Entity<TaiKhoan>()
                .ToTable("TaiKhoans");

            modelBuilder.Entity<TheLoai>()
                .ToTable("TheLoais");

            modelBuilder.Entity<Sach>()
                .ToTable("Sach");

            modelBuilder.Entity<DocGia>()
                .ToTable("DocGias");

            modelBuilder.Entity<PhieuMuon>()
                .ToTable("PhieuMuons");
        }
    }
}