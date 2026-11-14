using CongMotCua.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CongMotCua.Api.Models.Data.Configurations;
public sealed class PhanCongXuLyConfiguration : IEntityTypeConfiguration<PhanCongXuLy>
{
    public void Configure(EntityTypeBuilder<PhanCongXuLy> entity)
    {
        entity.ToTable(t => t.UseSqlOutputClause(false));
        entity.ToTable("PhanCongXuLy");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("PhanCongId").HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.HoSoId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<HoSo>().WithMany().HasForeignKey(x => x.HoSoId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.NguoiChuyenId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.NguoiChuyenId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.CanBoNhanId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.CanBoNhanId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.BoPhanNhanId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<BoPhan>().WithMany().HasForeignKey(x => x.BoPhanNhanId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.NoiDungChuyen).HasColumnType("nvarchar(max)");
        entity.HasIndex(x => x.HoSoId).IsUnique().HasFilter("[KetThucLuc] IS NULL");
    }
}
