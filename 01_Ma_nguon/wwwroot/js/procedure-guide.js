import { esc } from "./ui.js";

const preparationGuides = {
  "TH-01": {
    declaration: "Ghi họ tên người có sự kiện hộ tịch, loại sự kiện (ví dụ khai sinh hoặc kết hôn), ngày và nơi đăng ký, số bản cần cấp. Ghi thêm số đăng ký nếu biết.",
    notes: [
      "Tại bước Kê khai thông tin, kiểm tra thông tin liên hệ rồi ghi loại sự kiện hộ tịch, nơi đăng ký và số bản cần cấp vào ô Nội dung đề nghị.",
      "Tại bước Thành phần hồ sơ, đính kèm bản PDF của tờ khai đã điền. Dùng mục Giấy tờ đối chiếu khi có yêu cầu, ví dụ khi cần chứng minh nơi cư trú do cơ quan chưa khai thác được dữ liệu.",
    ],
  },
  "TH-02": {
    declaration: "Ghi tên giấy tờ cần chứng thực và số bản sao cần chứng thực của từng loại.",
    notes: [
      "Tại bước Kê khai thông tin, ghi tên từng giấy tờ và số bản sao cần chứng thực vào ô Nội dung đề nghị.",
      "Tải bản quét từ bản chính vào mục Bản số hóa giấy tờ dùng để đối chiếu bản chính. Chụp rõ, đủ trang và đủ hai mặt nếu cả hai mặt có nội dung. Tải bản sao vào mục riêng nếu đã chuẩn bị.",
      "Giữ bản chính để xuất trình khi cơ quan chứng thực cần đối chiếu.",
    ],
  },
  "TH-03": {
    declaration: "Ghi địa chỉ cư trú cần xác nhận, khoảng thời gian cần xác nhận nếu có và mục đích đề nghị.",
    notes: [
      "Tại bước Kê khai thông tin, kiểm tra thông tin người nộp rồi ghi địa chỉ cư trú cần xác nhận và nội dung cần xác nhận.",
      "Bạn có thể tiếp tục sau khi kê khai trực tuyến. Để trống mục CT01 nếu không có bản PDF lưu nội dung đã kê khai. Chỉ đính kèm tài liệu hỗ trợ khi có yêu cầu bổ sung cụ thể.",
    ],
  },
};

export function declarationGuide(procedure) {
  return Object.hasOwn(preparationGuides, procedure.ma)
    ? preparationGuides[procedure.ma].declaration
    : "Ghi rõ nội dung cần giải quyết và thông tin giúp cán bộ kiểm tra hồ sơ.";
}

export function attachmentRequirement(component) {
  return component.batBuoc
    ? '<span class="badge attachment-required">Bắt buộc</span>'
    : '<span class="badge">Không bắt buộc</span>';
}

export function procedureDocuments(procedure, components) {
  const guide = Object.hasOwn(preparationGuides, procedure.ma)
    ? preparationGuides[procedure.ma].notes
    : ["Kê khai thông tin theo thủ tục đã chọn và chuẩn bị các tệp được đánh dấu Bắt buộc."];
  return `<section class="procedure-documents">
    <h3>Giấy tờ cần chuẩn bị</h3>
    ${components.length ? `<div class="table-scroll">
      <table class="case-table">
        <thead><tr><th scope="col">Giấy tờ</th><th scope="col">Tệp tải lên tại đây</th><th scope="col">Nội dung cần có</th></tr></thead>
        <tbody>${components.map((component) => `<tr>
          <th scope="row">${esc(component.ten)}</th>
          <td>${attachmentRequirement(component)}</td>
          <td class="preserve">${esc(component.huongDan)}</td>
        </tr>`).join("")}</tbody>
      </table>
    </div>` : '<p class="muted">Thủ tục này không yêu cầu tệp đính kèm.</p>'}
    <div class="procedure-preparation">
      <h4>Cách chuẩn bị</h4>
      <ol>${guide.map((note) => `<li>${esc(note)}</li>`).join("")}</ol>
      ${components.length ? "<p>Mỗi mục nhận một tệp tối đa 10 MB. Nếu giấy tờ có nhiều trang, gộp theo thứ tự thành một tệp PDF.</p>" : ""}
    </div>
  </section>`;
}
