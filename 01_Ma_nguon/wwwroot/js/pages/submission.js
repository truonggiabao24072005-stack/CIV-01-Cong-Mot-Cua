import { api, post } from "../api.js";
import { $, $$, esc, errorBox, route, toast } from "../ui.js";
import { fileFields } from "../files.js";

export async function submissionPage(root, actor, initialId) {
  if (actor.vaiTro !== "NGUOI_NOP")
    throw new Error("Chỉ người dân được nộp hồ sơ.");
  const form = $("#submission-form", root),
    select = form.elements.thuTucId,
    error = $("[data-error]", form);
  const procedures = await api("/thu-tuc");
  let components = [],
    files,
    step = 1,
    saving = false,
    generation = 0;
  form.elements.hoTen.value = actor.ten;
  form.elements.email.value = actor.email || "Chưa cập nhật";
  form.elements.soDienThoai.value = actor.soDienThoai || "";
  procedures.forEach((p) => select.add(new Option(p.ma + " — " + p.ten, p.id)));
  async function chooseProcedure() {
    const current = ++generation;
    const proc = procedures.find((p) => p.id === select.value);
    components = [];
    files = null;
    errorBox(error, "");
    updateButtons();
    $("#selected-procedure", root).innerHTML = "";
    $("#submission-files", root).innerHTML = "";
    if (!proc) return;
    try {
      const result = await api(
        `/thu-tuc/${encodeURIComponent(proc.id)}/thanh-phan`,
      );
      if (current !== generation) return;
      components = result;
      $("#selected-procedure", root).innerHTML =
        `<dl class="procedure-facts"><dt>Mã thủ tục</dt><dd>${esc(proc.ma)}</dd><dt>Tên thủ tục</dt><dd>${esc(proc.ten)}</dd><dt>Thời hạn giải quyết</dt><dd>${proc.soNgaySLA} ${proc.donViSLA === "NGAY_LAM_VIEC" ? "ngày làm việc" : "ngày lịch"} từ khi tiếp nhận</dd><dt>Hướng dẫn</dt><dd>${esc(proc.moTa)}</dd></dl><h3 class="official-fieldset-title">THÀNH PHẦN CẦN CHUẨN BỊ</h3><ul>${components.map((c) => `<li>${esc(c.ten)} — ${c.batBuoc ? "Bắt buộc" : "Theo trường hợp"}</li>`).join("")}</ul>`;
      files = fileFields(
        $("#submission-files", root),
        components,
        updateButtons,
      );
      updateButtons();
    } catch (e) {
      if (current === generation) errorBox(error, e);
    }
  }
  select.onchange = chooseProcedure;
  function updateButtons() {
    const busy = saving || files?.busy;
    $("#previous-step", root).disabled = busy || step === 1;
    $("#save-draft", root).disabled = busy || !select.value || !files;
    $("#next-step", root).disabled = busy || !select.value || !files;
    $("#submit-case", root).disabled = busy || !form.elements.agree.checked;
    select.disabled = busy;
  }
  function showStep() {
    $$("[data-step]", root).forEach((node) => {
      node.hidden = Number(node.dataset.step) !== step;
    });
    $$("[data-step-label]", root).forEach((node) => {
      const n = Number(node.dataset.stepLabel);
      node.classList.toggle("current", n === step);
      node.classList.toggle("completed", n < step);
      if (n === step) node.setAttribute("aria-current", "step");
      else node.removeAttribute("aria-current");
    });
    const title = [
      "Chọn thủ tục",
      "Kê khai thông tin",
      "Thành phần hồ sơ",
      "Xác nhận và nộp",
    ][step - 1];
    $("#step-title", root).textContent = `Bước ${step}. ${title}`;
    $("#next-step", root).hidden = step === 4;
    $("#submit-case", root).hidden = step !== 4;
    if (step === 4) {
      const proc = procedures.find((p) => p.id === select.value);
      $("#submission-review", root).innerHTML =
        `<dl class="procedure-facts"><dt>Thủ tục</dt><dd>${esc(proc.ten)}</dd><dt>Người nộp</dt><dd>${esc(actor.ten)}</dd><dt>Liên hệ</dt><dd>${esc([form.elements.soDienThoai.value, form.elements.diaChi.value].filter(Boolean).join(" · "))}</dd><dt>Nội dung đề nghị</dt><dd class="preserve">${esc(form.elements.noiDung.value)}</dd></dl><h3>Tài liệu đính kèm</h3><ul>${files
          .values()
          .map((f) => `<li>${esc(f.name)}</li>`)
          .join(
            "",
          )}</ul><p>Kết quả được nhận dưới dạng điện tử sau khi phát hành.</p>`;
    }
    updateButtons();
    window.scrollTo({ top: 0 });
  }
  $("#previous-step", root).onclick = () => {
    if (step > 1 && !saving && !files?.busy) {
      step--;
      errorBox(error, "");
      showStep();
    }
  };
  $("#next-step", root).onclick = () => {
    errorBox(error, "");
    if (saving || files?.busy) return;
    if (step === 2 && !form.elements.noiDung.value.trim()) {
      errorBox(error, "Vui lòng nhập nội dung đề nghị.");
      form.elements.noiDung.focus();
      return;
    }
    if (
      step === 3 &&
      components.some(
        (c) => c.batBuoc && !files.values().some((f) => f.thanhPhanId === c.id),
      )
    ) {
      errorBox(error, "Vui lòng đính kèm đủ thành phần bắt buộc.");
      return;
    }
    if (step < 4 && select.value && files) {
      step++;
      showStep();
    }
  };
  form.elements.agree.onchange = updateButtons;
  async function save(nop) {
    if (saving || files?.busy) return;
    errorBox(error, "");
    if (!select.value || !files) {
      errorBox(error, "Vui lòng chọn thủ tục.");
      return;
    }
    if (nop && (step !== 4 || !form.elements.agree.checked)) {
      errorBox(error, "Kiểm tra thông tin và xác nhận cam kết trước khi nộp.");
      return;
    }
    saving = true;
    updateButtons();
    try {
      const h = await post("/ho-so", {
        thuTucId: select.value,
        noiDung: form.elements.noiDung.value,
        taiLieu: files.data(),
        nop,
        thongTinLienHe: {
          soDienThoai: form.elements.soDienThoai.value,
          diaChi: form.elements.diaChi.value,
        },
      });
      if (!root.isConnected) return;
      if (!nop) {
        toast("Đã lưu bản nháp.");
        location.hash = route("detail", h.id);
      } else
        root.innerHTML = `<section class="panel submitted-page"><h1>Nộp hồ sơ thành công</h1><p>Hồ sơ đã được gửi và đang chờ cán bộ kiểm tra.</p><dl class="procedure-facts"><dt>Mã gửi hồ sơ</dt><dd><strong>${esc(h.maGui)}</strong></dd><dt>Trạng thái</dt><dd>Chờ kiểm tra</dd></dl><p>Mã hồ sơ chính thức và ngày hẹn trả được cấp sau khi tiếp nhận.</p><div class="actions"><a class="btn primary" href="${route("detail", h.id)}">Xem hồ sơ đã nộp</a><a class="btn" href="#/cases">Hồ sơ của tôi</a></div></section>`;
    } catch (e) {
      errorBox(error, e);
    } finally {
      saving = false;
      if (form.isConnected) updateButtons();
    }
  }
  $("#save-draft", root).onclick = () => save(false);
  form.onsubmit = (event) => {
    event.preventDefault();
    save(true);
  };
  if (initialId && procedures.some((p) => p.id === initialId)) {
    select.value = initialId;
    await chooseProcedure();
  }
  updateButtons();
}
