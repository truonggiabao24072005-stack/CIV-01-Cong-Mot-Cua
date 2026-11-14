using CongMotCua.Api.Models;
using CongMotCua.Api.Models.Requests;
using CongMotCua.Api.Filters;
using CongMotCua.Api.Models.Data;
using System.Text.Json;

namespace CongMotCua.Services;
public record StoredFile(string Key, string Name, string ContentType, string Owner, long Size);
public class FileStore(IConfiguration cfg, IHostEnvironment environment)
{
    public string Root => Path.GetFullPath(cfg["Files:Root"] ?? ".data/files", environment.ContentRootPath);

    public async Task<StoredFile> Put(IFormFile file, string sub)
    {
        if (file.Length == 0 || file.Length > 10 * 1024 * 1024)
            throw new BusinessError("Mỗi tệp cần có nội dung và không vượt quá 10 MB.");
        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var types = new Dictionary<string, string>
        {
            {
                ".pdf",
                "application/pdf"
            },
            {
                ".png",
                "image/png"
            },
            {
                ".jpg",
                "image/jpeg"
            },
            {
                ".jpeg",
                "image/jpeg"
            },
            {
                ".txt",
                "text/plain"
            }
        };
        if (!types.TryGetValue(ext, out var mime))
            throw new BusinessError("Chấp nhận PDF, PNG, JPG và TXT.");
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var data = ms.ToArray();
        bool magic = ext switch
        {
            ".pdf" => data.Length > 4 && System.Text.Encoding.ASCII.GetString(data, 0, 5) == "%PDF-",
            ".png" => data.Length > 8 && data.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".jpg" or ".jpeg" => data.Length > 3 && data[0] == 255 && data[1] == 216 && data[2] == 255,
            _ => !data.Contains((byte)0)};
        if (!magic)
            throw new BusinessError("Nội dung tệp không khớp định dạng được chọn.");
        return await Write(Path.GetFileName(file.FileName), mime, sub, data);
    }

    public async Task<StoredFile> Write(string name, string mime, string owner, byte[] bytes)
    {
        Directory.CreateDirectory(Root);
        string key = Guid.NewGuid().ToString("N");
        var meta = new StoredFile(key, name, mime, owner, bytes.Length);
        await File.WriteAllBytesAsync(Path.Combine(Root, key), bytes);
        await File.WriteAllTextAsync(Path.Combine(Root, key + ".json"), JsonSerializer.Serialize(meta));
        return meta;
    }

    public StoredFile Get(string key)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(key, "^[a-f0-9]{32}$"))
            throw new BusinessError("Tệp không hợp lệ.");
        string p = Path.Combine(Root, key + ".json");
        if (!File.Exists(p))
            throw new BusinessError("Không tìm thấy tệp.", 404);
        var meta = JsonSerializer.Deserialize<StoredFile>(File.ReadAllText(p));
        var dataPath = Path.Combine(Root, key);
        if (meta == null || meta.Key != key || !File.Exists(dataPath) || new FileInfo(dataPath).Length != meta.Size)
            throw new BusinessError("Dữ liệu tệp không còn đầy đủ. Vui lòng tải lại tệp.", 404);
        return meta;
    }

    public StoredFile Owned(string key, string sub)
    {
        var m = Get(key);
        if (m.Owner != sub)
            throw new BusinessError("Tệp thuộc tài khoản khác.", 403);
        return m;
    }

    public string PathFor(string key)
    {
        Get(key);
        return Path.Combine(Root, key);
    }
}
