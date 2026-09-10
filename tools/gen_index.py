#!/usr/bin/env python3
"""
gen_index.py — 扫描目录下的 APK 文件，生成带下载链接和二维码的 HTML 页面
二维码由浏览器端 qrcode.js 生成，无需任何 Python 额外依赖。

用法:
  python3 gen_index.py [目录路径] [--base-url URL] [--output 输出文件名]

示例:
  python3 gen_index.py /path/to/upload/2026/06/25 \
      --base-url http://192.168.1.18:5005/upload/2026/06/25
"""

import argparse
import json
import sys
import time
from pathlib import Path


def human_size(n: int) -> str:
    for unit in ['B', 'KB', 'MB', 'GB']:
        if n < 1024:
            return f"{n:.1f} {unit}" if unit != 'B' else f"{n} B"
        n /= 1024
    return f"{n:.1f} TB"


def scan_apk_files(directory: Path) -> list[dict]:
    """只扫描 .apk 文件"""
    files = []
    for entry in sorted(directory.rglob("*.apk")):
        if not entry.is_file() or entry.name.startswith('.'):
            continue
        rel  = entry.relative_to(directory)
        stat = entry.stat()
        files.append({
            'name':     entry.name,
            'rel_path': str(rel).replace('\\', '/'),
            'size':     stat.st_size,
            'size_hr':  human_size(stat.st_size),
            'mtime':    time.strftime('%Y-%m-%d %H:%M', time.localtime(stat.st_mtime)),
        })
    return files


def build_html(files: list[dict], base_url: str, scan_dir: Path) -> str:
    now       = time.strftime('%Y-%m-%d %H:%M:%S')
    total_sz  = human_size(sum(f['size'] for f in files))
    files_json   = json.dumps(
        [{'name': f['name'], 'rel': f['rel_path'],
          'size': f['size_hr'], 'mtime': f['mtime']}
         for f in files],
        ensure_ascii=False
    )
    base_url_js = json.dumps(base_url.rstrip('/') if base_url else '')

    return f"""<!DOCTYPE html>
<html lang="zh-CN">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>APK 下载 — {scan_dir.name}</title>
<script src="https://cdnjs.cloudflare.com/ajax/libs/qrcode/1.5.0/qrcode.min.js"></script>
<style>
  *, *::before, *::after {{ box-sizing: border-box; margin: 0; padding: 0; }}
  body {{
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    background: #0f172a; color: #e2e8f0; padding: 24px; min-height: 100vh;
  }}
  h1  {{ font-size: 20px; color: #38bdf8; margin-bottom: 4px; }}
  .meta {{ color: #64748b; font-size: 13px; margin-bottom: 16px; }}

  /* 搜索栏 */
  .search-bar {{ display: flex; gap: 8px; margin-bottom: 16px; }}
  .search-bar input {{
    flex: 1; padding: 9px 14px; background: #1e293b;
    border: 1px solid #334155; border-radius: 8px;
    color: #e2e8f0; font-size: 14px; outline: none;
  }}
  .search-bar input:focus {{ border-color: #38bdf8; }}

  /* 表格 */
  table {{
    width: 100%; border-collapse: collapse;
    background: #1e293b; border-radius: 12px; overflow: hidden;
    box-shadow: 0 4px 24px rgba(0,0,0,.35);
  }}
  th {{
    background: #0f172a; color: #94a3b8; font-size: 12px;
    padding: 11px 16px; text-align: left; font-weight: 500;
    border-bottom: 1px solid #334155;
  }}
  tr:hover td {{ background: #263548; }}
  td {{
    padding: 12px 16px; border-bottom: 1px solid #1e293b;
    font-size: 13px; vertical-align: middle;
  }}
  tr:last-child td {{ border-bottom: none; }}
  .icon  {{ font-size: 22px; width: 44px; text-align: center; }}
  .name a {{ color: #e2e8f0; text-decoration: none; font-weight: 600; font-size: 14px; }}
  .name a:hover {{ color: #38bdf8; }}
  .subpath {{ color: #475569; font-size: 11px; margin-top: 3px; }}
  .size  {{ color: #94a3b8; white-space: nowrap; }}
  .mtime {{ color: #64748b; white-space: nowrap; font-size: 12px; }}
  .actions {{ white-space: nowrap; }}
  .btn-dl, .btn-open {{
    display: inline-block; padding: 5px 12px; border-radius: 6px;
    font-size: 12px; text-decoration: none; margin-right: 5px;
  }}
  .btn-dl   {{ background: #0ea5e9; color: #fff; }}
  .btn-dl:hover {{ background: #0284c7; }}
  .btn-open {{ background: #334155; color: #94a3b8; }}
  .btn-open:hover {{ background: #475569; color: #e2e8f0; }}

  /* 小二维码（表格内） */
  .qr-thumb {{
    display: block; width: 72px; height: 72px; cursor: zoom-in;
    border-radius: 6px; border: 2px solid #334155;
    background: #fff; transition: border-color .15s;
  }}
  .qr-thumb:hover {{ border-color: #38bdf8; }}

  /* 空行 */
  #emptyRow td {{ text-align: center; padding: 60px; color: #475569; }}

  /* ── 模态框（放大二维码） ── */
  .modal-overlay {{
    display: none; position: fixed; inset: 0;
    background: rgba(0,0,0,.75); z-index: 1000;
    align-items: center; justify-content: center;
    cursor: zoom-out;
  }}
  .modal-overlay.active {{ display: flex; }}
  .modal-box {{
    background: #1e293b; border-radius: 16px; padding: 28px 32px;
    text-align: center; box-shadow: 0 20px 60px rgba(0,0,0,.6);
    max-width: 90vw; cursor: default;
    animation: pop .15s ease;
  }}
  @keyframes pop {{
    from {{ transform: scale(.85); opacity: 0; }}
    to   {{ transform: scale(1);   opacity: 1; }}
  }}
  .modal-box canvas {{
    display: block; margin: 0 auto 14px;
    border-radius: 10px; border: 3px solid #38bdf8;
  }}
  .modal-filename {{
    color: #e2e8f0; font-size: 14px; font-weight: 600;
    margin-bottom: 6px; word-break: break-all;
  }}
  .modal-url {{
    color: #64748b; font-size: 11px; word-break: break-all;
    margin-bottom: 16px;
  }}
  .modal-close {{
    background: #334155; color: #94a3b8; border: none;
    padding: 8px 20px; border-radius: 6px; cursor: pointer;
    font-size: 13px;
  }}
  .modal-close:hover {{ background: #475569; color: #e2e8f0; }}

  @media (max-width: 640px) {{
    .mtime {{ display: none; }}
    .subpath {{ display: none; }}
    th:nth-child(4), td:nth-child(4) {{ display: none; }}
  }}
</style>
</head>
<body>

<h1>📦 APK 下载中心 — {scan_dir.name}</h1>
<p class="meta">共 {len(files)} 个安装包，合计 {total_sz} &nbsp;|&nbsp; 生成时间: {now}</p>

<div class="search-bar">
  <input id="searchInput" type="search" placeholder="🔍 搜索 APK 文件名..." autocomplete="off">
</div>

<table id="fileTable">
  <thead>
    <tr>
      <th></th>
      <th>文件名</th>
      <th>大小</th>
      <th>修改时间</th>
      <th>操作</th>
      <th>扫码下载</th>
    </tr>
  </thead>
  <tbody id="tbody"></tbody>
</table>

<!-- 放大模态框 -->
<div class="modal-overlay" id="modalOverlay" onclick="closeModal(event)">
  <div class="modal-box" onclick="event.stopPropagation()">
    <div class="modal-filename" id="modalName"></div>
    <div class="modal-url"     id="modalUrl"></div>
    <canvas id="modalCanvas"></canvas>
    <br>
    <button class="modal-close" onclick="hideModal()">✕ 关闭</button>
  </div>
</div>

<script>
const BASE_URL = {base_url_js};
const FILES    = {files_json};

function dlUrl(rel) {{
  return BASE_URL ? BASE_URL + '/' + rel : rel;
}}

// ── 渲染表格 ──────────────────────────────────────────────────────────────
function renderTable(list) {{
  const tbody = document.getElementById('tbody');
  tbody.innerHTML = '';

  if (!list.length) {{
    const tr = document.createElement('tr');
    tr.id = 'emptyRow';
    tr.innerHTML = '<td colspan="6">没有匹配的 APK 文件</td>';
    tbody.appendChild(tr);
    return;
  }}

  list.forEach((f, idx) => {{
    const url = dlUrl(f.rel);
    const tr  = document.createElement('tr');
    tr.dataset.idx = idx;
    tr.innerHTML = `
      <td class="icon">📦</td>
      <td class="name">
        <a href="${{url}}" download="${{f.name}}" title="${{f.rel}}">${{f.name}}</a>
        <div class="subpath">${{f.rel}}</div>
      </td>
      <td class="size">${{f.size}}</td>
      <td class="mtime">${{f.mtime}}</td>
      <td class="actions">
        <a class="btn-dl"   href="${{url}}" download="${{f.name}}">⬇ 下载</a>
        <a class="btn-open" href="${{url}}" target="_blank">🔗 打开</a>
      </td>
      <td style="text-align:center;width:100px">
        <canvas id="qr-${{idx}}" class="qr-thumb"
                onclick="openModal('${{f.name}}','${{url}}',${{idx}})"
                title="点击放大"></canvas>
      </td>
    `;
    tbody.appendChild(tr);
  }});
}}

// ── 生成缩略二维码 ─────────────────────────────────────────────────────────
function generateQRCodes(list) {{
  let i = 0;
  function next() {{
    if (i >= list.length) return;
    const url    = dlUrl(list[i].rel);
    const canvas = document.getElementById('qr-' + i);
    if (canvas) {{
      QRCode.toCanvas(canvas, url, {{
        width: 72, margin: 1,
        color: {{ dark: '#000', light: '#fff' }},
        errorCorrectionLevel: 'M',
      }});
    }}
    i++;
    (i % 8 === 0) ? requestAnimationFrame(next) : next();
  }}
  if (typeof QRCode === 'undefined') {{
    window.addEventListener('load', next);
  }} else {{
    requestAnimationFrame(next);
  }}
}}

// ── 模态框放大二维码 ────────────────────────────────────────────────────────
function openModal(name, url, idx) {{
  document.getElementById('modalName').textContent = name;
  document.getElementById('modalUrl').textContent  = url;
  const canvas = document.getElementById('modalCanvas');

  // 根据屏幕宽度决定放大尺寸
  const size = Math.min(window.innerWidth - 100, 360);

  QRCode.toCanvas(canvas, url, {{
    width: size, margin: 2,
    color: {{ dark: '#000', light: '#fff' }},
    errorCorrectionLevel: 'H',  // 放大版用高纠错等级
  }});

  document.getElementById('modalOverlay').classList.add('active');
  document.body.style.overflow = 'hidden';
}}

function hideModal() {{
  document.getElementById('modalOverlay').classList.remove('active');
  document.body.style.overflow = '';
}}

function closeModal(e) {{
  if (e.target === document.getElementById('modalOverlay')) hideModal();
}}

// ESC 键关闭
document.addEventListener('keydown', e => {{
  if (e.key === 'Escape') hideModal();
}});

// ── 搜索过滤 ──────────────────────────────────────────────────────────────
let currentList = FILES.slice();

document.getElementById('searchInput').addEventListener('input', function() {{
  const kw = this.value.trim().toLowerCase();
  currentList = kw
    ? FILES.filter(f => f.name.toLowerCase().includes(kw) || f.rel.toLowerCase().includes(kw))
    : FILES.slice();
  renderTable(currentList);
  generateQRCodes(currentList);
}});

// ── 初始化 ────────────────────────────────────────────────────────────────
renderTable(FILES);
generateQRCodes(FILES);
</script>
</body>
</html>
"""


def main():
    parser = argparse.ArgumentParser(
        description="扫描目录下的 APK 文件，生成带下载链接和二维码的 HTML 页面"
    )
    parser.add_argument("directory", nargs="?", default=".",
                        help="要扫描的目录（默认当前目录）")
    parser.add_argument("--base-url", default="",
                        help="下载链接 URL 前缀，例如 http://192.168.1.18:5005/upload/2026/06/25")
    parser.add_argument("--output", default="index.html",
                        help="输出文件名（默认 index.html）")
    args = parser.parse_args()

    scan_dir = Path(args.directory).resolve()
    if not scan_dir.is_dir():
        print(f"[错误] 目录不存在: {scan_dir}")
        sys.exit(1)

    print(f"扫描目录: {scan_dir}")
    files = scan_apk_files(scan_dir)
    print(f"找到 {len(files)} 个 APK 文件")

    html = build_html(files, args.base_url, scan_dir)

    out_path = scan_dir / args.output
    out_path.write_text(html, encoding="utf-8")
    print(f"✅ 已生成: {out_path}")
    if args.base_url:
        print(f"   访问地址: {args.base_url.rstrip('/')}/index.html")


if __name__ == "__main__":
    main()
