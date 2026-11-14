using CongMotCua.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CongMotCua.Api.Models.Data.Configurations;
public sealed class LanBoSungConfiguration : IEntityTypeConfiguration<LanBoSung>
{
    public void Configure(EntityTypeBuilder<LanBoSung> entity)
    {
        entity.ToTable("LanBoSung");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("LanId").HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.YeuCauId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<YeuCauBoSung>().WithMany().HasForeignKey(x => x.YeuCauId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.NoiDungGiaiTrinh).HasColumnType("nvarchar(max)");
        entity.Property(x => x.ThongTinDieuChinh).HasColumnType("nvarchar(max)");
        entity.Property(x => x.DanhGia).HasMaxLength(255);
        entity.Property(x => x.NhanXet).HasColumnType("nvarchar(max)");
        entity.Property(x => x.NguoiDanhGiaId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.NguoiDanhGiaId).OnDelete(DeleteBehavior.Restrict);
        entity.HasIndex(x => new { x.YeuCauId, x.SoLan }).IsUnique();
        entity.ToTable(t => t.HasCheckConstraint("CK_LanBoSung_SoLan", "[SoLan]>0"));
    }
}
