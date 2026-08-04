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
		// 获取下拉列表数据
		getDropdownData: (fromPage: Boolean = false, cancel: boolean = false) => baseApi.dropdownData({ fromPage }, cancel),
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
	// 检测时间
	time?: string;
	// 班次
	shiftId?: number;
	// 保压时间
	pressureHoldTime?: number;
	// 气压值
	pressure?: number;
	// 产品类型
	productTypeId?: number;
	// 产品状态
	productStatusId?: number;
	// 零件状态
	partStatusId?: number;
	// 二维码
	productModel: string;
	// 钢印号
	steelStamp: string;
	// 检测结果
	testResult?: string;
	// Ng位置
	ngPositionId: number;
	// 泄露值，null 表示未检测/未记录，0 表示已检测且无泄露
	leakage?: number | null;
	// 试气次数（同一二维码的第 N 次检测）
	testCount?: number;
	// 图片URL列表
	images: string;
	// 用户
	userId?: number;
	// 备注
	remarks: string;
	// 创建时间
	createTime: string;
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