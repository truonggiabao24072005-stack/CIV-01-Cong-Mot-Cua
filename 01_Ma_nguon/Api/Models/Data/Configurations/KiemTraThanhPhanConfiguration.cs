using CongMotCua.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CongMotCua.Api.Models.Data.Configurations;
public sealed class KiemTraThanhPhanConfiguration : IEntityTypeConfiguration<KiemTraThanhPhan>
{
    public void Configure(EntityTypeBuilder<KiemTraThanhPhan> entity)
    {
        entity.ToTable("KiemTraThanhPhan");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("KiemTraId").HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.TaiLieuId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<TaiLieuHoSo>().WithMany().HasForeignKey(x => x.TaiLieuId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.CanBoId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.CanBoId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.KetQua).HasMaxLength(255);
        entity.Property(x => x.NhanXet).HasColumnType("nvarchar(max)");
        entity.HasIndex(x => new { x.TaiLieuId, x.ThoiDiem });
        entity.ToTable(t => t.HasCheckConstraint("CK_KiemTra_KetQua", "[KetQua] IN ('DAT','CHUA_DAT')"));
    }
}
