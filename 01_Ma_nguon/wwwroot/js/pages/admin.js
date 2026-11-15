import { api, post } from "../api.js";
import {
  $,
  $$,
  esc,
  ROLES,
  errorBox,
  openDialog,
  submitForm,
  formObject,
  toast,
} from "../ui.js";

export async function catalogPage(root, actor) {
  if (actor.vaiTro !== "TIEP_NHAN")
    throw new Error("Bạn không có quyền quản lý danh mục.");
  let data,
    tab = "thuTuc";
  const content = $("#catalog-content", root),
    error = $("#catalog-error", root);
  async function load() {
    data = await api("/quan-tri");
    if (root.isConnected) draw();
  }
  function draw() {
    errorBox(error, "");
    $$("[data-catalog]", root).forEach((button) =>
      button.classList.toggle("active", button.dataset.catalog === tab),
    );
    $("#add-catalog", root).hidden = tab === "nguoiDung";
    $("#add-catalog", root).textContent =
      tab === "thuTuc" ? "Thêm thủ tục" : "Thêm bộ phận";
    content.innerHTML = `<section class="panel table-scroll"><table class="case-table"><thead><tr><th>${tab === "nguoiDung" ? "Họ và tên" : "Mã"}</th><th>${tab === "nguoiDung" ? "Vai trò" : "Tên"}</th><th>Trạng thái</th><th>Thao tác</th></tr></thead><tbody>${data[tab].map((row) => `<tr><td>${esc(row.maThuTuc || row.maBoPhan || row.hoTen)}</td><td>${esc(row.tenThuTuc || row.tenBoPhan || ROLES[row.vaiTro])}</td><td>${row.hoatDong ? "Đang sử dụng" : "Ngừng sử dụng"}</td><td><button class="btn" data-edit="${esc(row.id)}">${tab === "nguoiDung" ? (row.hoatDong ? "Khóa" : "Mở lại") : "Chỉnh sửa"}</button></td></tr>`).join("")}</tbody></table></section>`;
  }
  $("#catalog-tabs", root).onclick = (event) => {
    const button = event.target.closest("[data-catalog]");
    if (button) {
      tab = button.dataset.catalog;
      draw();
    }
  };
  $("#add-catalog", root).onclick = () => edit();
  content.onclick = (event) => {
    const button = event.target.closest("[data-edit]");
    if (!button) return;
    const row = data[tab].find((r) => r.id === button.dataset.edit);
    tab === "nguoiDung" ? toggleUser(row) : edit(row);
  };
  function toggleUser(row) {
    const dialog = openDialog(
      row.hoatDong ? "Khóa tài khoản" : "Mở lại tài khoản",
      `<form id="user-form"><p>${row.hoatDong ? "Khóa" : "Mở lại"} tài khoản của <strong>${esc(row.hoTen)}</strong>?</p><p data-error class="error-box" hidden role="alert"></p><div class="modal-footer"><button class="btn" type="button" data-close>Hủy</button><button class="btn primary" type="submit">Xác nhận</button></div></form>`,
    );
    const form = $("#user-form");
    form.onsubmit = (event) => {
      event.preventDefault();
      submitForm(form, async () => {
        await post("/quan-tri/nguoi-dung", {
          id: row.id,
          hoatDong: !row.hoatDong,
        });
        dialog.close();
        toast("Đã cập nhật tài khoản.");
        await load();
      });
    };
  }
  function edit(row = null) {
    const procedure = tab === "thuTuc";
    const current = row || {
      maThuTuc: "",
      maBoPhan: "",
      tenThuTuc: "",
      tenBoPhan: "",
      moTa: "",
      hoatDong: true,
      soNgaySLA: 3,
      soGioPhanHoi: 8,
      donViSLA: "NGAY_LAM_VIEC",
      quyTacBoSung: "TINH_LAI",
      boPhanChuyenMonId: "",
    };
    const selectOptions = (options, value) =>
      options
        .map(
          ([key, label]) =>
            `<option value="${esc(key)}" ${key === value ? "selected" : ""}>${esc(label)}</option>`,
        )
        .join("");
    content.innerHTML = `<form class="panel form-body" id="catalog-form"><h2>${row ? "Cập nhật" : "Thêm"} ${procedure ? "thủ tục" : "bộ phận"}</h2><p data-error class="error-box" hidden role="alert"></p><div class="admin-form-grid"><label class="field">Mã<input name="ma" required maxlength="60" value="${esc(current.maThuTuc || current.maBoPhan)}"></label><label class="field">Tên<input name="ten" required maxlength="255" value="${esc(current.tenThuTuc || current.tenBoPhan)}"></label></div><label class="checkbox-field"><input name="hoatDong" type="checkbox" ${current.hoatDong ? "checked" : ""}>Đang sử dụng</label>
      ${
        procedure
          ? `<label class="field">Mô tả và hướng dẫn<textarea name="moTa" required maxlength="6000">${esc(current.moTa)}</textarea></label><div class="admin-form-grid"><label class="field">Bộ phận chuyên môn<select name="boPhanId" required><option value="">Chọn bộ phận</option>${data.boPhan
              .filter((b) => b.hoatDong || b.id === current.boPhanChuyenMonId)
              .map(
                (b) =>
                  `<option value="${esc(b.id)}" ${b.id === current.boPhanChuyenMonId ? "selected" : ""}>${esc(b.tenBoPhan)}</option>`,
              )
              .join(
                "",
              )}</select></label><label class="field">Số ngày giải quyết<input name="soNgaySLA" required type="number" min="1" max="365" value="${current.soNgaySLA}"></label><label class="field">Cách tính thời hạn<select name="donViSLA">${selectOptions(
              [
                ["NGAY_LAM_VIEC", "Ngày làm việc"],
                ["NGAY_LICH", "Ngày lịch"],
              ],
              current.donViSLA,
            )}</select></label><label class="field">Thời gian phản hồi (giờ)<input name="soGioPhanHoi" required type="number" min="1" max="720" value="${current.soGioPhanHoi}"></label><label class="field">Cách tính hạn khi bổ sung<select name="quyTacBoSung">${selectOptions(
              [
                ["TINH_LAI", "Tính lại từ ngày gửi bản bổ sung được chấp nhận"],
                ["GIU_NGUYEN", "Giữ nguyên hạn"],
                ["TAM_DUNG", "Bù thời gian chờ theo lịch áp dụng"],
              ],
              current.quyTacBoSung,
            )}</select></label></div><h3>Thành phần hồ sơ</h3><p class="muted">Thủ tục đã phát sinh hồ sơ phải giữ bộ phận và cấu trúc thành phần. Tạo thủ tục mới nếu cần thay đổi cấu trúc.</p><div id="component-editor"></div><button class="btn" type="button" id="add-component">Thêm thành phần</button>`
          : ""
      }<div class="modal-footer"><button class="btn" type="button" id="cancel-edit">Hủy</button><button class="btn primary" type="submit">Lưu thông tin</button></div></form>`;
    const form = $("#catalog-form", root);
    if (procedure) {
      const editor = $("#component-editor", form);
      function controls() {
        const count = editor.children.length;
        $$("[data-remove]", editor).forEach((button) => {
          button.disabled = count === 1;
        });
        $("#add-component", form).disabled = count >= 30;
      }
      function addComponent(
        item = { tenThanhPhan: "", huongDan: "", batBuoc: true },
      ) {
        if (editor.children.length >= 30) return;
        const node = document.createElement("div");
        node.className = "admin-component";
        node.innerHTML = `<label class="field">Tên thành phần<input name="componentName" required maxlength="255" value="${esc(item.tenThanhPhan)}"></label><label class="field">Hướng dẫn<input name="componentGuide" required maxlength="6000" value="${esc(item.huongDan)}"></label><label class="checkbox-field"><input name="componentRequired" type="checkbox" ${item.batBuoc ? "checked" : ""}>Bắt buộc</label><button type="button" class="btn" data-remove>Bỏ</button>`;
        $("[data-remove]", node).onclick = () => {
          node.remove();
          controls();
        };
        editor.append(node);
        controls();
      }
      const components = row
        ? data.thanhPhan
            .filter((c) => c.thuTucId === row.id)
            .sort((a, b) => a.thuTu - b.thuTu)
        : [
            {
              tenThanhPhan: "Đơn đề nghị",
              huongDan: "Đính kèm bản rõ ràng.",
              batBuoc: true,
            },
          ];
      components.forEach(addComponent);
      $("#add-component", form).onclick = () => addComponent();
    }
    $("#cancel-edit", form).onclick = draw;
    form.onsubmit = (event) => {
      event.preventDefault();
      submitForm(form, async () => {
        const input = formObject(form);
        input.id = row?.id || null;
        input.hoatDong = form.elements.hoatDong.checked;
        if (procedure) {
          input.soNgaySLA = Number(input.soNgaySLA);
          input.soGioPhanHoi = Number(input.soGioPhanHoi);
          input.thanhPhan = $$(".admin-component", form).map((node) => ({
            ten: $('[name="componentName"]', node).value,
            huongDan: $('[name="componentGuide"]', node).value,
            batBuoc: $('[name="componentRequired"]', node).checked,
          }));
          delete input.componentName;
          delete input.componentGuide;
          delete input.componentRequired;
        }
        await post(
          procedure ? "/quan-tri/thu-tuc" : "/quan-tri/bo-phan",
          input,
        );
        toast("Đã lưu thông tin.");
        await load();
      });
    };
  }
  await load();
}

export async function reportPage(root, actor) {
  if (actor.vaiTro !== "TIEP_NHAN")
    throw new Error("Bạn không có quyền xem báo cáo.");
  const form = $("#report-filter", root);
  const exportButton = $("#export-report", root);
  const message = $("#report-error", root);
  let rows = [];
  let loading = false;
  const values = (r) => [
    r.thuTuc,
    r.tong,
    r.daTra,
    r.tuChoi,
    r.quaHan,
    r.hoanTatDungHan,
    r.hoanTatTreHan,
  ];
  function draw() {
    $("#report-rows", root).innerHTML = rows.map((row) =>
      `<tr>${values(row).map((value) => `<td>${esc(value)}</td>`).join("")}</tr>`,
    ).join("") || '<tr><td colspan="7">Không có hồ sơ trong khoảng ngày đã chọn.</td></tr>';
    $("#report-summary", root).textContent =
      `${rows.reduce((sum, row) => sum + row.tong, 0).toLocaleString("vi-VN")} hồ sơ đã nộp · ${rows.length} thủ tục`;
  }
  async function load() {
    if (loading) return;
    loading = true;
    exportButton.disabled = true;
    errorBox(message, "");
    $("#report-summary", root).textContent = "Đang tải số liệu…";
    $("#report-rows", root).replaceChildren();
    const query = new URLSearchParams();
    for (const name of ["tuNgay", "denNgay"]) {
      if (form.elements[name].value) query.set(name, form.elements[name].value);
    }
    try {
      rows = await api("/bao-cao?" + query);
      if (root.isConnected) {
        draw();
        exportButton.disabled = !rows.length;
      }
    } catch (e) {
      rows = [];
      $("#report-summary", root).textContent = "";
      errorBox(message, e);
    } finally {
      loading = false;
    }
  }
  form.onsubmit = (event) => { event.preventDefault(); load(); };
  $("#clear-report", root).onclick = () => {
    if (loading) return;
    form.reset();
    load();
  };
  exportButton.onclick = () => {
    const heads = [
      "Thủ tục",
      "Tổng số",
      "Đã trả",
      "Từ chối",
      "Đang quá hạn",
      "Hoàn thành đúng hạn",
      "Hoàn thành quá hạn",
    ];
    const quote = (value) => {
      let text = String(value ?? "");
      if (/^[=+\-@]/.test(text)) text = "'" + text;
      return '"' + text.replaceAll('"', '""') + '"';
    };
    const text =
      "\uFEFF" +
      [heads, ...rows.map(values)]
        .map((row) => row.map(quote).join(","))
        .join("\r\n");
    const url = URL.createObjectURL(
      new Blob([text], { type: "text/csv;charset=utf-8" }),
    );
    const link = document.createElement("a");
    link.href = url;
    link.download = "bao-cao-ho-so.csv";
    link.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  };
  await load();
}
