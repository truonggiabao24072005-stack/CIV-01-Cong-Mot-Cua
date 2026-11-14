import { upload } from "./api.js";
import { $, $$, esc } from "./ui.js";

// Một tệp cho mỗi thành phần trong lần gửi; backend lưu lịch sử các phiên bản.
export function fileFields(container, components, onBusy = () => {}, savedDocuments = []) {
  const files = new Map();
  const saved = new Map(savedDocuments.map((document) => [document.thanhPhanId, document.tenTep]));
  const savedLabel = (id) => saved.has(id) ? `${saved.get(id)} (đã lưu)` : "Chưa chọn tệp";
  container.innerHTML = `<div class="table-scroll"><table class="case-table attachment-table"><thead><tr><th>Thành phần hồ sơ</th><th>Tài liệu</th><th>Thao tác</th></tr></thead><tbody>${components.map((c) => `<tr data-component="${esc(c.id)}"><td><strong>${esc(c.ten)}${c.batBuoc ? ' <b class="required">*</b>' : ""}</strong><small class="cell-sub">${esc(c.huongDan)}</small></td><td data-filename>${esc(savedLabel(c.id))}</td><td><label class="attachment-button">${saved.has(c.id) ? "Thay tệp" : "Chọn tệp"}<input type="file" aria-label="Đính kèm ${esc(c.ten)}" accept=".pdf,.png,.jpg,.jpeg,.txt"></label><button type="button" class="remove-file btn" hidden>Bỏ tệp vừa chọn</button></td></tr>`).join("")}</tbody></table></div><p class="muted">Hỗ trợ PDF, ảnh JPG, PNG và văn bản TXT; tối đa 10 MB mỗi tệp.</p><p class="error-box" data-file-error hidden role="alert"></p>`;
  let busy = false;
  container.onchange = async (event) => {
    if (!event.target.matches('input[type="file"]')) return;
    const input = event.target,
      file = input.files[0];
    if (!file || busy) return;
    const row = input.closest("tr"),
      id = row.dataset.component;
    busy = true;
    onBusy(true);
    $$('input[type="file"]', container).forEach((n) => {
      n.disabled = true;
    });
    $("[data-file-error]", container).hidden = true;
    try {
      const meta = await upload(file);
      files.set(id, { thanhPhanId: id, tepKey: meta.key, name: meta.name });
      $("[data-filename]", row).textContent = meta.name;
      $(".remove-file", row).hidden = false;
    } catch (e) {
      const error = $("[data-file-error]", container);
      error.textContent = e.message;
      error.hidden = false;
    } finally {
      busy = false;
      onBusy(false);
      input.value = "";
      $$('input[type="file"]', container).forEach((n) => {
        n.disabled = false;
      });
    }
  };
  container.onclick = (event) => {
    const button = event.target.closest(".remove-file");
    if (!button || busy) return;
    const row = button.closest("tr");
    files.delete(row.dataset.component);
    $("[data-filename]", row).textContent = savedLabel(row.dataset.component);
    button.hidden = true;
  };
  return {
    get busy() {
      return busy;
    },
    values: () => [...files.values()],
    data: () =>
      [...files.values()].map(({ thanhPhanId, tepKey }) => ({
        thanhPhanId,
        tepKey,
      })),
  };
}
