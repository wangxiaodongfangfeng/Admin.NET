<template>
	<el-card shadow="hover" class="custom-stat-card">
		<!-- Header: title + filters + actions -->
		<template #header>
			<div class="card-header">
				<span class="title">
					<el-icon style="display:inline;vertical-align:middle"><ele-DataLine /></el-icon>
					{{ currentDef?.name ?? '自定义统计' }}
				</span>
				<div class="header-controls">
					<!-- Definition selector -->
					<el-select
						v-model="selectedDefId"
						placeholder="选择统计"
						size="small"
						style="width:150px"
						:loading="defLoading"
						@change="onDefChange"
					>
						<el-option v-for="d in defList" :key="d.id" :label="d.name" :value="d.id" />
					</el-select>

					<!-- Dynamic filters based on filterConfig -->
					<template v-if="currentDef">
						<el-date-picker
							v-if="hasFilter('date')"
							v-model="filterDate"
							type="date"
							format="YYYY年MM月DD日"
							value-format="YYYY-MM-DD"
							placeholder="统计日期"
							:clearable="false"
							size="small"
							style="width:150px"
							@change="loadData"
						/>
						<el-date-picker
							v-if="hasFilter('month')"
							v-model="filterMonth"
							type="month"
							format="YYYY年MM月"
							value-format="YYYY-MM"
							placeholder="统计月份"
							:clearable="false"
							size="small"
							style="width:130px"
							@change="loadData"
						/>
						<el-select
							v-if="hasFilter('product_status')"
							v-model="filterProductStatusId"
							placeholder="产品状态"
							size="small"
							style="width:110px"
							:loading="statusLoading"
							@change="loadData"
						>
							<el-option v-for="s in statusList" :key="s.id" :label="s.name" :value="s.id" />
						</el-select>
						<el-select
							v-if="hasFilter('user')"
							v-model="filterUserId"
							placeholder="用户"
							size="small"
							style="width:110px"
							clearable
							@change="loadData"
						>
							<el-option v-for="u in userList" :key="u.id" :label="u.realName" :value="u.id" />
						</el-select>
					</template>

					<el-button size="small" :icon="Refresh" circle @click="loadData" :loading="loading" />
					<el-button size="small" :icon="Download" :loading="exporting" @click="handleExport" :disabled="!result" />
				</div>
			</div>
		</template>

		<!-- Empty / no selection state -->
		<el-empty v-if="!selectedDefId" description="请选择一个统计" :image-size="60" />

		<!-- Table with dynamic columns -->
		<template v-else>
			<el-table
				v-loading="loading"
				:data="tableRows"
				stripe
				border
				size="small"
				style="width:100%"
				:max-height="360"
			>
				<el-table-column
					v-for="col in result?.columns ?? []"
					:key="col"
					:prop="col"
					:label="col"
					min-width="90"
					show-overflow-tooltip
					align="center"
				/>
			</el-table>
			<div v-if="!loading && tableRows.length === 0" class="empty-tip">
				<el-empty description="暂无数据" :image-size="60" />
			</div>
		</template>
	</el-card>
</template>

<script lang="ts">
export default {
	title: '自定义统计',
	icon: 'ele-DataLine',
	description: '可自由配置的统计 Widget，通过 SQL 模板动态生成统计视图',
};
</script>

<script setup lang="ts" name="customStat">
import { ref, computed, onMounted } from 'vue';
import { Refresh, Download } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import {
	listStatDefs, runStat, exportStat,
	type LkCustomStatDef, type LkCustomStatResult,
} from '/@/api/system/lkCustomStat';
import { useLkProductStatusApi, type LkProductStatus } from '/@/api/system/lkProductStatus';
import request from '/@/utils/request';

// ── Def list ──────────────────────────────────────────────────────────────────
const defList = ref<LkCustomStatDef[]>([]);
const defLoading = ref(false);
const selectedDefId = ref<number | undefined>(undefined);

const currentDef = computed(() =>
	defList.value.find(d => d.id === selectedDefId.value));

const parseFilters = (cfg?: string): string[] => {
	if (!cfg) return [];
	try { return JSON.parse(cfg); } catch { return []; }
};
const hasFilter = (key: string) =>
	parseFilters(currentDef.value?.filterConfig).includes(key);

// ── Filters ───────────────────────────────────────────────────────────────────
const computeDefaultDate = () => {
	const now = new Date();
	const cutoff = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 8, 30, 0);
	const t = now >= cutoff ? now : new Date(now.getTime() - 86400000);
	return `${t.getFullYear()}-${String(t.getMonth()+1).padStart(2,'0')}-${String(t.getDate()).padStart(2,'0')}`;
};
const computeDefaultMonth = () => {
	const now = new Date();
	return `${now.getFullYear()}-${String(now.getMonth()+1).padStart(2,'0')}`;
};

const filterDate          = ref<string>(computeDefaultDate());
const filterMonth         = ref<string>(computeDefaultMonth());
const filterProductStatusId = ref<number | undefined>(undefined);
const filterUserId        = ref<number | undefined>(undefined);

// product status list
const statusList    = ref<LkProductStatus[]>([]);
const statusLoading = ref(false);

// user list (simple fetch from sys user)
interface SimpleUser { id: number; realName: string }
const userList = ref<SimpleUser[]>([]);

const loadProductStatuses = async () => {
	statusLoading.value = true;
	try {
		const res = await useLkProductStatusApi().page({ page: 1, pageSize: 100 });
		statusList.value = res.data?.result?.items ?? [];
		const def = statusList.value.find(s => s.isDefault) ?? statusList.value[0];
		if (def && !filterProductStatusId.value) filterProductStatusId.value = def.id;
	} catch {
		statusList.value = [];
	} finally {
		statusLoading.value = false;
	}
};

const loadUsers = async () => {
	try {
		const res = await request.post('/api/sysUser/page', { page: 1, pageSize: 200 });
		userList.value = (res.data?.result?.items ?? []).map((u: any) => ({
			id: u.id,
			realName: u.realName || u.account,
		}));
	} catch {
		userList.value = [];
	}
};

// ── Result ────────────────────────────────────────────────────────────────────
const result   = ref<LkCustomStatResult | null>(null);
const loading  = ref(false);
const exporting = ref(false);

/** Convert rows (arrays) to objects keyed by column name for el-table */
const tableRows = computed(() => {
	if (!result.value) return [];
	const cols = result.value.columns;
	return result.value.rows.map(row => {
		const obj: Record<string, any> = {};
		cols.forEach((col, i) => { obj[col] = row[i] ?? ''; });
		return obj;
	});
});

// ── Actions ───────────────────────────────────────────────────────────────────
const onDefChange = () => {
	result.value = null;
	loadData();
};

const loadData = async () => {
	if (!selectedDefId.value) return;
	loading.value = true;
	result.value = null;
	try {
		const res = await runStat({
			id:              selectedDefId.value,
			date:            filterDate.value,
			month:           filterMonth.value,
			productStatusId: filterProductStatusId.value,
			userId:          filterUserId.value,
		});
		result.value = res.data?.result ?? null;
	} catch (e: any) {
		ElMessage.error(e?.response?.data?.message ?? '查询失败');
	} finally {
		loading.value = false;
	}
};

const handleExport = async () => {
	if (!selectedDefId.value) return;
	exporting.value = true;
	try {
		const res = await exportStat({
			id:              selectedDefId.value,
			date:            filterDate.value,
			month:           filterMonth.value,
			productStatusId: filterProductStatusId.value,
			userId:          filterUserId.value,
		});
		const blob = new Blob([res.data as any]);
		const url = URL.createObjectURL(blob);
		const a = document.createElement('a');
		a.href = url;
		a.download = `${currentDef.value?.name ?? '统计'}_${filterDate.value || filterMonth.value}.xlsx`;
		a.click();
		URL.revokeObjectURL(url);
	} catch {
		ElMessage.error('导出失败');
	} finally {
		exporting.value = false;
	}
};

onMounted(async () => {
	defLoading.value = true;
	try {
		const res = await listStatDefs();
		defList.value = res.data?.result ?? [];
		if (defList.value.length > 0) {
			selectedDefId.value = defList.value[0].id;
		}
	} finally {
		defLoading.value = false;
	}
	// Pre-load supporting data
	await Promise.all([loadProductStatuses(), loadUsers()]);
	if (selectedDefId.value) loadData();
});
</script>

<style scoped>
.card-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	flex-wrap: wrap;
	gap: 6px;
}
.title {
	font-weight: 500;
	white-space: nowrap;
}
.header-controls {
	display: flex;
	align-items: center;
	flex-wrap: wrap;
	gap: 4px;
}
.empty-tip {
	padding: 8px 0;
}
</style>
