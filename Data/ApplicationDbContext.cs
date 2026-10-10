
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
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

            modelBuilder.Entity<TaiKhoan>()
                .HasOne(tk => tk.DocGia)
                .WithOne(dg => dg.TaiKhoan)
                .HasForeignKey<DocGia>(dg => dg.MaTaiKhoan)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
