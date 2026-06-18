import {useBaseApi} from '/@/api/base';

// 产品状态接口服务
export const useLkProductStatusApi = () => {
	const baseApi = useBaseApi("lkProductStatus");
	return {
		// 分页查询产品状态
		page: baseApi.page,
		// 查看产品状态详细
		detail: baseApi.detail,
		// 新增产品状态
		add: baseApi.add,
		// 更新产品状态
		update: baseApi.update,
		// 删除产品状态
		delete: baseApi.delete,
		// 批量删除产品状态
		batchDelete: baseApi.batchDelete,
		// 导出产品状态数据
		exportData: baseApi.exportData,
		// 导入产品状态数据
		importData: baseApi.importData,
		// 下载产品状态数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 产品状态实体
export interface LkProductStatus {
	// 主键Id
	id: number;
	// 产品状态名称
	name?: string;
	// 是否默认
	isDefault?: boolean;
	// 描述
	description: string;
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