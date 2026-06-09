import {useBaseApi} from '/@/api/base';

// NG位置详情接口服务
export const useLkInspectionNgPositionApi = () => {
	const baseApi = useBaseApi("lkInspectionNgPosition");
	return {
		// 分页查询NG位置详情
		page: baseApi.page,
		// 查看NG位置详情详细
		detail: baseApi.detail,
		// 新增NG位置详情
		add: baseApi.add,
		// 更新NG位置详情
		update: baseApi.update,
		// 删除NG位置详情
		delete: baseApi.delete,
		// 批量删除NG位置详情
		batchDelete: baseApi.batchDelete,
		// 导出NG位置详情数据
		exportData: baseApi.exportData,
		// 导入NG位置详情数据
		importData: baseApi.importData,
		// 下载NG位置详情数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
		// 获取下拉列表数据
		getDropdownData: (fromPage: Boolean = false, cancel: boolean = false) => baseApi.dropdownData({ fromPage }, cancel),
	}
}

// NG位置详情实体
export interface LkInspectionNgPosition {
	// 主键Id
	id: number;
	// 检测记录
	inspectionId?: number;
	// NG位置
	positionId?: number;
	// 图片URL
	imageUrl: string;
	// 泄露程度
	leakageSeverityId: number;
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