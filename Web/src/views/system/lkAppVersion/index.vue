<template>
  <div class="lk-app-version" v-loading="state.loading">

    <!-- 标题栏 + 上传按钮 -->
    <el-card shadow="hover" :body-style="{ padding: '16px 20px' }">
      <div style="display:flex; align-items:center; justify-content:space-between; flex-wrap:wrap; gap:10px">
        <div>
          <span style="font-size:16px; font-weight:600">📦 Android APK 版本管理</span>
          <el-tag type="info" size="small" style="margin-left:8px">{{ state.list.length }} 个版本</el-tag>
        </div>
        <div style="display:flex; gap:8px; align-items:center">
          <!-- 隐藏的 file input，用 axios 手动上传，完全掌控响应解析 -->
          <input
            ref="fileInputRef"
            type="file"
            accept=".apk"
            style="display:none"
            @change="onFileSelected"
          />
          <el-button type="primary" :loading="state.uploading" icon="ele-Upload"
                     @click="fileInputRef?.click()">
            上传新版本 (.apk)
          </el-button>
        </div>
      </div>

      <!-- 上传进度条 -->
      <template v-if="state.uploading">
        <el-progress
          :percentage="state.uploadPercent"
          :stroke-width="6"
          style="margin-top:12px"
        />
        <div style="margin-top:4px; font-size:12px; color:#909399">
          {{ state.uploadFileName }} — {{ state.uploadPercent }}%
        </div>
      </template>
    </el-card>

    <!-- 文件列表 -->
    <el-card shadow="hover" style="margin-top:10px">
      <el-table :data="state.list" border stripe size="default" style="width:100%">
        <el-table-column type="index" label="#" width="50" align="center" />

        <el-table-column label="文件名" min-width="220" show-overflow-tooltip>
          <template #default="{ row }">
            <el-icon style="vertical-align:middle; margin-right:4px; color:#0ea5e9"><ele-Document /></el-icon>
            <span style="font-weight:500">{{ row.fileName }}</span>
          </template>
        </el-table-column>

        <el-table-column label="大小" width="100" align="center">
          <template #default="{ row }">{{ row.sizeInfo || formatSize(row.sizeKb) }}</template>
        </el-table-column>

        <el-table-column label="上传时间" width="160" align="center">
          <template #default="{ row }">{{ formatDate(row.createTime) }}</template>
        </el-table-column>

        <el-table-column label="下载链接" min-width="180" show-overflow-tooltip>
          <template #default="{ row }">
            <el-link :href="row.downloadUrl" target="_blank" type="primary" style="font-size:12px">
              {{ row.downloadUrl }}
            </el-link>
          </template>
        </el-table-column>

        <el-table-column label="操作" width="220" align="center" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" text icon="ele-View"
                       @click="showQrCode(row)">二维码</el-button>
            <el-button size="small" type="success" text icon="ele-Download"
                       @click="downloadApk(row)">下载</el-button>
            <el-button size="small" type="danger"  text icon="ele-Delete"
                       @click="deleteApk(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div v-if="!state.loading && state.list.length === 0" style="padding:60px">
        <el-empty description="暂无 APK 文件，请上传" />
      </div>
    </el-card>

    <!-- 二维码弹框 -->
    <el-dialog
      v-model="qrState.visible"
      :title="`二维码 — ${qrState.fileName}`"
      width="360px"
      align-center
      destroy-on-close
      @opened="renderQrCode"
    >
      <div style="text-align:center; padding:8px 0">
        <div id="qrcode-container"
             style="display:inline-block; padding:12px; background:#fff; border-radius:8px; border:3px solid #e4e7ed">
        </div>
        <p style="margin-top:14px; font-size:12px; color:#909399; word-break:break-all; padding:0 8px">
          {{ qrState.url }}
        </p>
      </div>
      <template #footer>
        <el-button @click="qrState.visible = false">关闭</el-button>
        <el-button type="primary" @click="downloadQr">下载二维码</el-button>
      </template>
    </el-dialog>

  </div>
</template>

<script lang="ts" setup name="lkAppVersion">
import { reactive, ref, onMounted } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { getToken } from '/@/utils/axios-utils';
import axios from 'axios';

// ── 类型 ──────────────────────────────────────────────────────────────────────
interface ApkVersion {
  id: number;
  fileName: string;
  sizeKb: number;
  sizeInfo?: string;
  createTime: string;
  url: string;
  downloadUrl: string;
}

// ── 状态 ──────────────────────────────────────────────────────────────────────
const apiBase = (window as any).__env__?.VITE_API_URL ?? '';

const state = reactive({
  loading:        false,
  list:           [] as ApkVersion[],
  uploading:      false,
  uploadPercent:  0,
  uploadFileName: '',
});

const qrState = reactive({
  visible:  false,
  fileName: '',
  url:      '',
});

const fileInputRef = ref<HTMLInputElement>();

const authHeaders = () => ({ Authorization: `Bearer ${getToken()}` });

// ── API ────────────────────────────────────────────────────────────────────────
const loadList = async () => {
  state.loading = true;
  try {
    const res = await axios.get(`${apiBase}/api/lkAppVersion/list`,
      { headers: authHeaders() });
    state.list = res.data?.result ?? [];
  } catch {
    ElMessage.error('加载列表失败');
  } finally {
    state.loading = false;
  }
};

// ── 上传（用 axios 手动发，避免 el-upload 解析 NonUnify 响应出错）────────────
const onFileSelected = async (e: Event) => {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;
  // 重置 input，同一文件可重复选
  (e.target as HTMLInputElement).value = '';

  if (!file.name.endsWith('.apk')) {
    ElMessage.error('只允许上传 .apk 文件');
    return;
  }

  state.uploading      = true;
  state.uploadPercent  = 0;
  state.uploadFileName = file.name;

  const form = new FormData();
  form.append('file', file);

  try {
    const res = await axios.post(`${apiBase}/api/lkAppVersion/upload`, form, {
      headers: { ...authHeaders(), 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (e) => {
        state.uploadPercent = e.total
          ? Math.round((e.loaded / e.total) * 100)
          : 0;
      },
    });

    // NonUnify 接口直接返回数据对象（不是 { code, result } 包装）
    const result: ApkVersion = res.data;
    if (result?.id) {
      ElMessage.success(`上传成功：${result.fileName}`);
      loadList();
    } else {
      ElMessage.error('上传失败，服务器返回异常');
    }
  } catch (err: any) {
    const msg = err?.response?.data?.message ?? err?.message ?? '上传失败';
    ElMessage.error(msg);
  } finally {
    state.uploading     = false;
    state.uploadPercent = 0;
  }
};

// ── 删除 ──────────────────────────────────────────────────────────────────────
const deleteApk = async (row: ApkVersion) => {
  await ElMessageBox.confirm(`确定删除「${row.fileName}」？此操作不可撤销。`, '提示',
    { type: 'warning' });
  try {
    await axios.post(`${apiBase}/api/lkAppVersion/delete`,
      { id: row.id }, { headers: authHeaders() });
    ElMessage.success('已删除');
    loadList();
  } catch {
    ElMessage.error('删除失败');
  }
};

// ── 下载 ──────────────────────────────────────────────────────────────────────
const downloadApk = (row: ApkVersion) => {
  // download URL is [AllowAnonymous], no token needed
  const a = document.createElement('a');
  a.href     = row.downloadUrl;
  a.download = row.fileName;
  a.click();
};

// ── 二维码（使用 davidshimjs/qrcodejs，cdnjs 上的 1.5.0 版本就是这个库）────
let qrcodeInstance: any = null;

const showQrCode = (row: ApkVersion) => {
  qrState.fileName = row.fileName;
  qrState.url      = row.downloadUrl;
  qrState.visible  = true;
  // 渲染在 @opened 回调里，确保 DOM 已挂载
};

const renderQrCode = async () => {
  // 懒加载 qrcodejs
  if (!(window as any).QRCode) {
    await loadScript('https://cdnjs.cloudflare.com/ajax/libs/qrcode/1.5.0/qrcode.min.js');
  }

  const container = document.getElementById('qrcode-container');
  if (!container) return;
  container.innerHTML = ''; // 清空上次

  // davidshimjs/qrcodejs API：new QRCode(element, options)
  qrcodeInstance = new (window as any).QRCode(container, {
    text:           qrState.url,
    width:          240,
    height:         240,
    colorDark:      '#000000',
    colorLight:     '#ffffff',
    correctLevel:   (window as any).QRCode.CorrectLevel.H,
  });
};

const downloadQr = () => {
  const img = document.querySelector('#qrcode-container img') as HTMLImageElement;
  if (!img) return;
  const a = document.createElement('a');
  a.href     = img.src;
  a.download = `${qrState.fileName}_qrcode.png`;
  a.click();
};

// ── 工具 ──────────────────────────────────────────────────────────────────────
const formatSize = (kb: number) => {
  if (kb < 1024) return `${kb} KB`;
  return `${(kb / 1024).toFixed(1)} MB`;
};

const formatDate = (dt?: string) => {
  if (!dt) return '-';
  return dt.replace('T', ' ').slice(0, 16);
};

const loadScript = (src: string): Promise<void> =>
  new Promise((resolve, reject) => {
    const existing = document.querySelector(`script[src="${src}"]`);
    if (existing) { resolve(); return; }
    const s = document.createElement('script');
    s.src = src;
    s.onload  = () => resolve();
    s.onerror = reject;
    document.head.appendChild(s);
  });

onMounted(loadList);
</script>

<style scoped>
.lk-app-version { padding: 12px; }
</style>
