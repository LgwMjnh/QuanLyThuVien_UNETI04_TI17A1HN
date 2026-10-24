using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyThuVien_UNETI04_TI17A1HN.Migrations
{
    /// <inheritdoc />
    public partial class Update5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietPhieuMuons_Sach_MaSach",
                table: "ChiTietPhieuMuons");

            migrationBuilder.DropForeignKey(
                name: "FK_PhieuMuons_DocGias_MaDocGia",
                table: "PhieuMuons");

            migrationBuilder.AlterColumn<string>(
                name: "VaiTro",
                table: "TaiKhoans",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TenDangNhap",
                table: "TaiKhoans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MatKhau",
                table: "TaiKhoans",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "TaiKhoans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "PhieuMuons",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "GioiTinh",
                table: "DocGias",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DiaChi",
                table: "DocGias",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhTrangTra",
                table: "ChiTietPhieuMuons",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_TenDangNhap",
                table: "TaiKhoans",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietPhieuMuons_Sach_MaSach",
                table: "ChiTietPhieuMuons",
                column: "MaSach",
                principalTable: "Sach",
                principalColumn: "MaSach",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PhieuMuons_DocGias_MaDocGia",
                table: "PhieuMuons",
                column: "MaDocGia",
                principalTable: "DocGias",
                principalColumn: "MaDocGia",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietPhieuMuons_Sach_MaSach",
                table: "ChiTietPhieuMuons");

            migrationBuilder.DropForeignKey(
                name: "FK_PhieuMuons_DocGias_MaDocGia",
                table: "PhieuMuons");

            migrationBuilder.DropIndex(
                name: "IX_TaiKhoans_TenDangNhap",
                table: "TaiKhoans");

            migrationBuilder.AlterColumn<string>(
                name: "VaiTro",
                table: "TaiKhoans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "TenDangNhap",
                table: "TaiKhoans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "MatKhau",
                table: "TaiKhoans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "TaiKhoans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "PhieuMuons",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "GioiTinh",
                table: "DocGias",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DiaChi",
                table: "DocGias",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TinhTrangTra",
                table: "ChiTietPhieuMuons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietPhieuMuons_Sach_MaSach",
                table: "ChiTietPhieuMuons",
                column: "MaSach",
                principalTable: "Sach",
                principalColumn: "MaSach",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PhieuMuons_DocGias_MaDocGia",
                table: "PhieuMuons",
                column: "MaDocGia",
                principalTable: "DocGias",
                principalColumn: "MaDocGia",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
