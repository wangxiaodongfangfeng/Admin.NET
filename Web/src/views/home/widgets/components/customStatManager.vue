<template>
	<!-- Trigger button shown inline, dialog shown on click -->
	<el-card shadow="hover">
		<template #header>
			<div class="card-header">
				<span>
					<el-icon style="display: inline; vertical-align: middle"><ele-Setting /></el-icon>
					自定义统计管理
				</span>
				<el-button size="small" type="primary" :icon="Plus" @click="openAdd">新增统计</el-button>
			</div>
		</template>

		<el-table :data="defList" v-loading="listLoading" stripe border size="small">
			<el-table-column prop="name" label="统计名称" min-width="120" show-overflow-tooltip />
			<el-table-column prop="description" label="描述" min-width="160" show-overflow-tooltip />
			<el-table-column label="筛选器" width="160" show-overflow-tooltip>
				<template #default="{ row }">
					<el-tag v-for="f in parseFilters(row.filterConfig)" :key="f" size="small" style="margin:1px">{{ filterLabel(f) }}</el-tag>
				</template>
			</el-table-column>
			<el-table-column label="状态" width="72" align="center">
				<template #default="{ row }">
					<el-tag :type="row.isEnabled ? 'success' : 'info'" size="small">{{ row.isEnabled ? '启用' : '禁用' }}</el-tag>
				</template>
			</el-table-column>
			<el-table-column label="操作" width="130" align="center">
				<template #default="{ row }">
					<el-button link size="small" type="primary" @click="openEdit(row)">编辑</el-button>
					<el-button link size="small" type="danger" @click="handleDelete(row)">删除</el-button>
				</template>
			</el-table-column>
		</el-table>

		<!-- Add / Edit dialog -->
		<el-dialog
			v-model="dialogVisible"
			:title="isEdit ? '编辑统计定义' : '新增统计定义'"
			width="720px"
			destroy-on-close
		>
			<el-form :model="form" :rules="rules" ref="formRef" label-width="90px" size="default">
				<el-form-item label="统计名称" prop="name">
					<el-input v-model="form.name" maxlength="128" show-word-limit />
				</el-form-item>

				<el-form-item label="描述">
					<el-input v-model="form.description" type="textarea" :rows="2" maxlength="512" show-word-limit />
				</el-form-item>

				<el-form-item label="筛选器">
					<el-checkbox-group v-model="selectedFilters">
						<el-checkbox label="date">统计日期</el-checkbox>
						<el-checkbox label="month">统计月份</el-checkbox>
						<el-checkbox label="product_status">产品状态</el-checkbox>
						<el-checkbox label="user">用户</el-checkbox>
					</el-checkbox-group>
				</el-form-item>

				<el-form-item label="SQL 模板" prop="sqlTemplate">
					<div style="width:100%">
						<el-input
							v-model="form.sqlTemplate"
							type="textarea"
							:rows="10"
							placeholder="SELECT ... FROM ... WHERE ..."
							style="font-family: monospace; font-size: 13px"
						/>
						<div class="sql-hint">
							<strong>可用占位符：</strong>
							<code>&#123;&#123;date_start&#125;&#125;</code> 统计天起始日期 &nbsp;
							<code>&#123;&#123;date_end&#125;&#125;</code> 统计天结束日期 &nbsp;
							<code>&#123;&#123;month_prefix&#125;&#125;</code> 年月（yyyy-MM）&nbsp;
							<code>&#123;&#123;shift_cutoff&#125;&#125;</code> 班次分界（08:30:00）&nbsp;
							<code>&#123;&#123;product_status_id&#125;&#125;</code> 产品状态ID &nbsp;
							<code>&#123;&#123;user_id&#125;&#125;</code> 用户ID
						</div>
						<div class="sql-example">
							示例：<br>
							<code>SELECT ProductModel AS 产品型号, COUNT(*) AS 检测次数 FROM Lk_InspectionRecord WHERE Date LIKE '&#123;&#123;month_prefix&#125;&#125;%' GROUP BY ProductModel ORDER BY COUNT(*) DESC</code>
						</div>
					</div>
				</el-form-item>

				<el-form-item label="是否启用">
					<el-switch v-model="form.isEnabled" />
				</el-form-item>
			</el-form>

			<template #footer>
				<el-button @click="dialogVisible = false">取消</el-button>
				<el-button type="primary" :loading="saving" @click="handleSave">保存</el-button>
			</template>
		</el-dialog>
	</el-card>
</template>

<script lang="ts">
export default {
	title: '自定义统计管理',
	icon: 'ele-Setting',
	description: '管理自定义统计定义（SQL模板），生成可复用的统计 Widget',
};
</script>

<script setup lang="ts" name="customStatManager">
import { ref, onMounted } from 'vue';
import { ElMessage, ElMessageBox, FormInstance } from 'element-plus';
import { Plus } from '@element-plus/icons-vue';
import {
	listAllStatDefs, addStatDef, updateStatDef, deleteStatDef,
	type LkCustomStatDef,
} from '/@/api/system/lkCustomStat';

const defList = ref<LkCustomStatDef[]>([]);
const listLoading = ref(false);
const dialogVisible = ref(false);
const isEdit = ref(false);
const saving = ref(false);
const formRef = ref<FormInstance>();
const selectedFilters = ref<string[]>([]);

const emptyForm = (): Omit<LkCustomStatDef, 'createTime' | 'updateTime'> => ({
	id: 0,
	name: '',
	sqlTemplate: '',
	filterConfig: '',
	description: '',
	isEnabled: true,
});
const form = ref(emptyForm());

const rules = {
	name: [{ required: true, message: '请输入统计名称', trigger: 'blur' }],
	sqlTemplate: [{ required: true, message: '请输入SQL模板', trigger: 'blur' }],
};

const parseFilters = (config?: string): string[] => {
	if (!config) return [];
	try { return JSON.parse(config); } catch { return []; }
};

const filterLabel = (key: string) => {
	const map: Record<string, string> = {
		date: '统计日期', month: '统计月份',
		product_status: '产品状态', user: '用户',
	};
	return map[key] ?? key;
};

const loadList = async () => {
	listLoading.value = true;
	try {
		const res = await listAllStatDefs();
		defList.value = res.data?.result ?? [];
	} finally {
		listLoading.value = false;
	}
};

const openAdd = () => {
	isEdit.value = false;
	form.value = emptyForm();
	selectedFilters.value = [];
	dialogVisible.value = true;
};

const openEdit = (row: LkCustomStatDef) => {
	isEdit.value = true;
	form.value = { ...row };
	selectedFilters.value = parseFilters(row.filterConfig);
	dialogVisible.value = true;
};

const handleSave = async () => {
	await formRef.value?.validate();
	saving.value = true;
	try {
		form.value.filterConfig = JSON.stringify(selectedFilters.value);
		if (isEdit.value) {
			await updateStatDef(form.value as LkCustomStatDef);
		} else {
			await addStatDef(form.value);
		}
		ElMessage.success('保存成功');
		dialogVisible.value = false;
		await loadList();
	} catch (e: any) {
		ElMessage.error(e?.response?.data?.message ?? '保存失败');
	} finally {
		saving.value = false;
	}
};

const handleDelete = async (row: LkCustomStatDef) => {
	await ElMessageBox.confirm(`确定删除「${row.name}」？`, '提示', { type: 'warning' });
	await deleteStatDef(row.id);
	ElMessage.success('已删除');
	await loadList();
};

onMounted(loadList);
</script>

<style scoped>
.card-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
}
.sql-hint {
	margin-top: 6px;
	font-size: 12px;
	color: #909399;
	line-height: 1.8;
}
.sql-hint code {
	background: #f5f7fa;
	padding: 1px 4px;
	border-radius: 3px;
	color: #e6a23c;
}
.sql-example {
	margin-top: 6px;
	font-size: 11px;
	color: #aaa;
	line-height: 1.6;
	word-break: break-all;
}
.sql-example code {
	color: #67c23a;
}
</style>
