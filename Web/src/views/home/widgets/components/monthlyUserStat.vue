<template>
	<el-card shadow="hover">
		<template #header>
			<div class="card-header">
				<span>
					<el-icon style="display: inline; vertical-align: middle"><ele-User /></el-icon>
					月度人员检测统计
				</span>
				<div class="header-filter">
					<el-date-picker
						v-model="selectedMonth"
						type="month"
						placeholder="选择年月"
						format="YYYY年MM月"
						value-format="YYYY-MM"
						:clearable="false"
						size="small"
						style="width: 140px"
						@change="loadData"
					/>
					<el-button size="small" :icon="Download" :loading="exporting" @click="handleExport" style="margin-left:6px">导出</el-button>
				</div>
			</div>
		</template>

		<el-table :data="tableData" v-loading="loading" stripe border size="small" style="width: 100%"
			show-summary :summary-method="getSummary">
			<el-table-column prop="userName" label="人员" min-width="100" show-overflow-tooltip />
			<el-table-column prop="total" label="检测总数" align="center" width="80" />
			<el-table-column prop="okCount" label="OK 数" align="center" width="72">
				<template #default="{ row }">
					<span style="color: var(--el-color-success)">{{ row.okCount }}</span>
				</template>
			</el-table-column>
			<el-table-column prop="ngCount" label="NG 数" align="center" width="72">
				<template #default="{ row }">
					<span style="color: var(--el-color-danger)">{{ row.ngCount }}</span>
				</template>
			</el-table-column>
			<el-table-column prop="passRate" label="合格率" align="center" width="80">
				<template #default="{ row }">
					<el-progress
						:percentage="toPercent(row.passRate)"
						:color="rateColor(row.passRate)"
						:stroke-width="6"
						:show-text="false"
						style="margin-bottom: 2px"
					/>
					<span :style="{ color: rateColor(row.passRate), fontSize: '12px' }">
						{{ formatRate(row.passRate) }}
					</span>
				</template>
			</el-table-column>
		</el-table>

		<div v-if="!loading && tableData.length === 0" class="empty-tip">
			<el-empty description="暂无数据" :image-size="60" />
		</div>
	</el-card>
</template>

<script lang="ts">
export default {
	title: '月度人员统计',
	icon: 'ele-User',
	description: '按人员统计指定年月的检测总数、OK数、NG数及合格率',
};
</script>

<script setup lang="ts" name="monthlyUserStat">
import { ref, onMounted } from 'vue';
import { Download } from '@element-plus/icons-vue';
import { getAPI } from '/@/utils/axios-utils';
import { LkStatisticsApi } from '/@/api-services/api';
import { LkUserMonthlyStatOutput } from '/@/api-services/models';

// 默认选择当前年月
const now = new Date();
const defaultMonth = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;

const selectedMonth = ref<string>(defaultMonth);
const tableData = ref<LkUserMonthlyStatOutput[]>([]);
const loading = ref(false);
const exporting = ref(false);

const parseYearMonth = (val: string): { year: number; month: number } => {
	const [y, m] = val.split('-');
	return { year: parseInt(y), month: parseInt(m) };
};

const loadData = async () => {
	loading.value = true;
	try {
		const { year, month } = parseYearMonth(selectedMonth.value);
		const res = await getAPI(LkStatisticsApi).apiLkStatisticsMonthlyUserStatGet(year, month);
		tableData.value = res.data?.result ?? [];
	} catch (e) {
		tableData.value = [];
	} finally {
		loading.value = false;
	}
};

const toPercent = (rate?: number) => {
	if (rate == null) return 0;
	return Math.min(100, Math.round(rate * 100));
};

const formatRate = (rate?: number) => {
	if (rate == null) return '-';
	return (rate * 100).toFixed(1) + '%';
};

const rateColor = (rate?: number) => {
	if (rate == null) return '#909399';
	if (rate >= 0.95) return '#67c23a';
	if (rate >= 0.8) return '#e6a23c';
	return '#f56c6c';
};

onMounted(() => {
	loadData();
});

const getSummary = () => {
	const total = tableData.value.reduce((s, r) => s + (r.total  ?? 0), 0);
	const ok    = tableData.value.reduce((s, r) => s + (r.okCount ?? 0), 0);
	const ng    = tableData.value.reduce((s, r) => s + (r.ngCount ?? 0), 0);
	const rate  = total === 0 ? '-' : ((ok / total) * 100).toFixed(1) + '%';
	return ['合计', String(total), String(ok), String(ng), rate];
};

const handleExport = async () => {
	exporting.value = true;
	try {
		const { year, month } = parseYearMonth(selectedMonth.value);
		const res = await getAPI(LkStatisticsApi).apiLkStatisticsExportMonthlyUserStatPost(year, month);
		const url = URL.createObjectURL(new Blob([res.data as any]));
		const a = document.createElement('a');
		a.href = url;
		a.download = `人员月度统计_${selectedMonth.value}.xlsx`;
		a.click();
		URL.revokeObjectURL(url);
	} finally {
		exporting.value = false;
	}
};
</script>

<style scoped>
.card-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
}
.header-filter {
	display: flex;
	align-items: center;
}
.empty-tip {
	padding: 8px 0;
}
</style>
