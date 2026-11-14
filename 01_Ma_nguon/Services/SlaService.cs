using CongMotCua.Api.Models;
using CongMotCua.Api.Models.Requests;
using CongMotCua.Api.Filters;
using CongMotCua.Api.Models.Data;
using System.Text.Json;

namespace CongMotCua.Services;
public record CalendarSnapshot(string[] Holidays, string TimeZone = "Asia/Ho_Chi_Minh");
public class SlaService(IConfiguration config)
{
    public void Capture(HoSo h, ThuTuc t, DateTime now)
    {
        h.SoNgaySLA = t.SoNgaySLA;
        h.DonViSLA = t.DonViSLA;
        h.QuyTacBoSung = t.QuyTacBoSung;
        h.LichApDung = JsonSerializer.Serialize(new CalendarSnapshot(config.GetSection("Sla:NgayNghi").Get<string[]>() ?? []));
        h.MocTinhSLA = now;
        h.NgayTiepNhan = now;
        h.HanTraGoc = h.HanTra = Add(now, h);
    }

    public static DateTime Add(DateTime start, HoSo h, int? count = null)
    {
        if (h.SoNgaySLA is null or <= 0 || string.IsNullOrWhiteSpace(h.LichApDung))
            throw new BusinessError("Thiếu cấu hình thời hạn của hồ sơ.");
        var calendar = JsonSerializer.Deserialize<CalendarSnapshot>(h.LichApDung) ?? throw new BusinessError("Lịch áp dụng không hợp lệ.");
        var local = DateTime.SpecifyKind(start, DateTimeKind.Utc).AddHours(7);
        var remaining = count ?? h.SoNgaySLA.Value;
        if (remaining < 0 || remaining > 3650)
            throw new BusinessError("Thời hạn nằm ngoài phạm vi cho phép.");
        while (remaining > 0)
        {
            local = local.AddDays(1);
            if (Eligible(local, h.DonViSLA!, calendar))
                remaining--;
        }

        return DateTime.SpecifyKind(local.AddHours(-7), DateTimeKind.Utc);
    }

    static bool Eligible(DateTime d, string unit, CalendarSnapshot c) => unit == "NGAY_LICH" || (d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday && !c.Holidays.Contains(d.ToString("yyyy-MM-dd")));
    public static void Resume(HoSo h, YeuCauBoSung y, LanBoSung accepted)
    {
        if (h.NgayTiepNhan == null)
            return;
        var calendar = JsonSerializer.Deserialize<CalendarSnapshot>(h.LichApDung!)!;
        if (h.QuyTacBoSung == "TINH_LAI")
            h.HanTra = Add(accepted.GuiLuc, h);
        else if (h.QuyTacBoSung == "TAM_DUNG")
        {
            var d = y.TaoLuc.AddHours(7).Date;
            var end = accepted.GuiLuc.AddHours(7).Date;
            var count = 0;
            while (d < end)
            {
                d = d.AddDays(1);
                if (Eligible(d, h.DonViSLA!, calendar))
                    count++;
            }

            h.HanTra = Add(h.HanTra!.Value, h, count);
        }

        h.MocTinhSLA = accepted.GuiLuc;
    }
}
