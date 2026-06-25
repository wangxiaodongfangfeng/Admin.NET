<script lang="ts" name="lkInspectionRecord" setup>
import { ref, reactive, onMounted } from "vue";
import { ElMessage } from "element-plus";
import type { FormRules } from "element-plus";
import { formatDate } from '/@/utils/formatTime';
import { useLkInspectionRecordApi } from '/@/api/system/lkInspectionRecord';
//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const lkInspectionRecordApi = useLkInspectionRecordApi();
const ruleFormRef = ref();

const state = reactive({
	title: '',
	loading: false,
	showDialog: false,
	ruleForm: {} as any,
	stores: {},
	dropdownData: {} as any,
});

// 自行添加其他规则
const rules = ref<FormRules>({
  operator: [{required: true, message: '请选择操作员！', trigger: 'blur',},],
  date: [{required: true, message: '请选择检测日期！', trigger: 'blur',},],
  shiftId: [{required: true, message: '请选择班次！', trigger: 'blur',},],
  pressureHoldTime: [{required: true, message: '请选择保压时间！', trigger: 'blur',},],
  pressure: [{required: true, message: '请选择气压值！', trigger: 'blur',},],
  productTypeId: [{required: true, message: '请选择产品类型！', trigger: 'blur',},],
  productStatusId: [{required: true, message: '请选择产品状态！', trigger: 'blur',},],
  partStatusId: [{required: true, message: '请选择零件状态！', trigger: 'blur',},],
  ngPositionId: [{required: true, message: '请选择Ng位置！', trigger: 'blur',},],
  testResult: [{required: true, message: '请选择检测结果！', trigger: 'blur',},],
  userId: [{required: true, message: '请选择用户！', trigger: 'blur',},],
});

// 页面加载时
onMounted(async () => {
  const data = await lkInspectionRecordApi.getDropdownData(false).then(res => res.data.result) ?? {};
  state.dropdownData.shiftId = data.shiftId ?? [];
  state.dropdownData.productTypeId = data.productTypeId ?? [];
  state.dropdownData.productStatusId = data.productStatusId ?? [];
  state.dropdownData.partStatusId = data.partStatusId ?? [];
  state.dropdownData.ngPositionId = data.ngPositionId ?? [];
  state.dropdownData.userId = data.userId ?? [];
});

// 打开弹窗
const openDialog = async (row: any, title: string) => {
	state.title = title;
	row = row ?? {  };
	state.ruleForm = row.id ? await lkInspectionRecordApi.detail(row.id).then(res => res.data.result) : JSON.parse(JSON.stringify(row));
	state.showDialog = true;
};

// 关闭弹窗
const closeDialog = () => {
	emit("reloadTable");
	state.showDialog = false;
};

// 提交
const submit = async () => {
	ruleFormRef.value.validate(async (isValid: boolean, fields?: any) => {
		if (isValid) {
			let values = state.ruleForm;
			await lkInspectionRecordApi[state.ruleForm.id ? 'update' : 'add'](values);
			closeDialog();
		} else {
			ElMessage({
				message: `表单有${Object.keys(fields).length}处验证失败，请修改后再提交`,
				type: "error",
			});
		}
	});
};

//将属性或者函数暴露给父组件
defineExpose({ openDialog });
</script>
<template>
	<div class="lkInspectionRecord-container">
		<el-dialog v-model="state.showDialog" :width="800" draggable :close-on-click-modal="false">
			<template #header>
				<div style="color: #fff">
					<span>{{ state.title }}</span>
				</div>
			</template>
			<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto" :rules="rules">
				<el-row :gutter="35">
					<el-form-item v-show="false">
						<el-input v-model="state.ruleForm.id" />
					</el-form-item>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="操作员" prop="operator">
							<el-input v-model="state.ruleForm.operator" placeholder="请输入操作员" maxlength="32" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="检测日期" prop="date">
							<el-input v-model="state.ruleForm.date" placeholder="请输入检测日期" maxlength="16" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="班次" prop="shiftId">
							<el-select clearable filterable v-model="state.ruleForm.shiftId" placeholder="请选择班次">
								<el-option v-for="(item,index) in state.dropdownData.shiftId" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="保压时间" prop="pressureHoldTime">
							<el-input-number v-model="state.ruleForm.pressureHoldTime" placeholder="请输入保压时间" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="气压值" prop="pressure">
							<el-input-number v-model="state.ruleForm.pressure" placeholder="请输入气压值" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="产品类型" prop="productTypeId">
							<el-select clearable filterable v-model="state.ruleForm.productTypeId" placeholder="请选择产品类型">
								<el-option v-for="(item,index) in state.dropdownData.productTypeId" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="产品状态" prop="productStatusId">
							<el-select clearable filterable v-model="state.ruleForm.productStatusId" placeholder="请选择产品状态">
								<el-option v-for="(item,index) in state.dropdownData.productStatusId" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="零件状态" prop="partStatusId">
							<el-select clearable filterable v-model="state.ruleForm.partStatusId" placeholder="请选择零件状态">
								<el-option v-for="(item,index) in state.dropdownData.partStatusId" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="Ng位置" prop="ngPositionId">
							<el-select clearable filterable v-model="state.ruleForm.ngPositionId" placeholder="请选择Ng位置">
								<el-option v-for="(item,index) in state.dropdownData.ngPositionId" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="二维码" prop="productModel">
							<el-input v-model="state.ruleForm.productModel" placeholder="请输入二维码" maxlength="64" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="钢印号" prop="steelStamp">
							<el-input v-model="state.ruleForm.steelStamp" placeholder="请输入钢印号" maxlength="64" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="检测结果" prop="testResult">
							<el-input v-model="state.ruleForm.testResult" placeholder="请输入检测结果" maxlength="8" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="泄露值" prop="leakage">
							<el-input-number v-model="state.ruleForm.leakage" placeholder="请输入泄露值" clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="图片URL列表" prop="images">
							<el-input v-model="state.ruleForm.images" placeholder="请输入图片URL列表" show-word-limit clearable />
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="用户" prop="userId">
							<el-select clearable filterable v-model="state.ruleForm.userId" placeholder="请选择用户">
								<el-option v-for="(item,index) in state.dropdownData.userId" :key="index" :value="item.value" :label="item.label" />
							</el-select>
						</el-form-item>
					</el-col>
						<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" >
						<el-form-item label="备注" prop="remarks">
							<el-input v-model="state.ruleForm.remarks" placeholder="请输入备注" maxlength="512" show-word-limit clearable />
						</el-form-item>
					</el-col>
				</el-row>
			</el-form>
			<template #footer>
				<span class="dialog-footer">
					<el-button @click="() => state.showDialog = false">取 消</el-button>
					<el-button @click="submit" type="primary" v-reclick="1000">确 定</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>
<style lang="scss" scoped>
:deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>