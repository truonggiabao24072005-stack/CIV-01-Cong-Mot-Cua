import { api, signIn, register } from "../api.js";
import {
  $,
  $$,
  esc,
  badge,
  date,
  navigate,
  errorBox,
  openDialog,
  submitForm,
  formObject,
} from "../ui.js";

export function authDialog(mode, onSignedIn, hint = "") {
  const registration = mode === "register";
  const dialog = openDialog(
    registration ? "Đăng ký tài khoản người dân" : "Đăng nhập",
    `<form id="auth-form"><p data-error class="error-box" role="alert" hidden></p><label class="field">Tên đăng nhập<input name="username" required maxlength="40" autocomplete="username" value="${esc(hint)}"></label>${registration ? '<label class="field">Họ và tên<input name="hoTen" required maxlength="100" autocomplete="name"></label><label class="field">Thư điện tử<input name="email" type="email" required maxlength="255" autocomplete="email"></label>' : ""}<label class="field">Mật khẩu<input name="password" type="password" required minlength="${registration ? 12 : 1}" maxlength="128" autocomplete="${registration ? "new-password" : "current-password"}">${registration ? "<small>Mật khẩu có ít nhất 12 ký tự.</small>" : ""}</label><button type="submit" class="btn primary">${registration ? "Tạo tài khoản" : "Đăng nhập"}</button></form>`,
    true,
  );
  $("#auth-form").addEventListener("submit", (event) => {
    event.preventDefault();
    const form = event.currentTarget;
    submitForm(form, async () => {
      const data = formObject(form);
      const actor = registration
        ? await register(data)
        : await signIn(data.username, data.password);
      dialog.close();
      onSignedIn(actor);
    });
  });
}

export async function procedureDialog(procedure, onBegin) {
  const components = await api(
    `/thu-tuc/${encodeURIComponent(procedure.id)}/thanh-phan`,
  );
  const dialog = openDialog(
    procedure.ten,
    `<dl class="procedure-facts"><dt>Mã thủ tục</dt><dd>${esc(procedure.ma)}</dd><dt>Hướng dẫn</dt><dd class="preserve">${esc(procedure.moTa)}</dd><dt>Thời hạn giải quyết</dt><dd>${procedure.soNgaySLA} ${procedure.donViSLA === "NGAY_LAM_VIEC" ? "ngày làm việc" : "ngày lịch"} kể từ khi tiếp nhận</dd></dl><h3>Thành phần hồ sơ</h3><div class="table-scroll"><table class="case-table"><thead><tr><th>Thành phần</th><th>Yêu cầu</th><th>Hướng dẫn</th></tr></thead><tbody>${components.map((c) => `<tr><td>${esc(c.ten)}</td><td>${c.batBuoc ? "Bắt buộc" : "Theo trường hợp"}</td><td>${esc(c.huongDan)}</td></tr>`).join("")}</tbody></table></div>${onBegin ? '<div class="modal-footer"><button class="btn primary" data-begin>Nộp trực tuyến</button></div>' : ""}`,
  );
  if (onBegin) $("[data-begin]", dialog).onclick = onBegin;
}

export async function publicPage(root, page, params, actor, onSignedIn) {
  const onAuth = (mode, procedureId) =>
    authDialog(mode, (user) => onSignedIn(user, procedureId));
  for (const button of $$("[data-auth]", root)) {
    button.hidden = Boolean(actor);
    button.onclick = () => onAuth(button.dataset.auth);
  }
  $("#portal-workspace", root).hidden = !actor;

  const search = $("#public-search", root);
  if (search) {
    search.elements.q.value = params.get("q") || "";
    search.onsubmit = (event) => {
      event.preventDefault();
      const query = search.elements.q.value.trim();
      location.hash = "#/dich-vu" + (query ? "?" + new URLSearchParams({ q: query }) : "");
    };
  }

  if (page === "dich-vu") await serviceList(root, actor, onAuth, search.elements.q.value);
  if (page === "tra-cuu") {
    $("#lookup-workspace", root).hidden = !actor;
    setupLookup(root);
  }
}

async function serviceList(root, actor, onAuth, query) {
  const list = $("#public-procedures", root);
  const showError = (error) => errorBox($("#public-error", root), error);
  let procedures;
  try {
    procedures = await api("/thu-tuc");
  } catch (error) {
    list.innerHTML = "";
    showError(error);
    return;
  }
  const term = query.toLocaleLowerCase("vi");
  const rows = procedures.filter((p) =>
    (p.ma + " " + p.ten).toLocaleLowerCase("vi").includes(term),
  );
  const canSubmit = !actor || actor.vaiTro === "NGUOI_NOP";
  $("#procedure-list-label", root).textContent = term ? "Kết quả tìm kiếm" : "Thủ tục đang tiếp nhận";
  $("#procedure-count", root).textContent = rows.length + " thủ tục";
  list.innerHTML = rows.map((p) => {
    const timeUnit = p.donViSLA === "NGAY_LAM_VIEC" ? "ngày làm việc" : "ngày lịch";
    const submit = canSubmit
      ? `<button type="button" class="btn primary" data-begin="${esc(p.id)}">Nộp trực tuyến</button>`
      : "";
    return `<article class="portal-procedure"><div><span class="procedure-code">Mã thủ tục ${esc(p.ma)}</span><h3>${esc(p.ten)}</h3><p>Thời hạn giải quyết: ${esc(p.soNgaySLA)} ${timeUnit} kể từ khi tiếp nhận.</p></div><div class="actions"><button type="button" class="btn" data-procedure="${esc(p.id)}">Xem thành phần</button>${submit}</div></article>`;
  }).join("") || `<p class="empty">${term ? "Không tìm thấy thủ tục phù hợp." : "Chưa có thủ tục đang tiếp nhận trực tuyến."}</p>`;
  function beginProcedure(id) {
    if (actor) navigate("new", id);
    else onAuth("login", id);
  }
  list.onclick = (event) => {
    const detail = event.target.closest("[data-procedure]");
    const begin = event.target.closest("[data-begin]");
    if (detail) {
      const id = detail.dataset.procedure;
      procedureDialog(
        procedures.find((procedure) => procedure.id === id),
        canSubmit ? () => beginProcedure(id) : undefined,
      ).catch(showError);
    }
    if (begin) beginProcedure(begin.dataset.begin);
  };
}

function setupLookup(root) {
  const form = $("#public-lookup", root);
  const submit = $('button[type="submit"]', form);
  const retry = $("[data-retry-challenge]", form);
  const label = $("#challenge-label", form);
  const error = $("[data-error]", form);
  const output = $("#lookup-result", root);
  let challenge;
  async function loadChallenge() {
    try {
      challenge = await api("/tra-cuu/thu-thach");
      label.textContent = "Kết quả phép tính " + challenge.cauHoi;
      form.elements.answer.value = "";
    } catch (e) {
      challenge = null;
      label.textContent = "Chưa tải được phép tính xác nhận";
      errorBox(error, e);
    }
    submit.disabled = !challenge;
    retry.hidden = Boolean(challenge);
  }
  retry.onclick = () => {
    errorBox(error, "");
    loadChallenge();
  };
  form.onsubmit = async (event) => {
    event.preventDefault();
    if (!challenge) return;
    await submitForm(form, async () => {
      output.replaceChildren();
      try {
        const result = await api("/tra-cuu", {
          method: "POST",
          body: JSON.stringify({ ...formObject(form), challengeId: challenge.id }),
        });
        output.innerHTML = `<div class="lookup-result"><strong>${esc(result.ma)}</strong> ${badge(result.trangThai)}<dl class="lookup-dates"><dt>Ngày nộp</dt><dd>${date(result.ngayNop)}</dd><dt>Ngày tiếp nhận</dt><dd>${date(result.ngayTiepNhan)}</dd><dt>Ngày hẹn trả</dt><dd>${date(result.hanTra)}</dd><dt>Ngày kết thúc</dt><dd>${date(result.ngayKetThuc)}</dd></dl></div>`;
      } finally {
        await loadChallenge();
      }
    });
    submit.disabled = !challenge;
  };
  loadChallenge();
}
