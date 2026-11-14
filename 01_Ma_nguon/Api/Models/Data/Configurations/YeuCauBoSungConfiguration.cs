using CongMotCua.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CongMotCua.Api.Models.Data.Configurations;
public sealed class YeuCauBoSungConfiguration : IEntityTypeConfiguration<YeuCauBoSung>
{
    public void Configure(EntityTypeBuilder<YeuCauBoSung> entity)
    {
        entity.ToTable("YeuCauBoSung");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("YeuCauId").HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.HoSoId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<HoSo>().WithMany().HasForeignKey(x => x.HoSoId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.NguoiYeuCauId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.NguoiYeuCauId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.TrangThai).HasMaxLength(255);
        entity.Property(x => x.TrangThaiQuayLai).HasMaxLength(255);
        entity.HasIndex(x => x.HoSoId).IsUnique().HasFilter("[DongLuc] IS NULL");
        entity.ToTable(t => t.HasCheckConstraint("CK_YeuCau_TrangThai", "[TrangThai] IN ('DANG_CHO','DA_GUI','DA_DAP_UNG')"));
    }
}
