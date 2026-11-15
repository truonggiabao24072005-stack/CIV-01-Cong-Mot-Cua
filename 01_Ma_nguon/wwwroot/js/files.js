import { upload } from "./api.js";
import { attachmentRequirement } from "./procedure-guide.js";
import { $, $$, esc } from "./ui.js";

// Một tệp cho mỗi thành phần trong lần gửi; backend lưu lịch sử các phiên bản.
export function fileFields(container, components, onBusy = () => {}, savedDocuments = []) {
  const files = new Map();
  const saved = new Map(savedDocuments.map((document) => [document.thanhPhanId, document.tenTep]));
  const savedLabel = (id) => saved.has(id) ? `${saved.get(id)} (đã lưu)` : "Chưa chọn tệp";
  container.innerHTML = components.length ? `
    <p class="muted">Mỗi thành phần nhận một tệp PDF, JPG, PNG hoặc TXT, tối đa 10 MB. Nếu giấy tờ có nhiều trang, nên gộp thành một PDF theo đúng thứ tự.</p>
    <div class="table-scroll"><table class="case-table attachment-table">
      <thead><tr><th scope="col">Giấy tờ cần đính kèm</th><th scope="col">Tệp đã chọn</th><th scope="col">Thao tác</th></tr></thead>
      <tbody>${components.map((c, index) => {
        const guideId = `${container.id}-guide-${index}`;
        return `<tr data-component="${esc(c.id)}">
          <td><strong>${esc(c.ten)}</strong> ${attachmentRequirement(c)}
            <small class="cell-sub preserve" id="${esc(guideId)}">${esc(c.huongDan)}</small></td>
          <td data-filename aria-live="polite">${esc(savedLabel(c.id))}</td>
          <td><label class="attachment-button">${saved.has(c.id) ? "Thay tệp" : "Chọn tệp"}
            <input type="file" aria-label="Đính kèm ${esc(c.ten)}" aria-describedby="${esc(guideId)}" accept=".pdf,.png,.jpg,.jpeg,.txt"></label>
            <button type="button" class="remove-file btn" hidden>Bỏ tệp vừa chọn</button></td>
        </tr>`;
      }).join("")}</tbody>
    </table></div>
    <p class="error-box" data-file-error hidden role="alert"></p>`
    : '<p class="muted">Không có giấy tờ cần đính kèm ở bước này.</p>';
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
