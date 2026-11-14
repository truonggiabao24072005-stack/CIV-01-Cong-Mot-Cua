import { api, download } from "../api.js";
import { $, contactInfo, esc, openDialog, STATES, toast } from "../ui.js";

const dateParts = new Intl.DateTimeFormat("vi-VN", {
  timeZone: "Asia/Ho_Chi_Minh", hourCycle: "h23",
  year: "numeric", month: "2-digit", day: "2-digit",
  hour: "2-digit", minute: "2-digit",
});

function parts(value) {
  if (!value || !Number.isFinite(Date.parse(value))) return null;
  return Object.fromEntries(dateParts.formatToParts(new Date(value))
    .map(({ type, value }) => [type, value]));
}

function formDate(value, withTime = false) {
  const p = parts(value);
  if (!p) return "";
  const day = `ngày ${p.day} tháng ${p.month} năm ${p.year}`;
  return withTime ? `${p.hour} giờ ${p.minute} phút, ${day}` : day;
}

function field(label, value, missing = "Chưa ghi nhận") {
  return `<p class="document-field"><strong>${esc(label)}:</strong> <span>${esc(value || missing)}</span></p>`;
}

function paragraph(label, value) {
  if (!value) return "";
  return `<section class="document-section"><h3>${esc(label)}</h3><p class="preserve">${esc(value)}</p></section>`;
}

function conclusionSections(text, fallbackTitle) {
  const headings = new Set([
    "CĂN CỨ KIỂM TRA, ĐỐI CHIẾU",
    "KẾT LUẬN GIẢI QUYẾT",
    "KẾT QUẢ CỤ THỂ",
    "HƯỚNG DẪN THỰC HIỆN",
  ]);
  if (!text.startsWith("CĂN CỨ KIỂM TRA, ĐỐI CHIẾU\n"))
    return paragraph(fallbackTitle, text);

  return text.split("\n\n").map((block) => {
    const separator = block.indexOf("\n");
    const title = separator < 0 ? "" : block.slice(0, separator);
    return headings.has(title)
      ? paragraph(title, block.slice(separator + 1))
      : paragraph("", block);
  }).join("");
}

function barcode(code) {
  const alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-.";
  if (!code || code.length > 32 || [...code].some((character) => !alphabet.includes(character))) return "";
  // Code 39 encodes each character with nine alternating bars and spaces.
  // The encodings below are ordered by the supported alphabet.
  const patterns = [
    52, 289, 97, 352, 49, 304, 112, 37, 292, 100,
    265, 73, 328, 25, 280, 88, 13, 268, 76, 28,
    259, 67, 322, 19, 274, 82, 7, 262, 70, 22,
    385, 193, 448, 145, 400, 208, 133, 388,
  ];
  const bars = [...`*${code}*`].map((character) => {
    const pattern = character === "*" ? 148 : patterns[alphabet.indexOf(character)];
    return Array.from({ length: 9 }, (_, i) => {
      const width = pattern & (1 << (8 - i)) ? 3 : 1;
      return `<span class="${i % 2 === 0 ? "barcode-line" : ""}" style="--modules:${width}"></span>`;
    }).join("");
  }).join('<span style="--modules:1"></span>');
  return `<div class="document-barcode" role="img" aria-label="Mã vạch hồ sơ ${esc(code)}">
    <span style="--modules:10"></span>${bars}<span style="--modules:10"></span>
  </div>`;
}

function signature(role, name, note = "Họ tên theo hồ sơ điện tử") {
  return `<div class="document-signature"><strong>${esc(role)}</strong>
    <em>${esc(note)}</em>
    <span>${esc(name || "Chưa ghi nhận")}</span>
  </div>`;
}

function documentContent(detail, kind) {
  const { hoSo, nguoiNop, thuTuc, ketQua, sla } = detail;
  const contact = contactInfo(hoSo);
  const applicant = contact.hoTen || nguoiNop.ten;
  const owner = field("Họ và tên người nộp", applicant)
    + field("Địa chỉ", contact.diaChi)
    + `<div class="document-contact">${field("Điện thoại", contact.soDienThoai ?? nguoiNop.soDienThoai)}${field("Email", contact.email ?? nguoiNop.lienHe)}</div>`;
  const reference = field("Mã hồ sơ", hoSo.maHoSo, "Chưa cấp")
    + field("Mã gửi", hoSo.maGui)
    + field("Thủ tục hành chính", thuTuc.ten);
  const requestText = paragraph("Nội dung yêu cầu đang lưu trong hồ sơ", hoSo.noiDung);

  if (kind === "receipt" && hoSo.ngayTiepNhan) {
    const official = detail.lichSu.find((item) => item.hanhDong === "TIEP_NHAN");
    const attachments = detail.taiLieu.filter((item) => item.hienHanh);
    const files = attachments.length
      ? `<section class="document-section"><h3>Thành phần hồ sơ hiện hành</h3><ol class="document-list">${attachments.map((item) =>
          `<li>${esc(item.thanhPhan || item.tenTep)}${item.thanhPhan ? ` <span class="document-filename">(${esc(item.tenTep)})</span>` : ""}</li>`,
        ).join("")}</ol></section>`
      : "";
    const timeUnit = sla?.donViSLA === "NGAY_LICH" ? "ngày lịch" : "ngày làm việc";
    const initialDeadline = hoSo.hanTraGoc && hoSo.hanTraGoc !== hoSo.hanTra
      ? field("Thời điểm hẹn trả ban đầu", formDate(hoSo.hanTraGoc, true)) : "";
    return {
      title: "Giấy tiếp nhận hồ sơ và hẹn trả kết quả",
      issued: hoSo.ngayTiepNhan,
      code: hoSo.maHoSo,
      body: owner + field("Mã gửi", hoSo.maGui)
        + field("Thủ tục hành chính", thuTuc.ten) + requestText + files
        + field("Số lượng hồ sơ", "01 bộ điện tử")
        + field("Thời hạn giải quyết", sla?.soNgaySLA ? `${sla.soNgaySLA} ${timeUnit}` : "")
        + field("Thời điểm tiếp nhận", formDate(hoSo.ngayTiepNhan, true))
        + field("Thời điểm hẹn trả", formDate(hoSo.hanTra, true))
        + initialDeadline + field("Nơi nhận kết quả", "Tài khoản người nộp trên Cổng một cửa"),
      signatures: [
        signature("Người nộp hồ sơ", applicant, "Nộp hồ sơ trực tuyến"),
        signature("Người tiếp nhận hồ sơ", official?.nguoiThucHien),
      ],
    };
  }

  if (kind === "supplement") {
    const request = detail.yeuCauBoSung.find((item) => !item.dongLuc);
    if (!request) return;
    const items = request.noiDungChiTiet.map((item) => {
      const component = detail.thanhPhan.find((part) => part.id === item.thanhPhanId);
      return `<li><strong>${esc(item.vanDe)}</strong>
        ${component ? field("Thành phần", component.ten) : ""}
        ${field("Lý do", item.lyDo)}
        ${field("Hướng dẫn", item.huongDan)}
      </li>`;
    });
    return {
      title: "Phiếu yêu cầu bổ sung, hoàn thiện hồ sơ",
      issued: request.taoLuc,
      body: owner + reference + requestText
        + `<section class="document-section"><h3>Nội dung cần bổ sung</h3><ol class="document-list">${items.join("")}</ol></section>
        <p>Vui lòng mở hồ sơ trong tài khoản để gửi tài liệu hoặc giải trình theo từng nội dung.</p>`,
      signatures: [signature("Người hướng dẫn", request.nguoiHuongDan)],
    };
  }

  if (kind === "result" && ketQua?.traLuc) {
    const rejectedAtIntake = ketQua.loai === "TU_CHOI_TIEP_NHAN";
    const title = rejectedAtIntake ? "Phiếu từ chối tiếp nhận giải quyết hồ sơ"
      : ketQua.loai === "TU_CHOI_GIAI_QUYET" ? "Thông báo từ chối giải quyết hồ sơ"
      : "Thông báo kết quả giải quyết hồ sơ";
    const recipient = rejectedAtIntake ? owner : field("Kính gửi", applicant);
    const intake = hoSo.ngayTiepNhan
      ? `Hồ sơ đã được tiếp nhận vào ${formDate(hoSo.ngayTiepNhan, true)}. `
      : `Hồ sơ được gửi vào ${formDate(hoSo.ngayNop, true)}. `;
    const outcome = ketQua.loai === "THANH_CONG"
      ? "Bộ phận Tiếp nhận và Trả kết quả thông báo nội dung giải quyết đã được ghi nhận và phát hành trên hệ thống như sau."
      : "Bộ phận Tiếp nhận và Trả kết quả thông báo kết luận xử lý hồ sơ và lý do để người nộp đối chiếu, thực hiện tiếp.";
    const receiving = detail.coTepKetQua
      ? `Vào Hồ sơ của tôi, chọn hồ sơ ${hoSo.maHoSo || hoSo.maGui} để xem hoặc tải tệp đã phát hành. `
      : "Theo dõi kết luận và hướng dẫn xử lý tại chi tiết hồ sơ trong tài khoản. ";
    const checking = "Khi cần đối chiếu hoặc làm rõ thông tin, liên hệ Bộ phận Tiếp nhận và Trả kết quả, cung cấp mã hồ sơ hoặc mã gửi cùng nội dung cần trao đổi.";
    return {
      title,
      issued: ketQua.traLuc,
      body: recipient + reference
        + field("Tình trạng hồ sơ", STATES[hoSo.trangThai])
        + `<p>${esc(intake + outcome)}</p>`
        + conclusionSections(ketQua.noiDung, rejectedAtIntake ? "Nội dung thông báo" : "Nội dung xử lý và kết quả")
        + paragraph("Lý do từ chối", ketQua.lyDoTuChoi),
      followup: paragraph("Tra cứu kết quả và liên hệ", receiving + checking),
      signatures: [signature(rejectedAtIntake ? "Người tiếp nhận hồ sơ" : "Người phát hành", ketQua.nguoiPhatHanh)],
    };
  }
}

export function previewDocument(detail, kind) {
  const content = documentContent(detail, kind);
  if (!content) return;
  const paper = `<article class="case-document">
    <header class="document-letterhead">
      <div class="document-agency">
        <strong>CỔNG MỘT CỬA ĐIỆN TỬ</strong>
        <strong>BỘ PHẬN TIẾP NHẬN VÀ TRẢ KẾT QUẢ</strong>
      </div>
      <div class="document-state">
        <strong>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</strong>
        <b>Độc lập - Tự do - Hạnh phúc</b>
        <em>${esc(formDate(content.issued))}</em>
      </div>
    </header>
    <div class="document-title">
      <h2>${esc(content.title)}</h2>
      ${content.code ? `<p><strong>Mã hồ sơ</strong> ${esc(content.code)}</p>${barcode(content.code)}` : ""}
    </div>
    <div class="document-body">${content.body}</div>
    <div class="document-closing">${content.followup || ""}<div class="document-signatures">${content.signatures.join("")}</div>
    <footer>Trích xuất từ Cổng một cửa điện tử. Chứng từ này chưa có chữ ký số.</footer></div>
  </article>`;
  const dialog = openDialog("Xem trước bản in", paper +
    '<div class="modal-footer"><button type="button" class="btn" data-close>Đóng</button><button type="button" class="btn primary" data-print-sheet>In / Lưu PDF</button></div>');
  dialog.addEventListener("close", clearPrintSheet, { once: true });
  $("[data-print-sheet]", dialog).onclick = () => {
    const printable = $("#case-print-sheet");
    printable.replaceChildren($(".case-document", dialog).cloneNode(true));
    document.body.classList.add("printing-case-document");
    window.print();
  };
}

function clearPrintSheet() {
  document.body.classList.remove("printing-case-document");
  $("#case-print-sheet")?.replaceChildren();
}

window.addEventListener("afterprint", clearPrintSheet);

export async function previewResultFile(caseId) {
  const path = `/ho-so/${encodeURIComponent(caseId)}/tep-ket-qua`;
  const dialog = openDialog("Tệp kết quả", '<div data-file-preview aria-live="polite"><p>Đang tải tệp…</p></div><div class="modal-footer"><button type="button" class="btn" data-close>Đóng</button><button type="button" class="btn primary" data-download-file>Tải tệp</button></div>');
  const viewer = $("[data-file-preview]", dialog);
  $("[data-download-file]", dialog).onclick = () =>
    download(path, "ket-qua").catch((error) => toast(error.message));
  try {
    const response = await api(path, { download: true });
    const file = await response.blob();
    if (!dialog.open || !viewer.isConnected) return;
    const type = file.type.split(";", 1)[0];
    if (type === "text/plain") {
      const preview = document.createElement("pre");
      preview.className = "result-file-text";
      preview.textContent = await file.text();
      viewer.replaceChildren(preview);
      return;
    }
    if (["image/jpeg", "image/png", "application/pdf"].includes(type)) {
      const source = URL.createObjectURL(file);
      const pdf = type === "application/pdf";
      const element = document.createElement(pdf ? "iframe" : "img");
      element.className = "result-file-preview";
      if (pdf) element.title = "Bản PDF kết quả";
      else element.alt = "Ảnh kết quả đã lưu trong hồ sơ";
      element.src = source;
      dialog.addEventListener("close", () => URL.revokeObjectURL(source), { once: true });
      viewer.replaceChildren(element);
      return;
    }
    viewer.textContent = "Dùng nút Tải tệp để mở định dạng này.";
  } catch (error) {
    viewer.textContent = error.message;
  }
}
