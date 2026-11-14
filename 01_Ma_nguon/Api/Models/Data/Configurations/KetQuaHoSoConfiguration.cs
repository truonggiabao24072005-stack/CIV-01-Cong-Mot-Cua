using CongMotCua.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CongMotCua.Api.Models.Data.Configurations;
public sealed class KetQuaHoSoConfiguration : IEntityTypeConfiguration<KetQuaHoSo>
{
    public void Configure(EntityTypeBuilder<KetQuaHoSo> entity)
    {
        entity.ToTable("KetQuaHoSo");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("KetQuaId").HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.HoSoId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<HoSo>().WithMany().HasForeignKey(x => x.HoSoId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.LoaiKetQua).HasMaxLength(255);
        entity.Property(x => x.NoiDungKetLuan).HasColumnType("nvarchar(max)");
        entity.Property(x => x.LyDoTuChoi).HasColumnType("nvarchar(max)");
        entity.Property(x => x.TepKey).HasMaxLength(255);
        entity.Property(x => x.NguoiTaoId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.NguoiTaoId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.NguoiPhatHanhId).HasMaxLength(50).IsUnicode(false);
        entity.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.NguoiPhatHanhId).OnDelete(DeleteBehavior.Restrict);
        entity.HasIndex(x => x.HoSoId).IsUnique();
        entity.ToTable(t => t.HasCheckConstraint("CK_KetQua_Loai", "[LoaiKetQua] IN ('THANH_CONG','TU_CHOI_TIEP_NHAN','TU_CHOI_GIAI_QUYET') AND ([LoaiKetQua]='THANH_CONG' OR LEN(LTRIM(RTRIM(ISNULL([LyDoTuChoi],''))))>0)"));
    }
}
