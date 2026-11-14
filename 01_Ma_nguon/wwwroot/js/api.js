// Giao tiếp HTTP với backend. Không truy cập SQL Server từ trình duyệt.
const tokenKey = "cmc-access-token";
export const session = {
  get token() {
    return sessionStorage.getItem(tokenKey);
  },
  set token(value) {
    value
      ? sessionStorage.setItem(tokenKey, value)
      : sessionStorage.removeItem(tokenKey);
  },
};

export async function api(path, options = {}) {
  const headers = new Headers(options.headers);
  if (session.token) headers.set("Authorization", "Bearer " + session.token);
  if (options.body && !(options.body instanceof FormData))
    headers.set("Content-Type", "application/json");
  let response;
  try {
    response = await fetch("/api" + path, { ...options, headers });
  } catch {
    throw new Error("Không kết nối được hệ thống. Vui lòng thử lại.");
  }
  if (!response.ok) {
    let body = {};
    try {
      body = await response.json();
    } catch {
      /* Phản hồi lỗi có thể không phải JSON. */
    }
    if (response.status === 401) session.token = null;
    const message =
      body.message ||
      (response.status === 401
        ? "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
        : "Chưa thực hiện được thao tác.");
    throw Object.assign(
      new Error(message.replace(/\bSLA\b/g, "thời hạn giải quyết")),
      { status: response.status },
    );
  }
  if (options.download) return response;
  return response.status === 204 ? null : response.json();
}
export const post = (path, data) =>
  api(path, { method: "POST", body: JSON.stringify(data) });
export const put = (path, data) =>
  api(path, { method: "PUT", body: JSON.stringify(data) });
export async function signIn(username, password) {
  const result = await post("/auth/login", { username, password });
  session.token = result.accessToken;
  return api("/me");
}
export async function register(data) {
  const result = await post("/auth/register", data);
  session.token = result.accessToken;
  return api("/me");
}
export async function upload(file) {
  if (file.size > 10 * 1024 * 1024)
    throw new Error("Tệp không được vượt quá 10 MB.");
  const form = new FormData();
  form.append("file", file);
  return api("/tep", { method: "POST", body: form });
}
export async function download(path, fallback = "tai-lieu") {
  const response = await api(path, { download: true });
  const disposition = response.headers.get("content-disposition") || "";
  const filename = disposition.match(/filename\*=UTF-8''([^;]+)/i);
  const url = URL.createObjectURL(await response.blob());
  const link = document.createElement("a");
  link.href = url;
  link.download = filename ? decodeURIComponent(filename[1]) : fallback;
  link.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
