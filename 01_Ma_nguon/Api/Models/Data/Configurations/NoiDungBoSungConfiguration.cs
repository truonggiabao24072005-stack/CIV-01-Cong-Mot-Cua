using CongMotCua.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CongMotCua.Api.Models.Data.Configurations;
public sealed class NoiDungBoSungConfiguration : IEntityTypeConfiguration<NoiDungBoSung>
{
    public void Configure(EntityTypeBuilder<NoiDungBoSung> entity)
    {
        entity.ToTable(t => t.UseSqlOutputClause(false));
        entity.ToTable("NoiDungBoSung");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("NoiDungId").HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.YeuCauId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<YeuCauBoSung>().WithMany().HasForeignKey(x => x.YeuCauId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.ThanhPhanId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<ThanhPhanYeuCau>().WithMany().HasForeignKey(x => x.ThanhPhanId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.TruongThongTin).HasMaxLength(255);
        entity.Property(x => x.VanDe).HasColumnType("nvarchar(max)");
        entity.Property(x => x.LyDo).HasColumnType("nvarchar(max)");
        entity.Property(x => x.HuongDan).HasColumnType("nvarchar(max)");
    }
}
