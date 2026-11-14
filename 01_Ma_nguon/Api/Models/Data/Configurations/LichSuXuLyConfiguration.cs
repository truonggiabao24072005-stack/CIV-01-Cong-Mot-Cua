using CongMotCua.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CongMotCua.Api.Models.Data.Configurations;
public sealed class LichSuXuLyConfiguration : IEntityTypeConfiguration<LichSuXuLy>
{
    public void Configure(EntityTypeBuilder<LichSuXuLy> entity)
    {
        entity.ToTable(t => t.UseSqlOutputClause(false));
        entity.ToTable("LichSuXuLy");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("SuKienId").HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.HoSoId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<HoSo>().WithMany().HasForeignKey(x => x.HoSoId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.NguoiThucHienId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.NguoiThucHienId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.HanhDong).HasMaxLength(255);
        entity.Property(x => x.TrangThaiTruoc).HasMaxLength(255);
        entity.Property(x => x.TrangThaiSau).HasMaxLength(255);
        entity.Property(x => x.NoiDung).HasColumnType("nvarchar(max)");
        entity.HasIndex(x => new { x.HoSoId, x.ThoiDiem });
    }
}
