using CongMotCua.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CongMotCua.Api.Models.Data.Configurations;
public sealed class BoPhanConfiguration : IEntityTypeConfiguration<BoPhan>
{
    public void Configure(EntityTypeBuilder<BoPhan> entity)
    {
        entity.ToTable("BoPhan");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("BoPhanId").HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.MaBoPhan).HasMaxLength(255);
        entity.HasIndex(x => x.MaBoPhan).IsUnique();
        entity.Property(x => x.TenBoPhan).HasMaxLength(255);
    }
}
