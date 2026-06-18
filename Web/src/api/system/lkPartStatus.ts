import {useBaseApi} from '/@/api/base';

// 零件状态接口服务
export const useLkPartStatusApi = () => {
	const baseApi = useBaseApi("lkPartStatus");
	return {
		// 分页查询零件状态
		page: baseApi.page,
		// 查看零件状态详细
		detail: baseApi.detail,
		// 新增零件状态
		add: baseApi.add,
		// 更新零件状态
		update: baseApi.update,
		// 删除零件状态
		delete: baseApi.delete,
		// 批量删除零件状态
		batchDelete: baseApi.batchDelete,
		// 导出零件状态数据
		exportData: baseApi.exportData,
		// 导入零件状态数据
		importData: baseApi.importData,
		// 下载零件状态数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 零件状态实体
export interface LkPartStatus {
	// 主键Id
	id: number;
	// 零件状态名称
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