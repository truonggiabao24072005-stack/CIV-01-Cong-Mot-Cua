using CongMotCua.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CongMotCua.Api.Models.Data;
public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<BoPhan> BoPhan => Set<BoPhan>();
    public DbSet<NguoiDung> NguoiDung => Set<NguoiDung>();
    public DbSet<ThuTuc> ThuTuc => Set<ThuTuc>();
    public DbSet<ThanhPhanYeuCau> ThanhPhanYeuCau => Set<ThanhPhanYeuCau>();
    public DbSet<HoSo> HoSo => Set<HoSo>();
    public DbSet<TaiLieuHoSo> TaiLieuHoSo => Set<TaiLieuHoSo>();
    public DbSet<KiemTraThanhPhan> KiemTraThanhPhan => Set<KiemTraThanhPhan>();
    public DbSet<YeuCauBoSung> YeuCauBoSung => Set<YeuCauBoSung>();
    public DbSet<NoiDungBoSung> NoiDungBoSung => Set<NoiDungBoSung>();
    public DbSet<LanBoSung> LanBoSung => Set<LanBoSung>();
    public DbSet<PhanCongXuLy> PhanCongXuLy => Set<PhanCongXuLy>();
    public DbSet<LichSuXuLy> LichSuXuLy => Set<LichSuXuLy>();
    public DbSet<KetQuaHoSo> KetQuaHoSo => Set<KetQuaHoSo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDb).Assembly);
    }
}
