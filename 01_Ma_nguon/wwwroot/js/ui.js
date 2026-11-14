export const STATES = {
  BAN_NHAP: "Bản nháp",
  DA_GUI: "Chờ kiểm tra",
  DA_TIEP_NHAN: "Đã tiếp nhận",
  DANG_GIAI_QUYET: "Đang giải quyết",
  CHO_BO_SUNG: "Chờ bổ sung",
  DA_CO_KET_QUA: "Đã có kết quả",
  DA_TRA_KET_QUA: "Đã trả kết quả",
  TU_CHOI_TIEP_NHAN: "Từ chối tiếp nhận",
  TU_CHOI_GIAI_QUYET: "Từ chối giải quyết",
};
export const ROLES = {
  NGUOI_NOP: "Người dân",
  TIEP_NHAN: "Cán bộ tiếp nhận",
  GIAI_QUYET: "Cán bộ chuyên môn",
};
export const DONE = [
  "DA_TRA_KET_QUA",
  "TU_CHOI_TIEP_NHAN",
  "TU_CHOI_GIAI_QUYET",
];
export const $ = (selector, root = document) => root.querySelector(selector);
export const $$ = (selector, root = document) => [
  ...root.querySelectorAll(selector),
];
// Mọi dữ liệu từ API/người dùng phải được mã hóa trước khi ghép vào HTML.
export const esc = (value) =>
  String(value ?? "").replace(
    /[&<>"']/g,
    (c) =>
      ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" })[
        c
      ],
  );
export const date = (value, short = false) =>
  value
    ? new Intl.DateTimeFormat("vi-VN", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        ...(short ? {} : { hour: "2-digit", minute: "2-digit" }),
        timeZone: "Asia/Ho_Chi_Minh",
      }).format(new Date(value))
    : "Chưa có";
export function contactInfo(h) {
  try {
    return JSON.parse(h.thongTinNguoiNop || "{}") || {};
  } catch {
    return {};
  }
}

export const late = (h) =>
  h.hanTra && new Date(h.hanTra) < new Date() && !DONE.includes(h.trangThai);
export const badge = (state) =>
  `<span class="badge ${esc(String(state).toLowerCase())}">${esc(STATES[state] || "Chưa xác định")}</span>`;
export const route = (page, id = "") =>
  "#/" + page + (id ? "/" + encodeURIComponent(id) : "");
export function navigate(page, id = "") {
  location.hash = route(page, id);
}
export function errorBox(node, error) {
  node.textContent = error?.message || error || "";
  node.hidden = !node.textContent;
}
export function toast(message) {
  const node = $("#notice");
  node.textContent = message;
  node.hidden = false;
  clearTimeout(toast.timer);
  toast.timer = setTimeout(() => {
    node.hidden = true;
  }, 4500);
}
export async function template(name) {
  const response = await fetch("/pages/" + name + ".html");
  if (!response.ok)
    throw new Error("Không tải được giao diện. Vui lòng tải lại trang.");
  return response.text();
}
export function nextCaseStep(h, actor, supplement = h.boSung) {
  const citizen = actor.vaiTro === "NGUOI_NOP";
  const reception = actor.vaiTro === "TIEP_NHAN";
  switch (h.trangThai) {
    case "BAN_NHAP":
      return "Hoàn thiện và nộp hồ sơ";
    case "DA_GUI":
      return reception ? "Kiểm tra hồ sơ" : "Chờ cán bộ kiểm tra";
    case "DA_TIEP_NHAN":
      return reception ? "Phân công cán bộ" : "Chờ phân công";
    case "DANG_GIAI_QUYET":
      return !citizen && !reception ? "Xử lý và ghi kết luận" : "Cán bộ chuyên môn đang xử lý";
    case "CHO_BO_SUNG":
      if (supplement?.trangThai === "DANG_CHO")
        return citizen ? "Gửi nội dung bổ sung" : "Chờ người dân bổ sung";
      if (supplement?.trangThai === "DA_GUI") {
        const responsible = reception
          ? supplement.trangThaiQuayLai === "DA_GUI"
          : !citizen && supplement.trangThaiQuayLai === "DANG_GIAI_QUYET";
        return responsible ? "Kiểm tra lần bổ sung" : "Chờ kiểm tra lần bổ sung";
      }
      return "Xem yêu cầu bổ sung";
    case "DA_CO_KET_QUA":
      return reception ? "Kiểm tra và phát hành kết quả" : "Chờ phát hành kết quả";
    case "DA_TRA_KET_QUA":
      return "Xem kết quả đã phát hành";
    case "TU_CHOI_TIEP_NHAN":
    case "TU_CHOI_GIAI_QUYET":
      return "Xem căn cứ và lý do từ chối";
    default:
      return "Xem chi tiết hồ sơ";
  }
}

export function rowsTable(items, actor, returnQuery = "") {
  if (!items?.length) return '<p class="empty">Không có hồ sơ phù hợp.</p>';
  const citizen = actor.vaiTro === "NGUOI_NOP";
  const back = returnQuery ? "?" + new URLSearchParams({ from: returnQuery }) : "";
  const rows = items.map((h) => {
    const who = citizen ? "" : `<small class="cell-sub">Người nộp: ${esc(h.nguoiNop)}</small>`;
    const handler = !citizen && h.canBo
      ? `<small class="cell-sub">Phụ trách: ${esc(h.canBo)}</small>`
      : "";
    return `<tr><td><a class="case-code" href="${esc(route("detail", h.id) + back)}">${esc(h.ma)}</a>${h.ngayNop ? `<small class="cell-sub">Nộp: ${date(h.ngayNop, true)}</small>` : ""}</td><td>${esc(h.thuTuc)}${who}</td><td>${badge(h.trangThai)}</td><td class="${late(h) ? "overdue-text" : ""}">${h.hanTra ? date(h.hanTra, true) : "—"}</td><td>${nextCaseStep(h, actor)}${handler}</td></tr>`;
  });
  return `<div class="table-scroll"><table class="case-table"><thead><tr><th scope="col">Mã hồ sơ</th><th scope="col">Thủ tục</th><th scope="col">Trạng thái</th><th scope="col">Hẹn trả</th><th scope="col">Bước tiếp theo</th></tr></thead><tbody>${rows.join("")}</tbody></table></div>`;
}
export function openDialog(title, content, compact = false) {
  const dialog = $("#dialog");
  if (dialog.open) dialog.close();
  dialog.classList.toggle("modal-compact", compact);
  $("#dialog-title").textContent = title;
  $("#dialog-content").innerHTML = content;
  dialog.showModal();
  return dialog;
}
export function formObject(form) {
  return Object.fromEntries(new FormData(form));
}
export async function submitForm(form, work) {
  if (form.dataset.busy) return;
  if (!form.reportValidity()) return;
  const buttons = $$('button[type="submit"]', form);
  form.dataset.busy = "true";
  buttons.forEach((button) => {
    button.disabled = true;
  });
  const error = $("[data-error]", form);
  if (error) errorBox(error, "");
  try {
    await work();
  } catch (e) {
    if (error) errorBox(error, e);
    else toast(e.message);
  } finally {
    delete form.dataset.busy;
    buttons.forEach((button) => {
      button.disabled = false;
    });
  }
}
export function installVietnameseValidation() {
  document.addEventListener(
    "invalid",
    (event) => {
      const field = event.target;
      if (!field.setCustomValidity) return;
      field.setCustomValidity("");
      const v = field.validity;
      if (v.valueMissing)
        field.setCustomValidity(
          field.type === "checkbox"
            ? "Vui lòng xác nhận trước khi tiếp tục."
            : "Vui lòng điền hoặc chọn thông tin này.",
        );
      else if (v.typeMismatch)
        field.setCustomValidity("Địa chỉ thư điện tử chưa đúng định dạng.");
      else if (v.tooShort)
        field.setCustomValidity(
          `Vui lòng nhập ít nhất ${field.minLength} ký tự.`,
        );
      else if (v.rangeUnderflow || v.rangeOverflow)
        field.setCustomValidity(
          `Vui lòng nhập giá trị từ ${field.min} đến ${field.max}.`,
        );
      else if (v.badInput || v.stepMismatch)
        field.setCustomValidity("Vui lòng nhập số hợp lệ.");
      else if (v.patternMismatch)
        field.setCustomValidity("Thông tin chưa đúng định dạng yêu cầu.");
    },
    true,
  );
  for (const type of ["input", "change"])
    document.addEventListener(type, (e) => e.target.setCustomValidity?.(""));
}
