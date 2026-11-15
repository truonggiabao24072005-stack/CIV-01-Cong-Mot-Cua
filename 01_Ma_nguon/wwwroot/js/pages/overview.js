import { api } from "../api.js";
import { $, $$, esc, ROLES, STATES, badge, rowsTable, route, errorBox } from "../ui.js";
import { procedureDialog } from "./public.js";

export async function dashboardPage(root, actor) {
  const [stats, cases] = await Promise.all([
    api("/thong-ke"),
    api("/ho-so?pageSize=5"),
  ]);
  const citizen = actor.vaiTro === "NGUOI_NOP";
  const reception = actor.vaiTro === "TIEP_NHAN";
  const count = (status) =>
    stats.theoTrangThai.find((row) => row.trangThai === status)?.soLuong ?? 0;
  const statusCard = (label, status, text) => [label, count(status), text, { status }];
  const supplement = [
    citizen ? "Cần bổ sung" : "Chờ kiểm tra bổ sung",
    stats.boSungCanThaoTac,
    citizen ? "Có yêu cầu đang chờ bạn gửi" : "Đã có lần gửi chờ bạn kiểm tra",
    { work: "supplement" },
  ];
  let cards;
  if (citizen) {
    cards = [
      statusCard("Bản nháp", "BAN_NHAP", "Mở hồ sơ để tiếp tục kê khai"),
      supplement,
      statusCard("Chờ kiểm tra", "DA_GUI", "Đã nộp, đang chờ tiếp nhận"),
      statusCard("Đã trả kết quả", "DA_TRA_KET_QUA", "Xem và tải kết quả"),
    ];
  } else if (reception) {
    cards = [
      statusCard("Chờ kiểm tra", "DA_GUI", "Xem thành phần và đánh giá tài liệu"),
      statusCard("Chờ phân công", "DA_TIEP_NHAN", "Giao hồ sơ cho đúng bộ phận"),
      supplement,
      statusCard("Chờ trả kết quả", "DA_CO_KET_QUA", "Kiểm tra và phát hành kết quả"),
    ];
  } else {
    cards = [
      statusCard("Đang giải quyết", "DANG_GIAI_QUYET", "Hồ sơ đang được giao cho bạn"),
      supplement,
      statusCard("Chờ phát hành", "DA_CO_KET_QUA", "Tiếp nhận sẽ phát hành kết quả"),
      ["Hồ sơ quá hạn", stats.quaHan, "Theo ngày hẹn trả hiện tại", { deadline: "overdue" }],
    ];
  }

  $("#overview-title", root).textContent = citizen ? "Theo dõi hồ sơ" : "Bàn làm việc";
  $("#overview-description", root).textContent = citizen
    ? "Tiếp tục bản nháp, gửi bổ sung hoặc nhận kết quả từ hồ sơ của bạn."
    : "Mở nhóm hồ sơ cần xử lý theo vai trò của bạn.";
  $("#new-case-link", root).hidden = !citizen;
  $("#stats", root).innerHTML = cards.map(([label, total, text, query]) =>
    `<a class="stat-card" href="${route("cases")}?${new URLSearchParams(query)}"><span class="stat-top">${label}</span><strong>${total}</strong><p>${text}</p></a>`,
  ).join("");
  $("#recent-cases", root).innerHTML = rowsTable(cases.items, actor);
  $("#overdue-count", root).textContent = stats.quaHan;
  $("#status-breakdown", root).innerHTML = Object.keys(STATES)
    .filter((status) => count(status) > 0)
    .map((status) => `<li><a href="${route("cases")}?${new URLSearchParams({ status })}">
      ${badge(status)} <strong>${count(status).toLocaleString("vi-VN")}</strong>
    </a></li>`).join("") || "<li>Chưa có hồ sơ.</li>";
}

export async function casesPage(root, actor, params) {
  const citizen = actor.vaiTro === "NGUOI_NOP";
  $("#cases-title", root).textContent = citizen ? "Hồ sơ của tôi" : "Danh sách hồ sơ";
  $("#cases-description", root).textContent = citizen
    ? "Xem bước tiếp theo của từng hồ sơ hoặc lọc hồ sơ đang cần bổ sung."
    : "Lọc hồ sơ theo thủ tục, trạng thái, thời hạn hoặc lượt bổ sung đang chờ bạn kiểm tra.";
  $("#supplement-work-label", root).textContent = citizen
    ? "Cần gửi bổ sung" : "Chờ tôi kiểm tra bổ sung";
  $("#new-case-link", root).hidden = !citizen;
  const form = $("#case-filter", root);
  const { q, status, deadline, work, thuTucId, sort, pageSize } = form.elements;
  const requestedProcedure = params.get("thuTucId") || "";
  if (requestedProcedure) thuTucId.add(new Option(requestedProcedure, requestedProcedure));
  for (const [value, text] of Object.entries(STATES))
    status.add(new Option(text, value));
  for (const field of [q, status, deadline, work, thuTucId, sort])
    field.value = params.get(field.name) ?? "";
  pageSize.value = ["20", "50"].includes(params.get("pageSize")) ? params.get("pageSize") : "10";
  const page = Math.max(1, Math.min(1_000_000, Number.parseInt(params.get("page"), 10) || 1));
  function filters(targetPage) {
    const query = new URLSearchParams();
    for (const field of [q, status, deadline, work, thuTucId, sort]) {
      if (field.value.trim()) query.set(field.name, field.value.trim());
    }
    if (pageSize.value !== "10") query.set("pageSize", pageSize.value);
    if (targetPage > 1) query.set("page", String(targetPage));
    return query;
  }
  function show(targetPage = 1) {
    const query = filters(targetPage).toString();
    const target = route("cases") + (query ? "?" + query : "");
    if (location.hash !== target) location.hash = target;
  }
  form.onsubmit = (event) => {
    event.preventDefault();
    show();
  };
  status.onchange = () => {
    work.value = "";
    show();
  };
  work.onchange = () => {
    if (work.value) status.value = "";
    show();
  };
  for (const field of [deadline, thuTucId, sort, pageSize]) field.onchange = () => show();
  $("#clear-case-filter", root).onclick = () => {
    form.reset();
    show();
  };
  const table = $("#case-list", root);
  table.innerHTML = '<p class="loading" role="status">Đang tải danh sách hồ sơ…</p>';
  try {
    const query = filters(page);
    query.delete("page");
    const request = new URLSearchParams(query);
    request.set("page", String(page));
    request.set("pageSize", pageSize.value);
    request.set("includeFilters", "true");
    const result = await api("/ho-so?" + request);
    if (!root.isConnected) return;
    if (Array.isArray(result.procedures)) {
      thuTucId.replaceChildren(new Option("Tất cả thủ tục", ""));
      for (const procedure of result.procedures)
        thuTucId.add(new Option(`${procedure.ma} — ${procedure.ten}`, procedure.id));
      if (requestedProcedure && !result.procedures.some((p) => p.id === requestedProcedure))
        thuTucId.add(new Option("Thủ tục không có hồ sơ", requestedProcedure));
      thuTucId.value = requestedProcedure;
      thuTucId.disabled = false;
    }
    if (result.page > 1) query.set("page", String(result.page));
    table.innerHTML = rowsTable(result.items, actor, query.toString());
    $("#case-total", root).textContent = `${result.total.toLocaleString("vi-VN")} hồ sơ phù hợp`;
    $("#page-count", root).textContent = result.total
      ? `${(result.page - 1) * result.pageSize + 1}–${Math.min(result.page * result.pageSize, result.total)} / ${result.total} hồ sơ · Trang ${result.page}/${Math.ceil(result.total / result.pageSize)}`
      : "Không có hồ sơ";
    const previous = $("#previous-page", root);
    const next = $("#next-page", root);
    previous.disabled = result.page <= 1;
    next.disabled = result.page * result.pageSize >= result.total;
    previous.onclick = () => show(result.page - 1);
    next.onclick = () => show(result.page + 1);
  } catch (e) {
    if (!root.isConnected) return;
    table.innerHTML = "";
    errorBox($("[data-error]", root), e);
  }
}

export async function proceduresPage(root, actor) {
  const procedures = await api("/thu-tuc");
  $("#procedure-list", root).innerHTML =
    procedures
      .map(
        (p) =>
          `<article class="panel procedure-card"><span class="mini-label">${esc(p.ma)}</span><h2>${esc(p.ten)}</h2><p>${p.soNgaySLA} ${p.donViSLA === "NGAY_LAM_VIEC" ? "ngày làm việc" : "ngày lịch"} kể từ khi tiếp nhận</p><div class="actions"><button class="btn" data-procedure="${esc(p.id)}">Xem thành phần hồ sơ</button>${actor.vaiTro === "NGUOI_NOP" ? `<a class="btn primary" href="${route("new", p.id)}">Nộp hồ sơ</a>` : ""}</div></article>`,
      )
      .join("") || '<p class="empty">Chưa có thủ tục đang sử dụng.</p>';
  $("#procedure-list", root).onclick = (event) => {
    const button = event.target.closest("[data-procedure]");
    if (button)
      procedureDialog(
        procedures.find((p) => p.id === button.dataset.procedure),
      ).catch((e) => alertMessage(root, e));
  };
}
function alertMessage(root, error) {
  const p = document.createElement("p");
  p.className = "error-box";
  p.textContent = error.message;
  root.prepend(p);
}

export function guidePage(root, actor) {
  $("#guide-role", root).textContent =
    "Dành cho " + ROLES[actor.vaiTro].toLowerCase() + ".";
  const citizen = [
    [
      "Chọn thủ tục",
      "Xem thành phần, thời hạn và hướng dẫn trong Thủ tục hành chính.",
    ],
    [
      "Kê khai và nộp",
      "Điền thông tin, đính kèm đủ tài liệu, kiểm tra lại rồi xác nhận và nộp. Có thể lưu nháp để tiếp tục sau.",
    ],
    [
      "Bổ sung hồ sơ",
      "Mở Hồ sơ của tôi, đọc yêu cầu và hướng dẫn, gửi tài liệu hoặc giải trình rồi chờ cán bộ kiểm tra.",
    ],
    [
      "Nhận kết quả",
      "Khi hồ sơ đã trả kết quả, mở chi tiết và tải tệp. Biên nhận được cấp sau tiếp nhận.",
    ],
  ];
  const reception = [
    [
      "Kiểm tra tài liệu",
      "Mở hồ sơ chờ kiểm tra, đánh giá từng bản tài liệu. Nếu chưa đạt, ghi rõ lý do.",
    ],
    [
      "Tiếp nhận và phân công",
      "Tiếp nhận khi hồ sơ đủ điều kiện, sau đó giao cán bộ đúng bộ phận và ghi nội dung bàn giao.",
    ],
    [
      "Kiểm tra bổ sung",
      "Đánh giá các bản tệp mới trước khi chấp nhận cả lần gửi bổ sung. Lần gửi cũ vẫn được lưu.",
    ],
    [
      "Trả kết quả",
      "Kiểm tra tệp kết quả đã lập và xác nhận trả. Người dân có thể tải kết quả sau bước này.",
    ],
  ];
  const officer = [
    [
      "Xem hồ sơ được giao",
      "Danh sách hiển thị hồ sơ đang phụ trách và kết quả mình là cán bộ xử lý cuối.",
    ],
    [
      "Xử lý và yêu cầu bổ sung",
      "Nếu cần bổ sung, ghi rõ vấn đề, thành phần hoặc trường thông tin cần sửa, lý do và hướng dẫn.",
    ],
    [
      "Kiểm tra lần bổ sung",
      "Đánh giá từng bản tệp mới rồi kết luận lần gửi. Nếu chưa đạt, nêu rõ lý do.",
    ],
    [
      "Ghi kết quả",
      "Nhập kết luận, đính kèm tệp kết quả. Tiếp nhận thực hiện bước trả kết quả. Từ chối phải có căn cứ và lý do.",
    ],
  ];
  const steps =
    actor.vaiTro === "NGUOI_NOP"
      ? citizen
      : actor.vaiTro === "TIEP_NHAN"
        ? reception
        : officer;
  $("#guide-content", root).innerHTML = steps
    .map(
      ([title, text], i) =>
        `<article><b>${i + 1}</b><div><h2>${title}</h2><p>${text}</p></div></article>`,
    )
    .join("");
}
