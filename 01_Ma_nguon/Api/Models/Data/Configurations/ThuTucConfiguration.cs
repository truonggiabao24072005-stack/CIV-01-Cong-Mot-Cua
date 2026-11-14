using CongMotCua.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CongMotCua.Api.Models.Data.Configurations;
public sealed class ThuTucConfiguration : IEntityTypeConfiguration<ThuTuc>
{
    public void Configure(EntityTypeBuilder<ThuTuc> entity)
    {
        entity.ToTable("ThuTuc");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("ThuTucId").HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.MaThuTuc).HasMaxLength(255);
        entity.HasIndex(x => x.MaThuTuc).IsUnique();
        entity.Property(x => x.TenThuTuc).HasMaxLength(255);
        entity.Property(x => x.MoTa).HasColumnType("nvarchar(max)");
        entity.Property(x => x.DonViSLA).HasMaxLength(255);
        entity.Property(x => x.QuyTacBoSung).HasMaxLength(255);
        entity.Property(x => x.BoPhanChuyenMonId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<BoPhan>().WithMany().HasForeignKey(x => x.BoPhanChuyenMonId).OnDelete(DeleteBehavior.Restrict);
        entity.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ThuTuc_SLA", "[SoNgaySLA]>0 AND [SoGioPhanHoi]>0");
            t.HasCheckConstraint("CK_ThuTuc_DonVi", "[DonViSLA] IN ('NGAY_LICH','NGAY_LAM_VIEC')");
            t.HasCheckConstraint("CK_ThuTuc_QuyTac", "[QuyTacBoSung] IN ('TINH_LAI','GIU_NGUYEN','TAM_DUNG')");
        });
    }
}
