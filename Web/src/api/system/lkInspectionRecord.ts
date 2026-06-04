import {useBaseApi} from '/@/api/base';

// 检测记录接口服务
export const useLkInspectionRecordApi = () => {
	const baseApi = useBaseApi("lkInspectionRecord");
	return {
		// 分页查询检测记录
		page: baseApi.page,
		// 查看检测记录详细
		detail: baseApi.detail,
		// 新增检测记录
		add: baseApi.add,
		// 更新检测记录
		update: baseApi.update,
		// 删除检测记录
		delete: baseApi.delete,
		// 批量删除检测记录
		batchDelete: baseApi.batchDelete,
		// 导出检测记录数据
		exportData: baseApi.exportData,
		// 导入检测记录数据
		importData: baseApi.importData,
		// 下载检测记录数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 检测记录实体
export interface LkInspectionRecord {
	// 主键Id
	id: number;
	// 操作员
	operator?: string;
	// 检测日期
	date?: string;
	// 班次ID
	shiftId?: number;
	// 压力值
	pressure?: number;
	// 产品类型ID
	productTypeId?: number;
	// 产品型号
	productModel: string;
	// 批次号
	batchNumber?: string;
	// 规格
	specification?: string;
	// 钢印号
	steelStamp: string;
	// 检测结果
	testResult?: string;
	// 图片URL列表
	images: string;
	// 用户ID
	userId?: number;
	// 创建时间
	createTime?: string;
	// 备注
	remarks: string;
	// 更新时间
	updateTime: string;
	// 创建者Id
	createUserId: number;
	// 创建者姓名
	createUserName: string;
	// 修改者Id
	updateUserId: number;
	// 修改者姓名
	updateUserName: string;
}