using CongMotCua.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CongMotCua.Api.Models.Data.Configurations;
public sealed class NguoiDungConfiguration : IEntityTypeConfiguration<NguoiDung>
{
    public void Configure(EntityTypeBuilder<NguoiDung> entity)
    {
        entity.ToTable("NguoiDung");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("NguoiDungId").HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.Issuer).HasMaxLength(255);
        entity.Property(x => x.Subject).HasMaxLength(255);
        entity.Property(x => x.HoTen).HasMaxLength(255);
        entity.Property(x => x.SoDienThoai).HasMaxLength(255);
        entity.Property(x => x.Email).HasMaxLength(255);
        entity.Property(x => x.BoPhanId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<BoPhan>().WithMany().HasForeignKey(x => x.BoPhanId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.VaiTro).HasMaxLength(255);
        entity.HasIndex(x => new { x.Issuer, x.Subject }).IsUnique();
        entity.ToTable(t => t.HasCheckConstraint("CK_NguoiDung_VaiTro", "[VaiTro] IN ('NGUOI_NOP','TIEP_NHAN','GIAI_QUYET')"));
    }
}
