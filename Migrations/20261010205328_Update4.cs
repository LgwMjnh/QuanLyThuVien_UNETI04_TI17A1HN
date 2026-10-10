using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyThuVien_UNETI04_TI17A1HN.Migrations
{
    /// <inheritdoc />
    public partial class Update4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DocGias_MaTaiKhoan",
                table: "DocGias",
                column: "MaTaiKhoan",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DocGias_TaiKhoans_MaTaiKhoan",
                table: "DocGias",
                column: "MaTaiKhoan",
                principalTable: "TaiKhoans",
                principalColumn: "MaTaiKhoan",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocGias_TaiKhoans_MaTaiKhoan",
                table: "DocGias");

            migrationBuilder.DropIndex(
                name: "IX_DocGias_MaTaiKhoan",
                table: "DocGias");
        }
    }
}
