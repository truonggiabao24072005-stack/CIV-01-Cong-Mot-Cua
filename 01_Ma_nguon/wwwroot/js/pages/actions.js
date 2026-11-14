import { api, post, upload } from "../api.js";
import {
  $,
  $$,
  esc,
  badge,
  openDialog,
  submitForm,
  toast,
} from "../ui.js";
import { fileFields } from "../files.js";

export const ACTIONS = {
  nop: "Nộp bản nháp",
  "kiem-tra-tai-lieu": "Đánh giá tài liệu",
  "tiep-nhan": "Xác nhận tiếp nhận",
  "luan-chuyen": "Phân công / chuyển cán bộ",
  "yeu-cau-bo-sung": "Yêu cầu bổ sung",
  "gui-bo-sung": "Gửi hồ sơ bổ sung",
  "kiem-tra-bo-sung": "Kiểm tra bổ sung",
  "ket-qua": "Ghi nhận kết quả",
  "tu-choi": "Ghi nhận từ chối",
  "tra-ket-qua": "Xác nhận trả kết quả",
};

export function availableActions(d, actor) {
  const h = d.hoSo,
    y = d.yeuCauBoSung.find((x) => !x.dongLuc);
  const citizen = actor.vaiTro === "NGUOI_NOP",
    reception = actor.vaiTro === "TIEP_NHAN",
    actions = [];
  if (citizen && h.trangThai === "BAN_NHAP") actions.push("nop");
  if (citizen && y?.trangThai === "DANG_CHO") actions.push("gui-bo-sung");
  if (reception && h.trangThai === "DA_GUI")
    actions.push("tiep-nhan", "yeu-cau-bo-sung", "tu-choi");
  if (reception && ["DA_TIEP_NHAN", "DANG_GIAI_QUYET"].includes(h.trangThai))
    actions.push("luan-chuyen");
  if (!reception && !citizen && d.duocXuLy && h.trangThai === "DANG_GIAI_QUYET")
    actions.push("ket-qua", "yeu-cau-bo-sung", "tu-choi");
  const canReview =
    !citizen &&
    ((reception &&
      (h.trangThai === "DA_GUI" || y?.trangThaiQuayLai === "DA_GUI")) ||
      (!reception &&
        d.duocXuLy &&
        (h.trangThai === "DANG_GIAI_QUYET" ||
          y?.trangThaiQuayLai === "DANG_GIAI_QUYET")));
  if (y?.trangThai === "DA_GUI" && canReview) actions.push("kiem-tra-bo-sung");
  if (reception && h.trangThai === "DA_CO_KET_QUA") actions.push("tra-ket-qua");
  return { actions, canReview };
}
const textField = (name, label, required = false) =>
  `<label class="field">${label}<textarea name="${name}" rows="3" maxlength="6000" ${required ? "required" : ""}></textarea></label>`;

export async function actionDialog(action, d, onSaved, fileId = null) {
  let staff = [],
    body = "",
    resultFile = null,
    attachments;
  const checking = ["kiem-tra-tai-lieu", "kiem-tra-bo-sung"].includes(action);
  if (action === "luan-chuyen") {
    staff = (await api("/can-bo")).filter(
      (u) => u.boPhanId === d.thuTuc.boPhanChuyenMonId,
    );
    if (!staff.length) {
      openDialog(ACTIONS[action], '<p>Chưa có cán bộ chuyên môn đang hoạt động ở bộ phận phụ trách thủ tục. Kiểm tra nhân sự trong mục Quản lý danh mục rồi thực hiện lại.</p>');
      return;
    }
    body += `<label class="field">Cán bộ nhận<select name="canBoNhanId" required><option value="">Chọn cán bộ</option>${staff.map((u) => `<option value="${esc(u.id)}">${esc(u.hoTen)} — ${esc(u.boPhan)}</option>`).join("")}</select></label>`;
  }
  const pendingReviews = action === "kiem-tra-bo-sung" &&
    d.taiLieu.some((f) => f.choDanhGia && f.kiemTra[0]?.ketQua !== "DAT");
  if (pendingReviews) {
    body += '<p class="subtle-note">Còn tệp bổ sung chưa được đánh giá Đạt. Để chấp nhận lần gửi, hãy đánh giá từng tệp trước. Bạn vẫn có thể chọn Chưa đạt và ghi rõ lý do.</p>';
  }
  if (checking)
    body +=
      `<label class="field">Kết quả kiểm tra <b class="required">*</b><select name="dat" required><option value="">Chọn kết quả kiểm tra</option><option value="true" ${pendingReviews ? "disabled" : ""}>Đạt</option><option value="false">Chưa đạt</option></select></label>`;
  if (action === "ket-qua" || action === "tu-choi") {
    body += `<section class="notice-inputs"><h3>Nội dung thông báo cho người nộp</h3>
      <p>Ghi cụ thể căn cứ đã đối chiếu, quyết định xử lý và việc người nộp cần thực hiện sau khi nhận thông báo.</p>
      <label class="field">Căn cứ kiểm tra, đối chiếu <b class="required">*</b>
        <textarea name="canCuXuLy" rows="3" maxlength="1600" required placeholder="Nêu hồ sơ, tài liệu hoặc quy định đã đối chiếu và nội dung kiểm tra liên quan."></textarea>
      </label>
      <label class="field">Kết luận giải quyết <b class="required">*</b>
        <textarea name="noiDung" rows="3" maxlength="1200" required placeholder="Nêu rõ đồng ý giải quyết hoặc từ chối, kèm phạm vi nội dung đã xem xét."></textarea>
      </label>
      ${action === "ket-qua" ? '<label class="field">Kết quả cụ thể được cấp / xác nhận <b class="required">*</b><textarea name="ketQuaCuThe" rows="3" maxlength="1600" required placeholder="Tên kết quả, nội dung được xác nhận, số lượng hoặc thông tin cần người nộp biết."></textarea></label>' : ""}
      <label class="field">${action === "ket-qua" ? "Hướng dẫn nhận và sử dụng kết quả" : "Hướng dẫn khắc phục và thực hiện tiếp"} <b class="required">*</b>
        <textarea name="huongDanThucHien" rows="3" maxlength="1200" required placeholder="Nêu cách nhận kết quả hoặc cách chuẩn bị hồ sơ phù hợp, thông tin cần đối chiếu và nơi liên hệ."></textarea>
      </label></section>`;
  } else if (action === "tiep-nhan")
    body +=
      "<p>Hệ thống kiểm tra các bản tài liệu đã đánh giá Đạt, cấp mã hồ sơ chính thức, biên nhận và ngày hẹn trả.</p>";
  else if (action === "nop")
    body +=
      "<p>Nộp bản nháp hiện tại. Hồ sơ cần có nội dung đề nghị và đủ tài liệu bắt buộc.</p>";
  else if (action === "yeu-cau-bo-sung")
    body +=
      '<div id="supplement-items"></div><button class="btn" type="button" id="add-supplement">Thêm nội dung cần bổ sung</button>';
  else
    body += textField(
      "noiDung",
      action === "gui-bo-sung" ? "Nội dung giải trình" : "Nội dung xử lý",
      action === "luan-chuyen",
    );
  if (checking || action === "tu-choi")
    body += `<div id="reason-field" ${checking ? "hidden" : ""}>${textField("lyDo", "Lý do", action === "tu-choi")}</div>`;
  if (action === "gui-bo-sung") {
    const request = d.yeuCauBoSung.find((item) => !item.dongLuc);
    const items = request?.noiDungChiTiet ?? [];
    if (items.length) {
      body = `<div class="subtle-note"><strong>Nội dung cần bổ sung</strong><ul>${items.map((item) => `<li><b>${esc(item.vanDe)}</b><br>${esc(item.huongDan)}</li>`).join("")}</ul></div>` + body;
    }
    body +=
      textField("thongTinDieuChinh", "Nội dung đề nghị thay thế (nếu được yêu cầu)") +
      '<p class="muted">Nếu điền, hãy nhập đầy đủ nội dung đề nghị mới. Hệ thống thay toàn bộ nội dung đề nghị sau khi cán bộ chấp nhận lần bổ sung.</p><div id="supplement-files"></div>';
  }
  if (action === "ket-qua")
    body +=
      '<label class="field">Tệp kết quả bắt buộc<span class="attachment-button">Chọn tệp kết quả<input name="resultFile" aria-label="Chọn tệp kết quả" type="file" required accept=".pdf,.png,.jpg,.jpeg,.txt"></span><small id="result-file-name">Chưa chọn tệp. Tối đa 10 MB.</small></label>';
  const dialog = openDialog(
    ACTIONS[action],
    `<form id="action-form"><div class="modal-case">${esc(d.hoSo.ma)} ${badge(d.hoSo.trangThai)}</div><p data-error class="error-box" hidden role="alert"></p>${body}<div class="modal-footer"><button type="button" class="btn" data-close>Hủy</button><button type="submit" class="btn ${action === "tu-choi" ? "danger" : "primary"}">${checking ? "Lưu đánh giá" : ACTIONS[action]}</button></div></form>`,
  );
  const form = $("#action-form"),
    submit = $('button[type="submit"]', form);
  function uploading(busy) {
    form.dataset.uploading = busy ? "true" : "";
    submit.disabled =
      busy ||
      Boolean(form.dataset.busy) ||
      (action === "ket-qua" && !resultFile);
  }
  if (checking)
    form.elements.dat.onchange = () => {
      const needed = form.elements.dat.value === "false";
      $("#reason-field", form).hidden = !needed;
      form.elements.lyDo.required = needed;
    };
  if (action === "gui-bo-sung")
    attachments = fileFields(
      $("#supplement-files", form),
      d.thanhPhan.map((component) => ({
        ...component,
        batBuoc: d.yeuCauBoSung.find((item) => !item.dongLuc)?.noiDungChiTiet.some(
          (item) => item.thanhPhanId === component.id,
        ) ?? false,
      })),
      uploading,
    );
  if (action === "ket-qua") {
    submit.disabled = true;
    form.elements.resultFile.onchange = async (event) => {
      const file = event.target.files[0];
      if (!file) return;
      resultFile = null;
      uploading(true);
      errorBoxInForm("");
      try {
        resultFile = await upload(file);
        $("#result-file-name", form).textContent = resultFile.name;
      } catch (e) {
        errorBoxInForm(e.message);
        event.target.value = "";
        $("#result-file-name", form).textContent = "Chưa chọn tệp.";
      } finally {
        uploading(false);
      }
    };
  }
  function errorBoxInForm(message) {
    const p = $("[data-error]", form);
    p.textContent = message;
    p.hidden = !message;
  }
  if (action === "yeu-cau-bo-sung") {
    const items = $("#supplement-items", form);
    const renumber = () => {
      $$(".supplement-item", items).forEach((node, i) => {
        $("h3", node).textContent = "Nội dung cần bổ sung " + (i + 1);
        $("[data-remove-item]", node).hidden = items.children.length === 1;
      });
      $("#add-supplement", form).disabled = items.children.length >= 20;
    };
    function addItem() {
      if (items.children.length >= 20) return;
      const section = document.createElement("section");
      section.className = "supplement-item";
      section.innerHTML = `<h3></h3><label class="field">Thành phần cần nộp lại<select name="thanhPhanId"><option value="">Không yêu cầu nộp lại tệp</option>${d.thanhPhan.map((c) => `<option value="${esc(c.id)}">${esc(c.ten)}</option>`).join("")}</select></label><label class="field">Phần nội dung đề nghị cần sửa<input name="truongThongTin" maxlength="255" placeholder="Ví dụ: lý do đề nghị, thông tin cần xác nhận"></label>${textField("vanDe", "Vấn đề cần sửa", true)}${textField("lyDoItem", "Lý do và căn cứ", true)}${textField("huongDan", "Hướng dẫn thực hiện", true)}<button type="button" class="btn" data-remove-item>Bỏ nội dung</button>`;
      $("[data-remove-item]", section).onclick = () => {
        section.remove();
        renumber();
      };
      items.append(section);
      renumber();
    }
    $("#add-supplement", form).onclick = addItem;
    addItem();
  }
  form.onsubmit = (event) => {
    event.preventDefault();
    if (form.dataset.uploading) return;
    submitForm(form, async () => {
      if (action === "ket-qua" && !resultFile)
        throw new Error("Vui lòng tải tệp kết quả.");
      const value = (name) =>
        form.querySelector(`[name="${name}"]`)?.value || null;
      const receiver = staff.find((u) => u.id === value("canBoNhanId"));
      const input = {
        version: d.hoSo.version,
        noiDung: value("noiDung"),
        lyDo: value("lyDo"),
        canBoNhanId: receiver?.id,
        boPhanNhanId: receiver?.boPhanId,
        dat: checking ? value("dat") === "true" : null,
        taiLieuId: fileId,
        tepKey: resultFile?.key,
        taiLieu: attachments?.data(),
        thongTinDieuChinh: value("thongTinDieuChinh"),
        canCuXuLy: value("canCuXuLy"),
        ketQuaCuThe: value("ketQuaCuThe"),
        huongDanThucHien: value("huongDanThucHien"),
      };
      if (action === "yeu-cau-bo-sung")
        input.noiDungBoSung = $$(".supplement-item", form).map((section) => {
          const get = (name) => $(`[name="${name}"]`, section).value;
          return {
            thanhPhanId: get("thanhPhanId") || null,
            truongThongTin: get("truongThongTin") || null,
            vanDe: get("vanDe"),
            lyDo: get("lyDoItem"),
            huongDan: get("huongDan"),
          };
        });
      if (action === "gui-bo-sung") {
        const requirements = d.yeuCauBoSung.find((item) => !item.dongLuc)?.noiDungChiTiet ?? [];
        if (requirements.some((item) => item.thanhPhanId && !input.taiLieu.some((file) => file.thanhPhanId === item.thanhPhanId)))
          throw new Error("Hãy chọn tệp mới cho từng thành phần được yêu cầu bổ sung.");
        if (requirements.some((item) => item.truongThongTin?.trim()) && !input.thongTinDieuChinh?.trim())
          throw new Error("Hãy nhập đầy đủ nội dung đề nghị thay thế.");
        if (requirements.some((item) => !item.thanhPhanId && !item.truongThongTin?.trim()) && !input.noiDung?.trim())
          throw new Error("Hãy nhập giải trình theo yêu cầu bổ sung.");
      }
      await post(`/ho-so/${encodeURIComponent(d.hoSo.id)}/${action}`, input);
      dialog.close();
      toast("Đã thực hiện: " + ACTIONS[action]);
      await onSaved();
    });
  };
}
