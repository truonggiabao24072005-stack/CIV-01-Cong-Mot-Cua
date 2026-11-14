import { api, put, download } from "../api.js";
import {
  $,
  $$,
  esc,
  date,
  badge,
  late,
  nextCaseStep,
  contactInfo,
  errorBox,
  toast,
} from "../ui.js";
import { fileFields } from "../files.js";
import { previewDocument, previewResultFile } from "./documents.js";
import { ACTIONS, availableActions, actionDialog } from "./actions.js";

export async function detailPage(root, actor, id, returnQuery) {
  if (!id) throw new Error("Chưa chọn hồ sơ.");
  if (returnQuery) {
    $("#case-back", root).href = "#/cases?" + new URLSearchParams(returnQuery);
  }
  let detail,
    activeTab = "overview";
  const fail = (e) => toast(e.message);
  async function load() {
    const data = await api("/ho-so/" + encodeURIComponent(id));
    if (!root.isConnected) return;
    detail = data;
    const d = data,
      h = d.hoSo,
      y = d.yeuCauBoSung.find((x) => !x.dongLuc);
    const { actions } = availableActions(d, actor);
    const next = $("#case-next", root);
    next.innerHTML = `<p class="case-next-step"><span>Bước tiếp theo</span> <strong>${nextCaseStep(h, actor, y)}</strong></p>`;
    $("#case-heading", root).innerHTML =
      `<div class="page-heading detail-heading"><div><h1>${esc(h.ma)}</h1><p>${esc(d.thuTuc.ten)}</p></div><div class="actions">${badge(h.trangThai)}<button class="btn" data-reload>Tải lại</button>${h.ngayTiepNhan ? '<button class="btn" data-document="receipt">Xem / in giấy hẹn</button>' : ""}</div></div>`;
    $("[data-reload]", root).onclick = () => load().catch(fail);
    const latestSubmission = y?.lanGui[0];
    const missing = d.thanhPhan.filter(
      (c) => c.batBuoc && !d.taiLieu.some((f) => f.hienHanh && f.thanhPhanId === c.id),
    );
    const unchecked = d.taiLieu.filter((f) => f.hienHanh && f.kiemTra[0]?.ketQua !== "DAT");
    const pendingFiles = d.taiLieu.filter((f) => f.choDanhGia);
    const supplementReview = actions.includes("kiem-tra-bo-sung");
    const needsDocumentReview = pendingFiles.some((f) => f.kiemTra[0]?.ketQua !== "DAT");
    const alerts = $("#case-alerts", root);
    alerts.innerHTML = y
      ? `<div class="supplement-alert"><div><strong>${y.trangThai === "DA_GUI" ? "Đã gửi bổ sung, chờ cán bộ kiểm tra" : "Hồ sơ cần bổ sung"}</strong>${y.noiDungChiTiet.map((n) => `<p><b>${esc(n.vanDe)}</b><br>Lý do: ${esc(n.lyDo)}<br>Hướng dẫn: ${esc(n.huongDan)}</p>`).join("")}${y.trangThai === "DANG_CHO" && latestSubmission?.danhGia === "CHUA_DAT" ? `<p><b>Lần gửi trước chưa đạt:</b> ${esc(latestSubmission.nhanXet)}</p>` : ""}<p><button type="button" class="btn" data-document="supplement">Xem / in phiếu yêu cầu bổ sung</button></p>${supplementReview && pendingFiles.length ? `<p>${needsDocumentReview ? "Cần đánh giá Đạt từng tệp mới trước khi chấp nhận lần gửi." : "Tất cả tệp mới đã được đánh giá Đạt."}</p><button type="button" class="btn" data-open-files>Xem tệp bổ sung</button>` : ""}</div></div>`
      : actions.includes("tiep-nhan")
        ? `<div class="supplement-alert"><div><strong>Điều kiện tiếp nhận</strong>${missing.length ? `<p>Còn thiếu: ${missing.map((c) => esc(c.ten)).join(", ")}.</p>` : ""}${unchecked.length ? `<p>Chưa được đánh giá Đạt: ${unchecked.map((f) => esc(f.tenTep)).join(", ")}.</p><button type="button" class="btn" data-open-files>Mở tài liệu để đánh giá</button>` : ""}${!missing.length && !unchecked.length ? "<p>Đã đủ tài liệu bắt buộc và các bản hiện hành đều đạt.</p>" : ""}</div></div>`
        : "";
    const reviewLink = $("[data-open-files]", alerts);
    if (reviewLink) {
      reviewLink.onclick = () => {
        activeTab = "files";
        drawTab();
        $("#detail-tabs", root).scrollIntoView({ block: "nearest" });
        $('[data-tab="files"]', root).focus({ preventScroll: true });
      };
    }
    const bar = $("#case-actions", root);
    const showResult = actor.vaiTro === "NGUOI_NOP" && Boolean(d.ketQua?.traLuc);
    bar.hidden = !actions.length && !showResult;
    const blocked = actions.includes("tiep-nhan") && Boolean(missing.length || unchecked.length);
    const primary = blocked ? actions.find((a) => a === "yeu-cau-bo-sung") : actions[0];
    const buttons = actions.map((action) => {
      const style = action === "tu-choi" ? "danger" : action === primary ? "primary" : "";
      const disabled = action === "tiep-nhan" && blocked ? "disabled" : "";
      return `<button type="button" class="btn ${style}" data-action="${action}" ${disabled}>${ACTIONS[action]}</button>`;
    });
    if (showResult)
      buttons.push('<button type="button" class="btn primary" data-open-result>Xem kết quả</button>');
    bar.innerHTML = `<span>Thao tác hồ sơ</span><div>${buttons.join("")}</div>`;
    $("#draft-editor", root).innerHTML = "";
    if (actor.vaiTro === "NGUOI_NOP" && h.trangThai === "BAN_NHAP")
      draftEditor($("#draft-editor", root), d, load);
    const officer = d.luanChuyen.find((x) => !x.ketThucLuc) || d.luanChuyen[0];
    $("#case-aside", root).innerHTML =
      `<section class="panel deadline-card ${late(h) ? "late" : ""}"><p class="section-kicker">THỜI HẠN GIẢI QUYẾT</p><h3>${date(h.hanTra, true)}</h3><p>${h.ngayKetThuc ? "Đã kết thúc: " + date(h.ngayKetThuc) : !h.hanTra ? "Thời hạn bắt đầu sau tiếp nhận." : late(h) ? "Đang quá hạn" : "Chưa quá hạn"}</p>${h.hanTraGoc ? `<small>Ngày hẹn trả ban đầu: ${date(h.hanTraGoc)}<br>Thời gian giải quyết: ${d.sla.soNgaySLA} ${d.sla.donViSLA === "NGAY_LAM_VIEC" ? "ngày làm việc" : "ngày lịch"}</small>` : ""}</section><section class="panel assignment-card"><p class="section-kicker">${officer?.ketThucLuc ? "CÁN BỘ XỬ LÝ CUỐI" : "CÁN BỘ PHỤ TRÁCH"}</p><strong>${esc(officer?.canBo || "Chưa phân công cán bộ")}</strong><p>${esc(officer?.boPhan)}</p></section>`;
    drawTab();
  }
  function drawTab() {
    const d = detail,
      h = d.hoSo,
      body = $("#detail-body", root);
    $$("[data-tab]", root).forEach((button) => {
      button.classList.toggle("active", button.dataset.tab === activeTab);
    });
    if (activeTab === "history") {
      body.innerHTML = `<div class="timeline">${d.lichSu.map((x) => `<article><span class="timeline-dot"></span><div class="timeline-top"><strong>${esc(ACTIONS[x.hanhDong.toLowerCase().replaceAll("_", "-")] || "Cập nhật hồ sơ")}</strong><time>${date(x.thoiDiem)}</time></div><p class="preserve">${esc(x.noiDung)}</p><small>${esc(x.nguoiThucHien)}</small></article>`).join("")}</div>`;
      return;
    }
    if (activeTab === "files") {
      const { canReview } = availableActions(d, actor);
      body.innerHTML =
        [...d.taiLieu]
          .sort((a, b) => Number(b.choDanhGia) - Number(a.choDanhGia))
          .map(
            (f) =>
              `<article class="file-review"><div class="document-row"><div><strong>${esc(f.tenTep)}</strong><p>${esc(f.thanhPhan)} · Phiên bản ${f.soPhienBan}</p><small>${f.hienHanh ? "Bản hiện hành" : f.choDanhGia ? "Bản bổ sung chờ kiểm tra" : "Bản không hiện hành"} · ${Math.ceil(f.dungLuong / 1024)} KB</small></div><button class="btn" data-download="${esc(f.id)}">Tải tài liệu</button></div>${f.kiemTra.map((k) => `<p class="check-row"><b>${k.ketQua === "DAT" ? "Đạt" : "Chưa đạt"}</b> · ${esc(k.canBo)} · ${date(k.thoiDiem)}<br>${esc(k.nhanXet)}</p>`).join("")}${canReview && (f.hienHanh || f.choDanhGia) ? `<button class="btn" data-review="${esc(f.id)}">Đánh giá bản tài liệu</button>` : ""}</article>`,
          )
          .join("") || '<p class="empty">Chưa có tài liệu.</p>';
      return;
    }
    const contact = contactInfo(h);
    body.innerHTML = `<p class="section-kicker">NGƯỜI NỘP</p><strong>${esc(d.nguoiNop.ten)}</strong><p>${esc(contact.email || d.nguoiNop.lienHe)}</p>${contact.soDienThoai ? `<p>Điện thoại: ${esc(contact.soDienThoai)}</p>` : ""}${contact.diaChi ? `<p>Địa chỉ: ${esc(contact.diaChi)}</p>` : ""}<p class="section-kicker">NỘI DUNG ĐỀ NGHỊ</p><p class="preserve">${esc(h.noiDung)}</p><dl class="procedure-facts"><dt>Mã gửi</dt><dd>${esc(h.maGui || "Chưa nộp")}</dd><dt>Mã hồ sơ</dt><dd>${esc(h.maHoSo || "Chưa tiếp nhận")}</dd><dt>Ngày nộp</dt><dd>${date(h.ngayNop)}</dd></dl>
      ${d.ketQua ? `<section class="result-card" id="published-result" tabindex="-1"><div><h3>${d.ketQua.loai === "THANH_CONG" ? "Kết quả giải quyết" : "Quyết định từ chối"}</h3><p class="preserve">${esc(d.ketQua.noiDung)}</p>${d.ketQua.lyDoTuChoi ? `<p>Lý do: ${esc(d.ketQua.lyDoTuChoi)}</p>` : ""}<p>${d.ketQua.traLuc ? "Phát hành: " + date(d.ketQua.traLuc) : "Chờ phát hành"}</p>${d.ketQua.traLuc ? '<button type="button" class="btn primary" data-document="result">Xem / in thông báo</button>' : ""} ${d.coTepKetQua ? '<button type="button" class="btn" data-preview-result>Xem tệp kết quả</button> <button type="button" class="btn" data-result>Tải tệp</button>' : ""}</div></section>` : ""}
      ${d.yeuCauBoSung.length ? "<h3>Yêu cầu bổ sung và các lần gửi</h3>" : ""}${d.yeuCauBoSung.map((r) => `<section class="request-card"><strong>${esc(r.noiDung)}</strong><p>${r.dongLuc ? "Đã bổ sung đầy đủ" : "Đang mở"} · ${r.soLanGui} lần gửi</p>${r.lanGui.map((l) => `<div class="submission-row"><b>Lần ${l.soLan} · ${date(l.guiLuc)}</b><p class="preserve">${esc(l.noiDungGiaiTrinh || "Bổ sung tài liệu")}</p>${l.thongTinDieuChinh ? `<p>Đề nghị sửa: ${esc(l.thongTinDieuChinh)}</p>` : ""}<small>${l.danhGia === "DAT" ? "Đạt" : l.danhGia === "CHUA_DAT" ? "Chưa đạt" : "Chờ kiểm tra"} ${esc(l.nhanXet)}</small></div>`).join("")}</section>`).join("")}
      ${d.luanChuyen.length ? "<h3>Lịch sử phân công</h3>" : ""}${d.luanChuyen.map((l) => `<section class="request-card"><strong>${esc(l.canBo)} · ${esc(l.boPhan)}</strong><p>${date(l.chuyenLuc)} — ${l.ketThucLuc ? date(l.ketThucLuc) : "Đang phụ trách"}</p><p>${esc(l.noiDung)}</p></section>`).join("")}`;
  }
  $("#detail-tabs", root).onclick = (event) => {
    const button = event.target.closest("[data-tab]");
    if (button) {
      activeTab = button.dataset.tab;
      drawTab();
    }
  };
  root.addEventListener("click", (event) => {
    const action = event.target.closest("[data-action]"),
      review = event.target.closest("[data-review]"),
      file = event.target.closest("[data-download]");
    if (action) actionDialog(action.dataset.action, detail, load).catch(fail);
    if (review)
      actionDialog(
        "kiem-tra-tai-lieu",
        detail,
        load,
        review.dataset.review,
      ).catch(fail);
    if (file)
      download("/tai-lieu/" + encodeURIComponent(file.dataset.download)).catch(
        fail,
      );
    if (event.target.closest("[data-open-result]")) {
      activeTab = "overview";
      drawTab();
      const result = $("#published-result", root);
      result?.scrollIntoView({ block: "nearest" });
      result?.focus({ preventScroll: true });
    }
    const documentButton = event.target.closest("[data-document]");
    if (documentButton) previewDocument(detail, documentButton.dataset.document);
    if (event.target.closest("[data-preview-result]")) previewResultFile(id).catch(fail);
    if (event.target.closest("[data-result]"))
      download(`/ho-so/${encodeURIComponent(id)}/tep-ket-qua`, "ket-qua").catch(
        fail,
      );
  });
  await load();
}

function draftEditor(container, d, onSaved) {
  const contact = contactInfo(d.hoSo);
  container.innerHTML = `<form class="panel form-body" id="draft-form"><h2>Hoàn thiện bản nháp</h2><p data-error class="error-box" hidden role="alert"></p><div class="official-form-grid"><label class="field">Số điện thoại liên hệ<input name="soDienThoai" type="tel" maxlength="30" value="${esc(contact.soDienThoai ?? d.nguoiNop.soDienThoai)}"></label><label class="field">Địa chỉ liên hệ<input name="diaChi" maxlength="500" value="${esc(contact.diaChi)}"></label></div><label class="field">Nội dung đề nghị<textarea name="noiDung" rows="4" maxlength="6000">${esc(d.hoSo.noiDung)}</textarea></label><p>Tệp mới thay bản hiện hành của cùng thành phần. Bản cũ được lưu để đối chiếu.</p><div id="draft-files"></div><button class="btn primary" type="submit">Lưu thay đổi bản nháp</button></form>`;
  const form = $("#draft-form", container),
    submit = $('button[type="submit"]', form);
  const files = fileFields($("#draft-files", form), d.thanhPhan, (busy) => {
    submit.disabled = busy || Boolean(form.dataset.busy);
  }, d.taiLieu.filter((file) => file.hienHanh));
  form.onsubmit = (event) => {
    event.preventDefault();
    if (files.busy || form.dataset.busy) return;
    form.dataset.busy = "true";
    submit.disabled = true;
    errorBox($("[data-error]", form), "");
    put(
      `/ho-so/${encodeURIComponent(d.hoSo.id)}/nhap?version=${d.hoSo.version}`,
      {
        thuTucId: d.thuTuc.id,
        noiDung: form.elements.noiDung.value,
        taiLieu: files.data(),
        thongTinLienHe: {
          soDienThoai: form.elements.soDienThoai.value,
          diaChi: form.elements.diaChi.value,
        },
        nop: false,
      },
    )
      .then(async () => {
        toast("Đã cập nhật bản nháp.");
        await onSaved();
      })
      .catch((e) => errorBox($("[data-error]", form), e))
      .finally(() => {
        delete form.dataset.busy;
        submit.disabled = false;
      });
  };
}
