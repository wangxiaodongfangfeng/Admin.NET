import {useBaseApi} from '/@/api/base';

// 产品类型接口服务
export const useLkProductTypeApi = () => {
	const baseApi = useBaseApi("lkProductType");
	return {
		// 分页查询产品类型
		page: baseApi.page,
		// 查看产品类型详细
		detail: baseApi.detail,
		// 新增产品类型
		add: baseApi.add,
		// 更新产品类型
		update: baseApi.update,
		// 删除产品类型
		delete: baseApi.delete,
		// 批量删除产品类型
		batchDelete: baseApi.batchDelete,
		// 导出产品类型数据
		exportData: baseApi.exportData,
		// 导入产品类型数据
		importData: baseApi.importData,
		// 下载产品类型数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 产品类型实体
export interface LkProductType {
	// 主键Id
	id: number;
	// 产品类型名称
	name?: string;
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