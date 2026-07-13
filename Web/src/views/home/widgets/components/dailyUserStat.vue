<template>
	<el-card shadow="hover">
		<template #header>
			<div class="card-header">
				<span>
					<el-icon style="display: inline; vertical-align: middle"><ele-User /></el-icon>
					每日人员检测汇总
				</span>
				<div class="header-filter">
					<el-date-picker
						v-model="selectedDate"
						type="date"
						placeholder="选择日期"
						format="YYYY年MM月DD日"
						value-format="YYYY-MM-DD"
						:clearable="false"
						size="small"
						style="width: 150px"
						@change="loadData"
					/>
					<el-button size="small" :icon="Download" :loading="exporting" @click="handleExport" style="margin-left:6px">导出</el-button>
				</div>
			</div>
		</template>

		<el-table :data="tableData" v-loading="loading" stripe border size="small" style="width: 100%"
			show-summary :summary-method="getSummary">
			<el-table-column prop="userName" label="人员" min-width="90" show-overflow-tooltip />
			<el-table-column prop="total" label="总数" align="center" width="64" />
			<el-table-column prop="okCount" label="OK" align="center" width="60">
				<template #default="{ row }">
					<span style="color: var(--el-color-success)">{{ row.okCount }}</span>
				</template>
			</el-table-column>
			<el-table-column prop="ngCount" label="NG" align="center" width="60">
				<template #default="{ row }">
					<span style="color: var(--el-color-danger)">{{ row.ngCount }}</span>
				</template>
			</el-table-column>
			<el-table-column prop="passRate" label="合格率" align="center" width="90">
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
	title: '每日人员汇总',
	icon: 'ele-User',
	description: '按人员统计指定统计天（08:30~次日08:30）的检测总数、OK数、NG数及合格率',
};
</script>

<script setup lang="ts" name="dailyUserStat">
import { ref, onMounted } from 'vue';
import { Download } from '@element-plus/icons-vue';
import { getAPI } from '/@/utils/axios-utils';
import { LkStatisticsApi } from '/@/api-services/api';
import { LkUserDailyStatOutput } from '/@/api-services/models';

// 默认日期：按 08:30 规则计算当前统计天起始日期
const computeDefaultDate = (): string => {
	const now = new Date();
	const cutoff = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 8, 30, 0);
	const target = now >= cutoff ? now : new Date(now.getTime() - 86400000);
	return `${target.getFullYear()}-${String(target.getMonth() + 1).padStart(2, '0')}-${String(target.getDate()).padStart(2, '0')}`;
};

const selectedDate = ref<string>(computeDefaultDate());
const tableData = ref<LkUserDailyStatOutput[]>([]);
const loading = ref(false);
const exporting = ref(false);

const loadData = async () => {
	loading.value = true;
	try {
		const res = await getAPI(LkStatisticsApi).apiLkStatisticsDailyUserStatGet(selectedDate.value);
		tableData.value = res.data?.result ?? [];
	} catch {
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

onMounted(() => loadData());

const getSummary = () => {
	const total  = tableData.value.reduce((s, r) => s + (r.total  ?? 0), 0);
	const ok     = tableData.value.reduce((s, r) => s + (r.okCount ?? 0), 0);
	const ng     = tableData.value.reduce((s, r) => s + (r.ngCount ?? 0), 0);
	const rate   = total === 0 ? '-' : ((ok / total) * 100).toFixed(1) + '%';
	return ['合计', String(total), String(ok), String(ng), rate];
};

const handleExport = async () => {
	exporting.value = true;
	try {
		const res = await getAPI(LkStatisticsApi).apiLkStatisticsExportDailyUserStatPost(selectedDate.value);
		const url = URL.createObjectURL(new Blob([res.data as any]));
		const a = document.createElement('a');
		a.href = url;
		a.download = `人员每日统计_${selectedDate.value}.xlsx`;
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
