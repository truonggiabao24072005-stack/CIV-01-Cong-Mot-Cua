using CongMotCua.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CongMotCua.Api.Models.Data.Configurations;
public sealed class TaiLieuHoSoConfiguration : IEntityTypeConfiguration<TaiLieuHoSo>
{
    public void Configure(EntityTypeBuilder<TaiLieuHoSo> entity)
    {
        entity.ToTable(t => t.UseSqlOutputClause(false));
        entity.ToTable("TaiLieuHoSo");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("TaiLieuId").HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.HoSoId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<HoSo>().WithMany().HasForeignKey(x => x.HoSoId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.ThanhPhanId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<ThanhPhanYeuCau>().WithMany().HasForeignKey(x => x.ThanhPhanId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.LanId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<LanBoSung>().WithMany().HasForeignKey(x => x.LanId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.TenTep).HasMaxLength(255);
        entity.Property(x => x.LoaiTep).HasMaxLength(255);
        entity.Property(x => x.TepKey).HasMaxLength(255);
        entity.HasIndex(x => new { x.HoSoId, x.ThanhPhanId }).IsUnique().HasFilter("[HienHanh] = 1");
        entity.HasIndex(x => x.TepKey).IsUnique();
        entity.HasIndex(x => new { x.HoSoId, x.ThanhPhanId, x.SoPhienBan }).IsUnique();
        entity.ToTable(t => t.HasCheckConstraint("CK_TaiLieu_PhienBan", "[SoPhienBan]>0 AND [DungLuong]>0"));
    }
}
