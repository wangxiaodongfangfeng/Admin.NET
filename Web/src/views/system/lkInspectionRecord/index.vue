<script lang="ts" setup name="lkInspectionRecord">
import { ref, reactive, onMounted } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { downloadStreamFile } from "/@/utils/download";
import { useLkInspectionRecordApi } from '/@/api/system/lkInspectionRecord';
import editDialog from '/@/views/system/lkInspectionRecord/component/editDialog.vue'
import printDialog from '/@/views/system/print/component/hiprint/preview.vue'
import ModifyRecord from '/@/components/table/modifyRecord.vue';
import ImportData from "/@/components/table/importData.vue";

const lkInspectionRecordApi = useLkInspectionRecordApi();
const printDialogRef = ref();
const editDialogRef = ref();
const importDataRef = ref();
const state = reactive({
  exportLoading: false,
  tableLoading: false,
  stores: {},
  showAdvanceQueryUI: false,
  dropdownData: {} as any,
  selectData: [] as any[],
  tableQueryParams: {} as any,
  tableParams: {
    page: 1,
    pageSize: 20,
    total: 0,
    field: 'createTime', // 默认的排序字段
    order: 'descending', // 排序方向
    descStr: 'descending', // 降序排序的关键字符
  },
  tableData: [],
});

// 页面加载时
onMounted(async () => {
  const data = await lkInspectionRecordApi.getDropdownData(true).then(res => res.data.result) ?? {};
  state.dropdownData.shiftId = data.shiftId;
  state.dropdownData.productTypeId = data.productTypeId;
  state.dropdownData.productStatusId = data.productStatusId;
  state.dropdownData.partStatusId = data.partStatusId;
  state.dropdownData.ngPositionId = data.ngPositionId;
  state.dropdownData.userId = data.userId;
});

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await lkInspectionRecordApi.page(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
  state.tableParams.total = result?.total;
  state.tableData = result?.items ?? [];
  state.tableLoading = false;
};

// 列排序
const sortChange = async (column: any) => {
  state.tableParams.field = column.prop;
  state.tableParams.order = column.order;
  await handleQuery();
};

// 删除
const delLkInspectionRecord = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await lkInspectionRecordApi.delete({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => {});
};

// 批量删除
const batchDelLkInspectionRecord = () => {
  ElMessageBox.confirm(`确定要删除${state.selectData.length}条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await lkInspectionRecordApi.batchDelete(state.selectData.map(u => ({ id: u.id }) )).then(res => {
      ElMessage.success(`成功批量删除${res.data.result}条记录`);
      handleQuery();
    });
  }).catch(() => {});
};

// 导出数据
const exportLkInspectionRecordCommand = async (command: string) => {
  try {
    state.exportLoading = true;
    if (command === 'select') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { selectKeyList: state.selectData.map(u => u.id) });
      await lkInspectionRecordApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'current') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams);
      await lkInspectionRecordApi.exportData(params).then(res => downloadStreamFile(res));
    } else if (command === 'all') {
      const params = Object.assign({}, state.tableQueryParams, state.tableParams, { page: 1, pageSize: 99999999 });
      await lkInspectionRecordApi.exportData(params).then(res => downloadStreamFile(res));
    }
  } finally {
    state.exportLoading = false;
  }
}

handleQuery();
</script>
<template>
  <div class="lkInspectionRecord-container" v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }"> 
      <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="state.tableQueryParams.keyword" clearable placeholder="请输入模糊查询关键字"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="操作员">
              <el-input v-model="state.tableQueryParams.operator" clearable placeholder="请输入操作员"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="检测日期">
              <el-input v-model="state.tableQueryParams.date" clearable placeholder="请输入检测日期"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="班次">
              <el-select clearable filterable v-model="state.tableQueryParams.shiftId" placeholder="请选择班次">
                <el-option v-for="(item,index) in state.dropdownData.shiftId ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="保压时间">
              <el-input-number v-model="state.tableQueryParams.pressureHoldTime"  clearable placeholder="请输入保压时间"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="气压值">
              <el-input-number v-model="state.tableQueryParams.pressure"  clearable placeholder="请输入气压值"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="产品类型">
              <el-select clearable filterable v-model="state.tableQueryParams.productTypeId" placeholder="请选择产品类型">
                <el-option v-for="(item,index) in state.dropdownData.productTypeId ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="产品状态">
              <el-select clearable filterable v-model="state.tableQueryParams.productStatusId" placeholder="请选择产品状态">
                <el-option v-for="(item,index) in state.dropdownData.productStatusId ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="零件状态">
              <el-select clearable filterable v-model="state.tableQueryParams.partStatusId" placeholder="请选择零件状态">
                <el-option v-for="(item,index) in state.dropdownData.partStatusId ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="Ng位置">
              <el-select clearable filterable v-model="state.tableQueryParams.ngPositionId" placeholder="请选择Ng位置">
                <el-option v-for="(item,index) in state.dropdownData.ngPositionId ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="二维码">
              <el-input v-model="state.tableQueryParams.productModel" clearable placeholder="请输入二维码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="钢印号">
              <el-input v-model="state.tableQueryParams.steelStamp" clearable placeholder="请输入钢印号"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="检测结果">
              <el-input v-model="state.tableQueryParams.testResult" clearable placeholder="请输入检测结果"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="图片URL列表">
              <el-input v-model="state.tableQueryParams.images" clearable placeholder="请输入图片URL列表"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="用户">
              <el-select clearable filterable v-model="state.tableQueryParams.userId" placeholder="请选择用户">
                <el-option v-for="(item,index) in state.dropdownData.userId ?? []" :key="index" :value="item.value" :label="item.label" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="备注">
              <el-input v-model="state.tableQueryParams.remarks" clearable placeholder="请输入备注"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item >
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'lkInspectionRecord:page'" v-reclick="1000"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => state.tableQueryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="() => state.showAdvanceQueryUI = true" v-if="!state.showAdvanceQueryUI" style="margin-left:5px;"> 高级查询 </el-button>
                <el-button icon="ele-ZoomOut" @click="() => state.showAdvanceQueryUI = false" v-if="state.showAdvanceQueryUI" style="margin-left:5px;"> 隐藏 </el-button>
                <el-button type="danger" style="margin-left:5px;" icon="ele-Delete" @click="batchDelLkInspectionRecord" :disabled="state.selectData.length == 0" v-auth="'lkInspectionRecord:batchDelete'"> 删除 </el-button>
                <el-button type="primary" style="margin-left:5px;" icon="ele-Plus" @click="editDialogRef.openDialog(null, '新增检测记录')" v-auth="'lkInspectionRecord:add'"> 新增 </el-button>
                <el-dropdown :show-timeout="70" :hide-timeout="50" @command="exportLkInspectionRecordCommand">
                  <el-button type="primary" style="margin-left:5px;" icon="ele-FolderOpened" v-reclick="20000" v-auth="'lkInspectionRecord:export'"> 导出 </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="select" :disabled="state.selectData.length == 0">导出选中</el-dropdown-item>
                      <el-dropdown-item command="current">导出本页</el-dropdown-item>
                      <el-dropdown-item command="all">导出全部</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
                <el-button type="warning" style="margin-left:5px;" icon="ele-MostlyCloudy" @click="importDataRef.openDialog()" v-auth="'lkInspectionRecord:import'"> 导入 </el-button>
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" @selection-change="(val: any[]) => { state.selectData = val; }" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
        <el-table-column type="selection" width="40" align="center" v-if="auth('lkInspectionRecord:batchDelete') || auth('lkInspectionRecord:export')" />
        <el-table-column type="index" label="序号" width="55" align="center"/>
        <el-table-column prop='operator' label='操作员' show-overflow-tooltip />
        <el-table-column prop='date' label='检测日期' show-overflow-tooltip />
        <el-table-column prop='shiftId' label='班次' :formatter="(row: any) => row.shiftFkDisplayName" show-overflow-tooltip />
        <el-table-column prop='pressureHoldTime' label='保压时间' show-overflow-tooltip />
        <el-table-column prop='pressure' label='气压值' show-overflow-tooltip />
        <el-table-column prop='productTypeId' label='产品类型' :formatter="(row: any) => row.productTypeFkDisplayName" show-overflow-tooltip />
        <el-table-column prop='productStatusId' label='产品状态' :formatter="(row: any) => row.productStatusFkDisplayName" show-overflow-tooltip />
        <el-table-column prop='partStatusId' label='零件状态' :formatter="(row: any) => row.partStatusFkDisplayName" show-overflow-tooltip />
        <el-table-column prop='ngPositionId' label='Ng位置' :formatter="(row: any) => row.ngPositionFkDisplayName" show-overflow-tooltip />
        <el-table-column prop='productModel' label='二维码' show-overflow-tooltip />
        <el-table-column prop='steelStamp' label='钢印号' show-overflow-tooltip />
        <el-table-column prop='testResult' label='检测结果' show-overflow-tooltip />
        <el-table-column prop='leakage' label='泄露值' show-overflow-tooltip />
        <el-table-column prop='images' label='图片URL列表' show-overflow-tooltip />
        <el-table-column prop='userId' label='用户' :formatter="(row: any) => row.userFkDisplayName" show-overflow-tooltip />
        <el-table-column prop='remarks' label='备注' show-overflow-tooltip />
        <el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
          <template #default="scope">
            <ModifyRecord :data="scope.row" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip v-if="auth('lkInspectionRecord:update') || auth('lkInspectionRecord:delete')">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text type="primary" @click="editDialogRef.openDialog(scope.row, '编辑检测记录')" v-auth="'lkInspectionRecord:update'"> 编辑 </el-button>
            <el-button icon="ele-Delete" size="small" text type="primary" @click="delLkInspectionRecord(scope.row)" v-auth="'lkInspectionRecord:delete'"> 删除 </el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination 
              v-model:currentPage="state.tableParams.page"
              v-model:page-size="state.tableParams.pageSize"
              @size-change="(val: any) => handleQuery({ pageSize: val })"
              @current-change="(val: any) => handleQuery({ page: val })"
              layout="total, sizes, prev, pager, next, jumper"
              :page-sizes="[10, 20, 50, 100, 200, 500]"
              :total="state.tableParams.total"
              size="small"
              background />
      <ImportData ref="importDataRef" :import="lkInspectionRecordApi.importData" :download="lkInspectionRecordApi.downloadTemplate" v-auth="'lkInspectionRecord:import'" @refresh="handleQuery"/>
      <printDialog ref="printDialogRef" :title="'打印检测记录'" @reloadTable="handleQuery" />
      <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
    </el-card>
  </div>
</template>
<style scoped>
:deep(.el-input), :deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>