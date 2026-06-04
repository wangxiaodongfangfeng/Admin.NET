import {useBaseApi} from '/@/api/base';

// 班次接口服务
export const useLkShiftApi = () => {
	const baseApi = useBaseApi("lkShift");
	return {
		// 分页查询班次
		page: baseApi.page,
		// 查看班次详细
		detail: baseApi.detail,
		// 新增班次
		add: baseApi.add,
		// 更新班次
		update: baseApi.update,
		// 删除班次
		delete: baseApi.delete,
		// 批量删除班次
		batchDelete: baseApi.batchDelete,
		// 导出班次数据
		exportData: baseApi.exportData,
		// 导入班次数据
		importData: baseApi.importData,
		// 下载班次数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 班次实体
export interface LkShift {
	// 主键Id
	id: number;
	// 班次名称
	name?: string;
	// 开始时间
	startTime: string;
	// 结束时间
	endTime: string;
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