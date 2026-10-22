
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

            //cau hinh ten dang nhap khong trung
            modelBuilder.Entity<TaiKhoan>()
                 .HasIndex(tk => tk.TenDangNhap)
                 .IsUnique();

            //cau hinh 1 tai khoan, tuong ung 1 doc gia
            modelBuilder.Entity<TaiKhoan>()
                .HasOne(tk => tk.DocGia)
                .WithOne(dg => dg.TaiKhoan)
                .HasForeignKey<DocGia>(dg => dg.MaTaiKhoan) //moi doc gia co 1 tai khoan
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); // ngan xoa tai khoan
        }
    }
}
