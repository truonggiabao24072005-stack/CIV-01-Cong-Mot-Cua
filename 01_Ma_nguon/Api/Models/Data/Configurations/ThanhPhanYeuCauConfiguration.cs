using CongMotCua.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CongMotCua.Api.Models.Data.Configurations;
public sealed class ThanhPhanYeuCauConfiguration : IEntityTypeConfiguration<ThanhPhanYeuCau>
{
    public void Configure(EntityTypeBuilder<ThanhPhanYeuCau> entity)
    {
        entity.ToTable("ThanhPhanYeuCau");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("ThanhPhanId").HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.ThuTucId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<ThuTuc>().WithMany().HasForeignKey(x => x.ThuTucId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.TenThanhPhan).HasMaxLength(255);
        entity.Property(x => x.HuongDan).HasColumnType("nvarchar(max)");
    }
}
