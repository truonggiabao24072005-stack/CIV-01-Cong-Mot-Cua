import { api, session } from "./api.js";
import {
  $,
  esc,
  date,
  ROLES,
  route,
  navigate,
  template,
  installVietnameseValidation,
} from "./ui.js";
import { publicPage } from "./pages/public.js";
import {
  dashboardPage,
  casesPage,
  proceduresPage,
  guidePage,
} from "./pages/overview.js";
import { submissionPage } from "./pages/submission.js";
import { detailPage } from "./pages/detail.js";
import { catalogPage, reportPage } from "./pages/admin.js";

let actor = null, generation = 0;
const container = $("#app");
installVietnameseValidation();
function canCloseDialog() {
  const form = $("form", $("#dialog"));
  return !form?.dataset.busy && !form?.dataset.uploading;
}
$("#dialog").addEventListener("cancel", (event) => {
  if (!canCloseDialog()) event.preventDefault();
});
$("#dialog").addEventListener("click", (event) => {
  if (event.target.closest("[data-close]") && canCloseDialog())
    $("#dialog").close();
});
function signedIn(user, selected) {
  actor = user;
  const target =
    selected && actor.vaiTro === "NGUOI_NOP"
      ? route("new", selected)
      : route("dashboard");
  if (location.hash === target) render();
  else location.hash = target;
}
function signOut() {
  session.token = null;
  actor = null;
  history.replaceState(null, "", location.pathname);
  render();
}
const pageNames = {
  dashboard: "Tổng quan",
  cases: "Danh sách hồ sơ",
  new: "Nộp hồ sơ mới",
  detail: "Chi tiết hồ sơ",
  procedures: "Thủ tục hành chính",
  guide: "Hướng dẫn sử dụng",
  catalog: "Quản lý danh mục",
  reports: "Báo cáo thống kê",
};
const publicPages = new Map([
  ["trang-chu", ["portal-home", "Trang chủ"]],
  ["dich-vu", ["portal-services", "Dịch vụ công trực tuyến"]],
  ["tra-cuu", ["portal-lookup", "Tra cứu hồ sơ"]],
  ["huong-dan", ["portal-guide", "Hướng dẫn thực hiện"]],
]);
async function render() {
  const current = ++generation;
  const dialog = $("#dialog");
  if (dialog.open) dialog.close();
  const [path, query = ""] = location.hash.replace(/^#\/?/, "").split("?", 2);
  const params = new URLSearchParams(query);
  try {
    if (!actor || publicPages.has(path)) {
      const key = publicPages.has(path) ? path : "trang-chu";
      const [file, title] = publicPages.get(key);
      const [layout, content] = await Promise.all([template("public"), template(file)]);
      if (current !== generation) return;
      container.innerHTML = layout;
      const portal = container.firstElementChild;
      $("#portal-content", portal).innerHTML = content;
      $(`.portal-nav a[href="#/${key}"]`, portal).setAttribute("aria-current", "page");
      document.title = title + " | Cổng một cửa điện tử";
      window.scrollTo(0, 0);
      await publicPage(portal, key, params, actor, signedIn);
      return;
    }
    let [page = "dashboard", encodedId = ""] = path.split("/");
    if (!Object.hasOwn(pageNames, page)) page = "dashboard";
    const id = decodeURIComponent(encodedId);
    if (
      (["catalog", "reports"].includes(page) && actor.vaiTro !== "TIEP_NHAN") ||
      (page === "new" && actor.vaiTro !== "NGUOI_NOP")
    ) {
      navigate("dashboard");
      return;
    }
    const names = {
      ...pageNames,
      cases: actor.vaiTro === "NGUOI_NOP" ? "Hồ sơ của tôi" : "Danh sách hồ sơ",
    };
    const layout = await template("workspace");
    if (current !== generation) return;
    container.innerHTML = layout;
    $("#user-name").textContent = actor.ten;
    $("#user-role").textContent = ROLES[actor.vaiTro];
    $("#workspace-role").textContent = ROLES[actor.vaiTro];
    $("#user-initials").textContent = actor.ten
      .split(" ")
      .slice(-2)
      .map((word) => word[0])
      .join("");
    $("#page-title").textContent = names[page];
    document.title = names[page] + " | Cổng một cửa điện tử";
    if (page === "guide") $(".sidebar-guide").setAttribute("aria-current", "page");
    $("#today").textContent = date(new Date(), true);
    $("#logout").onclick = signOut;
    const links = [
      "dashboard",
      "cases",
      ...(actor.vaiTro === "NGUOI_NOP" ? ["new"] : []),
      "procedures",
      ...(actor.vaiTro === "TIEP_NHAN" ? ["catalog", "reports"] : []),
    ];
    $("#workspace-nav").innerHTML = links
      .map(
        (key) =>
          `<a href="${route(key)}" class="${key === page || (key === "cases" && page === "detail") ? "active" : ""}" ${key === page ? 'aria-current="page"' : ""}>${names[key]}</a>`,
      )
      .join("");
    const closeMenu = () => {
      $(".sidebar").classList.remove("open");
      $("#mobile-scrim").hidden = true;
      $("#menu-button").setAttribute("aria-expanded", "false");
    };
    $("#menu-button").onclick = () => {
      const open = $(".sidebar").classList.toggle("open");
      $("#mobile-scrim").hidden = !open;
      $("#menu-button").setAttribute("aria-expanded", String(open));
    };
    $("#mobile-scrim").onclick = closeMenu;
    const root = $("#page-content"),
      pageFile = { new: "new-case", reports: "report" }[page] || page;
    root.innerHTML = await template(pageFile);
    if (current !== generation) return;
    window.scrollTo({ top: 0 });
    const renderers = {
      dashboard: () => dashboardPage(root, actor),
      cases: () => casesPage(root, actor, params),
      procedures: () => proceduresPage(root, actor),
      new: () => submissionPage(root, actor, id),
      detail: () => detailPage(root, actor, id, params.get("from")),
      catalog: () => catalogPage(root, actor),
      reports: () => reportPage(root, actor),
      guide: () => guidePage(root, actor),
    };
    await renderers[page]();
  } catch (error) {
    if (current !== generation) return;
    if (error.status === 401 && actor) {
      signOut();
      return;
    }
    const root = $("#page-content") || container;
    root.innerHTML = `<section class="panel form-body"><h1>Chưa tải được nội dung</h1><p class="error-box" role="alert">${esc(error.message)}</p><a class="btn" href="${actor ? "#/dashboard" : "#/trang-chu"}">Về trang chủ</a><button class="btn" id="retry-page">Thử lại</button></section>`;
    $("#retry-page").onclick = render;
  }
}
window.addEventListener("hashchange", render);
async function start() {
  try {
    if (session.token) {
      try {
        actor = await api("/me");
      } catch (e) {
        if (e.status === 401 || e.status === 403) session.token = null;
        else throw e;
      }
    }
    await render();
  } catch (e) {
    container.innerHTML = `<section class="panel form-body"><h1>Chưa kết nối được hệ thống</h1><p>${esc(e.message)}</p><button class="btn" id="retry-start">Thử lại</button></section>`;
    $("#retry-start").onclick = start;
  }
}
start();
