using CongMotCua.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CongMotCua.Api.Models.Data.Configurations;
public sealed class HoSoConfiguration : IEntityTypeConfiguration<HoSo>
{
    public void Configure(EntityTypeBuilder<HoSo> entity)
    {
        entity.ToTable("HoSo");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("HoSoId").HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.MaGui).HasMaxLength(255);
        entity.HasIndex(x => x.MaGui).IsUnique().HasFilter("[MaGui] IS NOT NULL");
        entity.Property(x => x.MaHoSo).HasMaxLength(255);
        entity.HasIndex(x => x.MaHoSo).IsUnique().HasFilter("[MaHoSo] IS NOT NULL");
        entity.Property(x => x.ChuHoSoId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.ChuHoSoId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.ThuTucId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<ThuTuc>().WithMany().HasForeignKey(x => x.ThuTucId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.ThongTinNguoiNop).HasColumnType("nvarchar(max)");
        entity.Property(x => x.NoiDungDeNghi).HasColumnType("nvarchar(max)");
        entity.Property(x => x.TrangThai).HasMaxLength(255);
        entity.Property(x => x.DonViSLA).HasMaxLength(255);
        entity.Property(x => x.QuyTacBoSung).HasMaxLength(255);
        entity.Property(x => x.LichApDung).HasColumnType("nvarchar(max)");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.HasIndex(x => new { x.ChuHoSoId, x.NgayNop });
        entity.HasIndex(x => new { x.TrangThai, x.HanTra });
        entity.ToTable(t =>
        {
            t.HasCheckConstraint("CK_HoSo_TrangThai", "[TrangThai] IN ('BAN_NHAP','DA_GUI','DA_TIEP_NHAN','DANG_GIAI_QUYET','CHO_BO_SUNG','DA_CO_KET_QUA','DA_TRA_KET_QUA','TU_CHOI_TIEP_NHAN','TU_CHOI_GIAI_QUYET')");
            t.HasCheckConstraint("CK_HoSo_Version", "[RowVersion]>=1");
            t.HasCheckConstraint("CK_HoSo_Han", "[HanTra] IS NULL OR ([NgayTiepNhan] IS NOT NULL AND [HanTra]>=[NgayTiepNhan])");
        });
    }
}
