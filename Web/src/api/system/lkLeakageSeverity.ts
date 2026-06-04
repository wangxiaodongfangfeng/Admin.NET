import {useBaseApi} from '/@/api/base';

// 泄露程度接口服务
export const useLkLeakageSeverityApi = () => {
	const baseApi = useBaseApi("lkLeakageSeverity");
	return {
		// 分页查询泄露程度
		page: baseApi.page,
		// 查看泄露程度详细
		detail: baseApi.detail,
		// 新增泄露程度
		add: baseApi.add,
		// 更新泄露程度
		update: baseApi.update,
		// 删除泄露程度
		delete: baseApi.delete,
		// 批量删除泄露程度
		batchDelete: baseApi.batchDelete,
		// 导出泄露程度数据
		exportData: baseApi.exportData,
		// 导入泄露程度数据
		importData: baseApi.importData,
		// 下载泄露程度数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 泄露程度实体
export interface LkLeakageSeverity {
	// 主键Id
	id: number;
	// 泄露程度名称
	name?: string;
	// 显示颜色
	color?: string;
	// 严重等级
	level?: number;
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