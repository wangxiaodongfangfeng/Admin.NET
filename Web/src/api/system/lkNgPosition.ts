import {useBaseApi} from '/@/api/base';

// NG位置接口服务
export const useLkNgPositionApi = () => {
	const baseApi = useBaseApi("lkNgPosition");
	return {
		// 分页查询NG位置
		page: baseApi.page,
		// 查看NG位置详细
		detail: baseApi.detail,
		// 新增NG位置
		add: baseApi.add,
		// 更新NG位置
		update: baseApi.update,
		// 删除NG位置
		delete: baseApi.delete,
		// 批量删除NG位置
		batchDelete: baseApi.batchDelete,
		// 导出NG位置数据
		exportData: baseApi.exportData,
		// 导入NG位置数据
		importData: baseApi.importData,
		// 下载NG位置数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// NG位置实体
export interface LkNgPosition {
	// 主键Id
	id: number;
	// NG位置名称
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