<template>
	<el-card shadow="hover">
		<template #header>
			<div class="card-header">
				<span>
					<el-icon style="display: inline; vertical-align: middle"><ele-WarningFilled /></el-icon>
					二检异常
				</span>
				<div class="header-actions">
					<el-tag type="danger" v-if="anomalyList.length > 0" round style="margin-right: 8px">{{ anomalyList.length }} 条</el-tag>
					<el-tag type="success" v-else round style="margin-right: 8px">无异常</el-tag>
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
					<el-button
						:loading="loading"
						size="small"
						icon="ele-Refresh"
						circle
						style="margin-left: 6px"
						@click="loadData"
					/>
				</div>
			</div>
		</template>

		<el-table
			:data="anomalyList"
			v-loading="loading"
			stripe
			border
			size="small"
			style="width: 100%"
			:max-height="320"
		>
			<el-table-column type="index" label="#" width="42" align="center" />
			<el-table-column prop="productModel" label="二维码" min-width="120" show-overflow-tooltip />
			<el-table-column prop="steelStamp" label="钢印号" min-width="90" show-overflow-tooltip />
			<el-table-column label="首检时间" align="center" width="80">
				<template #default="{ row }">
					<span class="result-ok">{{ row.firstRecord.time }}</span>
				</template>
			</el-table-column>
			<el-table-column label="二检时间" align="center" width="80">
				<template #default="{ row }">
					<span class="result-ng">{{ row.secondRecord.time }}</span>
				</template>
			</el-table-column>
			<el-table-column label="操作员" min-width="80" show-overflow-tooltip>
				<template #default="{ row }">
					{{ row.secondRecord.operator || row.firstRecord.operator }}
				</template>
			</el-table-column>
			<el-table-column label="NG位置" min-width="90" show-overflow-tooltip>
				<template #default="{ row }">
					{{ row.secondRecord.ngPositionFkDisplayName || '-' }}
				</template>
			</el-table-column>
			<el-table-column label="泄露值" align="center" width="72">
				<template #default="{ row }">
					{{ row.secondRecord.leakage ?? '-' }}
				</template>
			</el-table-column>
		</el-table>

		<div v-if="!loading && anomalyList.length === 0" class="empty-tip">
			<el-empty description="该统计天无二检异常数据" :image-size="60" />
		</div>
	</el-card>
</template>

<script lang="ts">
export default {
	title: '二检异常',
	icon: 'ele-WarningFilled',
	description: '展示指定统计天（08:30~次日08:30）首检OK、二检NG的异常记录',
};
</script>

<script setup lang="ts" name="todayRecheckAnomaly">
import { ref, onMounted } from 'vue';
import { useLkInspectionRecordApi } from '/@/api/system/lkInspectionRecord';

const lkInspectionRecordApi = useLkInspectionRecordApi();

interface AnomalyRow {
	productModel: string;
	steelStamp: string;
	firstRecord: any;
	secondRecord: any;
}

const loading = ref(false);
const anomalyList = ref<AnomalyRow[]>([]);

/**
 * 与 dailyUserStat / dailyProductTypeStat 保持一致：
 * 当前时间 >= 08:30 → 统计天为今天，否则为昨天。
 */
const computeDefaultDate = (): string => {
	const now = new Date();
	const cutoff = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 8, 30, 0);
	const target = now >= cutoff ? now : new Date(now.getTime() - 86400000);
	return `${target.getFullYear()}-${String(target.getMonth() + 1).padStart(2, '0')}-${String(target.getDate()).padStart(2, '0')}`;
};

const selectedDate = ref<string>(computeDefaultDate());

/**
 * 根据所选统计天计算查询区间：
 *   开始：selectedDate 08:30:00
 *   结束：selectedDate + 1天 08:29:59
 * page 接口按 date + time 字段过滤，因此需要同时覆盖两个自然日。
 * 做法：拉取 selectedDate 当天 08:30 之后 及 次日 08:30 之前的记录，
 * 通过两次请求或在前端合并——这里用一次请求传足够大的 pageSize，
 * 后端按 date 过滤只支持单日，所以分两次请求再合并。
 */
const fetchDayRecords = async (date: string): Promise<any[]> => {
	const res = await lkInspectionRecordApi.page({
		date,
		page: 1,
		pageSize: 99999,
		field: 'time',
		order: 'ascending',
		descStr: 'descending',
	});
	return res.data?.result?.items ?? [];
};

const nextDateStr = (dateStr: string): string => {
	const d = new Date(dateStr + 'T00:00:00');
	d.setDate(d.getDate() + 1);
	return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
};

/** 把 "HH:mm:ss" 或 "HH:mm" 转为分钟数，方便比较 */
const timeToMinutes = (t?: string): number => {
	if (!t) return 0;
	const parts = t.split(':');
	return parseInt(parts[0] ?? '0') * 60 + parseInt(parts[1] ?? '0');
};

const CUTOFF_MINUTES = 8 * 60 + 30; // 08:30

const loadData = async () => {
	loading.value = true;
	try {
		const date = selectedDate.value;
		const nextDate = nextDateStr(date);

		// 并行拉取统计天当日 + 次日的记录
		const [todayItems, nextItems] = await Promise.all([
			fetchDayRecords(date),
			fetchDayRecords(nextDate),
		]);

		// 统计天：保留 time >= 08:30 的记录
		const filteredToday = todayItems.filter(
			(r) => timeToMinutes(r.time) >= CUTOFF_MINUTES,
		);
		// 次日：保留 time < 08:30 的记录
		const filteredNext = nextItems.filter(
			(r) => timeToMinutes(r.time) < CUTOFF_MINUTES,
		);

		// 合并，构造排序 key = date + time，整体升序
		const allItems = [
			...filteredToday.map((r) => ({ ...r, _sortKey: r.date + ' ' + (r.time ?? '') })),
			...filteredNext.map((r) => ({ ...r, _sortKey: r.date + ' ' + (r.time ?? '') })),
		].sort((a, b) => a._sortKey.localeCompare(b._sortKey));

		// 按 productModel 分组，保留分组内的时序
		const groupMap = new Map<string, any[]>();
		for (const item of allItems) {
			const key = item.productModel;
			if (!key) continue;
			if (!groupMap.has(key)) groupMap.set(key, []);
			groupMap.get(key)!.push(item);
		}

		// 找"相邻 OK → NG"配对
		const result: AnomalyRow[] = [];
		groupMap.forEach((records) => {
			for (let i = 0; i < records.length - 1; i++) {
				const first = records[i];
				const second = records[i + 1];
				const firstResult = (first.testResult ?? '').toUpperCase().trim();
				const secondResult = (second.testResult ?? '').toUpperCase().trim();
				if (firstResult === 'OK' && secondResult === 'NG') {
					result.push({
						productModel: first.productModel,
						steelStamp: first.steelStamp,
						firstRecord: first,
						secondRecord: second,
					});
					break; // 每个二维码只取第一对
				}
			}
		});

		anomalyList.value = result;
	} catch {
		anomalyList.value = [];
	} finally {
		loading.value = false;
	}
};

onMounted(() => loadData());
</script>

<style scoped>
.card-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	flex-wrap: wrap;
	gap: 6px;
}
.header-actions {
	display: flex;
	align-items: center;
	flex-wrap: wrap;
	gap: 4px;
}
.result-ok {
	color: var(--el-color-success);
	font-weight: 500;
}
.result-ng {
	color: var(--el-color-danger);
	font-weight: 500;
}
.empty-tip {
	padding: 8px 0;
}
</style>
